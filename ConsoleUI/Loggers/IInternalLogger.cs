using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerBackup
{
    public interface IInternalLogger
    {
        void Info(string message);
        void Error(string message, Exception e);
        //Used for errors that result in immediate termination
        void Fatal(string message, Exception e);
        void Warn(string message);
        void Debug(string message);
    }
}
