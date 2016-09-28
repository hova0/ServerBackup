using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerBackup
{
    public class FileTimeMatcher : IFileMatcher
    {

        private DateTime CompareTime { get; set; }
        private int daysold;
        private FileTimeMatcherType DateMatchType;
        public enum TimeCompare
        {
            NewerThan,
            OlderThan
        }
        public enum FileTimeMatcherType
        {
            Fixed,
            Relative
        }
        private TimeCompare TimeComparison { get; set; }

        public FileTimeMatcher(DateTime FileTime, TimeCompare d)
        {
            CompareTime = FileTime;
            TimeComparison = d;
            DateMatchType = FileTimeMatcherType.Fixed;
        }
        public FileTimeMatcher(int days)
        {
            //Days are implicitly negative.  That is, it doesn't make sense to make comparisons on future times, because no files will have those dates
            CompareTime = DateTime.Now.AddDays(days * -1);
            this.daysold = days;
            TimeComparison = TimeCompare.OlderThan;
            DateMatchType = FileTimeMatcherType.Relative;
        }

        public FileTimeMatcher(int days, TimeCompare tc )
        {
            //Days are implicitly negative.  That is, it doesn't make sense to make comparisons on future times, because no files will have those dates
            CompareTime = DateTime.Now.AddDays(days * -1);
            this.daysold = days;
            TimeComparison = tc;
            DateMatchType = FileTimeMatcherType.Relative;
        }

        public Boolean IsMatch(String filename)
        {
            System.IO.FileInfo file = new System.IO.FileInfo(filename);
            return IsMatch(file);
        }
        public Boolean IsMatch(System.IO.FileInfo filename)
        {
            //Update day if this object is kept alive over several days.   This will occur when running as a service.
            if (DateMatchType == FileTimeMatcherType.Relative)
            {
                this.CompareTime = DateTime.Now.AddDays(this.daysold * -1);
            }

            if (CompareTime != DateTime.MinValue)
                if (TimeComparison == TimeCompare.NewerThan && filename.LastWriteTime > CompareTime)
                    return true;
                else if (TimeComparison == TimeCompare.OlderThan && filename.LastWriteTime < CompareTime)
                    return true;

            return false;
        }
        public override String ToString()
        {
            string returnstring = "{ FileTimeMatcher { ";
            returnstring += " TimeComparison : " + TimeComparison.ToString() + ", ";
            returnstring += " DateMatchType : " + DateMatchType.ToString() + ", ";
            returnstring += " DaysOld : " + daysold.ToString() + ", ";
            returnstring += " Time : " + CompareTime.ToString() + " } } ";


            return returnstring;
        }

    }
}
