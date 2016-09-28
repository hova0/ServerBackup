using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ServerBackup
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }
        Scheduler.Scheduler mainscheduler;
        IInternalLogger _Logger;
        System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
        protected override void OnStart(string[] args)
        {
            //Load configuration from XML file
            _Logger = new ServerBackup.Loggers.log4netlogger();     //Always direct output to console 
            _Logger.Info("ServerBackup service startup");
            
            Configuration.ConfigurationUtil cu = new Configuration.ConfigurationUtil(_Logger);
            mainscheduler = new Scheduler.Scheduler(_Logger);

            foreach (Configuration.ConfiguredCommand cc in cu.ConfiguredCommands)
            {
                //BLAH mainscheduler.CommandsPending.Add(new Tuple<Scheduler.ScheduledCommand, DateTime>(, Scheduler.Scheduler.GetNextTime(DateTime.Now, new Scheduler.RecurringScheduleTime() { RecurringType = cc.RecurringSchedule, ScheduleDateTime = cc.ScheduleTime }  )));
                mainscheduler.AddScheduledCommand(cc);
                _Logger.Info(String.Format("Loaded Command {0}", cc.Identifier));
            }
            mainscheduler.ScheduleLoop(cts.Token);

        }

        protected override void OnStop()
        {
            _Logger.Info("Halting ServerBackup");
            cts.Cancel();

        }
    }
}
