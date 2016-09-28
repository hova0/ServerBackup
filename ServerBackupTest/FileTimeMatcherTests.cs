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
    public class FileTimeMatcherTests
    {

        [TestMethod]
        public void TestDateMatcherFiles()
        {
            ServerBackup.FileTimeMatcher fm = new ServerBackup.FileTimeMatcher(new DateTime(2016, 04, 04, 6, 0, 0, DateTimeKind.Local), ServerBackup.FileTimeMatcher.TimeCompare.NewerThan);
            if (!System.IO.File.Exists(@"C:\temp\test.dat"))
            {
                System.IO.FileStream testfile = System.IO.File.Create(@"C:\temp\test.dat");
                testfile.Close();
            }
            //Target file ( 05/04 ) is newer than target date  (04/04)
            System.IO.File.SetLastWriteTime(@"C:\temp\test.dat", new DateTime(2016, 05, 04, 6, 0, 0, DateTimeKind.Local));
            Assert.IsTrue(fm.IsMatch(@"C:\temp\test.dat"));
            //Target file (03/04) is older than target date (04/04)
            System.IO.File.SetLastWriteTime(@"C:\temp\test.dat", new DateTime(2016, 03, 04, 6, 0, 0, DateTimeKind.Local));
            Assert.IsTrue(!fm.IsMatch(@"C:\temp\test.dat"));

            //Target file (03/04) is older than target date (04/04)
            System.IO.File.SetLastWriteTime(@"C:\temp\test.dat", new DateTime(2016, 03, 04, 6, 0, 0, DateTimeKind.Local));
            fm = new ServerBackup.FileTimeMatcher(new DateTime(2016, 04, 04, 6, 0, 0, DateTimeKind.Local), ServerBackup.FileTimeMatcher.TimeCompare.OlderThan);
            Assert.IsTrue(fm.IsMatch(@"C:\temp\test.dat"));
            // Target file (05/04) is older than target date (04/04)
            System.IO.File.SetLastWriteTime(@"C:\temp\test.dat", new DateTime(2016, 05, 04, 6, 0, 0, DateTimeKind.Local));
            Assert.IsTrue(!fm.IsMatch(@"C:\temp\test.dat"));

            //Test days old
            System.IO.File.SetLastWriteTime(@"C:\temp\test.dat", DateTime.Now.AddDays(-30));
            fm = new FileTimeMatcher(20);
            Assert.IsTrue(fm.IsMatch(@"C:\temp\test.dat"));     //Older than 20 days
            fm = new FileTimeMatcher(40);
            Assert.IsTrue(!fm.IsMatch(@"C:\temp\test.dat"));    //Not older than 40 days

            if (!System.IO.File.Exists(@"C:\temp\test.dat"))
            {
                System.IO.File.Delete(@"C:\temp\test.dat");
            }
        }

      
    }
}