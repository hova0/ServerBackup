using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerBackup
{
    class CommandVerify : ICommand
    {
        public String CommandResult { get; set; }
        public bool Simulate { get; set;  }
        public void Initialize() { }
        public void Close() { }
        public bool ContinueOnError { get; set; }
        public Boolean Run(String sourcefilename, String destinationfilename)
        {
            throw new NotImplementedException();
        }

        public Task<Boolean> RunAsync(String sourcefilename, String destinationfilename, System.Threading.CancellationToken cts)
        {
            throw new NotImplementedException();
        }

        public string GetDestinationFileName(string sourcefilename, string sourcebasedirectory, string destination, bool recurse)
        {
            throw new NotImplementedException();
        }
    }
}
