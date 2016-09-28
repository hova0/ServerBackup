using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;


namespace ServerBackup.Configuration
{
    public class ConfigurationUtil
    {
        public List<string> ConfigurationSections = new List<string>();
        public string AlertEmail { get; set; }
        private IInternalLogger _logger;

        public List<ConfiguredCommand> ConfiguredCommands = new List<ConfiguredCommand>();

        public ConfigurationUtil(IInternalLogger Logger)
        {
            _logger = Logger;
            var rootsection = ConfigurationManager.GetSection("serverBackup/service") as System.Collections.Specialized.NameValueCollection;
            if (rootsection == null)
            {
                Console.WriteLine("Could not find serverBackup/service in config file");
                return;
            }
            var _configuredbackupentities = rootsection["ConfiguredBackupEntities"];
            if (_configuredbackupentities == null)
                throw new Exception("Key ConfiguredBackupEntities was not found!");
            AlertEmail = rootsection["AlertEmail"];
            ServerBackup.Program.DefaultAlertEmail = AlertEmail;
            if (!String.IsNullOrEmpty(rootsection["FromEmail"]))
                ServerBackup.Program.ServerBackupFromEmail = rootsection["FromEmail"];

            ConfigurationSections.AddRange(_configuredbackupentities.Split(';', ','));
            foreach (string s in ConfigurationSections)
            {
                if (String.IsNullOrEmpty(s))
                    continue;
                ConfiguredCommand tempc = GetCommand(s);
                if (tempc != null)
                {
                    ConfiguredCommands.Add(tempc);
                    _logger.Debug("Loaded Command [" + s + "]");
                }
                else
                {
                    _logger.Warn("Unable to load configuration section [" + s + "]");
                }
            }


        }

        public bool ValidateEmailSettings()
        {
            return false;
        }

        public ConfiguredCommand GetCommand(string commandsection)
        {

            System.Collections.Specialized.NameValueCollection nvc = ConfigurationManager.GetSection("serverBackup/" + commandsection) as System.Collections.Specialized.NameValueCollection;
            if (nvc == null)
                return null;
            ConfiguredCommand cc = null;
            if (isSectionLegacy(nvc))
                cc = GetLegacyConfiguration(nvc, _logger);
            else
                cc = GetConfiguration(nvc, _logger);
            if (cc != null)
                cc.Identifier = commandsection;
            return cc;
        }


        private ConfiguredCommand ConfigureCommand(System.Collections.Specialized.NameValueCollection section, IInternalLogger _logger)
        {
            ConfiguredCommand cc = new ConfiguredCommand();


            cc.Command = Program.DetermineCommand(_logger, section["Command"]);

            return cc;
        }



        private System.Collections.Specialized.NameValueCollection LoadConfigSection(string sectionname)
        {
            return ConfigurationManager.GetSection("serverBackup/" + sectionname) as System.Collections.Specialized.NameValueCollection;
        }

        public bool isSectionLegacy(System.Collections.Specialized.NameValueCollection configelement)
        {
            if (configelement.AllKeys.Contains("backupEntityType"))
                return true;
            return false;
        }

        public ConfiguredCommand GetConfiguration(System.Collections.Specialized.NameValueCollection nvc, IInternalLogger _logger)
        {

            ConfiguredCommand cc = new ConfiguredCommand();

            cc.Command = Program.DetermineCommand(_logger, nvc["Command"]);
            cc.SourceDirectory = nvc["SourceDirectory"];
            cc.DestinationDirectory = nvc["DestinationDirectory"];
            //Since hashing and zip output to single files, not directories, Set the appropriate Destination File Name property
            if (cc.Command is CommandHash)
                ((CommandHash)cc.Command).DestinationFileName = cc.DestinationDirectory;
            if (cc.Command is CommandZip)
            {
                ((CommandZip)cc.Command).DestinationFileName = cc.DestinationDirectory;
                ((CommandZip)cc.Command).BaseDirectory = cc.SourceDirectory;
            }
            //Ensure free space  (Usually used for Copy commands)
            if (!String.IsNullOrEmpty(nvc["EnsureFreeSpace"]))
            {
                bool b = false;
                Boolean.TryParse(nvc["EnsureFreeSpace"], out b);
                cc.EnsureFreeSpace = b;
            }

            //Set up schedule for running the command
            Scheduler.RecurringScheduleTime rst = new Scheduler.RecurringScheduleTime();
            rst.InitialScheduleTime = DateTime.Parse(nvc["ScheduleTime"]);
            if (!string.IsNullOrEmpty(nvc["Recurrance"]))
            {
                
                switch (nvc["Recurrance"])
                {
                    case "Daily": rst.RecurringType = Scheduler.ScheduleTypes.Daily; break;
                    case "Weekly": rst.RecurringType = Scheduler.ScheduleTypes.Weekly; break;
                    case "Once": rst.RecurringType = Scheduler.ScheduleTypes.Once; break;
                    case "Monthly": rst.RecurringType = Scheduler.ScheduleTypes.Monthly; break;
                    case "Yearly": rst.RecurringType = Scheduler.ScheduleTypes.Yearly; break;
                    default:
                        _logger.Warn(String.Format("Unknown Recurrance type {0}. Valid values are Daily, Weekly, Once, Monthly, Yearly.  Defaulting to Daily.", nvc["Recurrance"]));
                        rst.RecurringType = Scheduler.ScheduleTypes.Daily; break;
                }
            }
            cc.Schedule = rst;
            try
            {
                cc.fs = new FileSelector(cc.SourceDirectory);
                if (!string.IsNullOrEmpty(nvc["Recurse"]))
                    cc.fs.Recurse = Boolean.Parse(nvc["Recurse"]);
                if (!string.IsNullOrEmpty(nvc["IncludeMask"]))
                    cc.fs.IncludeMatchers.Add(new FileMaskMatcher(nvc["IncludeMask"]));
                if (!string.IsNullOrEmpty(nvc["ExcludeMask"]))
                    cc.fs.ExcludeMatchers.Add(new FileMaskMatcher(nvc["ExcludeMask"]));
                // Support for multiple masks ExcludeMask1 , ExcludeMask2, etc
                int i = 1;
                while (nvc["ExcludeMask" + i.ToString()] != null)
                {
                    cc.fs.ExcludeMatchers.Add(new FileMaskMatcher(nvc["ExcludeMask" + i.ToString()]));
                    i++;
                }
                i = 1;
                while (nvc["IncludeMask" + i.ToString()] != null)
                {
                    cc.fs.ExcludeMatchers.Add(new FileMaskMatcher(nvc["IncludeMask" + i.ToString()]));
                    i++;
                }


                if (!string.IsNullOrEmpty(nvc["OlderThan"]))
                    cc.fs.IncludeMatchers.Add(new FileTimeMatcher(int.Parse(nvc["OlderThan"]), FileTimeMatcher.TimeCompare.OlderThan));
                if (!string.IsNullOrEmpty(nvc["NewerThan"]))
                    cc.fs.IncludeMatchers.Add(new FileTimeMatcher(int.Parse(nvc["NewerThan"]), FileTimeMatcher.TimeCompare.NewerThan ));
            }
            catch (Exception e)
            {
                _logger.Error("Could not load directory [" + cc.SourceDirectory + "]", e);
                return null;
            }
            if (!string.IsNullOrEmpty(nvc["Simulate"]))
                cc.Command.Simulate = bool.Parse(nvc["Simulate"]);
            if(!String.IsNullOrEmpty(nvc["EmailonCompletion"]) || !String.IsNullOrEmpty(nvc["EmailOnCompletion"]))
            {
                bool emailoncompletion = false;
                if(! bool.TryParse(nvc["EmailonCompletion"], out emailoncompletion))
                    bool.TryParse(nvc["EmailOnCompletion"], out emailoncompletion);
                cc.EmailonCompletion = emailoncompletion;
            }
            return cc;
        }

        // For backwards compatibility with old ServerBackup configs  (Might not support all cases)
        public ConfiguredCommand GetLegacyConfiguration(System.Collections.Specialized.NameValueCollection configelement, IInternalLogger _logger)
        {
            ConfiguredCommand cc = new ConfiguredCommand();
            cc.SourceDirectory = configelement["sourceRootPath"];
            cc.DestinationDirectory = configelement["destinationRootPath"];
            //Since hashing and zip output to single files, not directories, Set the appropriate Destination File Name property
            if (cc.Command is CommandHash)
                ((CommandHash)cc.Command).DestinationFileName = cc.DestinationDirectory;
            if (cc.Command is CommandZip)
            {
                ((CommandZip)cc.Command).DestinationFileName = cc.DestinationDirectory;
                ((CommandZip)cc.Command).BaseDirectory = cc.SourceDirectory;
            }

            //Set up schedule for running the command
            Scheduler.RecurringScheduleTime rst = new Scheduler.RecurringScheduleTime();
            rst.InitialScheduleTime = DateTime.Parse(configelement["invocationTime"]);
            rst.RecurringType = Scheduler.ScheduleTypes.Daily;
            cc.Schedule = rst;

            //cc.ScheduleTime = DateTime.Parse(configelement["invocationTime"]);
            string backupentitytype = configelement["backupEntityType"];    //Usually FileFunction
            string backupOptions = configelement["backupOptions"];
            try
            {
                cc.fs = new FileSelector(cc.SourceDirectory);
            }
            catch (Exception e)
            {
                _logger.Error("Could not load directory [" + cc.SourceDirectory + "]", e);
                return null;
            }
            foreach (string s in backupOptions.Split(';', ','))
            {
                if (s == "RecurseDirectories")
                    cc.fs.Recurse = true;
                if (s == "DeleteFiles" && cc.Command == null)
                    cc.Command = new CommandDelete(_logger);
                if (s == "CopyFiles" && cc.Command == null)
                    cc.Command = new CommandCopy(_logger);

            }

            if (configelement["thresholdDays"] != null)
            {
                int days = 0;
                if (int.TryParse(configelement["thresholdDays"], out days))
                    cc.fs.IncludeMatchers.Add(new ServerBackup.FileTimeMatcher(days));
            }
            if (!string.IsNullOrEmpty(configelement["Simulate"]))
                cc.Command.Simulate = bool.Parse(configelement["Simulate"]);
            if (!string.IsNullOrEmpty(configelement["Recurrance"]))
            {
                //cc.RecurringSchedule = configelement["Recurrance"];
                switch (configelement["Recurrance"])
                {
                    case "Daily": rst.RecurringType= Scheduler.ScheduleTypes.Daily; break;
                    case "Weekly": rst.RecurringType = Scheduler.ScheduleTypes.Weekly; break;
                    case "Once": rst.RecurringType = Scheduler.ScheduleTypes.Once; break;
                    case "Monthly": rst.RecurringType = Scheduler.ScheduleTypes.Monthly; break;
                    case "Yearly": rst.RecurringType = Scheduler.ScheduleTypes.Yearly; break;
                    default: rst.RecurringType = Scheduler.ScheduleTypes.Daily; break;
                }
            }

            return cc;
        }

    }
}
