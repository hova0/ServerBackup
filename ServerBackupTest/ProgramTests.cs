using ServerBackup;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;


namespace ServerBackup.Tests
{
    [TestClass]
    public class ProgramTests
    {
        [TestMethod]
        public void TestParsingCommandLineArguments()
        {
            string[] args = { "copy", "-verify", "-olderthan", "90" };
            System.Collections.Specialized.NameValueCollection parsedargs = ServerBackup.Program.ParseArguments(args);
            Assert.IsTrue(parsedargs.Count == 3);
            Assert.IsTrue(parsedargs["olderthan"] == "90");
            Assert.IsTrue(parsedargs.AllKeys.Contains("copy"));
            Assert.IsTrue(parsedargs.AllKeys.Contains("verify"));

            args = new string[] { "copy", @"C:\temp\", @"C:\temp2\", "-verify", "-include", ".*test.*" };
            parsedargs = ServerBackup.Program.ParseArguments(args);
            Assert.IsTrue(parsedargs.AllKeys.Contains("copy"));
            Assert.IsTrue(parsedargs.AllKeys.Contains("verify"));
            Assert.IsTrue(parsedargs.AllKeys[1] == @"C:\temp\");
            Assert.IsTrue(parsedargs.Keys[2] == @"C:\temp2\");
            Assert.IsTrue(parsedargs.AllKeys[4] == "include" && parsedargs[4] == ".*test.*");



        }

        [TestMethod]
        public void TestNormalizeKey()
        {
            
            Assert.IsTrue(ServerBackup.Program.NormalizeKey("--test") == "test");
            Assert.IsTrue(ServerBackup.Program.NormalizeKey("-test") == "test");
            Assert.IsTrue(ServerBackup.Program.NormalizeKey("/test") == "test");
            Assert.IsTrue(ServerBackup.Program.NormalizeKey("test") == "test");
            
        }

        [TestMethod()]
        public void GetSourceTest()
        {
            if (!System.IO.Directory.Exists(@"C:\temp\dir1\"))
                System.IO.Directory.CreateDirectory(@"C:\temp\dir1\");
            System.Collections.Specialized.NameValueCollection cmdargs = new System.Collections.Specialized.NameValueCollection();
            cmdargs.Add("copy", null);
            cmdargs.Add(@"C:\temp\dir1\", null);
            cmdargs.Add(@"C:\temp\dir2\", null);
            cmdargs.Add("-verify", null);

            Assert.IsTrue(ServerBackup.Program.GetSource(cmdargs) == @"C:\temp\dir1\");
            if (System.IO.Directory.Exists(@"C:\temp\dir1\"))
                System.IO.Directory.Delete(@"C:\temp\dir1\");
        }

        [TestMethod()]
        public void GetDestTest()
        {
            System.Collections.Specialized.NameValueCollection cmdargs = new System.Collections.Specialized.NameValueCollection();
            cmdargs.Add("copy", null);
            cmdargs.Add(@"C:\temp\dir1\", null);
            cmdargs.Add(@"C:\temp\dir2\", null);
            cmdargs.Add("verify", null);

            Assert.IsTrue(ServerBackup.Program.GetDest(cmdargs) == @"C:\temp\dir2\");
        }

    }
}
