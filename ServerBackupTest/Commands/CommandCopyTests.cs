using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServerBackupTest
{
    


    [TestClass]
    public class CommandCopyTests
    {

        [TestMethod]
        public void CopyCommandTests()
        {
            SetUpFiles("CopyCommandTests");
            StubLogger sl = new StubLogger();
            ServerBackup.CommandCopy cc = new ServerBackup.CommandCopy(sl);
            cc.Run(@"C:\temp\CopyCommandTestsdir1\TestFile1.txt", @"C:\temp\CopyCommandTestsdir2\TestFile1.txt") ;
            Assert.IsTrue(System.IO.File.Exists(@"C:\temp\CopyCommandTestsdir2\TestFile1.txt"));

            CleanupFiles("CopyCommandTests");

            SetUpFiles("CopyCommandTests2");
            cc = new ServerBackup.CommandCopy( sl);
            cc.Verify = true;
            cc.HashingFunction = System.Security.Cryptography.HashAlgorithmName.MD5;
            cc.Run(@"C:\temp\CopyCommandTests2dir1\TestFile1.txt", @"C:\temp\CopyCommandTests2dir2\TestFile1.txt");

            Assert.IsTrue(System.IO.File.Exists(@"C:\temp\CopyCommandTests2dir2\TestFile1.txt"));
            Assert.IsTrue(cc.OriginalHashValue == "7497E539EEB905CB676EB986B24A98F3");
            Console.WriteLine(cc.OriginalHashValue);
            CleanupFiles("CopyCommandTests2");
        }

        [TestMethod]
        public void CopyCommandSimulateTests()
        {
            SetUpFiles("CopyCommandSimulateTests");

            ServerBackup.CommandCopy cc = new ServerBackup.CommandCopy(new StubLogger());
            cc.Simulate = true;
            cc.Run(@"C:\temp\CopyCommandSimulateTestsdir1\TestFile1.txt", @"C:\temp\CopyCommandSimulateTestsdir2\TestFile1.txt");
            Assert.IsTrue(!System.IO.File.Exists(@"C:\temp\CopyCommandSimulateTestsdir2\TestFile1.txt"));

            CleanupFiles("CopyCommandSimulateTests");
            SetUpFiles("CopyCommandSimulateTests2");
            cc = new ServerBackup.CommandCopy(new StubLogger());
            cc.Simulate = true;
            cc.Verify = true;
            cc.HashingFunction = System.Security.Cryptography.HashAlgorithmName.MD5;
            cc.Run(@"C:\temp\CopyCommandSimulateTests2dir1\TestFile1.txt", @"C:\temp\CopyCommandSimulateTests2dir2\TestFile1.txt");
            Assert.IsTrue(!System.IO.File.Exists(@"C:\temp\CopyCommandSimulateTests2dir2\TestFile1.txt"));
            //Assert.IsTrue(cc.HashValue == "7497E539EEB905CB676EB986B24A98F3");
            Console.WriteLine(cc.OriginalHashValue);
            CleanupFiles("CopyCommandSimulateTests2");
        }
        [TestMethod]
        public void CopyCommandTestOverwrite()
        {
            SetUpFiles("CopyCommandOverwrite");
            System.IO.File.WriteAllLines(@"C:\temp\CopyCommandOverwritedir2\TestFile1.txt", new string[] { "This file is supposed to be overwritten." });
            StubLogger sl = new StubLogger();
            ServerBackup.CommandCopy cc = new ServerBackup.CommandCopy(sl);
            cc.Overwrite = true;
            cc.Run(@"C:\temp\CopyCommandOverwritedir1\TestFile1.txt", @"C:\temp\CopyCommandOverwritedir2\TestFile1.txt");
            string[] alllines = System.IO.File.ReadAllLines(@"C:\temp\CopyCommandOverwritedir2\TestFile1.txt");
            Assert.IsTrue(alllines[0] != "This file is supposed to be overwritten.");
            

            CleanupFiles("CopyCommandOverwrite");

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
        }

        private void CleanupFiles(string prefix)
        {
            string dir1 = prefix + "dir1";
            string dir2 = prefix + "dir2";
            if (System.IO.File.Exists(@"C:\temp\" + dir1 + @"\TestFile1.txt"))
                System.IO.File.Delete(@"C:\temp\" + dir1 + @"\TestFile1.txt");
            if (System.IO.File.Exists(@"C:\temp\" + dir2 + @"\TestFile1.txt"))
                System.IO.File.Delete(@"C:\temp\" + dir2 + @"\TestFile1.txt");
            System.IO.Directory.Delete(@"C:\temp\" + dir1);
            System.IO.Directory.Delete(@"C:\temp\" + dir2);

        }

    }
}
