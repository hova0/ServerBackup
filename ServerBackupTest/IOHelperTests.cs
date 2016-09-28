using Microsoft.VisualStudio.TestTools.UnitTesting;
using EWR.ServerBackup.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerBackupLibrary.Tests
{
    [TestClass()]
    public class IOHelperTests
    {
        [TestMethod()]
        public void DoRetryIOTest()
        {
            IOHelper.DoRetryIO(DoNothing);

        }
        [ExpectedException(typeof(Exception))]
        [TestMethod()]
        public void DoRetryIOExceptionTest()
        {
            IOHelper.DoRetryIO(ThrowException);
        }
        [ExpectedException(typeof(System.IO.IOException))]
        [TestMethod()]
        public void DoRetryIOIOExceptionTest()
        {
            IOHelper.DoRetryIO(ThrowIOException);

        }
        public void DoNothing()
        {

        }
        public void ThrowException()
        {
            throw new Exception("Regular Exception");
        }
        public void ThrowIOException()
        {
            throw new System.IO.IOException("IO Exception");
        }

    }


}