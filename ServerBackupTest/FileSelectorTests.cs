using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServerBackupTest
{
    //[TestClass]
    //public class UnitTest1
    //{
    //    [TestMethod]
    //    public void TestMethod1()
    //    {
    //    }
    //}


    [TestClass]
    public class FileSelectorTests
    {
        [TestMethod]
        public void BasicFileSelectorTest()
        {
            try
            {
                System.IO.File.Delete(@"C:\temp\testFileSelect.txt");
            }
            catch (Exception) { }
            System.IO.File.CreateText(@"C:\temp\testFileSelect.txt").Close();

            //Test include mask.   Should only contain "testFileSelect.txt" as output.
            ServerBackup.FileSelector fs = new ServerBackup.FileSelector(@"C:\temp");
            fs.IncludeMatchers.Add(new ServerBackup.FileMaskMatcher("FileSelect"));
            //fs.IncludeMask.Add("FileSelect");
            foreach(System.IO.FileInfo f in fs.FileList())
            {
                Assert.IsTrue("testFileSelect.txt" == f.Name);
            }

            //Test exclude mask.  Should NOT contain "testFileSelect.txt" as output
            fs = new ServerBackup.FileSelector(@"C:\temp");
            fs.ExcludeMatchers.Add(new ServerBackup.FileMaskMatcher(@"FileSelect"));
            foreach (System.IO.FileInfo f in fs.FileList())
            {
                Assert.IsTrue("testFileSelect.txt" != f.Name);
            }


        }
        [TestMethod]
        // [ExpectedException(typeof(Exception))]  This was removed, because sometimes a network directory might not exist
        // or USB drive not plugged in.   So when scheduled, the directory might be created during the time it runs.
        public void ConstructorFileSelector_MissingDirectoryTest()
        {
            ServerBackup.FileSelector fs = new ServerBackup.FileSelector(@"C:\sdvmnknlkj4newldnlksndlfk\");
        }
    }

    [TestClass]
    public class FileMatcherTests
    {
        [TestMethod]
        public void TestCommonFiles()
        {



            ServerBackup.FileMaskMatcher fm = new ServerBackup.FileMaskMatcher(".*test.*");
            Assert.IsTrue(fm.IsMatch("test.txt"));
            Assert.IsTrue(!fm.IsMatch("notmatch.txt"));
            Assert.IsTrue(fm.IsMatch("sdkjeknrewtEstfdsdl.txt"));
            
            
        }

       

    }
}
