using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServerBackupTest
{
    [TestClass]
    public class ArgumentsTest
    {
        [TestMethod]
        public void ParseArgumentsGeneric()
        {
            string[] args = new string[] { "commanditem", "-switch", "switchvalue" };
            var z = ServerBackup.Program.ParseArguments(args);

            Assert.IsTrue(z.Keys[0] == "commanditem");
            Assert.IsTrue(z.AllKeys[1] == "-switch");
            Assert.IsTrue(z[1] == "switchvalue");


        }
    }
}
