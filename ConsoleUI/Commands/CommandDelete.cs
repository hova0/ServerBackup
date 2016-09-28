using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerBackup
{
    public class CommandDelete : ICommand
    {
        public String CommandResult { get { return sb.ToString(); }  set { } }
        public bool Simulate { get; set; }
        IInternalLogger _log;

        private System.Text.StringBuilder sb = new StringBuilder();
        public void Initialize() { }
        public void Close() { }
        public bool ContinueOnError { get; set; }
        public CommandDelete(IInternalLogger logger)
        {
            _log = logger;
            Simulate = false;
        }
        public Boolean Run(String sourcefilename, String destinationfilename)
        {
            try
            {
                if (Simulate)
                {
                    _log.Info(String.Format("Deleted {0}", sourcefilename));
                    return true;
                }
                EWR.ServerBackup.Library.IOHelper.DoRetryIO(() => System.IO.File.Delete(sourcefilename));
                _log.Debug(String.Format("Deleted {0}", sourcefilename));
                sb.AppendLine(sourcefilename );
            }
            catch (System.IO.IOException)
            {
                return false;
            }
            return true;
        }

        public Task<Boolean> RunAsync(String sourcefilename, String destinationfilename, System.Threading.CancellationToken cts)
        {
            Task<bool> t = new Task<bool>(() => Run(sourcefilename, destinationfilename));
            //return System.Threading.Tasks.Task.Run<bool>(;
            return t;
        }

    }
}
