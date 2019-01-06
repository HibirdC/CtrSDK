using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UTILITY;
using CTRCRC;
using CtrLog;

namespace ctrSdk
{
    class CMDComm
    {
        /*
         * 自检
        */
        public const string CMD_SELFCHECK = "68 02 02 68 00 0D 0D 16";
        public const string CMD_SELFCHECK_RETURN = "68030368000D263316";

        /*
         *启动泵 
        */
        public const string CMD_STARTBENG = "01 06 03 E8 00 01 C8 7A";
        public const string CMD_STARTBENG_RETURN = "010603E80001C87A";

        /*
         *停止泵 
        */
        public const string CMD_STOPBENG = "01 06 03 E8 00 00 09 BA";
        public const string CMD_STOPBENG_RETURN = "010603E8000009BA";

        /*
         * 点光源灭
         * 68 02 02 68 00 0A 0A 16
           返回值：68 02 02 68 00 0A 26 30 16
       */
        public const string CMD_STOP_POINTLIGHT = "68 02 02 68 00 0A 0A 16";
        public const string CMD_STOP_POINTLIGHT_RETURN = "68030368000A263016";

        //68 04 04 68 00 09 03 84 90 16（LEV9）
        //68 04 04 68 00 09 03 20 2C 16（LEV8）
        //68 04 04 68 00 09 01 F4 FE 16（LEV7）
        //68 04 04 68 00 09 01 90 9A 16（LEV6） 
        //68 04 04 68 00 09 01 2C 36 16（LEV5） 
        //68 04 04 68 00 09 00 C8 d1 16（LEV4）
        //68 04 04 68 00 09 00 64 6d 16（LEV3）
        //68 04 04 68 00 09 00 32 3b 16（LEV2） 
        // 68 04 04 68 00 09 00 16 1F 16（LEV1） 

        /*
         * 点光源亮
        */
        public static Dictionary<string, string> CMD_START_POINTLIGHT = new Dictionary<string, string> {
            { "1", "68 04 04 68 00 09 00 16 1F 16"}, 
            { "2", "68 04 04 68 00 09 00 32 3b 16"},
            { "3", "68 04 04 68 00 09 00 64 6d 16"},
            { "4", "68 04 04 68 00 09 00 C8 d1 16"},
            { "5", "68 04 04 68 00 09 01 2C 36 16"},
            { "6", "68 04 04 68 00 09 01 90 9A 16"},
            { "7", "68 04 04 68 00 09 01 F4 FE 16"},
            { "8", "68 04 04 68 00 09 03 20 2C 16"},
            { "9", "68 04 04 68 00 09 03 84 90 16"},
            { "T", "68 04 04 68 00 09 02 00 0B 16"}
        };

        public const string CMD_START_POINTLIGHT_RETURN = "680303680009262F16";

        /*
         * 设置转速
        */
        //01 10 03 EA 00 02 04 42 6B 33 33 58 29
        //01 10 03 EA 00 02 04 42 6B 33 33 58 29
        public static string GetBengSpeedCmd(string speed)
        {
            string cmd = "";
            if(speed.Equals(""))
            {
                return cmd;
            }
            if(speed.IndexOf(".") ==-1)
            {
                speed += ".00";
            }
            try
            {
                string middleCmd = utilCommon.GetFloatToHEXString(Convert.ToSingle(speed));
                string data = "01 10 03 EA 00 02 04 " + middleCmd;
                string CRCData = CRC.ToModbusCRC16(data, true);
                cmd = data + " " + CRCData.Substring(0, 2) + " " + CRCData.Substring(2, 2);
                return cmd;
            }
            catch (Exception ex)
            {
                Logger.Info(ex.ToString());
            }
            return cmd;
        }
        public const string CMD_BENGSPEED_RETURN = "011003EA00026078";

        //油道冲刷 相当于泵开启

        /*
         * 加磁场 
        */
        //68 04 04 68 00 13 00 0E 21 16(0.1A)
        //68 04 04 68 00 13 00 17 2A 16(0.2A)
        //68 04 04 68 00 13 00 21 34 16(0.3A)
        //68 04 04 68 00 13 00 2B 3E 16(0.4A)
        //68 04 04 68 00 13 00 35 48 16(0.5A)
        //68 04 04 68 00 13 00 40 53 16(0.6A)

        //返回值：68 03 03 68 00 13 26 39 16

        public static  Dictionary<string, string> CMD_PWM_ADD = new Dictionary<string,string>
        {{"0.1A","68 04 04 68 00 13 00 0E 21 16"},
        {"0.2A","68 04 04 68 00 13 00 17 2A 16"},
        {"0.3A","68 04 04 68 00 13 00 21 34 16"},
        {"0.4A","68 04 04 68 00 13 00 2B 3E 16"},
        {"0.5A","68 04 04 68 00 13 00 17 2A 16"},
        {"0.6A","68 04 04 68 00 13 00 45 58 16"},
        {"0.65A","68 04 04 68 00 13 00 52 65 16"}};

        public const string CMD_PWM_ADD_RETURN = "680303680013263916";

        /*
        * 停磁场 
        */
        public const string CMD_PWM_STOP = "68 02 02 68 00 14 14 16";
        public const string CMD_PWM_STOP_RETURN = "680303680014263A16";

        /*
        * 消磁场 
        */
        public const string CMD_PWM_DEL = "68 06 06 68 00 15 00 06 00 C8 E3 16";
        public const string CMD_PWM_DEL_RETURN = "680303680015263B16";

        //单向阀 
        public const string CMD_STARTDXF = "68 02 02 68 00 0F 0F 16";
        public const string CMD_STARTDXF_RETURN = "68030368000F263516";

        public const string CMD_STOPDXF  = "68 02 02 68 00 10 10 16";
        public const string CMD_STOPDXF_RETURN = "680303680010263616";


        /*
         * 面光源 亮
        */
        //68 04 04 68 00 0B 02 bc C9 16（LEV8） 
        //68 04 04 68 00 0B 02 58 65 16（LEV7） 
        //68 04 04 68 00 0B 01 F4 00 16（LEV6） 
        //68 04 04 68 00 0B 01 90 9C 16（LEV5）
        //68 04 04 68 00 0B 01 2C 38 16（LEV4） 
        //68 04 04 68 00 0B 00 C8 d3 16（LEV3） 
        //68 04 04 68 00 0B 00 64 6F 16（LEV2）
        //68 04 04 68 00 0B 00 32 3d 16（LEV1）

        //返回值：68 02 02 68 00 0B 26 31 16
        public static Dictionary<string, string> CMD_START_AREALIGHT = new Dictionary<string, string> {
            { "1", "68 04 04 68 00 0B 00 32 3d 16"}, 
            { "2", "68 04 04 68 00 0B 00 64 6F 16"},
            { "3", "68 04 04 68 00 0B 00 C8 d3 16"},
            { "4", "68 04 04 68 00 0B 01 2C 38 16"},
            { "5", "68 04 04 68 00 0B 01 90 9C 16"},
            { "6", "68 04 04 68 00 0B 01 F4 00 16"},
            { "7", "68 04 04 68 00 0B 02 58 65 16"},
            { "8", "68 04 04 68 00 0B 02 bc C9 16"}};

        public const string CMD_START_AREALIGHT_RETURN = "68030368000B263116";

        /*
         * 面光源 灭
        */
        public const string CMD_STOP_AREALIGHT = "68 02 02 68 00 0C 0C 16";
        public const string CMD_STOP_AREALIGHT_RETURN = "68030368000C263216";

        /*
         * 绿灯长亮
         */
        public const string CMD_GREENLIGHT_KEEP = "68 03 03 68 00 01 00 01 16";

        /*
         * 绿灯闪
         */
        public const string CMD_GREENLIGHT_NOKEEP = "68 03 03 68 00 01 01 02 16";

        /*
         * 绿灯返回
         */
        public const string CMD_GREENLIGHT_KEEP_RETURN = "680303680001262716";

        /*
        * 绿灯返回
        */
        public const string CMD_GREENLIGHT_NOKEEP_RETURN = "680303680001262716";


        /*
         * 绿灯关闭
         */
        public const string CMD_GREENLIGHT_CLOSE = "68 02 02 68 00 02 02 16";

        /*
         * 绿灯关闭返回
         */
        public const string CMD_GREENLIGHT_CLOSE_RETURN = "680303680002262816";


        /*
         *红灯长亮 
         */
        public const string CMD_REDLIGHT_KEEP = "68 03 03 68 00 03 00 03 16";

        /*
         * 红灯闪烁
         */
        public const string CMD_REDLIGHT_NOKEEP = "68 03 03 68 00 03 01 04 16";

        /*
         * 红灯返回
         */
        public const string CMD_REDLIGHT_KEEP_RETURN = "680303680003262916";

        /*
         * 红灯返回
         */
        public const string CMD_REDLIGHT_NOKEEP_RETURN = "680303680003262916";

        /*
         * 红灯关闭
         */
        public const string CMD_REDLIGHT_CLOSE = "68 02 02 68 00 04 04 16";

        /*
         * 红灯关闭返回
         */
        public const string CMD_REDLIGHT_CLOSE_RETURN = "680303680004262A16";

        /*
         * 重启板子
         */
        public const string CMD_RESET_BOARD = "68 02 02 68 00 0E 0E 16";
        /*
         * 5V 1A 开
         */
        public const string CMD_5V1A_START = "68 02 02 68 00 05 05 16";
        /*
         * 5V 1A 关
         */
        public const string CMD_5V1A_STOP = "68 02 02 68 00 06 06 16";
        /*
         * 12V 1A 开
         */
        public const string CMD_12V1A_START = "68 02 02 68 00 07 07 16";

        /*
         * 12V 1A 关
         */
        public const string CMD_12V1A_STOP = "68 02 02 68 00 08 08 16";

        /*
         * 继电器一 开
         */
        public const string CMD_JDQ1_START = "68 02 02 68 00 0F 0F 16";

        /*
         * 继电器一 关
         */
        public const string CMD_JDQ1_STOP = "68 02 02 68 00 10 10 16";

        /*
         * 继电器二 开
         */
        public const string CMD_JDQ2_START = "68 02 02 68 00 11 11 16";

        /*
         * 继电器二 关
         */
        public const string CMD_JDQ2_STOP = "68 02 02 68 00 12 12 16";
    }
}
