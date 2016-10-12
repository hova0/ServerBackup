using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerBackup
{
    public class CommandZip : ICommand, IDisposable
    {
        public String CommandResult { get; set; }
        public Boolean Simulate { get; set; }
        public string DestinationFileName { get; set; }
        public string BaseDirectory { get; set; }
        private IInternalLogger log;
        private System.Text.StringBuilder sb = new StringBuilder();
        public bool ContinueOnError { get; set; }
        private ICSharpCode.SharpZipLib.Zip.ZipOutputStream destinationzipfile;

        public CommandZip(IInternalLogger _logger)
        {
            log = _logger;
        }

        public void Initialize()
        {
            destinationzipfile = new ICSharpCode.SharpZipLib.Zip.ZipOutputStream(new System.IO.FileStream(DestinationFileName, System.IO.FileMode.CreateNew));
            destinationzipfile.UseZip64 = ICSharpCode.SharpZipLib.Zip.UseZip64.On;

        }
        public void Close()
        {
            if (destinationzipfile != null)
            {
                destinationzipfile.Close();
                destinationzipfile.Dispose();
            }
        }

        public Boolean Run(String sourcefilename, String destinationfilename)
        {
            //System.IO.File.Copy(filename, System.IO.Path.Combine(this.Destination, filename));
            System.Threading.CancellationToken cts = new System.Threading.CancellationToken();
            Task<bool> Ziptask = RunAsync(sourcefilename, destinationfilename, cts);

            Ziptask.Wait(cts);
            bool returnresult = Ziptask.Result;
            Ziptask.Dispose();
            return returnresult;
        }

        public async Task<Boolean> RunAsync(String sourcefilename, String destinationfilename, CancellationToken cancellationToken)
        {
            //Unfortunately, you cannot multithread this.   Hence the compiler warning.

            byte[] buffer = new byte[16384];
            bool copyaborted = false;

            //Early exit
            if (cancellationToken.IsCancellationRequested)
                return false;

            if (Simulate)
            {
                log.Info(String.Format("Zip {0} => {1}", sourcefilename, DestinationFileName));
                return true;
            }

            lock (this)
            {

                string zipfilename = sourcefilename;
                if (BaseDirectory != null)
                {
                    //log.Debug(String.Format("{0} => {1}  ({3})", zipfilename, zipfilename.Replace(BaseDirectory, ""), BaseDirectory));
                    zipfilename = zipfilename.Replace(BaseDirectory, "");
                }

                ICSharpCode.SharpZipLib.Zip.ZipEntry ze = new ICSharpCode.SharpZipLib.Zip.ZipEntry(zipfilename);
                // If flags or additional things need to be set on the ZipEntry, do it below

                destinationzipfile.PutNextEntry(ze);

                using (System.IO.FileStream rs = new System.IO.FileStream(sourcefilename, System.IO.FileMode.Open))
                {
                    bool donecopying = false;
                    while (!donecopying)
                    {

                        if (cancellationToken.IsCancellationRequested)
                        {
                            //Abort copy
                            copyaborted = true;
                            //log.Fatal("Aborting " + sourcefilename, null);
                            break;
                        }
                        int readbytes = rs.Read(buffer, 0, 16384);
                        if (readbytes > 0)
                        {
                            destinationzipfile.Write(buffer, 0, readbytes);
                            // Async writes are NOT SUPPORTED
                        }
                        if (rs.Position == rs.Length)
                        {
                            donecopying = true;
                        }
                    }
                    rs.Close();
                    destinationzipfile.CloseEntry();

                    if (!copyaborted)
                    {
                        log.Info(sourcefilename + " => " + DestinationFileName);
                        rs.Dispose();
                    }
                    else
                    {
                        //Clean up
                        rs.Dispose();
                        //System.IO.File.Delete(destinationfilename);
                        CommandResult = null;
                        log.Fatal(sourcefilename + " aborted.", null);
                        return false;
                    }
                }

            }


            //CommandResult = destinationfilename;


            return true;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~CommandZip() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
