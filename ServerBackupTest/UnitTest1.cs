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

            Assert.IsTrue(z[0].Item1 == "commanditem");
            Assert.IsTrue(z[1].Item1 == "-switch");
            Assert.IsTrue(z[1].Item2 == "switchvalue");


        }
    }
}
