using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using webCam;

namespace ctrsdktest
{
    public partial class process : Form
    {
        private bool _isHaveTempData = false;
        private string _haveTempData = "";
        private static volatile bool s_isStop = false;
        private int _timeOutTimes = 0;
        private static System.Timers.Timer s_Timer;
        private static bool _latestCmdRecieve = false;

        private webCamHelper camHelper = null;
        private IntPtr hBaseCamera;
        Dictionary<string, string> _deviceList = new Dictionary<string, string>();

        public process()
        {
            InitializeComponent();
            camHelper = new webCamHelper(ref this.pictureBox1, 640, 480);
        }
        private void CloseThread(object helper)
        {
            webCamHelper currenthelper = (webCamHelper)helper;
            currenthelper.Close();
        }
        private void button1_Click(object sender, EventArgs e)
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
            string tempDeviceName = deviceName.ToString();
            camHelper.GetCamList(ref _deviceList);
            foreach (var item in _deviceList)
            {
                if (item.Key == tempDeviceName)
                {
                    comboBox1.Items.Add(item.Key);
                    break;
                }
            }
            comboBox1.SelectedIndex = 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //执行等待耗时
            s_isStop = false;
            //ExcuteStartWaitingTime();
        }
    }
}
