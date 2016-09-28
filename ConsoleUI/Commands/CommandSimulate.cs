using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerBackup
{
    [System.Obsolete()]
    public class CommandSimulate : ICommand
    {
        public String CommandResult { get; set; }
        public bool Simulate { get; set; }
        public void Initialize() { }
        public void Close() { }
        public bool ContinueOnError { get; set; }

        public Boolean Run(String sourcefilename, String destinationfilename)
        {
            Console.WriteLine("{0} => {1}", sourcefilename, destinationfilename);
            return true;
        }

        public Task<Boolean> RunAsync(String sourcefilename, String destinationfilename, CancellationToken cts)
        {
            Task<bool> t = new Task<bool>(() => Run(sourcefilename, destinationfilename) );
            return t;
        }
    }
}
