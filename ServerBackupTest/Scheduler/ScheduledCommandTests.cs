using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServerBackup.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerBackup.Tests
{
    [TestClass()]
    public class ScheduledCommandTests
    {
        [TestMethod()]
        public void GetNextTime_Daily_Test()
        {
            //ServerBackup.Scheduler.ScheduledCommand sc = new ScheduledCommand();
            RecurringScheduleTime rst = new RecurringScheduleTime();
            rst.RecurringType = ScheduleTypes.Daily;
            rst.InitialScheduleTime = new DateTime(2016, 05, 05, 8, 30, 0);


            //sc.ScheduledCommandTime = rst;
            //Console.WriteLine(ScheduledCommand.GetNextTime(new DateTime(2016, 05, 05, 7, 00, 00), rst.InitialScheduleTime, rst.RecurringType ).ToString());

            Assert.IsTrue(Scheduler.RecurringScheduleTime.GetNextTime(new DateTime(2016, 05, 05, 7, 00, 00), rst.InitialScheduleTime, rst.RecurringType) == new DateTime(2016, 05, 05, 08, 30, 0));
            Assert.IsTrue(Scheduler.RecurringScheduleTime.GetNextTime(new DateTime(2016, 05, 05, 9, 00, 00), rst.InitialScheduleTime, rst.RecurringType) == new DateTime(2016, 05, 06, 08, 30, 0));
            rst.InitialScheduleTime = new DateTime(2016, 01, 01, 08, 30, 0);
            Assert.IsTrue(Scheduler.RecurringScheduleTime.GetNextTime(new DateTime(2016, 05, 05, 7, 00, 00), rst.InitialScheduleTime, rst.RecurringType) == new DateTime(2016, 05, 05, 08, 30, 0));
            Assert.IsTrue(Scheduler.RecurringScheduleTime.GetNextTime(new DateTime(2016, 05, 05, 9, 00, 00), rst.InitialScheduleTime, rst.RecurringType) == new DateTime(2016, 05, 06, 08, 30, 0));
            rst.InitialScheduleTime = new DateTime(2020, 12, 25, 08, 30, 0);
            Assert.IsTrue(Scheduler.RecurringScheduleTime.GetNextTime(new DateTime(2016, 05, 05, 7, 00, 00), rst.InitialScheduleTime, rst.RecurringType) == new DateTime(2016, 05, 05, 08, 30, 0));
            Assert.IsTrue(Scheduler.RecurringScheduleTime.GetNextTime(new DateTime(2016, 05, 05, 9, 00, 00), rst.InitialScheduleTime, rst.RecurringType) == new DateTime(2016, 05, 06, 08, 30, 0));


        }

        [TestMethod()]
        public void GetNextTime_Monthly_Test()
        {
            //ServerBackup.Scheduler.ScheduledCommand sc = new ScheduledCommand();
            RecurringScheduleTime rst = new RecurringScheduleTime();
            rst.RecurringType = ScheduleTypes.Monthly;
            rst.InitialScheduleTime = new DateTime(2016, 05, 05, 8, 30, 0);
            //sc.ScheduledCommandTime = rst;
            //Console.WriteLine(ScheduledCommand.GetNextTime(new DateTime(2016, 05, 05, 7, 00, 00), rst.InitialScheduleTime, rst.RecurringType ).ToString());
            Assert.IsTrue(Scheduler.RecurringScheduleTime.GetNextTime(new DateTime(2016, 05, 05, 7, 00, 00), rst.InitialScheduleTime, rst.RecurringType) == new DateTime(2016, 05, 05, 08, 30, 0));
            Assert.IsTrue(Scheduler.RecurringScheduleTime.GetNextTime(new DateTime(2016, 05, 05, 9, 00, 00), rst.InitialScheduleTime, rst.RecurringType) == new DateTime(2016, 06, 05, 08, 30, 0));
            rst.InitialScheduleTime = new DateTime(2016, 01, 01, 08, 30, 0);
            Assert.IsTrue(Scheduler.RecurringScheduleTime.GetNextTime(new DateTime(2016, 05, 05, 7, 00, 00), rst.InitialScheduleTime, rst.RecurringType) == new DateTime(2016, 06, 01, 08, 30, 0));
            Assert.IsTrue(Scheduler.RecurringScheduleTime.GetNextTime(new DateTime(2016, 05, 05, 9, 00, 00), rst.InitialScheduleTime, rst.RecurringType) == new DateTime(2016, 06, 01, 08, 30, 0));
            rst.InitialScheduleTime = new DateTime(2020, 12, 25, 08, 30, 0);
            Assert.IsTrue(Scheduler.RecurringScheduleTime.GetNextTime(new DateTime(2016, 05, 05, 7, 00, 00), rst.InitialScheduleTime, rst.RecurringType) == new DateTime(2016, 05, 25, 08, 30, 0));
            Assert.IsTrue(Scheduler.RecurringScheduleTime.GetNextTime(new DateTime(2016, 05, 05, 9, 00, 00), rst.InitialScheduleTime, rst.RecurringType) == new DateTime(2016, 05, 25, 08, 30, 0));

            //Assert.IsTrue()
        }

        [TestMethod()]
        public void GetNextTime_Weekly_Test()
        {
            //ServerBackup.Scheduler.ScheduledCommand sc = new ScheduledCommand();
            RecurringScheduleTime rst = new RecurringScheduleTime();
            rst.RecurringType = ScheduleTypes.Weekly;
            rst.InitialScheduleTime = new DateTime(2016, 05, 05, 8, 30, 0);
            //sc.ScheduledCommandTime = rst;
            //Console.WriteLine(ScheduledCommand.GetNextTime(new DateTime(2016, 05, 05, 7, 00, 00), rst.InitialScheduleTime, rst.RecurringType ).ToString());
            Assert.IsTrue(Scheduler.RecurringScheduleTime.GetNextTime(new DateTime(2016, 05, 05, 7, 00, 00), rst.InitialScheduleTime, rst.RecurringType) == new DateTime(2016, 05, 05, 08, 30, 0));
            Assert.IsTrue(Scheduler.RecurringScheduleTime.GetNextTime(new DateTime(2016, 05, 05, 9, 00, 00), rst.InitialScheduleTime, rst.RecurringType) == new DateTime(2016, 05, 12, 08, 30, 0));
            rst.InitialScheduleTime = new DateTime(2016, 01, 05, 08, 30, 0);  //Tuesday
            Assert.IsTrue(Scheduler.RecurringScheduleTime.GetNextTime(new DateTime(2016, 05, 05, 7, 00, 00), rst.InitialScheduleTime, rst.RecurringType) == new DateTime(2016, 05, 10, 08, 30, 0));
            Assert.IsTrue(Scheduler.RecurringScheduleTime.GetNextTime(new DateTime(2016, 05, 05, 9, 00, 00), rst.InitialScheduleTime, rst.RecurringType) == new DateTime(2016, 05, 10, 08, 30, 0));
            rst.InitialScheduleTime = new DateTime(2020, 12, 21, 08, 30, 0);  //Monday
            Assert.IsTrue(Scheduler.RecurringScheduleTime.GetNextTime(new DateTime(2016, 05, 05, 7, 00, 00), rst.InitialScheduleTime, rst.RecurringType) == new DateTime(2016, 05, 9, 08, 30, 0));
            Assert.IsTrue(Scheduler.RecurringScheduleTime.GetNextTime(new DateTime(2016, 05, 05, 9, 00, 00), rst.InitialScheduleTime, rst.RecurringType) == new DateTime(2016, 05, 9, 08, 30, 0));

            //Assert.IsTrue()
        }
    }
}