using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerBackup.Scheduler
{
    public enum ScheduleTypes
    {
        Daily,
        Weekly,
        Monthly,
        Yearly,
        Once
    }

    public class RecurringScheduleTime
    {
        public DateTime InitialScheduleTime { get; set; }
        public ScheduleTypes RecurringType {get; set; }
        private DateTime _nexttime;

        public DateTime NextScheduledTime
        {
            get
            {
                if (_nexttime == DateTime.MinValue)
                    _nexttime = GetNextTime(DateTime.Now, InitialScheduleTime, RecurringType);
                return _nexttime;
            }
        }

        public void AdvanceTime()
        {
            _nexttime = GetNextTime(DateTime.Now, InitialScheduleTime, RecurringType);
        }

        public static DateTime GetNextTime(DateTime currenttime, DateTime ScheduleTime, ScheduleTypes rst)
        {

            //DateTime ScheduleTime = rst.ScheduleDateTime;

            //NOTE:  The current time is passed in as a parameter, so that tests can be run in a variety of scenarios
            switch (rst)
            {
                case ScheduleTypes.Daily:
                    if (MinutesDifference(currenttime, ScheduleTime) <= 0)
                    {
                        //Add a day so it's tomorrow
                        // Then add negative minutes so time rewinds to the correct time
                        return currenttime.AddDays(1).AddMinutes(MinutesDifference(currenttime, ScheduleTime));
                    }
                    else
                        // Just add minutes until we're at the correct time from NOW.
                        return currenttime.AddMinutes(MinutesDifference(currenttime, ScheduleTime));
                case ScheduleTypes.Weekly:
                    int daysdiff = DaysOfWeekDifference(currenttime, ScheduleTime);
                    if (currenttime.DayOfWeek > ScheduleTime.DayOfWeek || (currenttime.DayOfWeek == ScheduleTime.DayOfWeek && MinutesDifference(currenttime, ScheduleTime) <= 0))
                    {
                        //Advance by one week
                        return currenttime.AddDays(7 + DaysOfWeekDifference(currenttime, ScheduleTime)).AddMinutes(MinutesDifference(currenttime, ScheduleTime));
                    }
                    else
                    {
                        return currenttime.AddDays(DaysOfWeekDifference(currenttime, ScheduleTime)).AddMinutes(MinutesDifference(currenttime, ScheduleTime));
                    }
                case ScheduleTypes.Monthly:
                    if (currenttime.Day > ScheduleTime.Day || (currenttime.Day == ScheduleTime.Day && MinutesDifference(currenttime, ScheduleTime) <= 0))
                    {
                        //Advance by one month
                        return currenttime.AddMonths(1).AddDays(ScheduleTime.Day - currenttime.Day).AddMinutes(MinutesDifference(currenttime, ScheduleTime));
                    }
                    else
                    {
                        return currenttime.AddDays(ScheduleTime.Day - currenttime.Day).AddMinutes(MinutesDifference(currenttime, ScheduleTime));
                    }
                default: return DateTime.Now;
            }
        }


        public static int MinutesDifference(DateTime currenttime, DateTime targettime)
        {
            return (targettime.Hour * 60 + targettime.Minute) - (currenttime.Hour * 60 + currenttime.Minute);
        }
        public static int DaysOfWeekDifference(DateTime currenttime, DateTime targettime)
        {
            return (int)targettime.DayOfWeek - (int)currenttime.DayOfWeek;
        }
    }

    public class ScheduledCommand
    {
        public Configuration.ConfiguredCommand CommandtoRun { get; set; }
        public RecurringScheduleTime ScheduledCommandTime { get; set; }

    }
}
