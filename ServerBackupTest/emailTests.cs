using Microsoft.VisualStudio.TestTools.UnitTesting;
using EWR.ServerBackup.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerBackup.Tests
{
    [TestClass()]
    public class emailTests
    {
        [TestMethod()]
        public void EmailSendTest()
        {
            Assert.IsTrue( email.Send("Test@example.com", "someone@example.com", "test Subject", "test Body"));

        }

        [TestMethod()]
        public void EmailSendAsyncTest()
        {
            email.SendAsync("Test@example.com", "someone@example.com", "test Subject", "test Body");
        }

        [TestMethod()]
        public void MultipleEmailSendTest()
        {
            Assert.IsTrue(email.Send("Test@example.com;Test2@example.com", "someone@example.com", "test Subject", "test Body"));
            Assert.IsTrue(email.Send("Test@example.com,Test2@example.com", "someone@example.com", "test Subject", "test Body"));
        }


        [TestMethod()]
        public void EmailIsEmailConfiguredTest()
        {
            Console.WriteLine(EWR.ServerBackup.Library.email.IsEmailConfigured());
        }
    }
}