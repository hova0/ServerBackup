using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServerBackupTest
{
    [TestClass]
    public class ServerBackupLibraryTest
    {
        [TestMethod]
        public void BytetoHexStringTest()
        {
            
            byte[] buffer = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,255 };

            Assert.IsTrue(EWR.ServerBackup.Library.HexConverter.ByteArraytoHex(buffer) == "0102030405060708090A0B0C0D0E0FFF");
        }
        [TestMethod]
        public void HexStringtoByteTest()
        {
            
            string buffer = "0102030405060708090A0B0C0D0E0FFF";
            byte[] result = EWR.ServerBackup.Library.HexConverter.HextoByteArray(buffer);
            byte[] validvalues = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 255 };
            for(int i = 0; i < result.Length; i++)
            {
                Assert.IsTrue(result[i] == validvalues[i]);
            }
        }
    }
}
