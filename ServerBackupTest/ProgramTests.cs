using ServerBackup;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;



namespace ServerBackup.Tests
{
    [TestClass]
    public class ProgramTests
    {
        [TestMethod]
        public void TestParsingCommandLineArguments()
        {
            string[] args = { "copy", "-verify", "-olderthan", "90" };
            List<Tuple<string, string>> parsedargs = ServerBackup.Program.ParseArguments(args);
            Assert.IsTrue(parsedargs.Count == 3);
            Assert.IsTrue(parsedargs.Find(x => x.Item1 == "-olderthan").Item2 == "90");
            Assert.IsTrue(parsedargs.Exists(x => x.Item1 == "copy"));
            Assert.IsTrue(parsedargs.Exists(x => x.Item1 == "-verify"));

            args = new string[] { "copy", @"C:\temp\", @"C:\temp2\", "-verify", "-include", ".*test.*" };
            parsedargs = ServerBackup.Program.ParseArguments(args);
            Assert.IsTrue(parsedargs.Exists(x => x.Item1 == "copy"));
            Assert.IsTrue(parsedargs.Exists(x => x.Item1 == "-verify"));
            Assert.IsTrue(parsedargs[1].Item1 == @"C:\temp\");
            Assert.IsTrue(parsedargs[2].Item1 == @"C:\temp2\");
            Assert.IsTrue(parsedargs[4].Item1 == "-include" && parsedargs[4].Item2 == ".*test.*");



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
            List<Tuple<string, string>> cmdargs = new List<Tuple<string, string>>();
            cmdargs.Add(new Tuple<string, string>("copy", null));
            cmdargs.Add(new Tuple<string, string>(@"C:\temp\dir1\", null));
            cmdargs.Add(new Tuple<string, string>(@"C:\temp\dir2\", null));
            cmdargs.Add(new Tuple<string, string>("-verify", null));

            Assert.IsTrue(ServerBackup.Program.GetSource(cmdargs) == @"C:\temp\dir1\");
            if (System.IO.Directory.Exists(@"C:\temp\dir1\"))
                System.IO.Directory.Delete(@"C:\temp\dir1\");
        }

        [TestMethod()]
        public void GetDestTest()
        {
            List<Tuple<string, string>> cmdargs = new List<Tuple<string, string>>();
            cmdargs.Add(new Tuple<string, string>("copy", null));
            cmdargs.Add(new Tuple<string, string>(@"C:\temp\dir1\", null));
            cmdargs.Add(new Tuple<string, string>(@"C:\temp\dir2\", null));
            cmdargs.Add(new Tuple<string, string>("-verify", null));

            Assert.IsTrue(ServerBackup.Program.GetDest(cmdargs) == @"C:\temp\dir2\");
        }

    }
}
