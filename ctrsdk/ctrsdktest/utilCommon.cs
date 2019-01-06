using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ctrsdktest
{
    class utilCommon
    {
        static public readonly string selfCheck_ZC = "正常";
        static public readonly string selfCheck_GZ = "故障";
        static public readonly string checkResult_ZC = "正常";
        static public readonly string checkResult_ZY = "注意";
        static public readonly string checkResult_JJ = "警戒";
        static public readonly string checkResult_BJ = "报警";
        static public readonly string warnningP_NA = "NA";
        static public readonly string warnningP_N = "N";
        static public readonly string warnningP_Y = "Y";
        static public readonly string check_has_start = "检测已开始";
        static public readonly string check_has_stop = "检测已停止";
        static public readonly string check_has_stop_byuser = "用户取消检测";
        static public readonly string B_starting = "正在开启泵";
        static public readonly string B_started = "泵已启动";
        static public readonly string B_start_failed = "泵启动失败";
        static public readonly string B_stop = "泵已停止";
        static public readonly string B_stoping = "正在停止泵";
        static public readonly string B_setSpeeding = "正在设置泵转速";
        static public readonly string B_setSpeed_SUCC = "泵转速设置成功";
        static public readonly string B_setSpeed_FAIL = "泵转速设置失败";
        static public readonly string JC_ing = "正在加磁";
        static public readonly string JC_finished = "加磁完成";
        static public readonly string JC_stop_ing = "正在停磁";
        static public readonly string JC_stop_finished = "停磁完成";
        static public readonly string JC_del_ing = "正在消磁";
        static public readonly string JC_del_finished = "消磁完成";
        static public readonly string B_openLED1ing = "正在开启反射光源";
        static public readonly string B_openLED1ed = "反射光源已开启";
        static public readonly string B_closeLED1ing = "正在停止反射光源";
        static public readonly string B_closeLED1ed = "反射光源已停止";
        static public readonly string startCapture = "正在采集图像";
        static public readonly string B_openLED2ing = "正在开启透射光源";
        static public readonly string B_openLED2ed = "透射光源已开启";
        static public readonly string B_closeLED2ing = "正在停止透射光源";
        static public readonly string B_closeLED2ed = "透射光源已停止";
        static public readonly string Default_ValueID = "1001";
        static public readonly string GREEN_KEEP = "检测仪绿灯长亮";
        static public readonly string GREEN_NOKEEP = "检测仪绿灯闪烁";
        static public readonly string GREEN_CLOSE = "检测仪绿灯关闭";
        static public readonly string RED_KEEP = "检测仪红灯长亮";
        static public readonly string RED_NOKEEP = "检测仪红灯闪烁";
        static public readonly string RED_CLOSE = "检测仪红灯关闭";
        static public readonly string STOP_CHECKING = "正在停止检测...";

        static public readonly string DEFAULT_CCCurrent = "0.6A";
        static public readonly string DEFAULT_LED1_POWER = "6";
        static public readonly string DEFAULT_LED2_POWER = "6";
        static public readonly string DEFAULT_BENG_Speed = "100.00";
        static public readonly string DEFAULT_TSGPrepare = "4";
        static public readonly string DEFAULT_TSGDelay = "2";

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern uint GetTickCount();
        public static int ExecDateDiffToInt(DateTime dateBegin, DateTime dateEnd)
        {
            TimeSpan ts1 = new TimeSpan(dateBegin.Ticks);
            TimeSpan ts2 = new TimeSpan(dateEnd.Ticks);
            TimeSpan ts3 = ts2.Subtract(ts1).Duration(); //返回绝对值
            int ret = Convert.ToInt32(ts3.TotalMinutes);
            return ret <= 0 ? 1 : ret;
        }
        public static int ExecDateDiffToIntV2(DateTime dateBegin, DateTime dateEnd)
        {
            TimeSpan ts1 = new TimeSpan(dateBegin.Ticks);
            TimeSpan ts2 = new TimeSpan(dateEnd.Ticks);
            TimeSpan ts3 = ts2.Subtract(ts1);
            return Convert.ToInt32(ts3.TotalMinutes);
        }
        /// <summary>
        /// 由连字符分隔的32位数字
        /// </summary>
        /// <returns></returns>
        private static string GetGuid()
        {
            System.Guid guid = new Guid();
            guid = Guid.NewGuid();
            return guid.ToString();
        }
        /// <summary>  
        /// 根据GUID获取16位的唯一字符串  
        /// </summary>  
        /// <param name=\"guid\"></param>  
        /// <returns></returns>  
        public static string GuidTo16String()
        {
            long i = 1;
            foreach (byte b in Guid.NewGuid().ToByteArray())
                i *= ((int)b + 1);
            return string.Format("{0:x}", i - DateTime.Now.Ticks);
        }
        /// <summary>  
        /// 根据GUID获取19位的唯一数字序列  
        /// </summary>  
        /// <returns></returns>  
        public static long GuidToLongID()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            return BitConverter.ToInt64(buffer, 0);
        }

        /// <summary>  
        /// float 转换16进制 输出 string
        /// </summary>  
        /// <returns></returns>  
        /// string n = "58.8";
        /// float f = Convert.ToSingle(n);
        /// string result = GetFloatToHEXString(f);
        public static string GetFloatToHEXString(float f)
        {
            var b = BitConverter.GetBytes(f);
            return BitConverter.ToString(b.Reverse().ToArray()).Replace("-", " ");
        }
        /// <summary>
        /// 程序等待延迟执行
        /// </summary>
        /// <param name="ms"></param>
        public static void MySleep(uint ms)
        {
            uint start = GetTickCount();
            while (GetTickCount() - start < ms)
            {
                Console.WriteLine("circle waiting");
                Application.DoEvents();
            }
        }
        public static string GetGUIDByTime()
        {
            Random ro = new Random();
            return DateTime.Now.ToString("yyyyMMddHHmmssfff") + ro.Next(100, 999).ToString();
        }
    }
}
