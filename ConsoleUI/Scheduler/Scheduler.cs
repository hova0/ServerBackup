using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerBackup.Scheduler
{



    public class Scheduler : IDisposable
    {

        public string AlertEmail { get; set; }

        public List<Configuration.ConfiguredCommand> CommandsPending = new List<Configuration.ConfiguredCommand>();
        System.Threading.Thread schedulerthread;
        System.Threading.CancellationToken cancellationtoken;
        IInternalLogger _logger;

        public Scheduler(IInternalLogger logger)
        {
            this._logger = logger;
            this.AlertEmail = ServerBackup.Program.DefaultAlertEmail;
        }

        public void ScheduleLoop(System.Threading.CancellationToken ct)
        {
            System.Threading.ThreadStart internalloop = new System.Threading.ThreadStart(InternalScheduleLoop);
            schedulerthread = new System.Threading.Thread(internalloop);
            cancellationtoken = ct;
            schedulerthread.Start();
        }

        public void AddScheduledCommand(ServerBackup.Configuration.ConfiguredCommand cc)
        {
            CommandsPending.Add(cc);
        }

        void InternalScheduleLoop()
        {
            while (true)
            {
                //Check every 10 seconds
                //but check every second for cancellation
                for (int spin = 0; spin < 10; spin++)
                {
                    System.Threading.Thread.Sleep(1000);
                    if (cancellationtoken.IsCancellationRequested)
                    {
                        return;
                    }
                }

                for (int i = 0; i < CommandsPending.Count; i++)
                {
                    if (CommandsPending[i].Schedule.NextScheduledTime < DateTime.Now)
                    {
                        Configuration.ConfiguredCommand cc = CommandsPending[i];
                        CommandRunner cr = new ServerBackup.CommandRunner(this._logger);

                        if (cc.EnsureFreeSpace)
                        {
                            try
                            {
                                //Run space calculation before main command
                                FileSelector fs2 = null;
                                EWR.ServerBackup.Library.IOHelper.DoRetryIO(() => fs2 = cc.fs.DeepClone());
                                if (fs2 == null)
                                {
                                    // Couldn't reach source directory
                                    if (EWR.ServerBackup.Library.email.IsEmailConfigured())
                                        EWR.ServerBackup.Library.email.Send(this.AlertEmail, ServerBackup.Program.ServerBackupFromEmail, "Could not reach source directory", String.Format("Could not reach source directory: {0}", cc.SourceDirectory));
                                    cc.Schedule.AdvanceTime();
                                    continue;
                                }

                                CommandEnsureFreeSpace cmdfreespace = new CommandEnsureFreeSpace();
                                CommandRunner cr2 = new CommandRunner(this._logger);
                                cr2.BaseDirectory = cc.SourceDirectory;
                                cr2.DestinationDirectory = cc.DestinationDirectory;
                                cmdfreespace.Initialize();
                                cr2.RunCommands(cmdfreespace, fs2, cancellationtoken);
                                cmdfreespace.Close();
                                if (cmdfreespace.CommandResult == "False")
                                {

                                    this._logger.Error(String.Format("Not enough free space for command {0}", cc.ToString()), null);
                                    if (EWR.ServerBackup.Library.email.IsEmailConfigured())
                                        EWR.ServerBackup.Library.email.Send(this.AlertEmail, ServerBackup.Program.ServerBackupFromEmail, String.Format("Not enough free space to run {0}", cc.Identifier),
                                            String.Format("Command : {0} ", cc.ToString()));
                                    cc.Schedule.AdvanceTime();
                                    continue;
                                }
                            }
                            catch (Exception e)
                            {
                                this._logger.Error("Unhandled Exception", e);
                                if (EWR.ServerBackup.Library.email.IsEmailConfigured())
                                    EWR.ServerBackup.Library.email.Send(this.AlertEmail, ServerBackup.Program.ServerBackupFromEmail, "Unhandled Exception", String.Format("{0} \r\n {1}", e.Message, e.StackTrace));
                                cc.Schedule.AdvanceTime();
                                continue;
                            }
                        }

                        //Configure command runner
                        cr.BaseDirectory = cc.SourceDirectory;
                        cr.DestinationDirectory = cc.DestinationDirectory;
                        cr.Threads = 1; //Default one "thread"  (one task at a time)
                        cr.ContinueOnError = true;
                        //Run command
                        try
                        {
                            cc.Command.Initialize();
                            cr.RunCommands(cc.Command, cc.fs, cancellationtoken);
                            cc.Command.Close();
                            if (cr.FailedFiles != null && cr.FailedFiles.Count > 0)
                            {
                                if (EWR.ServerBackup.Library.email.IsEmailConfigured())
                                    if (cr.FailedFiles.Count < 50)
                                        EWR.ServerBackup.Library.email.Send(this.AlertEmail, ServerBackup.Program.ServerBackupFromEmail, String.Format("ServerBackup: Failed Files for job {0}", cc.Identifier),
                                            String.Format("Command : {0} \n {1}", cc.ToString(), String.Join("\n", cr.FailedFiles)));
                                    else
                                        EWR.ServerBackup.Library.email.Send(this.AlertEmail, ServerBackup.Program.ServerBackupFromEmail, String.Format("ServerBackup: Failed Files for job {0}", cc.Identifier),
                                            String.Format("Command : {0} \n Over 50 files...", cc.ToString()));

                            }
                            //Email summary of job run 
                            if (cc.EmailonCompletion && EWR.ServerBackup.Library.email.IsEmailConfigured())
                            {
                                EWR.ServerBackup.Library.email.Send(this.AlertEmail, ServerBackup.Program.ServerBackupFromEmail, String.Format("ServerBackup: Job Summary for {0}", cc.Identifier),
                                           String.Format("Command : {0} \n Successful files: {1}  Failed Files {2}", cc.ToString(), cr.SuccessfulFiles.Count, cr.FailedFiles.Count));
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.Error("Unknown Exception during scheduled command [" + cc.Identifier + "]", e);
                            if (EWR.ServerBackup.Library.email.IsEmailConfigured())
                                EWR.ServerBackup.Library.email.Send(this.AlertEmail, ServerBackup.Program.ServerBackupFromEmail, String.Format("ServerBackup: Error during {0}", cc.Identifier),
                                    String.Format("Command : {0} \n Error: {1}", cc.ToString(), e.Message));
                        }
                        _logger.Info(String.Format("Done running command {0}", cc.Identifier));
                        //Reschedule
                        cc.Schedule.AdvanceTime();
                        _logger.Debug(String.Format("Rescheduled {0} to new time at {1}", cc.Identifier, cc.Schedule.NextScheduledTime));
                    }
                }

            }

        }





        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                    if (schedulerthread.IsAlive)
                        schedulerthread.Abort();

                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                // set large fields to null.

                disposedValue = true;
            }
        }

        //  override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Scheduler() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            //  uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion


    }
}
