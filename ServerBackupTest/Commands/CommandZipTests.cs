using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServerBackup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ServerBackup.Tests
{
    [TestClass()]
    public class CommandZipTests
    {

        [TestMethod()]
        public void CommandZipRunTest()
        {

            ServerBackup.CommandZip ch = new CommandZip(new ServerBackupTest.StubLogger());
            SetUpFiles("CommandZip");
            ch.DestinationFileName = @"C:\temp\CommandZip\dir2\output.zip";
            ch.BaseDirectory = @"C:\temp\CommandZip\dir1\";
            ch.Initialize();
            ch.Run(@"C:\temp\CommandZip\dir1\TestFile1.txt", @"C:\temp\CommandZip\dir2\output.txt\TestFile1.txt");
            ch.Close();

            string[] filecontents = System.IO.File.ReadAllLines(@"C:\temp\CommandZip\dir2\output.zip");
            //Assert.IsTrue(filecontents[0] == "C:\\temp\\CommandHashdir1\\TestFile1.txt\t7497E539EEB905CB676EB986B24A98F3");

            CleanupFiles("CommandZip");
        }

        [TestMethod()]
        public void CommandZipSimulateRunTest()
        {

            ServerBackup.CommandZip ch = new CommandZip(new ServerBackupTest.StubLogger());
            SetUpFiles("CommandZipSimulate");
            ch.Simulate = true;
            ch.DestinationFileName = @"C:\temp\CommandZipSimulate\dir2\output.zip";
            ch.Initialize();
            ch.Run(@"C:\temp\CommandZipSimulate\dir1\TestFile1.txt", @"C:\temp\CommandZipSimulate\dir2\output.txt\TestFile1.txt");
            ch.Run(@"C:\temp\CommandZipSimulate\dir1\TestFile2.txt", @"C:\temp\CommandZipSimulate\dir2\output.txt\TestFile2.txt");
            ch.Run(@"C:\temp\CommandZipSimulate\dir1\TestFile3.txt", @"C:\temp\CommandZipSimulate\dir2\output.txt\TestFile3.txt");
            ch.Close();
            ch.Dispose();
            Assert.IsTrue(!System.IO.File.Exists(@"C:\temp\CommandZipSimulate\dir1\output.txt"));
            CleanupFiles("CommandZipSimulate");
        }


        [TestMethod()]
        public void CommandZipRunAsyncTest()
        {
            ServerBackup.CommandZip ch = new CommandZip(new ServerBackupTest.StubLogger());
            SetUpFiles("CommandZipRunAsyncTest");
            System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
            ch.DestinationFileName = @"C:\temp\CommandZipRunAsyncTest\dir2\output.zip";
            ch.BaseDirectory = @"C:\temp\CommandZipRunAsyncTest\";
            ch.Initialize();
            Task<bool> testtask = ch.RunAsync(@"C:\temp\CommandZipRunAsyncTest\dir1\TestFile1.txt", @"C:\temp\CommandZipRunAsyncTest\dir2\output.txt\TestFile1.txt", cts.Token);
            Task<bool> testtask2 = ch.RunAsync(@"C:\temp\CommandZipRunAsyncTest\dir1\TestFile2.txt", @"C:\temp\CommandZipRunAsyncTest\dir2\output.txt\TestFile2.txt", cts.Token);
            Task<bool> testtask3 = ch.RunAsync(@"C:\temp\CommandZipRunAsyncTest\dir1\TestFile3.txt", @"C:\temp\CommandZipRunAsyncTest\dir2\output.txt\TestFile3.txt", cts.Token);
            Task.WaitAll(testtask, testtask2, testtask3);

            testtask.Dispose();
            testtask2.Dispose();
            testtask3.Dispose();
            ch.Close();
            ch.Dispose();
            string[] filecontents = System.IO.File.ReadAllLines(@"C:\temp\CommandZipRunAsyncTest\dir2\output.zip");
            //Assert.IsTrue(filecontents[0] == "C:\\temp\\CommandhashAsyncdir1\\TestFile1.txt\t7497E539EEB905CB676EB986B24A98F3");
            CleanupFiles("CommandZipRunAsyncTest");

        }

      
     

        private void SetUpFiles(string prefix)
        {
            string dir1 = prefix + Path.DirectorySeparatorChar + "dir1";
            string dir2 = prefix + Path.DirectorySeparatorChar + "dir2";
            if (!System.IO.Directory.Exists(@"C:\temp\" + dir1))
                System.IO.Directory.CreateDirectory(@"C:\temp\" + dir1);
            if (!System.IO.Directory.Exists(@"C:\temp\" + dir2))
                System.IO.Directory.CreateDirectory(@"C:\temp\" + dir2);
            if (!System.IO.File.Exists(@"C:\temp\" + dir1 + @"\TestFile1.txt"))
                System.IO.File.WriteAllLines(@"C:\temp\" + dir1 + @"\TestFile1.txt", new string[] { "This is a test file", "This is a test file", "Test is a test file." });
            if (!System.IO.File.Exists(@"C:\temp\" + dir1 + @"\TestFile2.txt"))
                System.IO.File.WriteAllLines(@"C:\temp\" + dir1 + @"\TestFile2.txt", new string[] { "Thds is a tfdest file", "This xa test file", "Test ixa test file." });
            if (!System.IO.File.Exists(@"C:\temp\" + dir1 + @"\TestFile3.txt"))
                System.IO.File.WriteAllLines(@"C:\temp\" + dir1 + @"\TestFile3.txt", new string[] { "This is a test file", "Test is a test file." });
        }

        private void CleanupFiles(string prefix)
        {
            string dir1 = prefix + Path.DirectorySeparatorChar + "dir1";
            string dir2 = prefix + Path.DirectorySeparatorChar + "dir2";
            System.IO.Directory.Delete(@"C:\temp\" + dir1, true);
            System.IO.Directory.Delete(@"C:\temp\" + dir2, true);
            System.IO.Directory.Delete(@"C:\temp\" + prefix);
        }
    }
}

