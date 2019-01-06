using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using UTILITY;
using CtrLog;
using ERRCODE;
using System.Threading;

namespace ctrSdk
{
    public class CTRSdk
    {
        private SerialPort commHelper = new SerialPort();
        private latestCommand _latestCmd = new latestCommand();

        private void addLog(string log)
        {
            string msg = string.Format("[{0} {1}] : {2}", "Info", DateTime.Now.ToString(), log);
            Logger.Info(log);
        }
        public string testc(int x)
        {
            return Convert.ToString(x);
        }
        public bool setCommHelper(SerialPort sp)
        {
            if(sp.IsOpen)
            {
                commHelper = sp;
                return true;
            }
            return false;
        }
        public int getLatestCmdType()
        {
            return (int)_latestCmd.type;
        }
        private string getLatestCmdReturn()
        {
            string cmd = "";
            switch (_latestCmd.type)
            {
                case cmd_Type.boardSelfCheck:
                    cmd = CMDComm.CMD_SELFCHECK_RETURN;
                    break;
                case cmd_Type.B_start:
                    cmd = CMDComm.CMD_STARTBENG_RETURN;
                    break;
                case cmd_Type.B_setSpeed:
                    cmd = CMDComm.CMD_BENGSPEED_RETURN;
                    break;
                case cmd_Type.JC_Start:
                    cmd = CMDComm.CMD_PWM_ADD_RETURN;
                    break;
                case cmd_Type.B_LED1:
                    cmd = CMDComm.CMD_START_POINTLIGHT_RETURN;
                    break;
                case cmd_Type.B_LED1_s:
                    cmd = CMDComm.CMD_START_POINTLIGHT_RETURN;
                    break;
                case cmd_Type.B_LED2:
                    cmd = CMDComm.CMD_START_AREALIGHT_RETURN;
                    break;
                case cmd_Type.B_stop:
                    cmd = CMDComm.CMD_STOPBENG_RETURN;
                    break;
                case cmd_Type.JC_Stop:
                    cmd = CMDComm.CMD_PWM_STOP_RETURN;
                    break;
                case cmd_Type.JC_Des:
                    cmd = CMDComm.CMD_PWM_DEL_RETURN;
                    break;
                case cmd_Type.B_LED1_stop:
                    cmd = CMDComm.CMD_STOP_POINTLIGHT_RETURN;
                    break;
                case cmd_Type.B_LED2_stop:
                    cmd = CMDComm.CMD_STOP_AREALIGHT_RETURN;
                    break;
                case cmd_Type.B_start_s:
                    cmd = CMDComm.CMD_STARTBENG_RETURN;
                    break;
                case cmd_Type.B_stop_s:
                    cmd = CMDComm.CMD_STOPBENG_RETURN;
                    break;
                case cmd_Type.B_stop_reset:
                    cmd = CMDComm.CMD_STOPBENG_RETURN;
                    break;
                case cmd_Type.GREEN_Keep:
                    cmd = CMDComm.CMD_GREENLIGHT_KEEP_RETURN;
                    break;
                case cmd_Type.GREEN_NoKeep:
                    cmd = CMDComm.CMD_GREENLIGHT_NOKEEP_RETURN;
                    break;
                case cmd_Type.RED_Keep:
                    cmd = CMDComm.CMD_REDLIGHT_KEEP_RETURN;
                    break;
                case cmd_Type.RED_NoKeep:
                    cmd = CMDComm.CMD_REDLIGHT_NOKEEP_RETURN;
                    break;
                case cmd_Type.GREEN_Close:
                    cmd = CMDComm.CMD_GREENLIGHT_CLOSE_RETURN;
                    break;
                case cmd_Type.RED_Close:
                    cmd = CMDComm.CMD_REDLIGHT_CLOSE_RETURN;
                    break;
                default:
                    cmd = "no latest cmd";
                    break;
            }
            return cmd;
        }
        private int sendCommData(string data, bool isStr)
        {
            if (!commHelper.IsOpen) //如果没打开
            {
                addLog("串口未打开，请到参数设置进行重新配置");
                return errCode.ERR_COM_UNOPEN;
            }

            String strSend = data;
            if (!isStr)	//“HEX发送” 按钮 
            {
                //处理数字转换
                string sendBuf = strSend;
                string sendnoNull = sendBuf.Trim();
                string sendNOComma = sendnoNull.Replace(',', ' ');    //去掉英文逗号
                string sendNOComma1 = sendNOComma.Replace('，', ' '); //去掉中文逗号
                string strSendNoComma2 = sendNOComma1.Replace("0x", "");   //去掉0x
                strSendNoComma2.Replace("0X", "");   //去掉0X
                string[] strArray = strSendNoComma2.Split(' ');

                int byteBufferLength = strArray.Length;
                for (int i = 0; i < strArray.Length; i++)
                {
                    if (strArray[i] == "")
                    {
                        byteBufferLength--;
                    }
                }
                // int temp = 0;
                byte[] byteBuffer = new byte[byteBufferLength];
                int ii = 0;
                for (int i = 0; i < strArray.Length; i++)        //对获取的字符做相加运算
                {

                    Byte[] bytesOfStr = Encoding.Default.GetBytes(strArray[i]);

                    int decNum = 0;
                    if (strArray[i] == "")
                    {
                        //ii--;     //加上此句是错误的，下面的continue以延缓了一个ii，不与i同步
                        continue;
                    }
                    else
                    {
                        decNum = Convert.ToInt32(strArray[i], 16); //atrArray[i] == 12时，temp == 18 
                    }

                    try    //防止输错，使其只能输入一个字节的字符
                    {
                        byteBuffer[ii] = Convert.ToByte(decNum);
                    }
                    catch (System.Exception ex)
                    {
                        addLog("字节越界，请逐个字节输入，发送失败;原因：" + ex.ToString());
                        return errCode.ERR_COM_DATAERR;
                    }

                    ii++;
                }
                commHelper.Write(byteBuffer, 0, byteBuffer.Length);
            }
            else		//以字符串形式发送时 
            {
                commHelper.WriteLine(strSend);    //写入数据
            }
            return errCode.ERR_SUCCESS;
        }

        //////////////////////////////////////////////////命令///////////////////////////////////////////////////////
        #region 板子自检
        /*
         * 板子自检
         */
        public int sendSelfCheckCmd()
        {
            string data = CMDComm.CMD_SELFCHECK;
            int ret = sendCommData(data, false);
            _latestCmd.type = cmd_Type.boardSelfCheck;
            _latestCmd.cmd = data;
            _latestCmd.sendDate = DateTime.Now.ToLocalTime().ToString();
            addLog(String.Format("检测仪自检命令已发送，返回值：{0}，CMD：{1}", ret, data));
            return ret;
        }
        #endregion
        #region 开启泵
        public int sendBengStartCmd()
        {
            string data = CMDComm.CMD_STARTBENG;
            int ret = sendCommData(data, false);
            _latestCmd.type = cmd_Type.B_start;
            _latestCmd.cmd = data;
            _latestCmd.sendDate = DateTime.Now.ToLocalTime().ToString();
            addLog(String.Format("开启泵命令已发送，返回值：{0}，CMD：{1}", ret, data));
            return ret;
        }

        public int sendBengStartCmdAgain()
        {
            string data = CMDComm.CMD_STARTBENG;
            int ret = sendCommData(data, false);
            _latestCmd.type = cmd_Type.B_start_s;
            _latestCmd.cmd = data;
            _latestCmd.sendDate = DateTime.Now.ToLocalTime().ToString();
            addLog(String.Format("开启泵命令已发送，返回值：{0}，CMD：{1}", ret, data));
            return ret;
        }
        #endregion
        #region 关闭泵
        /*
        * 关闭泵
        */
        public int sendStopBCmd()
        {
            string data = CMDComm.CMD_STOPBENG;
            int ret = sendCommData(data, false);
            _latestCmd.type = cmd_Type.B_stop;
            _latestCmd.cmd = data;
            _latestCmd.sendDate = DateTime.Now.ToLocalTime().ToString();
            addLog(String.Format("停止泵命令已发送，返回值：{0}，CMD：{1}", ret, data));
            return ret;
        }
        public int sendStopBCmdAgain()
        {
            string data = CMDComm.CMD_STOPBENG;
            int ret = sendCommData(data, false);
            _latestCmd.type = cmd_Type.B_stop_s;
            _latestCmd.cmd = data;
            _latestCmd.sendDate = DateTime.Now.ToLocalTime().ToString();
            addLog(String.Format("停止泵命令已发送，返回值：{0}，CMD：{1}", ret, data));
            return ret;
        }
        #endregion
        #region 开启阀
        public int sendDXFStartCmd()
        {
            string data = CMDComm.CMD_STARTDXF;
            int ret = sendCommData(data, false);
            _latestCmd.type = cmd_Type.F_start;
            _latestCmd.cmd = data;
            _latestCmd.sendDate = DateTime.Now.ToLocalTime().ToString();
            addLog(String.Format("开启单向阀命令已发送，返回值：{0}，CMD：{1}", ret, data));
            return ret;
        }
        #endregion
        #region 关闭阀
        /*
        * 关闭阀
        */
        public int sendStopDXFCmd()
        {
            string data = CMDComm.CMD_STOPDXF;
            int ret = sendCommData(data, false);
            _latestCmd.type = cmd_Type.F_stop;
            _latestCmd.cmd = data;
            _latestCmd.sendDate = DateTime.Now.ToLocalTime().ToString();
            addLog(String.Format("停止单向阀命令已发送，返回值：{0}，CMD：{1}", ret, data));
            return ret;
        }
        #endregion
        #region 重启板子
        public int sendResetBoard()
        {
            string dataReset = CMDComm.CMD_RESET_BOARD;
            int ret= sendCommData(dataReset, false);
            _latestCmd.type = cmd_Type.boardReset;
            _latestCmd.cmd = dataReset;
            _latestCmd.sendDate = DateTime.Now.ToLocalTime().ToString();
            addLog(String.Format("正在重启设备...，返回值：{0}，CMD：{1}", ret, dataReset));
            return ret;
        }
        #endregion
        #region 设置泵转速
        /*
         * 设置泵转速
         */
        public int sendBengSpeedCmd(string speed)
        {
            if (speed.Equals(""))
            {
                addLog(String.Format("泵转速格式无效,错误码: {0}", errCode.ERR_BENG_SPEEDINVALID));
                return errCode.ERR_BENG_SPEEDINVALID;
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
                addLog(String.Format("速度值超出限制,错误码: {0}", errCode.ERR_BENG_SPEEDINVALID));
                return errCode.ERR_BENG_SPEEDINVALID;
            }

            string data = CMDComm.GetBengSpeedCmd(speed);
            int ret = sendCommData(data, false);
            _latestCmd.type = cmd_Type.B_setSpeed;
            _latestCmd.cmd = data;
            _latestCmd.sendDate = DateTime.Now.ToLocalTime().ToString();
            addLog(String.Format("泵转速设置命令已发送，返回值：{0}，转速：{1} rpm,CMD：{2}", ret, speed,data));
            return ret;
        }
        #endregion
        #region 加磁
        /*
         *加磁场 
         */
        public int sendStartJCCmd(string cc)
        {
            string data = "";
            foreach (var item in CMDComm.CMD_PWM_ADD)
            {
                if (item.Key == cc)
                {
                    data = item.Value;
                    break;
                }
            }
            if(data.Equals(""))
            {
                return errCode.ERR_PARAM_INVALID;
            }
            int ret = sendCommData(data, false);
            _latestCmd.type = cmd_Type.JC_Start;
            _latestCmd.cmd = data;
            _latestCmd.sendDate = DateTime.Now.ToLocalTime().ToString();
            addLog(String.Format("加磁命令已发送，返回值：{0}，CMD：{1}", ret, data));
            return ret;
        }
        #endregion
        #region 停止加磁
        public int sendStopJCCmd()
        {
            string data = CMDComm.CMD_PWM_STOP;
            int ret= sendCommData(data, false);
            _latestCmd.type = cmd_Type.JC_Stop;
            _latestCmd.cmd = data;
            _latestCmd.sendDate = DateTime.Now.ToLocalTime().ToString();
            addLog(String.Format("停磁命令已发送，返回值：{0}，CMD：{1}", ret, data));
            return ret;
        }
        #endregion
        #region 消磁
        public int sendDelJCCmd()
        {
            string data = CMDComm.CMD_PWM_DEL;
            int ret = sendCommData(data, false);
            _latestCmd.type = cmd_Type.JC_Des;
            _latestCmd.cmd = data;
            _latestCmd.sendDate = DateTime.Now.ToLocalTime().ToString();
            addLog(String.Format("消磁命令已发送，返回值：{0}，CMD：{1}", ret, data));
            return ret;
        }
        #endregion
        #region 开启LED1
        /*
         * 开启LED1
         */
        public int sendStartLED1Cmd(string level)
        {
            string data = "";
            foreach (var item in CMDComm.CMD_START_POINTLIGHT)
            {
                if (item.Key == level)
                {
                    data = item.Value;
                    break;
                }
            }
            if(data.Equals(""))
            {
                return errCode.ERR_PARAM_INVALID;
            }
            int ret = sendCommData(data, false);
            _latestCmd.type = cmd_Type.B_LED1;
            _latestCmd.cmd = data;
            _latestCmd.sendDate = DateTime.Now.ToLocalTime().ToString();
            addLog(String.Format("开启反射光源(LED1)命令已发送，返回值：{0}，level:{1},CMD：{2}", ret, level, data));
            return ret;
        }
        #endregion
        #region 再次开启LED1
        /*
         * 再次开启LED1
         */
        public int sendStartLED1CmdAgain(string level)
        {
            string data = "";
            foreach (var item in CMDComm.CMD_START_POINTLIGHT)
            {
                if (item.Key == level)
                {
                    data = item.Value;
                    break;
                }
            }
            if (data.Equals(""))
            {
                return errCode.ERR_PARAM_INVALID;
            }
            int ret = sendCommData(data, false);
            _latestCmd.type = cmd_Type.B_LED1_s;
            _latestCmd.cmd = data;
            _latestCmd.sendDate = DateTime.Now.ToLocalTime().ToString();
            addLog(String.Format("开启反射光源(LED1)命令已发送，返回值：{0}，level:{1},CMD：{2}", ret, level, data));
            return ret;
        }
        #endregion
        #region 开启LED2
        /*
         * 开启LED2
         */
        public int sendStartLED2Cmd(string level)
        {
            string data = "";
            foreach (var item in CMDComm.CMD_START_AREALIGHT)
            {
                if (item.Key == level)
                {
                    data = item.Value;
                    break;
                }
            }
            if (data.Equals(""))
            {
                return errCode.ERR_PARAM_INVALID;
            }
            int ret = sendCommData(data, false);
            _latestCmd.type = cmd_Type.B_LED2;
            _latestCmd.cmd = data;
            _latestCmd.sendDate = DateTime.Now.ToLocalTime().ToString();
            addLog(String.Format("开启透射光源(LED2)命令已发送，返回值：{0}，level:{1},CMD：{2}", ret, level, data));
            return ret;
        }
        #endregion
        #region 关闭反射光源
        public int sendStoppointLED1Cmd()
        {
            string data = CMDComm.CMD_STOP_POINTLIGHT;
            int ret = sendCommData(data, false);
            _latestCmd.type = cmd_Type.B_LED1_stop;
            _latestCmd.cmd = data;
            _latestCmd.sendDate = DateTime.Now.ToLocalTime().ToString();
            addLog(String.Format("关闭反射光源命令已发送，返回值：{0}，CMD：{1}", ret, data));
            return ret;
        }
        public int sendStoppointLED1CmdAgain()
        {
            string data = CMDComm.CMD_STOP_POINTLIGHT;
            int ret = sendCommData(data, false);
            _latestCmd.type = cmd_Type.B_LED1_stop_s;
            _latestCmd.cmd = data;
            _latestCmd.sendDate = DateTime.Now.ToLocalTime().ToString();
            addLog(String.Format("关闭反射光源命令已发送，返回值：{0}，CMD：{1}", ret, data));
            return ret;
        }
        #endregion
        #region 关闭透射光源
        public int sendStoppointLED2Cmd()
        {
            string data = CMDComm.CMD_STOP_AREALIGHT;
            int ret = sendCommData(data, false);
            _latestCmd.type = cmd_Type.B_LED2_stop;
            _latestCmd.cmd = data;
            _latestCmd.sendDate = DateTime.Now.ToLocalTime().ToString();
            addLog(String.Format("关闭透射光源命令已发送，返回值：{0}，CMD：{1}", ret, data));
            return ret;
        }
        #endregion
        #region 绿灯控制
        /*启动长亮，重启设备长亮，执行耗时长亮（自检通过）*/
        public int sendGreenLightKeep()
        {
            string data = CMDComm.CMD_GREENLIGHT_KEEP;
            int ret = sendCommData(data, false);
            _latestCmd.type = cmd_Type.GREEN_Keep;
            _latestCmd.cmd = data;
            _latestCmd.sendDate = DateTime.Now.ToLocalTime().ToString();
            addLog(String.Format("绿色长亮指示灯已开启，返回值：{0}，CMD：{1}", ret, data));
            return ret;
        }
        /*只在自检失败时，绿灯闪烁*/
        public int sendGreenLightNOKeep()
        {
            string data = CMDComm.CMD_GREENLIGHT_NOKEEP;
            int ret = sendCommData(data, false);
            _latestCmd.type = cmd_Type.GREEN_NoKeep;
            _latestCmd.cmd = data;
            _latestCmd.sendDate = DateTime.Now.ToLocalTime().ToString();
            addLog(String.Format("绿色闪烁指示灯已开启，返回值：{0}，CMD：{1}", ret, data));
            return ret;
        }
        /*启动时关闭，然后长亮*/
        public int sendGreenLightClose()
        {
            string data = CMDComm.CMD_GREENLIGHT_CLOSE;
            int ret = sendCommData(data, false);
            _latestCmd.type = cmd_Type.GREEN_Close;
            _latestCmd.cmd = data;
            _latestCmd.sendDate = DateTime.Now.ToLocalTime().ToString();
            addLog(String.Format("绿色指示灯已关闭，返回值：{0}，CMD：{1}", ret, data));
            return ret;
        }
        #endregion

        #region 红灯控制
        public int sendRedLightKeep()
        {
            string data = CMDComm.CMD_REDLIGHT_KEEP;
            int ret = sendCommData(data, false);
            _latestCmd.type = cmd_Type.RED_Keep;
            _latestCmd.cmd = data;
            _latestCmd.sendDate = DateTime.Now.ToLocalTime().ToString();
            addLog(String.Format("红色长亮指示灯已开启，返回值：{0}，CMD：{1}", ret, data));
            return ret;
        }
        /*报警和警告红灯闪缩*/
        public int sendRedLightNOKeep()
        {
            string data = CMDComm.CMD_REDLIGHT_NOKEEP;
            int ret = sendCommData(data, false);
            _latestCmd.type = cmd_Type.RED_NoKeep;
            _latestCmd.cmd = data;
            _latestCmd.sendDate = DateTime.Now.ToLocalTime().ToString();
            addLog(String.Format("红色闪烁指示灯已开启，返回值：{0}，CMD：{1}", ret, data));
            return ret;
        }
        /*启动时，关闭一次。正常，注意指示灯不闪*/
        public int sendRedLightClose()
        {
            string data = CMDComm.CMD_REDLIGHT_CLOSE;
            int ret = sendCommData(data, false);
            _latestCmd.type = cmd_Type.RED_Close;
            _latestCmd.cmd = data;
            _latestCmd.sendDate = DateTime.Now.ToLocalTime().ToString();
            addLog(String.Format("红色指示灯已关闭，返回值：{0}，CMD：{1}", ret, data));
            return ret;
        }
        #endregion
        public void resetLatestCmd()
        {
            _latestCmd.type = cmd_Type.nothing;
            _latestCmd.cmd = "";
            _latestCmd.cmd_return = "";
            _latestCmd.returnDate = "";
            _latestCmd.sendDate = "";
        }

        #region 5V1A 开关
        public int send5V1APowerStartCmd()
        {
            string data = CMDComm.CMD_5V1A_START;
            int ret = sendCommData(data, false);
            _latestCmd.type = cmd_Type.P_5V1A_Start;
            _latestCmd.cmd = data;
            _latestCmd.sendDate = DateTime.Now.ToLocalTime().ToString();
            addLog(String.Format("5V1A电源开启命令已发送，返回值：{0}，CMD：{1}", ret, data));
            return ret;
        }
        public int send5V1APowerStopCmd()
        {
            string data = CMDComm.CMD_5V1A_STOP;
            int ret = sendCommData(data, false);
            _latestCmd.type = cmd_Type.P_5V1A_Stop;
            _latestCmd.cmd = data;
            _latestCmd.sendDate = DateTime.Now.ToLocalTime().ToString();
            addLog(String.Format("5V1A电源关闭命令已发送，返回值：{0}，CMD：{1}", ret, data));
            return ret;
        }
        #endregion
        #region 12V1A 开关
        public int send12V1APowerStartCmd()
        {
            string data = CMDComm.CMD_12V1A_START;
            int ret = sendCommData(data, false);
            _latestCmd.type = cmd_Type.P_12V1A_Start;
            _latestCmd.cmd = data;
            _latestCmd.sendDate = DateTime.Now.ToLocalTime().ToString();
            addLog(String.Format("12V1A电源开启命令已发送，返回值：{0}，CMD：{1}", ret, data));
            return ret;
        }
        public int send12V1APowerStopCmd()
        {
            string data = CMDComm.CMD_12V1A_STOP;
            int ret = sendCommData(data, false);
            _latestCmd.type = cmd_Type.P_12V1A_Stop;
            _latestCmd.cmd = data;
            _latestCmd.sendDate = DateTime.Now.ToLocalTime().ToString();
            addLog(String.Format("12V1A电源关闭命令已发送，返回值：{0}，CMD：{1}", ret, data));
            return ret;
        }
        #endregion
        #region 继电器一 开关
        public int sendJDQ1StartCmd()
        {
            string data = CMDComm.CMD_JDQ1_START;
            int ret = sendCommData(data, false);
            _latestCmd.type = cmd_Type.JDQ_1_Start;
            _latestCmd.cmd = data;
            _latestCmd.sendDate = DateTime.Now.ToLocalTime().ToString();
            addLog(String.Format("继电器一开启命令已发送，返回值：{0}，CMD：{1}", ret, data));
            return ret;
        }
        public int sendJDQ1StopCmd()
        {
            string data = CMDComm.CMD_JDQ1_STOP;
            int ret = sendCommData(data, false);
            _latestCmd.type = cmd_Type.JDQ_1_Stop;
            _latestCmd.cmd = data;
            _latestCmd.sendDate = DateTime.Now.ToLocalTime().ToString();
            addLog(String.Format("继电器一关闭命令已发送，返回值：{0}，CMD：{1}", ret, data));
            return ret;
        }
        #endregion
        #region 继电器二 开关
        public int sendJDQ2StartCmd()
        {
            string data = CMDComm.CMD_JDQ2_START;
            int ret = sendCommData(data, false);
            _latestCmd.type = cmd_Type.JDQ_2_Start;
            _latestCmd.cmd = data;
            _latestCmd.sendDate = DateTime.Now.ToLocalTime().ToString();
            addLog(String.Format("继电器二开启命令已发送，返回值：{0}，CMD：{1}", ret, data));
            return ret;
        }
        public int sendJDQ2StopCmd()
        {
            string data = CMDComm.CMD_JDQ2_STOP;
            int ret = sendCommData(data, false);
            _latestCmd.type = cmd_Type.JDQ_2_Stop;
            _latestCmd.cmd = data;
            _latestCmd.sendDate = DateTime.Now.ToLocalTime().ToString();
            addLog(String.Format("继电器二关闭命令已发送，返回值：{0}，CMD：{1}", ret, data));
            return ret;
        }
        #endregion
    }
}
