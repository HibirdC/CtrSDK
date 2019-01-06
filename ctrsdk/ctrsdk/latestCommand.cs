using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ctrSdk
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
    class latestCommand
    {
        public string cmd;
        public string cmd_return;
        public cmd_Type type;
        public string sendDate;
        public string returnDate;
    }
}
