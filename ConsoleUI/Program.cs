using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ServerBackup
{
    public static class Program
    {
        public static string ServerBackupFromEmail = "donotreply@example.com";
        public static string DefaultAlertEmail = "alert@example.com";
        static System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
        /// <summary>
        /// The main entry point for the application.
        /// </summary>

        static bool fatalError = false;


        public static IInternalLogger _Logger = new Loggers.ConsoleLogger();   //Always direct output to console 

        enum ExitCodes
        {
            NoError = 0,
            InvalidArguments = 1,
            NoSpaceAvailable = 2,
            ErrorDuringExecution = 3,


        }

        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Serverbackup launching as a SERVICE.   Use ServerBackup --help for launch options");
                Console.WriteLine("To install ServerBackup as a service, use the command \"sc create\"");
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new Service1()
                };
                ServiceBase.Run(ServicesToRun);
                return 0;

            }

            Console.CancelKeyPress += Console_CancelKeyPress;


            if (args[0] == "daemon")
            {
                //This is for primarily testing the Service mode.
                Scheduler.Scheduler mainscheduler = new Scheduler.Scheduler(_Logger);
                Configuration.ConfigurationUtil cu = new Configuration.ConfigurationUtil(_Logger);
                mainscheduler = new Scheduler.Scheduler(_Logger);

                foreach (Configuration.ConfiguredCommand cc in cu.ConfiguredCommands)
                {

                    mainscheduler.AddScheduledCommand(cc);
                    _Logger.Debug("Scheduled  " + cc.Identifier + " to run at " + cc.Schedule.NextScheduledTime.ToString());
                }
                mainscheduler.ScheduleLoop(cts.Token);
                Console.WriteLine("Scheduler loop running.  Press CTRL+C to stop");
                while (true)
                {
                    System.Threading.Thread.Sleep(200);
                    if (cts.IsCancellationRequested)
                        break;
                }

                return 0;

            }


            _Logger.Info("ServerBackup Started from Console");



            System.Collections.Specialized.NameValueCollection cmdargs = ParseArguments(args);
            //List of valid switches, if they typoed or something, show them help (first three arguments are command, source, destination)
            string[] validSwitches = new string[] { "simulate", "threads", "verify", "ensurespace", "olderthan", "newerthan", "include", "exclude", "hashalg", "recurse" };
            if (cmdargs.AllKeys.Length > 3)
                foreach (string _cmdswitch in cmdargs.AllKeys.Skip(3))
                    if (!validSwitches.Contains(_cmdswitch))
                        return PrintHelp();

            if (cmdargs.Count > 0 && cmdargs.AllKeys.Contains("help") || cmdargs.AllKeys.Contains("?"))
                return PrintHelp();


            string maincommand = cmdargs.Keys[0];
            ICommand c = null;
            CommandRunner cr = new CommandRunner(_Logger);
            try
            {
                c = DetermineCommand(_Logger, maincommand);
            }
            catch (System.Exception e)
            {
                _Logger.Fatal("Unknown command", e);
                PrintHelp();
                return (int)ExitCodes.InvalidArguments;
            }
            //Since hashing and zip output to single files, not directories, Set the appropriate Destination File Name property
            if (c is CommandHash)
                ((CommandHash)c).DestinationFileName = cmdargs.Keys[2];
            if (c is CommandZip)
            {
                ((CommandZip)c).DestinationFileName = cmdargs.Keys[2];
                ((CommandZip)c).BaseDirectory = GetSource(cmdargs);
            }

            //Set simulate mode if specified
            if (cmdargs.AllKeys.Any(x => NormalizeKey(x) == "simulate"))
                c.Simulate = true;

            //Create File Selector 
            FileSelector fs = GetSelector(cmdargs);

            //Sets the base directory (required)
            cr.BaseDirectory = GetSource(cmdargs);

            cr.DestinationDirectory = GetDest(cmdargs);
            cr.Threads = 1;  //Default no threading
            if (cmdargs.AllKeys.Contains("threads"))
            {
                int t = 1;
                Int32.TryParse(cmdargs["threads"], out t);
                cr.Threads = t == 0 ? 1 : t;
            }
            //Check global flag to see if there was any problem in getting source of dest directories
            if (fatalError)
                return (int)ExitCodes.InvalidArguments;

            if (cmdargs.AllKeys.Contains("recurse"))
                fs.Recurse = true;
            // Check to see if the user wanted a special Hashing algorithm.  Default is md5
            if (cmdargs.AllKeys.Contains("hashalg"))
            {
                string hasharg = cmdargs["hashalg"]; //.First(z => NormalizeKey(z.Item1) == "hashalg").Item2;
                if (c is CommandCopy)
                    ((CommandCopy)c).HashingFunction = DeriveHashingFunction(hasharg);
                if (c is CommandHash)
                    ((CommandHash)c).HashingFunction = DeriveHashingFunction(hasharg);
            }
            //Verify the copy succeeded (will take longer)
            //This operates by calculating an md5 hash during copy and then rereading only the destination and recalculating the hash
            //Source file is only read once, Destination file is written, and then read.
            if (c is CommandCopy && cmdargs.AllKeys.Contains("verify"))
            {
                ((CommandCopy)c).Verify = true;
            }

            //Ensure free space
            if (cmdargs.AllKeys.Contains("ensurespace"))
            {
                //Run space calculation before main command
                FileSelector fs2 = GetSelector(cmdargs);
                CommandEnsureFreeSpace cmdfreespace = new CommandEnsureFreeSpace();
                fs2.Recurse = fs.Recurse;
                CommandRunner cr2 = new CommandRunner(_Logger);
                cr2.BaseDirectory = GetSource(cmdargs);
                cr2.DestinationDirectory = GetDest(cmdargs);
                cr2.RunCommands(cmdfreespace, fs2, cts.Token);
                if (cmdfreespace.CommandResult == "False")
                {
                    _Logger.Error("Not enough free space for command", null);
                    return (int)ExitCodes.NoSpaceAvailable;
                }
            }

            if (c == null)
            {
                Console.WriteLine("Invalid command");
                return (int)ExitCodes.InvalidArguments;
            }

            //Run main command
            try
            {
                c.Initialize();
                cr.RunCommands(c, fs, cts.Token);
                c.Close();

            }
            catch (Exception e)
            {
                _Logger.Error("Error Running command", e);
                //Console.WriteLine(e.Message);
                return (int)ExitCodes.ErrorDuringExecution;
            }




            return (int)ExitCodes.NoError;
        }

        private static int PrintHelp()
        {
            //Print help
            Console.WriteLine("[ServerBackup Help]");
            Console.WriteLine("ServerBackup <command> <source> <dest> [options]");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("copy                  Copies files from <source> to <dest>");
            Console.WriteLine("hash                  Checksum on <source> written to <dest>");
            Console.WriteLine("delete                Deltes files in <source>");
            Console.WriteLine("verify                Verifies md5sums in <source> against <dest>");
            //Console.WriteLine("simulate              Simulates a copy by printing to console");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("-include <regex mask>            Includes files matching regular expression");
            Console.WriteLine("-exclude <regex mask>            Excludes files matching regular expression");
            Console.WriteLine();
            Console.WriteLine("Note: If -include is specified, only files matching will be included in <source>");
            Console.WriteLine("      If -exclude is specified, only files that do not match will be included in <source>");
            Console.WriteLine("      If both -include and -exlude is specified, only files matching -include EXCEPT for ones that also match -exclude will be in <source>");
            Console.WriteLine("      Lastly, multiple -include and -exclude expressions can be used.");
            Console.WriteLine();
            Console.WriteLine("-newerthan <date>                Include files newer than date");
            Console.WriteLine("-newerthan <number>              Include files newer than <number> days old");
            Console.WriteLine("-olderthan <date>                Include files older than date");
            Console.WriteLine("-olderthan <number>              Include files older than <number> days old");
            Console.WriteLine("-verify                          Verifies that a file was copied correctly by comparing MD5");
            Console.WriteLine("-ensurespace                     Ensures the destination has free space before copy.  Will error if not.");
            Console.WriteLine("                                 Not supported on some destinations.");
            Console.WriteLine("-simulate                        Print action to console instead of executing");
            Console.WriteLine("                                     Useful to test file matching is correct.");
            Console.WriteLine("-threads <number>                Enable multithreading.  Zip files cannot be threaded.");
            Console.WriteLine("                                     This usually degrades performance on single disks.");
            Console.WriteLine();
            Console.WriteLine("Exit Codes:");
            Console.WriteLine("0 - No Error");
            Console.WriteLine("1 - Invalid Arguments");
            Console.WriteLine("2 - No Space Available");
            Console.WriteLine("3 - Error During Execution (see logs for more information)");
            Console.WriteLine();
            return 0;
        }

        public static ICommand DetermineCommand(IInternalLogger _Logger, String maincommand)
        {
            ICommand c = null;
            switch (maincommand.ToLower())
            {
                case "copy": c = new CommandCopy(_Logger); break;
                case "delete": c = new CommandDelete(_Logger); break;
                case "hash": c = new CommandHash(_Logger); break;
                case "zip": c = new CommandZip(_Logger); break;
                case "verify": throw new NotImplementedException("Sorry, this feature is not yet implemented.");
                default: throw new Exception("Unknown command");
            }
            return c;
        }

        static FileSelector GetSelector(System.Collections.Specialized.NameValueCollection cmdargs)
        {
            //Build up FileSelector from arguments
            string sourcedirectory = GetSource(cmdargs);

            if (sourcedirectory == null)
                return null;
            FileSelector fs = null;
            try
            {
                fs = new FileSelector(sourcedirectory);
                fs.IncludeMatchers.AddRange(ParseIncludeMatchers(cmdargs));
                fs.ExcludeMatchers.AddRange(ParseExcludeMatchers(cmdargs));
            }
            catch (System.UnauthorizedAccessException uae)
            {
                _Logger.Error("Could not access directory [" + sourcedirectory + "]", uae);
                return null;
            }
            catch (Exception e)
            {
                _Logger.Error("Could not access directory [" + sourcedirectory + "]", e);
                return null;
            }
            return fs;
        }
        public static System.Security.Cryptography.HashAlgorithmName DeriveHashingFunction(string hashfunction)
        {
            switch (hashfunction.ToLower())
            {
                case "md5":
                    return System.Security.Cryptography.HashAlgorithmName.MD5;
                case "sha1":
                    return System.Security.Cryptography.HashAlgorithmName.SHA1;
                case "sha256":
                    return System.Security.Cryptography.HashAlgorithmName.SHA256;
                default: return System.Security.Cryptography.HashAlgorithmName.MD5;
            }
        }
        public static IFileMatcher[] ParseIncludeMatchers(System.Collections.Specialized.NameValueCollection cmdargs)
        {
            List<IFileMatcher> matchers = new List<IFileMatcher>();
            foreach (string keyvalue in cmdargs.Keys)
            {
                string normalizedkey = keyvalue;
                if (normalizedkey == "include")
                {
                    try
                    {
                        matchers.Add(new FileMaskMatcher(cmdargs[keyvalue]));
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e.Message);
                        Console.Error.WriteLine(string.Format("[ERROR] Discarded invalid filemask {0}", cmdargs[keyvalue]));
                    }
                }

                if (normalizedkey == "newerthan" || normalizedkey == "olderthan")
                {
                    string dateargument = cmdargs[keyvalue];
                    DateTime constdate;
                    int daysold = 0;
                    if (DateTime.TryParse(dateargument, out constdate))
                        matchers.Add(new FileTimeMatcher(constdate, normalizedkey == "newerthan" ? FileTimeMatcher.TimeCompare.NewerThan : FileTimeMatcher.TimeCompare.OlderThan));
                    if (Int32.TryParse(dateargument, out daysold))
                        matchers.Add(new FileTimeMatcher(daysold, normalizedkey == "newerthan" ? FileTimeMatcher.TimeCompare.NewerThan : FileTimeMatcher.TimeCompare.OlderThan));
                }

            }
            return matchers.ToArray();
        }
        public static IFileMatcher[] ParseExcludeMatchers(System.Collections.Specialized.NameValueCollection cmdargs)
        {
            List<IFileMatcher> matchers = new List<IFileMatcher>();
            foreach (string keyvalue in cmdargs.Keys)
            {
                string normalizedkey = NormalizeKey(keyvalue);
                if (normalizedkey == "exclude")
                {
                    try
                    {
                        matchers.Add(new FileMaskMatcher(cmdargs[keyvalue]));
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e.Message);
                        Console.Error.WriteLine(string.Format("[ERROR] Discarded invalid filemask {0}", cmdargs[keyvalue]));
                    }
                }
            }
            return matchers.ToArray();
        }

        public static string GetSource(System.Collections.Specialized.NameValueCollection cmdargs)
        {
            if (cmdargs.Count < 2)
                return null;
            string src = cmdargs.Keys[1];
            try
            {
                if (System.IO.Directory.Exists(src))
                    return src;
            }
            catch (Exception)
            {
                Console.WriteLine(src + " is not a valid directory!");
                fatalError = true;
                return null;
            }
            Console.WriteLine("Source Directory {0} does not exist!", src);
            fatalError = true;
            return null;
            //throw new Exception(String.Format("Source directory {0} does not exist!", src));
        }

        public static string GetDest(System.Collections.Specialized.NameValueCollection cmdargs)
        {

            if (cmdargs.Count < 3)
                return null;
            string src = cmdargs.AllKeys[2];
            //Destination may not exist, will be created when command executes
            return src;
        }

        private static void Console_CancelKeyPress(Object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;        //Prevents termination of the process so we can clean up.
            //CTRL+C pressed, abort all actions
            _Logger.Warn("Cancelling in progress commands...");
            cts.Cancel();

        }

        /// <summary>
        /// Parses command line arguments into a List of key/value pairs.  First argument is designated as the "command" argument.
        /// Subsequent arguments are either flags (key with no value) or key-value pairs.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static System.Collections.Specialized.NameValueCollection ParseArguments(string[] args)
        {
            System.Collections.Specialized.NameValueCollection cmdlist = new System.Collections.Specialized.NameValueCollection();

            int i = 0;
            while (i < args.Length)
            {
                string key = null;
                string value = null;

                key = args[i];
                if (i + 1 < args.Length
                    && !(args[i + 1].StartsWith("-") || args[i + 1].StartsWith("/") || i == 0)  //next argument does not start with a switch
                    && (args[i].StartsWith("-") || args[i].StartsWith("/") || i == 0))          // but current argument does
                {
                    // If the next param is not a switch but a value
                    value = args[i + 1];
                    i++;    //skip parsing the switch argument
                }
                //}
                if (key != null)
                    cmdlist.Add(NormalizeKey(key), value);
                i++;
            }

            return cmdlist;
        }

        public static string NormalizeKey(string key)
        {
            string normalized_key = key;
            if (String.IsNullOrEmpty(key))
                throw new Exception("Key was supposed to be normalized but was empty or null");

            if (key.StartsWith("-") || key.StartsWith("/"))  // Handle - and / 
                normalized_key = key.Substring(1);
            if (normalized_key.StartsWith("-")) // Handle --
                normalized_key = normalized_key.Substring(1);
            return normalized_key;
        }

    }
}
