using System;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace CtrLog
{
    public class Logger
    {
        private const string FILE_NAME = "Logs.txt";
        public static readonly object Locker = new object();
        private static StreamWriter WRITER;
        private static string ContinueWriteCaches;
        private static readonly Stopwatch Continue_WriteSw;
        public static int AllWriteCount = 0;

        static Logger()
        {
            Continue_WriteSw = new Stopwatch();
        }

        private static string ProjectFullName
        {
            get
            {
                string logFile = "ctrsdk_"+DateTime.Now.ToString("yyyy-MM-dd")+".txt";
                return Path.Combine(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase,logFile);
            }
        }

        private static void Write(string msg)
        {
            if (string.IsNullOrEmpty(msg)) return;

            lock (Locker)
            {
                ContinueWriteCaches = msg;
                 _Write();
            }
        }

        private static void _Write()
        {
            if (ContinueWriteCaches != null)
            {
                WRITER = new StreamWriter(ProjectFullName, true, Encoding.UTF8);
                WRITER.WriteLine(ContinueWriteCaches);
                WRITER.Flush();
                WRITER.Close();
            }
            Continue_WriteSw.Stop();
            Continue_WriteSw.Reset();
            ContinueWriteCaches = null;

            Interlocked.Increment(ref AllWriteCount);
        }

        public static void Debug(string msg)
        {
            msg = string.Format("[{0} {1}] : {2}", "Debug", DateTime.Now.ToString(), msg);
            Write(msg);
        }

        public static void Info(string msg)
        {
            msg = string.Format("[{0} {1} T{2}] : {3}", "Info", DateTime.Now.ToString(),Thread.CurrentThread.ManagedThreadId.ToString(), msg);
            Write(msg);
        }

        public static void Error(string msg)
        {
            msg = string.Format("[{0} {1}] : {2}", "Error", DateTime.Now.ToString(), msg);
            Write(msg);
        }
    }
}
