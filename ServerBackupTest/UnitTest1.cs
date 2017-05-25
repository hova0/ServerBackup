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
            

        }
    }
}
