using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
namespace ServerBackup
{
    public class CommandHash : ICommand, IDisposable

    {
        public String CommandResult { get {
                lock (lockObject) { return sb.ToString(); }  } set { } }
        public Boolean Simulate { get; set; }
        public System.Security.Cryptography.HashAlgorithmName HashingFunction { get; set; }
        public string DestinationFileName { get; set; }
        public bool ContinueOnError { get; set; }
        private System.IO.StreamWriter hashoutputfile;
        private System.Text.StringBuilder sb = new StringBuilder();
        private IInternalLogger log;
        private object lockObject = new object();

        public CommandHash(IInternalLogger _logger)
        {
            log = _logger;

        }
        public void Initialize()
        {
            if (!String.IsNullOrEmpty(DestinationFileName))
                hashoutputfile = new System.IO.StreamWriter(DestinationFileName);
            else
                throw new Exception("DestionationFileName not set!");

        }
        public void Close()
        {
            if (hashoutputfile != null)
                hashoutputfile.Close();
        }

        public Boolean Run(String sourcefilename, String destinationfilename)
        {

            System.Threading.CancellationTokenSource c = new CancellationTokenSource();
            Task<bool> runit = RunAsync(sourcefilename, destinationfilename, c.Token);
            runit.Wait();
            return runit.Result;
        }

        public async Task<Boolean> RunAsync(String sourcefilename, String destinationfilename, CancellationToken cts)
        {
            byte[] buffer = new byte[16384];
            System.IO.FileStream verifystream = new System.IO.FileStream(sourcefilename, System.IO.FileMode.Open);

            System.Security.Cryptography.HashAlgorithm localhashfunction = CreateLocalHashFunction(HashingFunction);
            localhashfunction.Initialize();
            //byte[] hashvalue = verifyhash.ComputeHash(verifystream);
            while (true)
            {
                int readbytes = await verifystream.ReadAsync(buffer, 0, 16384);
                if (readbytes == 0)
                    break;
                localhashfunction.TransformBlock(buffer, 0, readbytes, buffer, 0);
            }
            localhashfunction.TransformFinalBlock(buffer, 0, 0);
            verifystream.Dispose();
            string stringhash = EWR.ServerBackup.Library.HexConverter.ByteArraytoHex(localhashfunction.Hash);

            if (Simulate)   //Simulate, do not write out to dest file
            {
                if (log != null)
                    log.Info(sourcefilename + "\t" + stringhash);
                return true;
            }

            //Write hash out to file
            lock (lockObject)
            {
                if (hashoutputfile != null)
                    hashoutputfile.WriteLine(sourcefilename + "\t" + stringhash);
                sb.AppendLine(sourcefilename + "\t" + stringhash);
            }

            return true;

        }


        private System.Security.Cryptography.HashAlgorithm CreateLocalHashFunction(System.Security.Cryptography.HashAlgorithmName source)
        {

            if (source == HashAlgorithmName.MD5)
                return MD5.Create();
            if (source == HashAlgorithmName.SHA1)
                return SHA1.Create();
            if (source == HashAlgorithmName.SHA256)
                return SHA256.Create();
            if (source == HashAlgorithmName.SHA384)
                return SHA384.Create();
            if (source == HashAlgorithmName.SHA512)
                return SHA512.Create();

            return MD5.Create();
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
                    if (hashoutputfile != null)
                        hashoutputfile.Dispose();
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                // set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~CommandHash() {
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
