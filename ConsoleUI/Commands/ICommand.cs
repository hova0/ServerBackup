using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerBackup
{
    public interface ICommand 
    {
        
        string CommandResult { get; set; }
        bool Simulate { get; set; }
        bool ContinueOnError { get; set; }
        bool Run(string sourcefilename, string destinationfilename);
        Task<bool> RunAsync(string sourcefilename, string destinationfilename, System.Threading.CancellationToken cts);

        void Initialize();
        void Close();

       
    }
}
