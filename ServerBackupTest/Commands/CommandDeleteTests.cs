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
    public class CommandDeleteTests
    {
        [TestMethod()]
        public void RunTest()
        {
            SetupTestDirectory("CommandDeleteRunTests");
            ServerBackupTest.StubLogger sl = new ServerBackupTest.StubLogger();
            ServerBackup.CommandDelete cd = new CommandDelete(sl);
            cd.Run(@"C:\temp\CommandDeleteRunTestsdir1\test0.txt", null);
            Assert.IsTrue(System.IO.Directory.GetFiles(@"C:\temp\CommandDeleteRunTestsdir1\").Count() == 9);
            CleanupTestDirectory("CommandDeleteRunTests");
            
        }

        [TestMethod()]
        public void RunSimulateTest()
        {
            SetupTestDirectory("CommandDeleteRunSimulateTests");
            ServerBackup.CommandDelete cd = new CommandDelete(new ServerBackupTest.StubLogger());
            cd.Simulate = true;
            cd.Run(@"C:\temp\CommandDeleteRunSimulateTestsdir1\test0.txt", null);
            Assert.IsTrue(System.IO.Directory.GetFiles(@"C:\temp\CommandDeleteRunSimulateTestsdir1\").Count() == 10);
            CleanupTestDirectory("CommandDeleteRunSimulateTests");
        }

        [TestMethod()]
        public void RunAsyncTest()
        {
            SetupTestDirectory("CommandDeleteRunAsyncTest");
            ServerBackup.CommandDelete cd = new CommandDelete(new ServerBackupTest.StubLogger());
            System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
            Task<bool> runtask = cd.RunAsync(@"C:\temp\CommandDeleteRunAsyncTestdir1\test0.txt", null, cts.Token);
            Assert.IsTrue(System.IO.Directory.GetFiles(@"C:\temp\CommandDeleteRunAsyncTestdir1\").Count() == 10);
            runtask.Start();
            runtask.Wait();
            Assert.IsTrue(System.IO.Directory.GetFiles(@"C:\temp\CommandDeleteRunAsyncTestdir1\").Count() == 9);
            runtask.Dispose();
            CleanupTestDirectory("CommandDeleteRunAsyncTest");
        }

        //[TestMethod()]
        //public void GetDestinationFileNameTest()
        //{
        //    ServerBackup.CommandDelete cd = new CommandDelete(new ServerBackupTest.StubLogger(););
        //    //Not supported on delete, so should return NULL
        //    Assert.IsTrue(cd.GetDestinationFileName(@"C:\temp\CommandDeleteRunAsyncTestdir1\test0.txt", @"C:\temp\CommandDeleteRunAsyncTestdir1\", @"C:\temp\CommandDeleteRunAsyncTestdir2\test0.txt", true) == null);
        //}

        private void SetupTestDirectory(string prefix)
        {
            if (!System.IO.Directory.Exists(@"C:\temp\" + prefix + @"dir1\"))
                System.IO.Directory.CreateDirectory(@"C:\temp\" + prefix + @"dir1");
            if (!System.IO.Directory.Exists(@"C:\temp\" + prefix + @"dir2\"))
                System.IO.Directory.CreateDirectory(@"C:\temp\" + prefix + @"dir2");
            for (int i = 0; i < 10; i++)
            {
                System.IO.File.WriteAllLines(@"C:\temp\" + prefix + @"dir1\test" + i.ToString() + ".txt", new string[] { "File " + i.ToString(), "." });
            }
        }

        private void CleanupTestDirectory(string prefix)
        {
            System.IO.Directory.Delete(@"C:\temp\" + prefix + @"dir1\", true);
            System.IO.Directory.Delete(@"C:\temp\" + prefix + @"dir2\", true);
        }

    }
}