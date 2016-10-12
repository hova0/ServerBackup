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
            NoError=0,
            InvalidArguments=1,
            NoSpaceAvailable=2,
            ErrorDuringExecution=3,


        }

        static int Main(string[] args)
        {
            //for(int i = 0; i < 16; i++) 
            //{
            //    Console.ForegroundColor = (ConsoleColor)i;
            //    Console.WriteLine(((ConsoleColor)i).ToString());
            //}
            if (args.Length == 0)
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new Service1()
                };
                ServiceBase.Run(ServicesToRun);
            }
            else
            {
                Console.CancelKeyPress += Console_CancelKeyPress;

                //if(args[0] == "testemail")
                //{
                //    System.Net.Mail.SmtpClient sc = new System.Net.Mail.SmtpClient();
                //    Console.WriteLine(sc.Credentials);
                //    Console.WriteLine(sc.Host);
                //    return 0;
                //}

                if (args[0] == "testservice")
                {
                    Scheduler.Scheduler mainscheduler = new Scheduler.Scheduler(_Logger);
                    Configuration.ConfigurationUtil cu = new Configuration.ConfigurationUtil(_Logger);
                    mainscheduler = new Scheduler.Scheduler(_Logger);

                    foreach (Configuration.ConfiguredCommand cc in cu.ConfiguredCommands)
                    {

                        //BLAH mainscheduler.CommandsPending.Add(new Tuple<Scheduler.ScheduledCommand, DateTime>(, Scheduler.Scheduler.GetNextTime(DateTime.Now, new Scheduler.RecurringScheduleTime() { RecurringType = cc.RecurringSchedule, ScheduleDateTime = cc.ScheduleTime }  )));
                        mainscheduler.AddScheduledCommand(cc);
                        _Logger.Debug("Scheduled  " + cc.Identifier + " to run at " + cc.Schedule.NextScheduledTime.ToString());
                        //_Logger.Debug(cc.fs.IncludeMatchers[0].ToString());
                    }
                    mainscheduler.ScheduleLoop(cts.Token);
                    Console.WriteLine("Scheduler loop running.  Press CTRL+C to stop");
                    while (true)
                    {
                        System.Threading.Thread.Sleep(100);
                        if (cts.IsCancellationRequested)
                            break;
                    }

                    return 0;

                }


                _Logger.Info("ServerBackup Started from Console");

                

                List<Tuple<string, string>> cmdargs = ParseArguments(args);
                if (cmdargs.Count > 0 && NormalizeKey(cmdargs[0].Item1) == "help" || NormalizeKey(cmdargs[0].Item1) == "?")
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
                    Console.WriteLine("                                     This usually degrades performance.");
                    return 0;
                }

                string maincommand = cmdargs[0].Item1;
                ICommand c = null;
                CommandRunner cr = new CommandRunner(_Logger);
                try
                {
                    c = DetermineCommand(_Logger, maincommand);
                }catch(System.Exception e)
                {
                    _Logger.Fatal("Unknown command", e);
                    return (int)ExitCodes.InvalidArguments;
                }
                //Since hashing and zip output to single files, not directories, Set the appropriate Destination File Name property
                if (c is CommandHash)
                    ((CommandHash)c).DestinationFileName = cmdargs[2].Item1;
                if (c is CommandZip)
                {
                    ((CommandZip)c).DestinationFileName = cmdargs[2].Item1;
                    ((CommandZip)c).BaseDirectory = GetSource(cmdargs);
                }
                
                //Set simulate mode if specified
                if (cmdargs.Any(x => NormalizeKey(x.Item1) == "simulate"))
                    c.Simulate = true;
                
                //Create File Selector 
                FileSelector fs = GetSelector(cmdargs);

                //Sets the base directory (required)
                cr.BaseDirectory = GetSource(cmdargs);
                
                cr.DestinationDirectory = GetDest(cmdargs);
                cr.Threads = 1;  //Default no threading
                if(cmdargs.Any(x => NormalizeKey(x.Item1) == "threads"))
                {
                    int t = 1;
                    Int32.TryParse(cmdargs.First(x => NormalizeKey(x.Item1) == "threads").Item2, out t);
                    cr.Threads = t;
                }
                //Check global flag to see if there was any problem in getting source of dest directories
                if (fatalError)
                    return (int)ExitCodes.InvalidArguments;

                if (cmdargs.Any(x => NormalizeKey(x.Item1) == "recurse"))
                    fs.Recurse = true;
                // Check to see if the user wanted a special Hashing algorithm.  Default is md5
                if (cmdargs.Any(x => NormalizeKey(x.Item1) == "hashalg"))
                {
                    string hasharg = cmdargs.First(z => NormalizeKey(z.Item1) == "hashalg").Item2;
                    if (c is CommandCopy)
                        ((CommandCopy)c).HashingFunction = DeriveHashingFunction(hasharg);
                    if (c is CommandHash)
                        ((CommandHash)c).HashingFunction = DeriveHashingFunction(hasharg);
                }
                //Verify the copy succeeded (will take longer)
                //This operates by calculating an md5 hash during copy and then rereading only the destination and recalculating the hash
                //Source file is only read once, Destination file is written, and then read.
                if (c is CommandCopy && cmdargs.Any(x => NormalizeKey(x.Item1) == "verify"))
                {
                    ((CommandCopy)c).Verify = true;
                }

                //Ensure free space
                if (cmdargs.Any(x => NormalizeKey(x.Item1) == "ensurespace"))
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
                

            }

            return (int)ExitCodes.NoError;
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
              
                    //case "simulate": c = new CommandSimulate(); break;
                default : throw new Exception("Unknown command");
            }
            return c;
        }

        static FileSelector GetSelector(List<Tuple<string, string>> cmdargs)
        {
            //Build up FileSelector from arguments
            string sourcedirectory = GetSource(cmdargs);
            //string destinationdirectory = GetDest(cmdargs);
            if (sourcedirectory == null)
                return null;
            FileSelector fs = null;
            try
            {
                fs = new FileSelector(sourcedirectory);
                fs.IncludeMatchers.AddRange(ParseIncludeMatchers(cmdargs));
                fs.ExcludeMatchers.AddRange(ParseExcludeMatchers(cmdargs));
            }catch(System.UnauthorizedAccessException uae)
            {
                _Logger.Error("Could not access directory [" + sourcedirectory + "]", uae);
                return null;
            } catch(Exception e)
            {
                _Logger.Error("Could not access directory [" + sourcedirectory + "]", e);
                return null;
            }
            return fs;
        }
        public static System.Security.Cryptography.HashAlgorithmName DeriveHashingFunction(string hashfunction)
        {
            switch(hashfunction.ToLower())
            {
                case "md5":
                    return System.Security.Cryptography.HashAlgorithmName.MD5;
                case "sha1":
                    return System.Security.Cryptography.HashAlgorithmName.SHA1;
                case "sha256":
                    return System.Security.Cryptography.HashAlgorithmName.SHA256;
            }
            return System.Security.Cryptography.HashAlgorithmName.MD5;
        }
        public static IFileMatcher[] ParseIncludeMatchers(List<Tuple<string, string>> cmdargs)
        {
            List<IFileMatcher> matchers = new List<IFileMatcher>();
            foreach (Tuple<string, string> keyvalue in cmdargs)
            {
                string normalizedkey = NormalizeKey(keyvalue.Item1);
                if (normalizedkey == "include")
                {
                    try
                    {
                        matchers.Add(new FileMaskMatcher(keyvalue.Item2));
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e.Message);
                        Console.Error.WriteLine(string.Format("[ERROR] Discarded invalid filemask {0}", keyvalue.Item2));
                    }
                }
              
                if (normalizedkey == "newerthan" || normalizedkey == "olderthan")
                {
                    string dateargument = keyvalue.Item2;
                    DateTime constdate;
                    int daysold = 0;
                    if (DateTime.TryParse(dateargument, out constdate))
                        matchers.Add(new FileTimeMatcher(constdate, normalizedkey == "newerthan" ? FileTimeMatcher.TimeCompare.NewerThan : FileTimeMatcher.TimeCompare.OlderThan));
                    if (Int32.TryParse(dateargument, out daysold ))
                        matchers.Add(new FileTimeMatcher(daysold, normalizedkey == "newerthan" ? FileTimeMatcher.TimeCompare.NewerThan : FileTimeMatcher.TimeCompare.OlderThan));
                }

            }
            return matchers.ToArray();
        }
        public static IFileMatcher[] ParseExcludeMatchers(List<Tuple<string, string>> cmdargs)
        {
            List<IFileMatcher> matchers = new List<IFileMatcher>();
            foreach (Tuple<string, string> keyvalue in cmdargs)
            {
                string normalizedkey = NormalizeKey(keyvalue.Item1);
                if (normalizedkey == "exclude")
                {
                    try
                    {
                        matchers.Add(new FileMaskMatcher(keyvalue.Item2));
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e.Message);
                        Console.Error.WriteLine(string.Format("[ERROR] Discarded invalid filemask {0}", keyvalue.Item2));
                    }
                }
            }
            return matchers.ToArray();
        }

        public static string GetSource(List<Tuple<string, string>> cmdargs)
        {
            if (cmdargs.Count < 2)
                return null;
            string src = cmdargs[1].Item1;
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

        public static string GetDest(List<Tuple<string, string>> cmdargs)
        {

            if (cmdargs.Count < 3)
                return null;
            string src = cmdargs[2].Item1;
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

        //static Dictionary<string, string> cmdArguments = new Dictionary<string, string>();
        /// <summary>
        /// Parses command line arguments into a List of key/value pairs.  First argument is designated as the "command" argument.
        /// Subsequent arguments are either flags (key with no value) or key-value pairs.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static List<Tuple<string, string>> ParseArguments(string[] args)
        {
            List<Tuple<string, string>> cmdlist = new List<Tuple<string, string>>();

            int i = 0;
            while (i < args.Length)
            {
                string key = null;
                string value = null;
                //if(args[i].StartsWith("-") || args[i].StartsWith("/") || i == 0)
                //{
                key = args[i];
                if (i + 1 < args.Length
                    && !(args[i + 1].StartsWith("-") || args[i + 1].StartsWith("/") || i == 0)
                    && (args[i].StartsWith("-") || args[i].StartsWith("/") || i == 0))
                {
                    // If the next param is not a switch but a value
                    value = args[i + 1];
                    i++;
                }
                //}
                if (key != null)
                    cmdlist.Add(new Tuple<string, string>(key, value));
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
