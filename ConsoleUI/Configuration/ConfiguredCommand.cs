using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerBackup.Configuration
{
    public class ConfiguredCommand
    {
        //public DateTime ScheduleTime;
        public ICommand Command { get; set; }
        public FileSelector fs { get; set; }
        public string SourceDirectory { get; set; }
        public string DestinationDirectory { get; set; }
        public string hashalgorithm { get; set; }
        public bool verifyflag { get; set; }
        public bool EnsureFreeSpace { get; set; }
        public string Identifier { get; set; }
        public bool EmailonCompletion { get; set; } 
        //public Scheduler.ScheduleTypes RecurringSchedule;
        public Scheduler.RecurringScheduleTime Schedule { get; set; }

        public ConfiguredCommand()
        {
            EmailonCompletion = false;
        }

        public override String ToString()
        {
            string objectinfo = "";
            objectinfo = String.Format("{{ Identifier: {0}, Command : {1}, SourceDirectory: {2}, DestinationDirectory: {3} }} ", Identifier, Command, SourceDirectory, DestinationDirectory);
            return objectinfo;
        }

    }
}
