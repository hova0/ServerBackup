using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace ServerBackup
{
    public class CommandCopy : ICommand
    {
        //public String Destination { get; set; }
        public bool Overwrite { get; set; }
        public bool Verify { get; set; }
        public string OriginalHashValue { get; set; }
        //public bool Recurse { get; set; }
        public string CommandResult { get; set; }
        //public string BaseDirectory { get; set; }
        public bool Simulate { get; set; }
        public IInternalLogger log;
        public bool ContinueOnError { get; set; }
        public System.Security.Cryptography.HashAlgorithmName HashingFunction { get; set; }
        public void Initialize() { }
        public void Close() { }

        //public String Source { get; set; }
        public CommandCopy(IInternalLogger _logger)
        {
            log = _logger;
        }


        public bool Run(string sourcefilename, string destinationfilename)
        {
            //System.IO.File.Copy(filename, System.IO.Path.Combine(this.Destination, filename));
            System.Threading.CancellationToken cts = new System.Threading.CancellationToken();
            Task<bool> copytask = RunAsync(sourcefilename, destinationfilename, cts);

            copytask.Wait(cts);
            bool returnresult = copytask.Result;
            copytask.Dispose();
            return returnresult;
        }

        public async Task<bool> RunAsync(String sourcefilename, string destinationfilename, System.Threading.CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[16384];
            bool copyaborted = false;
            HashAlgorithm localhashfunction = null;
            bool updateDestinationFileTimes = false;
            DateTime originalLastWriteTime = DateTime.Now;
            DateTime originalCreationTime = DateTime.Now;
            DateTime originalLastAccessTime = DateTime.Now;
            //Early exit
            if (cancellationToken.IsCancellationRequested)
                return false;

            if (Simulate)
            {
                log.Info(String.Format("Copy {0} => {1}", sourcefilename, destinationfilename));
                return true;
            }
            if (System.IO.File.Exists(destinationfilename) )
            {
                if (!Overwrite)
                {
                    log.Info(destinationfilename + " already exists.  Skipped.");
                    return true;
                }

                // Overwriting existing file, but need to preserve origin times
                System.IO.FileInfo originalfile = new System.IO.FileInfo(sourcefilename);
                originalLastWriteTime = originalfile.LastWriteTime;
                originalCreationTime = originalfile.CreationTime;
                originalLastAccessTime = originalfile.LastAccessTime;
                updateDestinationFileTimes = true;
            }
            //If destination directory does not exist, create it.
            if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(destinationfilename)))
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(destinationfilename));

            if (Verify && HashingFunction == null)
                throw new Exception("Verify was requested, but no hashing function specified");
            else if (Verify)
            {
                localhashfunction = CreateLocalHashFunction(HashingFunction);
                localhashfunction.Initialize();
            }
            


            using (System.IO.FileStream ws = new System.IO.FileStream(destinationfilename, Overwrite ? System.IO.FileMode.OpenOrCreate : System.IO.FileMode.CreateNew))
            {

                System.IO.FileStream rs = null; 
                EWR.ServerBackup.Library.IOHelper.DoRetryIO(() => rs = new System.IO.FileStream(sourcefilename, System.IO.FileMode.Open, System.IO.FileAccess.Read));
                using (rs)
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
                        int readbytes = await rs.ReadAsync(buffer, 0, 16384);
                        if (Verify && localhashfunction != null)
                        {
                            localhashfunction.TransformBlock(buffer, 0, readbytes, buffer, 0);    //This part seems a bit strange, the output buffer?
                        }
                        if (readbytes > 0)
                        {
                            await ws.WriteAsync(buffer, 0, readbytes);
                        }


                        if (rs.Position == rs.Length)
                        {
                            donecopying = true;
                            if (Verify && localhashfunction != null)
                                localhashfunction.TransformFinalBlock(buffer, 0, 0);
                        }
                    }

                    if (!copyaborted)
                    {
                        log.Debug(sourcefilename + " => " + destinationfilename);
                        rs.Dispose();
                        ws.Close();
                        ws.Dispose();

                        if (Verify && localhashfunction != null)
                        {
                            OriginalHashValue = EWR.ServerBackup.Library.HexConverter.ByteArraytoHex(localhashfunction.Hash);
                            localhashfunction.Dispose();
                            System.IO.FileStream verifystream = new System.IO.FileStream(destinationfilename, System.IO.FileMode.Open);

                            localhashfunction = CreateLocalHashFunction(HashingFunction);
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

                            string stringhash = EWR.ServerBackup.Library.HexConverter.ByteArraytoHex(localhashfunction.Hash);
                            if (stringhash != OriginalHashValue)
                            {
                                localhashfunction.Dispose();
                                verifystream.Dispose();
                                log.Error("Mismatch on verification", null);
                                log.Error(sourcefilename + " => " + OriginalHashValue, null);
                                log.Error(destinationfilename + " => " + stringhash, null);
                                throw new Exception("Failure on copy");
                            }
                            localhashfunction.Dispose();
                            verifystream.Dispose();
                            log.Debug(destinationfilename + " verified.  Hash=" + OriginalHashValue);
                        }
                    }
                    else
                    {
                        //Clean up aborted copy
                        rs.Dispose();
                        ws.Close();
                        ws.Dispose();
                        System.IO.File.Delete(destinationfilename);
                        CommandResult = null;
                        log.Fatal(sourcefilename + " aborted.", null);
                        return false;
                    }

                }

            }
            //Restore last write time to desetination.
            if (updateDestinationFileTimes)
            {
                System.IO.FileInfo destinationfile = new System.IO.FileInfo(destinationfilename);
                destinationfile.LastWriteTime = originalLastWriteTime;
                destinationfile.LastAccessTime = originalLastAccessTime;
                destinationfile.CreationTime = originalCreationTime;
            }
            CommandResult = destinationfilename;


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

        

    }
}
