using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ctrSdk;
using System.IO.Ports;
using webCam;
using System.Timers;
using System.Threading;
using System.IO;

using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.Util;
using debris_processing;

namespace ctrsdktest
{
    public partial class demo : Form
    {
        enum cmd_Type
        {
            boardSelfCheck,
            B_selfCheck,
            B_start,
            B_stop,
            B_start_s,
            B_stop_s,
            B_stop_reset,
            B_setSpeed,
            B_LED1,
            B_LED1_s,
            B_LED2,
            B_LED1_stop,
            B_LED1_stop_s,
            B_LED2_stop,
            JC_Start,
            JC_Stop,
            JC_Des,
            GREEN_Keep,
            GREEN_NoKeep,
            GREEN_Close,
            RED_Keep,
            RED_NoKeep,
            RED_Close,
            nothing,
            F_start,
            F_stop,
            boardReset,
            P_5V1A_Start,
            P_5V1A_Stop,
            P_12V1A_Start,
            P_12V1A_Stop,
            JDQ_1_Start,
            JDQ_1_Stop,
            JDQ_2_Start,
            JDQ_2_Stop
        }

        private int waitingTime = 5;
        private string speed = "100";
        private bool isRecieveAsStr = false;
        static SerialPort _commHelper = new SerialPort();
        static CTRSdk sdk = new CTRSdk();
        private static int _highestPercentageReached = 0;
        private static int _stepHighestPercentage = 0;

        private bool _isHaveTempData = false;
        private string _haveTempData = "";
        private static volatile bool s_isStop = false;
        private int _timeOutTimes = 0;
        private static System.Timers.Timer s_Timer;
        private static bool _latestCmdRecieve = false;

        private webCamHelper _camHelper = null;
        private IntPtr hBaseCamera;
        Dictionary<string, string> _deviceList = new Dictionary<string, string>();

        public demo()
        {
            InitializeComponent();
            button3.Enabled = false;
            button4.Enabled = false;

            button1.Enabled = false;
            button33.Enabled = false;
            initTimer();
            _camHelper = new webCamHelper(ref this.pictureBox1, 250, 250);
        }

        private void addLog(string log)
        {
            string msg = string.Format("[{0} {1}] : {2}", "Info", DateTime.Now.ToString(), log);
            listBox1.Items.Add(msg);
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string test = sdk.testc(5);
            MessageBox.Show(test);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            List<string> deviceList = new List<string>();
            int count = getComDeviceList(ref deviceList);
            for (int i = 0; i <count; i++)
            {
                comboBox1.Items.Add(deviceList[i]);
            }
            comboBox1.SelectedIndex = 0;

            if(count >0)
            {
                button3.Enabled = true;
                button4.Enabled = false;
            }
        }
        #region 串口管理
        public int getComDeviceList(ref List<string> list)
        {
            //检查是否含有串口
            string[] str = SerialPort.GetPortNames();
            if (str == null)
            {
                return 0;
            }
            //添加串口项目
            for (int i = str.Length - 1; i >= 0; i--)
            {
                list.Add(str[i]);
            }
            return list.Count;
        }

        public bool openCom(string comId, string baudRate, string dataBits, string stopBits, string parity)
        {
            //if (comId.Equals("") || !utilCommon.serialPortBaudBox.Contains(baudRate)
            //    || !utilCommon.serialPortDataBox.Contains(dataBits)
            //    || !utilCommon.serialPortStopBox.Contains(stopBits)
            //    || !utilCommon.serialPortParityBox.Contains(parity))
            //{
            //    addLog(String.Format("参数不正确，打开串口失败。错误码 {0}", errCode.ERR_COM_PARAM));
            //    return errCode.ERR_COM_PARAM;
            //}
            try
            {
                //设置串口号
                _commHelper.PortName = comId;

                string strBaudRate = baudRate;
                string strDateBits = dataBits;
                string strStopBits = stopBits;
                string strParity = parity;

                Int32 iBaudRate = Convert.ToInt32(strBaudRate);
                Int32 iDateBits = Convert.ToInt32(strDateBits);

                _commHelper.BaudRate = iBaudRate;       //波特率
                _commHelper.DataBits = iDateBits;       //数据位
                switch (strStopBits)            //停止位
                {
                    case "1":
                        _commHelper.StopBits = StopBits.One;
                        break;
                    case "1.5":
                        _commHelper.StopBits = StopBits.OnePointFive;
                        break;
                    case "2":
                        _commHelper.StopBits = StopBits.Two;
                        break;
                    default:
                        break;
                }
                switch (strParity)             //校验位
                {
                    case "NONE":
                        _commHelper.Parity = Parity.None;
                        break;
                    case "ODD":
                        _commHelper.Parity = Parity.Odd;
                        break;
                    case "EVEN":
                        _commHelper.Parity = Parity.Even;
                        break;
                    default:
                        break;
                }

                if (_commHelper.IsOpen == true)//如果打开状态，则先关闭一下
                {
                    _commHelper.Close();
                }

                string msg = String.Format("串口名称：{0}，波特率：{1}，数据位：{2}，校验位：{3}，停止位：{4}", _commHelper.PortName, _commHelper.BaudRate, _commHelper.DataBits, _commHelper.Parity, _commHelper.StopBits);
                Control.CheckForIllegalCrossThreadCalls = false;    //这个类中我们不检查跨线程的调用是否合法(因为.net 2.0以后加强了安全机制,，不允许在winform中直接跨线程访问控件的属性)
                _commHelper.DataReceived += new SerialDataReceivedEventHandler(_commHelper_DataReceived);
                //准备就绪              
                _commHelper.DtrEnable = true;
                _commHelper.RtsEnable = true;
                _commHelper.ReceivedBytesThreshold = 1;
                //设置数据读取超时为1秒
                _commHelper.ReadTimeout = 5000;
                _commHelper.Open();     //打开串口
                
                button3.Enabled = false;
                button4.Enabled = true;

                addLog("打开成功，"+ msg);

                if(sdk.setCommHelper(_commHelper))
                {
                    addLog("sdk设置串口成功");
                }
            }
            catch (System.Exception ex)
            {
                addLog("串口打开失败，原因：" + ex.ToString());
                return false;
            }
            return true;
        }

        public bool closeCom()
        {
            if (_commHelper.IsOpen == true)
            {
                _commHelper.Close();
                button3.Enabled = true;
                button4.Enabled = false;
                addLog("串口关闭成功");
            }
            return true;
        }
        void _commHelper_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_commHelper.IsOpen)     //此处可能没有必要判断是否打开串口，但为了严谨性，我还是加上了
            {
                string receiveBuffer = "";
                if (isRecieveAsStr)
                {
                    byte[] byteRead = new byte[_commHelper.BytesToRead];
                    _commHelper.Read(byteRead, 0, byteRead.Length);
                    receiveBuffer += System.Text.Encoding.Default.GetString(byteRead) + "\r\n";
                    _commHelper.DiscardInBuffer();                      //清空SerialPort控件的Buffer 
                }
                else
                {
                    try
                    {
                        Byte[] receivedData = new Byte[_commHelper.BytesToRead];        //创建接收字节数组
                        _commHelper.Read(receivedData, 0, receivedData.Length);         //读取数据
                        //清空SerialPort控件的Buffer
                        _commHelper.DiscardInBuffer();
                        //string text = _commHelper.Read();   
                        //Encoding.ASCII.GetString(receivedData);                                
                        //这是用以显示字符串
                        //    string strRcv = null;
                        //    for (int i = 0; i < receivedData.Length; i++ )
                        //    {
                        //        strRcv += ((char)Convert.ToInt32(receivedData[i])) ;
                        //    }
                        //    txtReceive.Text += strRcv + "\r\n";             //显示信息
                        //}
                        string strRcv = null;
                        for (int i = 0; i < receivedData.Length; i++) //窗体显示
                        {

                            strRcv += receivedData[i].ToString("X2");  //16进制显示
                        }
                        receiveBuffer += strRcv;
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("串口接收出错，原因：" + ex.ToString());
                        receiveBuffer = "";
                    }
                    processRecieve(receiveBuffer);
                }
            }
            else
            {
                MessageBox.Show("串口未打开失败");
            }
        }
        private void processRecieve(string data)
        {
            //if (_isHaveTempData)
            //{
            //    data = _haveTempData + data;
            //}
            //int pos = data.IndexOf(getLatestCmdReturn());
            //if (pos != -1)
            //{
            //    _isHaveTempData = false;
            //    _haveTempData = "";
            //    data = getLatestCmdReturn();
            //}
            //else
            //{
            //    _isHaveTempData = true;
            //    _haveTempData = data;
            //    addLog("收到数据：" + data);
            //    if (_haveTempData.Length > 200)
            //    {
            //        _haveTempData = "";
            //        addLog("临时缓存超过200字节，已自动清理");
            //    }
            //    return;
            //}
            //data = data.Replace(" ", "");//16 进制按照字符串处理
            //addLog("收到数据：" + data);
            //switch (_latestCmd.type)
            //{
            //    case cmd_Type.boardSelfCheck:
            //        if (data.Equals(CMDComm.CMD_SELFCHECK_RETURN))
            //        {
            //            addLog("检测仪自检状态：" + utilCommon.selfCheck_ZC);
            //            resetLatestCmd();
            //            finishedSelfCheck();
            //        }
            //        break;
            //    case cmd_Type.B_setSpeed:
            //        if (data.Equals(CMDComm.CMD_BENGSPEED_RETURN))
            //        {
            //            addLog(utilCommon.B_setSpeed_SUCC);
            //            resetLatestCmd();
            //            updateStatusText(utilCommon.B_setSpeed_SUCC);
            //            //泵： 1 单向阀： 2
            //            StartProcessWithB();
            //        }
            //        break;
            //    case cmd_Type.B_start:
            //        if (data.Equals(CMDComm.CMD_STARTBENG_RETURN))
            //        {
            //            addLog("泵已启动");
            //            resetLatestCmd();
            //            updateStatusText(utilCommon.B_started);
            //            _stepHighestPercentage++;
            //            updateStatusText(utilCommon.B_openLED1ing);
            //            StartPointLightLED1();
            //        }
            //        break;
            //    case cmd_Type.B_LED1:
            //        if (data.Equals(CMDComm.CMD_START_POINTLIGHT_RETURN))
            //        {
            //            addLog(utilCommon.B_openLED1ed);
            //            resetLatestCmd();
            //            updateStatusText(utilCommon.B_openLED1ed);
            //            _stepHighestPercentage++;
            //            ExcuteBeforeCheckTime();
            //        }
            //        break;
            //    case cmd_Type.B_LED1_s://再次开启反射光源后，停磁
            //        if (data.Equals(CMDComm.CMD_START_POINTLIGHT_RETURN))
            //        {
            //            addLog(utilCommon.B_openLED1ed);
            //            resetLatestCmd();
            //            updateStatusText(utilCommon.B_openLED1ed);
            //            _stepHighestPercentage++;
            //            updateStatusText(utilCommon.JC_stop_ing);
            //            StopJC();
            //        }
            //        break;
            //    case cmd_Type.B_LED2: //透射灯开启，执行透射等准备时间
            //        if (data.Equals(CMDComm.CMD_START_AREALIGHT_RETURN))
            //        {
            //            addLog(utilCommon.B_openLED2ed);
            //            resetLatestCmd();
            //            updateStatusText(utilCommon.B_openLED2ed);
            //            _stepHighestPercentage++;
            //            ExcuteLED2PrepareTime();
            //        }
            //        break;
            //    case cmd_Type.JC_Start:
            //        if (data.Equals(CMDComm.CMD_PWM_ADD_RETURN))
            //        {
            //            addLog(utilCommon.JC_finished);
            //            resetLatestCmd();
            //            updateStatusText(utilCommon.JC_finished);
            //            _stepHighestPercentage++;
            //            ExcuteMLCJTime();
            //        }
            //        break;
            //    case cmd_Type.B_stop:
            //        if (data.Equals(CMDComm.CMD_STOPBENG_RETURN))
            //        {
            //            addLog(utilCommon.B_stop);
            //            resetLatestCmd();
            //            updateStatusText(utilCommon.B_stop);
            //            _stepHighestPercentage++;
            //            ExcutePictureTime();
            //        }
            //        break;
            //    case cmd_Type.JC_Stop:
            //        if (data.Equals(CMDComm.CMD_PWM_STOP_RETURN))
            //        {
            //            addLog(utilCommon.JC_stop_finished);
            //            resetLatestCmd();
            //            updateStatusText(utilCommon.JC_stop_finished);
            //            _stepHighestPercentage++;
            //            updateStatusText(utilCommon.JC_del_ing);
            //            DelJC();
            //        }
            //        break;
            //    case cmd_Type.JC_Des:
            //        if (data.Equals(CMDComm.CMD_PWM_DEL_RETURN))
            //        {
            //            addLog(utilCommon.JC_del_finished);
            //            resetLatestCmd();
            //            updateStatusText(utilCommon.JC_del_finished);
            //            _stepHighestPercentage++;
            //            ExcuteReDelJC();
            //        }
            //        break;
            //    case cmd_Type.B_LED1_stop: //反射灯关闭，开启投射灯
            //        if (data.Equals(CMDComm.CMD_STOP_POINTLIGHT_RETURN))
            //        {
            //            addLog(utilCommon.B_closeLED1ed);
            //            resetLatestCmd();
            //            updateStatusText(utilCommon.B_closeLED1ed);
            //            _stepHighestPercentage++;
            //            updateStatusText(utilCommon.B_openLED2ing);
            //            StartPointLightLED2();
            //        }
            //        break;
            //    case cmd_Type.B_LED2_stop://透射灯关闭，开启反射灯
            //        if (data.Equals(CMDComm.CMD_STOP_AREALIGHT_RETURN))
            //        {
            //            addLog(utilCommon.B_closeLED2ed);
            //            resetLatestCmd();
            //            updateStatusText(utilCommon.B_closeLED2ed);
            //            _stepHighestPercentage++;
            //            if (_isGetIPCABack)
            //            {
            //                finishedGetIPCABack();
            //                return;
            //            }
            //            updateStatusText(utilCommon.B_openLED1ing);
            //            StartPointLightLED1Again();
            //        }
            //        break;
            //    case cmd_Type.B_start_s:
            //        if (data.Equals(CMDComm.CMD_STARTBENG_RETURN))
            //        {
            //            addLog(utilCommon.B_started);
            //            resetLatestCmd();
            //            updateStatusText(utilCommon.B_started);
            //            _stepHighestPercentage++;
            //            ExcuteAfterCheckTime();
            //        }
            //        break;
            //    case cmd_Type.B_stop_s:
            //        if (data.Equals(CMDComm.CMD_STOPBENG_RETURN))
            //        {
            //            addLog(utilCommon.B_stop);
            //            resetLatestCmd();
            //            updateStatusText(utilCommon.B_stop);
            //            _stepHighestPercentage++;
            //            InvestigateResult();
            //        }
            //        break;
            //    case cmd_Type.GREEN_Keep:
            //        if (data.Equals(CMDComm.CMD_GREENLIGHT_KEEP_RETURN))
            //        {
            //            addLog(utilCommon.GREEN_KEEP);
            //            resetLatestCmd();
            //        }
            //        break;
            //    case cmd_Type.GREEN_NoKeep:
            //        if (data.Equals(CMDComm.CMD_GREENLIGHT_NOKEEP_RETURN))
            //        {
            //            addLog(utilCommon.GREEN_NOKEEP);
            //            resetLatestCmd();
            //        }
            //        break;
            //    case cmd_Type.GREEN_Close:
            //        if (data.Equals(CMDComm.CMD_GREENLIGHT_CLOSE_RETURN))
            //        {
            //            addLog(utilCommon.GREEN_CLOSE);
            //            resetLatestCmd();
            //        }
            //        break;
            //    case cmd_Type.RED_Keep:
            //        if (data.Equals(CMDComm.CMD_REDLIGHT_KEEP_RETURN))
            //        {
            //            addLog(utilCommon.RED_KEEP);
            //            resetLatestCmd();
            //        }
            //        break;
            //    case cmd_Type.RED_NoKeep:
            //        if (data.Equals(CMDComm.CMD_REDLIGHT_NOKEEP_RETURN))
            //        {
            //            addLog(utilCommon.RED_NOKEEP);
            //            resetLatestCmd();
            //        }
            //        break;
            //    case cmd_Type.RED_Close:
            //        if (data.Equals(CMDComm.CMD_REDLIGHT_CLOSE_RETURN))
            //        {
            //            addLog(utilCommon.RED_CLOSE);
            //            resetLatestCmd();
            //        }
            //        break;
            //    default:
            //        break;
            //}
        }
        #endregion

        private void button3_Click(object sender, EventArgs e)
        {
            string strName = comboBox1.Text;
            if(openCom(strName,"9600","8","1","NONE"))
            {
                
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            closeCom();
        }

        private void button23_Click(object sender, EventArgs e)
        {
            if (sdk.sendDXFStartCmd() != 0)
            {
                MessageBox.Show("执行失败，确保串口已打开", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }
            addLog("开启单向阀命令发送成功");
        }

        private void button22_Click(object sender, EventArgs e)
        {
            if (sdk.sendStopDXFCmd() != 0)
            {
                MessageBox.Show("执行失败，确保串口已打开", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }
            addLog("关闭单向阀命令发送成功");
        }

        //自检
        private void button18_Click(object sender, EventArgs e)
        {
            if(sdk.sendSelfCheckCmd() != 0)
            {
                MessageBox.Show("执行失败，确保串口已打开", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }
            addLog("自检命令发送成功");
        }

        private void button20_Click(object sender, EventArgs e)
        {
            if (sdk.sendBengStartCmd() != 0)
            {
                MessageBox.Show("执行失败，确保串口已打开", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }
            addLog("开启泵命令发送成功");
        }

        private void button21_Click(object sender, EventArgs e)
        {
            if (sdk.sendStopBCmd() != 0)
            {
                MessageBox.Show("执行失败，确保串口已打开", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }
            addLog("关闭泵命令发送成功");
        }

        private void button10_Click(object sender, EventArgs e)
        {
            string speed = comboSpeed.Text;
            if (speed.Equals(""))
            {
                MessageBox.Show("泵转速无效，请重新设置", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }
            string INT_BENGSpeed = "";
            int pos = speed.IndexOf(".");
            if (pos == -1)
            {
                INT_BENGSpeed = speed;
                speed = speed + ".00";
            }
            else
            {
                INT_BENGSpeed = speed.Substring(0, pos);
            }
            if (Convert.ToInt32(INT_BENGSpeed) > 300 || Convert.ToInt32(INT_BENGSpeed) <= 0)
            {
                MessageBox.Show("泵速应该在0.00-300.00之间，请重新设置", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }

            if (sdk.sendBengSpeedCmd(speed) != 0)
            {
                MessageBox.Show("执行失败，确保串口已打开", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }
            addLog("泵转速命令发送成功");
        }

        private void button24_Click(object sender, EventArgs e)
        {
            if (sdk.sendStopJCCmd() != 0)
            {
                MessageBox.Show("执行失败，确保串口已打开", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }
            addLog("停磁命令发送成功");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            string cc = comboCCCurrent.Text;
            if (cc.Equals(""))
            {
                MessageBox.Show("请选择磁场电流", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }
            if (sdk.sendStartJCCmd(cc) != 0)
            {
                MessageBox.Show("执行失败，确保串口已打开", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }
            addLog("加磁命令发送成功");
        }

        private void button17_Click(object sender, EventArgs e)
        {
            if (sdk.sendDelJCCmd() != 0)
            {
                MessageBox.Show("执行失败，确保串口已打开", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }
            addLog("消磁命令发送成功");
        }

        private void button19_Click(object sender, EventArgs e)
        {
            if (sdk.sendResetBoard() != 0)
            {
                MessageBox.Show("执行失败，确保串口已打开", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }
            addLog("重启板子命令发送成功");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string level = comboFSGYCurrent.Text;
            if (level.Equals(""))
            {
                MessageBox.Show("请选择反射光源亮度", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }
            if (sdk.sendStartLED1Cmd(level) != 0)
            {
                MessageBox.Show("执行失败，确保串口已打开", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }
            addLog("开启反射光命令发送成功");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (sdk.sendStoppointLED1Cmd() != 0)
            {
                MessageBox.Show("执行失败，确保串口已打开", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }
            addLog("关闭反射光命令发送成功");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            string level = comboTSGYCurrent.Text;
            if (level.Equals(""))
            {
                MessageBox.Show("请选择透射光源亮度", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }
            if (sdk.sendStartLED2Cmd(level) != 0)
            {
                MessageBox.Show("执行失败，确保串口已打开", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }
            addLog("开启透射光命令发送成功");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (sdk.sendStoppointLED2Cmd() != 0)
            {
                MessageBox.Show("执行失败，确保串口已打开", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }
            addLog("关闭透射光命令发送成功");
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (sdk.sendGreenLightKeep() != 0)
            {
                MessageBox.Show("执行失败，确保串口已打开", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }
            addLog("绿灯常亮命令发送成功");
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (sdk.sendGreenLightClose() != 0)
            {
                MessageBox.Show("执行失败，确保串口已打开", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }
            addLog("绿灯关闭命令发送成功");
        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (sdk.sendGreenLightNOKeep() != 0)
            {
                MessageBox.Show("执行失败，确保串口已打开", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }
            addLog("绿灯闪烁命令发送成功");
        }

        private void button16_Click(object sender, EventArgs e)
        {
            if (sdk.sendRedLightKeep() != 0)
            {
                MessageBox.Show("执行失败，确保串口已打开", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }
            addLog("红灯常亮命令发送成功");
        }

        private void button15_Click(object sender, EventArgs e)
        {
            if (sdk.sendRedLightClose() != 0)
            {
                MessageBox.Show("执行失败，确保串口已打开", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }
            addLog("红灯关闭命令发送成功");
        }

        private void button14_Click(object sender, EventArgs e)
        {
            if (sdk.sendRedLightNOKeep() != 0)
            {
                MessageBox.Show("执行失败，确保串口已打开", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }
            addLog("红灯闪烁命令发送成功");
        }

        private void button25_Click(object sender, EventArgs e)
        {
            if (sdk.send5V1APowerStartCmd() != 0)
            {
                MessageBox.Show("执行失败，确保串口已打开", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }
            addLog("5V1A电源开启命令发送成功");
        }

        private void button26_Click(object sender, EventArgs e)
        {
            if (sdk.send5V1APowerStopCmd() != 0)
            {
                MessageBox.Show("执行失败，确保串口已打开", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }
            addLog("5V1A电源关闭命令发送成功");
        }

        private void button27_Click(object sender, EventArgs e)
        {
            if (sdk.send12V1APowerStartCmd() != 0)
            {
                MessageBox.Show("执行失败，确保串口已打开", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }
            addLog("12V1A电源开启命令发送成功");
        }

        private void button28_Click(object sender, EventArgs e)
        {
            if (sdk.send12V1APowerStopCmd() != 0)
            {
                MessageBox.Show("执行失败，确保串口已打开", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }
            addLog("12V1A电源关闭命令发送成功");
        }

        private void button31_Click(object sender, EventArgs e)
        {
            if (sdk.sendJDQ1StartCmd() != 0)
            {
                MessageBox.Show("执行失败，确保串口已打开", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }
            addLog("继电器一开启命令发送成功");
        }

        private void button32_Click(object sender, EventArgs e)
        {
            if (sdk.sendJDQ1StopCmd() != 0)
            {
                MessageBox.Show("执行失败，确保串口已打开", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }
            addLog("继电器一关闭命令发送成功");
        }

        private void button29_Click(object sender, EventArgs e)
        {
            if (sdk.sendJDQ2StartCmd() != 0)
            {
                MessageBox.Show("执行失败，确保串口已打开", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }
            addLog("继电器二开启命令发送成功");
        }

        private void button30_Click(object sender, EventArgs e)
        {
            if (sdk.sendJDQ2StopCmd() != 0)
            {
                MessageBox.Show("执行失败，确保串口已打开", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }
            addLog("继电器二关闭命令发送成功");
        }
        private void button34_Click(object sender, EventArgs e)
        {
            int deviceNameLen = 100;
            StringBuilder deviceName = new StringBuilder(deviceNameLen);
            if (usbcamera.uvsGetAvailableDevice() > 0)
            {
                hBaseCamera = usbcamera.uvsOpenDevice(0);
                if (hBaseCamera != null)
                {
                    if (usbcamera.uvsGetDeviceName(hBaseCamera, deviceName, deviceNameLen) != 0)
                    {
                        return;
                    }
                }
            }

            bool findDevice = false;
            string tempDeviceName = deviceName.ToString();
            _camHelper.GetCamList(ref _deviceList);
            foreach (var item in _deviceList)
            {
                if (item.Key == tempDeviceName)
                {
                    findDevice = true;
                    comboBox2.Items.Add(item.Key);
                    break;
                }
            }
            if(findDevice)
            {
                button1.Enabled = true;
                comboBox2.SelectedIndex = 0;
            }
        }
        private void button1_Click_1(object sender, EventArgs e)
        {
            if (!_commHelper.IsOpen) //如果没打开
            {
                MessageBox.Show("串口未打开", "温馨提示", 0, MessageBoxIcon.Information, 0);
                return;
            }

            button1.Enabled = false;
            button33.Enabled = true;
            //执行等待耗时
            s_isStop = false;
            ExcuteStartWaitingTime();
        }
        private bool startCam(string name)
        {
            string camName = name;
            if (!camName.Equals(""))
            {
                try
                {
                    _camHelper.Open(camName);
                }
                catch (System.Exception ex)
                {
                    return false;
                }
                return true;
            }
            return false;
        }
        private bool stopCam()
        {
            /*
             * 异步关闭摄像头
             */
            closeCam();
            return true;
        }
        private void closeCam()
        {
            if (_camHelper != null)
            {
                Thread t2 = new Thread(CloseCamThread);
                t2.Start(_camHelper);
            }
        }
        private void CloseCamThread(object helper)
        {
            webCamHelper currenthelper = (webCamHelper)helper;
            currenthelper.Close();
        }
        private bool snapCam(string name)
        {
            if (File.Exists(name))
            {
                File.Delete(name);
            }
            if (true == _camHelper.Capture(name))
            {
                addLog("检测图像获取成功,文件：" + name);
                return true;
            }
            addLog("检测图像获取失败");
            return false;
        }
        private void ForceCloseForm()
        {
            System.Environment.Exit(0);
        }
        private void CloseForm(object sender, FormClosingEventArgs e)
        {
            if (_camHelper != null && _camHelper.isRunning())
            {
                e.Cancel = true;
                MessageBox.Show("为了设备安全使用，请先停止检测，再关闭应用程序", "关闭提示", 0, MessageBoxIcon.Information, 0);
                return;
            }
            if (DialogResult.OK == MessageBox.Show("你确定要关闭应用程序吗？", "关闭提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question))
            {
                if (_camHelper != null && _camHelper.isRunning())
                {
                    closeCam();
                }
                if (_commHelper.IsOpen)
                {
                    _commHelper.Close();
                }
            }
            else
            {
                e.Cancel = true;
            }
        }

        delegate void timeMessageBoxShow(string msg, int count);
        public void asynTimeMessageShow(string msg, int count)
        {
            this.Invoke(new timeMessageBoxShow(timeMessageBoxShow_F), new object[] { msg, count });
        }
        void timeMessageBoxShow_F(string msg, int count)
        {
            timingMessageBox messageBox = new timingMessageBox(msg, count);
            messageBox.ShowDialog();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_latestCmdRecieve || s_isStop)
                return;
            s_Timer.Stop();
            cmd_Type type = (cmd_Type)sdk.getLatestCmdType();
            switch (type)
            {
                case cmd_Type.boardSelfCheck:
                    if (_timeOutTimes >= 3)
                    {
                        s_isStop = true;
                        //自检失败改为绿灯闪烁2018.11.3
                        sdk.sendGreenLightNOKeep();
                        if (_camHelper != null && _camHelper.isRunning())
                        {
                            closeCam();
                        }
                        if (_commHelper.IsOpen)
                        {
                            _commHelper.Close();
                        }
                        asynTimeMessageShow("检测仪自检失败", 0);
                        return;
                    }
                    _timeOutTimes++;
                    addLog(String.Format("自检命令超时，重新发送,第 {0} 次", _timeOutTimes));

                    s_Timer.Stop();
                    sdk.sendSelfCheckCmd();
                    s_Timer.Start();
                    break;
                case cmd_Type.B_setSpeed:
                    if (_timeOutTimes >= 3)
                    {
                        s_isStop = true;
                        addLog("设置泵转速失败");
                        asynTimeMessageShow("泵转速设置失败", 0);
                        return;
                    }
                    _timeOutTimes++;
                    addLog(String.Format("泵转速设置命令超时，重新发送,第 {0} 次", _timeOutTimes));

                    s_Timer.Stop();
                    sdk.sendBengSpeedCmd(speed);
                    s_Timer.Start();
                    break;
                case cmd_Type.B_start:
                    if (_timeOutTimes >= 3)
                    {
                        s_isStop = true;
                        addLog("开启泵失败");
                        asynTimeMessageShow("开启泵失败", 0);
                        //MessageBox.Show("开启泵失败", "温馨提示", 0, MessageBoxIcon.Information, 0);
                        return;
                    }
                    _timeOutTimes++;
                    addLog(String.Format("开启泵命令超时，重新发送,第 {0} 次", _timeOutTimes));

                    s_Timer.Stop();
                    sdk.sendBengStartCmd();
                    s_Timer.Start();
                    break;
                case cmd_Type.B_LED1:
                    if (_timeOutTimes >= 3)
                    {
                        s_isStop = true;
                        addLog("开启反射光源失败");
                        asynTimeMessageShow("开启反射光源失败", 0);
                        return;
                    }
                    _timeOutTimes++;
                    addLog(String.Format("开启反射光源超时，重新发送,第 {0} 次", _timeOutTimes));

                    s_Timer.Stop();
                    sdk.sendStartLED1Cmd("4");
                    s_Timer.Start();
                    break;
                case cmd_Type.B_LED1_s:
                    if (_timeOutTimes >= 3)
                    {
                        s_isStop = true;
                        addLog("开启反射光源失败");
                        asynTimeMessageShow("开启反射光源失败", 0);
                        return;
                    }
                    _timeOutTimes++;
                    addLog(String.Format("开启反射光源超时，重新发送,第 {0} 次", _timeOutTimes));

                    s_Timer.Stop();
                    sdk.sendStartLED1CmdAgain("4");
                    s_Timer.Start();
                    break;
                case cmd_Type.B_LED2:
                    if (_timeOutTimes >= 3)
                    {
                        s_isStop = true;
                        addLog("开启透射光源失败");
                        asynTimeMessageShow("开启透射光源失败", 0);
                        return;
                    }
                    _timeOutTimes++;
                    addLog(String.Format("开启透射光源超时，重新发送,第 {0} 次", _timeOutTimes));

                    s_Timer.Stop();
                    sdk.sendStartLED2Cmd("4");
                    s_Timer.Start();
                    break;
                case cmd_Type.JC_Start:
                    if (_timeOutTimes >= 3)
                    {
                        s_isStop = true;
                        addLog("加磁失败");
                        asynTimeMessageShow("加磁失败", 0);
                        return;
                    }
                    _timeOutTimes++;
                    addLog(String.Format("加磁超时，重新发送,第 {0} 次", _timeOutTimes));

                    s_Timer.Stop();
                    sdk.sendStartJCCmd("0.4A");
                    s_Timer.Start();
                    break;
                case cmd_Type.B_stop:
                    if (_timeOutTimes >= 3)
                    {
                        s_isStop = true;
                        addLog("停泵失败");
                        asynTimeMessageShow("停泵失败", 0);
                        return;
                    }
                    _timeOutTimes++;
                    addLog(String.Format("停泵超时，重新发送,第 {0} 次", _timeOutTimes));

                    s_Timer.Stop();
                    sdk.sendStopBCmd();
                    s_Timer.Start();
                    break;
                case cmd_Type.JC_Stop:
                    if (_timeOutTimes >= 3)
                    {
                        s_isStop = true;
                        addLog("停磁失败");
                        asynTimeMessageShow("停磁失败", 0);
                        return;
                    }
                    _timeOutTimes++;
                    addLog(String.Format("停磁超时，重新发送,第 {0} 次", _timeOutTimes));

                    s_Timer.Stop();
                    sdk.sendStopJCCmd();
                    s_Timer.Start();
                    break;
                case cmd_Type.JC_Des:
                    if (_timeOutTimes >= 3)
                    {
                        s_isStop = true;
                        addLog("消磁失败");
                        asynTimeMessageShow("消磁失败", 0);
                        return;
                    }
                    _timeOutTimes++;
                    addLog(String.Format("消磁超时，重新发送,第 {0} 次", _timeOutTimes));

                    s_Timer.Stop();
                    sdk.sendDelJCCmd();
                    s_Timer.Start();
                    break;
                case cmd_Type.B_LED1_stop:
                    if (_timeOutTimes >= 3)
                    {
                        s_isStop = true;
                        addLog("反射光源关闭失败");
                        asynTimeMessageShow("反射光源关闭失败", 0);
                        return;
                    }
                    _timeOutTimes++;
                    addLog(String.Format("反射光源关闭超时，重新发送,第 {0} 次", _timeOutTimes));

                    s_Timer.Stop();
                    sdk.sendStoppointLED1Cmd();
                    s_Timer.Start();
                    break;
                case cmd_Type.B_LED2_stop:
                    if (_timeOutTimes >= 3)
                    {
                        s_isStop = true;
                        addLog("透射光源关闭失败");
                        asynTimeMessageShow("透射光源关闭失败", 0);
                        return;
                    }
                    _timeOutTimes++;
                    addLog(String.Format("透射光源关闭超时，重新发送,第 {0} 次", _timeOutTimes));

                    s_Timer.Stop();
                    sdk.sendStoppointLED2Cmd();
                    s_Timer.Start();
                    break;
                case cmd_Type.B_start_s:
                    if (_timeOutTimes >= 3)
                    {
                        s_isStop = true;
                        addLog("二次开泵失败");
                        asynTimeMessageShow("二次开泵失败", 0);
                        return;
                    }
                    _timeOutTimes++;
                    addLog(String.Format("二次开泵超时，重新发送,第 {0} 次", _timeOutTimes));

                    s_Timer.Stop();
                    sdk.sendBengStartCmdAgain();
                    s_Timer.Start();
                    break;
                case cmd_Type.B_stop_s:
                    if (_timeOutTimes >= 3)
                    {
                        s_isStop = true;
                        addLog("二次停泵失败");
                        asynTimeMessageShow("二次停泵失败", 0);
                        return;
                    }
                    _timeOutTimes++;
                    addLog(String.Format("二次停泵超时，重新发送,第 {0} 次", _timeOutTimes));

                    s_Timer.Stop();
                    sdk.sendStopBCmdAgain();
                    s_Timer.Start();
                    break;
                default:
                    break;
            }
        }
        private void initTimer()
        {
            s_Timer = new System.Timers.Timer(10000);
            s_Timer.Elapsed += new ElapsedEventHandler(Timer_Tick);
            s_Timer.Interval = 5000;
            s_Timer.AutoReset = false;
        }
        private void updateStatusText(string msg)
        {
            this.toolStripStatusLabel1.Text = msg;
        }
        private void updateStatusCheckTimes(int i)
        {
            String str = String.Format("当前第{0}次检测", i);
            this.toolStripStatusLabel4.Text = str;
        }
        private void DoSomething(BackgroundWorker worker, DoWorkEventArgs e)
        {
            while (_stepHighestPercentage <= _highestPercentageReached)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    worker.ReportProgress(0);
                    break;
                }
                else
                {
                    Thread.Sleep(1000);
                    Console.WriteLine("当前percent: " + _stepHighestPercentage);
                    worker.ReportProgress(_stepHighestPercentage);
                    if (_stepHighestPercentage == _highestPercentageReached)
                    {
                        break;
                    }
                }
            }
        }
        private void InitializeBackgroundWorker()
        {
            backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
                    backgroundWorker1_RunWorkerCompleted);
            backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(
                    backgroundWorker1_ProgressChanged);
            backgroundWorker1.WorkerSupportsCancellation = true;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            //还原worker对象
            BackgroundWorker worker = sender as BackgroundWorker;
            //开始工作
            DoSomething(worker, e);
        }

        private void ExcuteResetTime()
        {
            addLog("等待设备重启，时间：5秒");
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler((s, arg) =>
            {
                int time = (int)arg.Argument;
                for (int i = time; i > 0; i--)
                {
                    this.toolStripStatusLabel1.Text = String.Format("---{0}---", i.ToString());
                    Thread.Sleep(1000);
                }
                stopCam();
                //绿灯开启
                sdk.sendGreenLightKeep();

                button33.Enabled = false;
                button1.Enabled = true;
                addLog("已停止");
                this.toolStripStatusLabel1.Text = "已停止";
                this.toolStripStatusLabel4.Text = "";
            });
            worker.RunWorkerAsync(Convert.ToInt32(5));
        }
        private void stopChecking()
        {
            button33.Text = "停止检测";
            button33.Enabled = false;
            s_isStop = true;
            s_Timer.Stop();
            if (backgroundWorker1.IsBusy)
            {
                backgroundWorker1.CancelAsync();
            }
            /*
             * 停止泵，重启板子，耗时 5s ,重置绿灯闪烁。
             */
            sdk.sendStopBCmd();
            Thread.Sleep(200);
            sdk.sendResetBoard();
            ExcuteResetTime();
        }
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //还原Worker对象
            BackgroundWorker worker = sender as BackgroundWorker;
            //判断是否由错误造成意外中止
            if (e.Error != null)
            {
                //若发生错误，弹窗显示错误信息
                this.toolStripStatusLabel1.Text = "异常终止";
                addLog("检测异常终止，原因： " + e.Error.Message);
                stopChecking();
            }
            //判断是否用户手动取消，若程序要支持此处功能，需要程序中有cancel的动作，并在该动作中将e.cancel置为true
            else if (e.Cancelled)
            {
                //添加用户手动取消的动作
                this.toolStripProgressBar1.Value = 0;
            }
            //判断是否正常结束
            else
            {
                this.toolStripProgressBar1.Value = 0;
            }
        }
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //更新进度条进度值
            this.toolStripProgressBar1.Value = e.ProgressPercentage;
        }
        /*
          * 执行等待耗时操作
          */
        private void ExcuteStartWaitingTime()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler((s, arg) =>
            {
                int time = (int)arg.Argument;
                for (int i = time; i > 0; i--)
                {
                    if (s_isStop)
                    {
                        updateStatusText(utilCommon.STOP_CHECKING);
                        return;
                    }
                    this.toolStripStatusLabel1.Text = String.Format("---{0}---", i.ToString());
                    Thread.Sleep(1000);
                }
                if (s_isStop)
                {
                    updateStatusText(utilCommon.STOP_CHECKING);
                    return;
                }
                //开启摄像头
                if (!startCam("SMI"))
                {
                    return;
                }
                sdk.sendGreenLightKeep();//绿灯长亮
                updateStatusText(utilCommon.B_setSpeeding);

                s_Timer.Stop();
                sdk.sendBengSpeedCmd(speed);
                s_Timer.Start();
            });
            worker.RunWorkerAsync(waitingTime);
        }

        private void button33_Click(object sender, EventArgs e)
        {
            stopChecking();
        }
    }
}
