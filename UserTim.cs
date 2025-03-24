using System;
using System.Timers;
using System.Threading;

namespace UserTim
{
    class UserTim_Class
    {
        private byte system1ms_count = 0;
        private byte system10ms_count = 0;
        private byte system100ms_count = 0;
        private UInt16 system500ms_count = 0;
        private UInt16 system1000ms_count = 0;

        public struct UserTimFlag_t
        {
            public bool system1ms_Flag;
            public bool system10ms_Flag;
            public bool system100ms_Flag;
            public bool system500ms_Flag;
            public bool system1000ms_Flag;
        }

        public UserTimFlag_t UserTimFlag;
        private System.Timers.Timer timer;
        private Thread workerThread;
        private bool isRunning = true;

        // 构造函数
        public UserTim_Class()
        {
            UserTimFlag = default(UserTimFlag_t);

            // 启动定时器，每 1ms 触发
            timer = new System.Timers.Timer(1);
            timer.Elapsed += userTimerCallback;
            timer.AutoReset = true;
            timer.Enabled = true;


        }

        private void userTimerCallback(object sender, ElapsedEventArgs e)
        {
            if (system1ms_count++ >= 1)
            {
                system1ms_count = 0;
                UserTimFlag.system1ms_Flag = true;
            }
            if (system10ms_count++ >= 10)
            {
                system10ms_count = 0;
                UserTimFlag.system10ms_Flag = true;
            }
            if (system100ms_count++ >= 100)
            {
                system100ms_count = 0;
                UserTimFlag.system100ms_Flag = true;
            }
            if (system500ms_count++ >= 500)
            {
                system500ms_count = 0;
                UserTimFlag.system500ms_Flag = true;
            }
            if (system1000ms_count++ >= 1000)
            {
                system1000ms_count = 0;
                UserTimFlag.system1000ms_Flag = true;
            }
        }
    }
}
