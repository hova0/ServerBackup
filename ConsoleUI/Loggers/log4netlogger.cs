using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerBackup.Loggers
{
    class log4netlogger : IInternalLogger
    {
        public log4net.ILog _Logger;

        public log4netlogger()
        {

            _Logger = log4net.LogManager.GetLogger("ServerBackup");
            log4net.Config.XmlConfigurator.Configure();

        }

        public void Info(string message)
        {
            _Logger.Info(message);
        }

        public void Error(string message, Exception e)
        {
            //_Logger.Error(message)
            if (e != null)
                _Logger.Error(message, e);
            else
                _Logger.Error(message);
        }
        public void Fatal(string message, Exception e)
        {
            if (e != null)
                _Logger.Fatal(message, e);
            else
                _Logger.Fatal(message);
        }
        public void Warn(string message)
        {
            _Logger.Warn(message);
        }
        public void Debug(string message)
        {
            _Logger.Debug(message);
        }
    }
}
