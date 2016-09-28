using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServerBackup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerBackup.Tests
{
    [TestClass()]
    public class CommandHashTests
    {
       

        [TestMethod()]
        public void CommandHashRunTest()
        {

            ServerBackup.CommandHash ch = new CommandHash(new ServerBackupTest.StubLogger());
            SetUpFiles("CommandHash");
            ch.DestinationFileName = @"C:\temp\CommandHashdir2\output.txt";
            ch.Initialize();
            ch.Run(@"C:\temp\CommandHashdir1\TestFile1.txt", @"C:\temp\CommandHashdir2\output.txt\TestFile1.txt");
            ch.Close();

            string[] filecontents = System.IO.File.ReadAllLines(@"C:\temp\CommandHashdir2\output.txt");
            Assert.IsTrue(filecontents[0] == "C:\\temp\\CommandHashdir1\\TestFile1.txt\t7497E539EEB905CB676EB986B24A98F3");
            CleanupFiles("CommandHash");
        }

        [TestMethod()]
        public void CommandHashSimulateRunTest()
        {

            ServerBackup.CommandHash ch = new CommandHash(new ServerBackupTest.StubLogger());
            SetUpFiles("CommandHashSimulate");
            ch.Simulate = true;
            ch.DestinationFileName = @"C:\temp\CommandHashSimulatedir2\output.txt";
            ch.Initialize();
            ch.Run(@"C:\temp\CommandHashSimulatedir1\TestFile1.txt", @"C:\temp\CommandHashSimulatedir2\output.txt\TestFile1.txt");
            ch.Run(@"C:\temp\CommandHashSimulatedir1\TestFile2.txt", @"C:\temp\CommandHashSimulatedir2\output.txt\TestFile2.txt");
            ch.Run(@"C:\temp\CommandHashSimulatedir1\TestFile3.txt", @"C:\temp\CommandHashSimulatedir2\output.txt\TestFile3.txt");
            ch.Close();
            ch.Dispose();
            Assert.IsTrue(!System.IO.File.Exists(@"C:\temp\CommandHashSimulatedir1\output.txt"));
            CleanupFiles("CommandHashSimulate");
        }


        [TestMethod()]
        public void CommandHashRunAsyncTest()
        {
            ServerBackup.CommandHash ch = new CommandHash(new ServerBackupTest.StubLogger());
            SetUpFiles("CommandHashAsync");
            System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
            ch.DestinationFileName = @"C:\temp\CommandhashAsyncdir2\output.txt";
            ch.Initialize();
            Task<bool> testtask = ch.RunAsync(@"C:\temp\CommandhashAsyncdir1\TestFile1.txt", @"C:\temp\CommandhashAsyncdir2\output.txt\TestFile1.txt", cts.Token);
            Task<bool> testtask2 = ch.RunAsync(@"C:\temp\CommandhashAsyncdir1\TestFile2.txt", @"C:\temp\CommandhashAsyncdir2\output.txt\TestFile2.txt", cts.Token);
            Task<bool> testtask3 = ch.RunAsync(@"C:\temp\CommandhashAsyncdir1\TestFile3.txt", @"C:\temp\CommandhashAsyncdir2\output.txt\TestFile3.txt", cts.Token);
            Task.WaitAll(testtask, testtask2, testtask3);
            testtask.Dispose();
            ch.Close();
            ch.Dispose();
            string[] filecontents = System.IO.File.ReadAllLines(@"C:\temp\CommandhashAsyncdir2\output.txt");
            Assert.IsTrue(filecontents.Any( x => x == "C:\\temp\\CommandhashAsyncdir1\\TestFile1.txt\t7497E539EEB905CB676EB986B24A98F3"));
           CleanupFiles("CommandHashAsync");

        }

        [TestMethod()]
        public void CommandHashComplicatedRunTest()
        {

            ServerBackup.CommandHash ch = new CommandHash(new ServerBackupTest.StubLogger());
            SetUpFiles("CommandHashComplicatedRunTest");
            ch.DestinationFileName = @"C:\temp\CommandHashComplicatedRunTestdir1\output.txt";
            ch.Initialize();
            ch.Run(@"C:\temp\CommandHashComplicatedRunTestdir1\TestFile1.txt", @"C:\temp\CommandHashComplicatedRunTestdir1\output.txt\TestFile1.txt");
            ch.Run(@"C:\temp\CommandHashComplicatedRunTestdir1\TestFile2.txt", @"C:\temp\CommandHashComplicatedRunTestdir1\output.txt\TestFile2.txt");
            ch.Run(@"C:\temp\CommandHashComplicatedRunTestdir1\TestFile3.txt", @"C:\temp\CommandHashComplicatedRunTestdir1\output.txt\TestFile3.txt");
            ch.Close();
            string[] filecontents = System.IO.File.ReadAllLines(@"C:\temp\CommandHashComplicatedRunTestdir1\output.txt");
            
            Assert.IsTrue(filecontents[0] == "C:\\temp\\CommandHashComplicatedRunTestdir1\\TestFile1.txt\t7497E539EEB905CB676EB986B24A98F3");
            Assert.IsTrue(filecontents[1] == "C:\\temp\\CommandHashComplicatedRunTestdir1\\TestFile2.txt\t3BA8BAAF7D714A5CBC338A404C32E6DB");
            Assert.IsTrue(filecontents[2] == "C:\\temp\\CommandHashComplicatedRunTestdir1\\TestFile3.txt\t133A43AE17BBBB64AAECC2DDFD40956C");
            CleanupFiles("CommandHashComplicatedRunTest");
        }


        private void SetUpFiles(string prefix)
        {
            string dir1 = prefix + "dir1";
            string dir2 = prefix + "dir2";
            if (!System.IO.Directory.Exists(@"C:\temp\" + dir1))
                System.IO.Directory.CreateDirectory(@"C:\temp\" + dir1);
            if (!System.IO.Directory.Exists(@"C:\temp\" + dir2))
                System.IO.Directory.CreateDirectory(@"C:\temp\" + dir2);
            if (!System.IO.File.Exists(@"C:\temp\" + dir1 + @"\TestFile1.txt"))
                System.IO.File.WriteAllLines(@"C:\temp\" + dir1 + @"\TestFile1.txt", new string[] { "This is a test file", "This is a test file", "Test is a test file." });
            if (!System.IO.File.Exists(@"C:\temp\" + dir1 + @"\TestFile2.txt"))
                System.IO.File.WriteAllLines(@"C:\temp\" + dir1 + @"\TestFile2.txt", new string[] { "Thds is a tfdest file", "This xa test file", "Test ixa test file." });
            if (!System.IO.File.Exists(@"C:\temp\" + dir1 + @"\TestFile3.txt"))
                System.IO.File.WriteAllLines(@"C:\temp\" + dir1 + @"\TestFile3.txt", new string[] { "This is a test file",  "Test is a test file." });
        }

        private void CleanupFiles(string prefix)
        {
            string dir1 = prefix + "dir1";
            string dir2 = prefix + "dir2";
            if (System.IO.File.Exists(@"C:\temp\" + dir1 + @"\TestFile1.txt"))
                System.IO.File.Delete(@"C:\temp\" + dir1 + @"\TestFile1.txt");
            if (System.IO.File.Exists(@"C:\temp\" + dir2 + @"\TestFile1.txt"))
                System.IO.File.Delete(@"C:\temp\" + dir2 + @"\TestFile1.txt");
            System.IO.Directory.Delete(@"C:\temp\" + dir1, true);
            System.IO.Directory.Delete(@"C:\temp\" + dir2, true);

        }

    }
}