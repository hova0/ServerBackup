using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerBackup
{
    public class CommandEnsureFreeSpace : ICommand

    {
        public String CommandResult
        {
            get
            {
                return (totalfilesize < destinationfreespace).ToString();
            }
            set { }
        }
        public long FileSpaceRequired { get { return totalfilesize;  } }
        public long FileSpaceAvailable { get { return destinationfreespace;  } }
        public bool ContinueOnError { get; set; }
        public Boolean Simulate { get; set; }

        private long totalfilesize = 0;
        private string destinationdrive;
        private long destinationfreespace = 0;
        public void Initialize() { }
        public void Close() { }

        public Boolean Run(String sourcefilename, String destinationfilename)
        {
            if(destinationdrive == null)
            {
                destinationdrive = System.IO.Path.GetPathRoot(destinationfilename);
                if (destinationdrive.Length <= 3)
                {
                    System.IO.DriveInfo di = new System.IO.DriveInfo(destinationdrive);
                    destinationfreespace = di.TotalFreeSpace;
                } else
                {
                    //Network path!
                    throw new Exception("Network paths are currently not supported");
                }

            }
            System.IO.FileInfo fi = new System.IO.FileInfo(sourcefilename);
            totalfilesize += fi.Length;
            return true;
        }

        public Task<Boolean> RunAsync(String sourcefilename, String destinationfilename, CancellationToken cts)
        {
            return new Task<bool>(() => Run(sourcefilename, destinationfilename));
        }

    }
}
