using System;
using System.Collections.Generic;
using System.Text;

namespace Dream_Protector_Free
{
    internal delegate void LogHandler(DateTime date, byte kind, string message);

    internal class Logger
    {

        public event LogHandler OnLog;

        private void Log(byte kind,string message)
        {
            OnLog?.Invoke(DateTime.Now, kind, message);
        }

        public void LogInformation(string message)
        {
            Log(0, message);
        }

        public void LogWarning(string message)
        {
            Log(1, message);
        }

        public void LogError(string message)
        {
            Log(2, message);
        }

        public void LogSuccess(string message)
        {
            Log(3, message);
        }
    }
}
