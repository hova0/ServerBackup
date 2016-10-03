using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerBackupTest
{
    public class StubLogger : ServerBackup.IInternalLogger
    {
        public void Error(String message, Exception e)
        {
            if (!string.IsNullOrEmpty(message))
                Console.WriteLine("[STUB]" + message);
        }

        public void Fatal(String message, Exception e)
        {
            if (!string.IsNullOrEmpty(message))
                Console.WriteLine("[STUB]" + message);
        }

        public void Info(String message)
        {
            if (!string.IsNullOrEmpty(message))
                Console.WriteLine("[STUB]" + message);
        }
        public void Warn(string message)
        {
            Console.WriteLine(message);
        }
        public void Debug(string message)
        {
            Console.WriteLine(message);
        }
    }
}
