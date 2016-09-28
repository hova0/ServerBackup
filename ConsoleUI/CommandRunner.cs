using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ServerBackup
{
    public class CommandRunner
    {

        public int Threads { get; set; }
        public string BaseDirectory { get; set; }
        public string DestinationDirectory { get; set; }

        private IInternalLogger logger;

        public CommandRunner(IInternalLogger log)
        {
            Threads = 1;
            SuccessfulFiles = new List<string>();
            FailedFiles = new List<string>();
            logger = log;
            ContinueOnError = true;
        }

        public List<string> SuccessfulFiles { get; set; }
        public List<string> FailedFiles { get; set; }
        public bool ContinueOnError { get; set; }

        public void RunCommands(ICommand cmd, FileSelector Files, System.Threading.CancellationToken cancellationToken)
        {
            if (cmd == null || Files == null)
                return; // Nothing to do!
            
            bool tasksucceeded = true;  // Signals that a task did not complete successfully
            bool taskfaulted = false;   // Signals that a task faulted due to exception
            Task<bool>[] runningtasks = new Task<bool>[this.Threads];
            string[] currentfiles = new string[this.Threads];
            long filesrun = 0;

            foreach (System.IO.FileInfo file in Files.FileList())
            {
                filesrun++;
                if (cancellationToken.IsCancellationRequested || taskfaulted)
                {
                    //Abort process
                    break;
                }
                if (runningtasks.All(x => x != null && x.IsCompleted != true))
                {
                    //Wait for a Task to free up
                    while (true)
                    {
                        Task.WaitAny(runningtasks, 500);
                        if (runningtasks.Any(x => x.IsCompleted))
                        {
                            Task<bool> currenttask; 
                            int taskindex = FirstFreeTask(runningtasks);
                            currenttask = runningtasks[taskindex];
                            
                            if (currenttask.Status == TaskStatus.Faulted)
                            {
                                taskfaulted = true;
                                tasksucceeded = false;
                            }
                            else if (currenttask.Status == TaskStatus.RanToCompletion)
                            {
                                tasksucceeded = currenttask.Result;
                            }
                            else
                            {
                                logger.Error(String.Format("Unknown Task Status {0}", currenttask.Status), null);
                                //Console.Error.WriteLine("Unknown Task Status {0}", currenttask.Status);
                            }
                            if (tasksucceeded)
                                SuccessfulFiles.Add(currentfiles[taskindex]);
                            else
                                FailedFiles.Add(currentfiles[taskindex]);
                            currenttask.Dispose();
                            runningtasks[taskindex] = null;
                            currentfiles[taskindex] = null;
                            //runningtasks.Remove(currenttask);
                            break;

                        }
                    }
                }
                //A task is free, so schedule a new command
                if (runningtasks.Any(x => x == null || x.IsCompleted))
                {
                    int i = 0;
                    i = FirstFreeTask(runningtasks);
                    if (runningtasks[i] != null)
                    {
                        if (runningtasks[i].Status != TaskStatus.Faulted && runningtasks[i].Result)
                            SuccessfulFiles.Add(currentfiles[i]);
                        else
                            FailedFiles.Add(currentfiles[i]);

                        if (runningtasks[i].Status == TaskStatus.Faulted)
                        {
                            taskfaulted = true;
                            runningtasks[i].Dispose();
                            if(!ContinueOnError)
                                break;
                        }
                        runningtasks[i].Dispose();
                    }
                    try
                    {
                        Task<bool> commandtask = cmd.RunAsync(file.FullName, GetDestinationFileName(file.FullName, this.BaseDirectory, this.DestinationDirectory), cancellationToken);
                        runningtasks[i] = commandtask;
                        currentfiles[i] = file.FullName;

                        //If the cancellation token is currently set, the Task will be in a faulted state on creation.  We will be exiting shortly.
                        if (commandtask.Status == TaskStatus.Created)
                            commandtask.Start();
                    }catch(Exception e)
                    {
                        logger.Error("Unexpected Exception", e);
                        if(!ContinueOnError)
                            break;
                    }
                }
                else
                {
                    throw new Exception("Missed job due to scheduler misfire!");
                }

            }   //file iterator

            
            //Wait for rest of tasks to complete
            for (int i = 0; i < runningtasks.Length; i++)
            {
                if (runningtasks[i] != null && (runningtasks[i].Status != TaskStatus.Faulted || runningtasks[i].Status != TaskStatus.RanToCompletion))
                {
                    try
                    {
                        runningtasks[i].Wait(cancellationToken);
                        if (runningtasks[i].Result)
                            SuccessfulFiles.Add(currentfiles[i]);
                        else
                            FailedFiles.Add(currentfiles[i]);
                        runningtasks[i].Dispose();
                        
                    }
                    catch (Exception e)
                    {
                        FailedFiles.Add(currentfiles[i]);
                        logger.Error(currentfiles[i], e);
                    }
                }
            }
            logger.Debug(String.Format("Ran command for {0} files.", filesrun));

        }
        private int FirstFreeTask(System.Threading.Tasks.Task[] tasks)
        {
            int i = 0;
            for (i = 0; i < tasks.Length; i++)
            {
                if (tasks[i] == null || tasks[i].IsCompleted)
                    break;
            }
            return i;
        }


        /// <summary>
        /// Determines the destination file name based on source and recursion flag
        /// </summary>
        /// <param name="sourceFullFileName">Full path to the filename</param>
        /// <param name="BaseDirectory">Directory where operations started.</param>
        /// <param name="destination">Destination directory</param>
        /// <returns>Full path to the destination file, mirroring original directory structure.</returns>
        public string GetDestinationFileName(string sourceFullFileName, string BaseDirectory, string DestinationDirectory)
        {
            if (String.IsNullOrEmpty(DestinationDirectory))
                return null;
            //Validation check to ensure the BaseDirectory is in the source file
            List<string> sourcedirs = GetDirectoryNames(sourceFullFileName);
            //NOTE: We assume that BaseDirectory is a directory, so it may not end with a \, so add one.
            if (!BaseDirectory.EndsWith(Path.DirectorySeparatorChar.ToString()) && BaseDirectory.IndexOf(Path.DirectorySeparatorChar) > 0)
                BaseDirectory += Path.DirectorySeparatorChar;
            if (!BaseDirectory.EndsWith(Path.AltDirectorySeparatorChar.ToString()) && BaseDirectory.IndexOf(Path.AltDirectorySeparatorChar) > 0)
                BaseDirectory += Path.AltDirectorySeparatorChar;

            List<string> basedirs = GetDirectoryNames(BaseDirectory);
            //Compare base directories to passed in filename
            if (!basedirs.All(x => sourcedirs.Contains(x)))
                throw new Exception("Invalid base directory or source file name!");


            //  Trim trailing slashes
            if (BaseDirectory.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                BaseDirectory.TrimEnd(System.IO.Path.DirectorySeparatorChar);
            if (BaseDirectory.EndsWith(System.IO.Path.AltDirectorySeparatorChar.ToString()))
                BaseDirectory.TrimEnd(System.IO.Path.AltDirectorySeparatorChar);
            //
            //      Source:  C:\temp\dir1\DirX\DirY\Test.txt 
            //      Dest:    C:\temp\dir2\
            //      Base:    C:\temp\dir1\
            //      Base + <intermediate directories> + Filename = Full Source Path
            //      The below removes the Base, and Filename to give us the intermediate directories in a string
            //      This allows us to replicate the directory structure
            string relativepath = sourceFullFileName.Replace(BaseDirectory, "").Replace(System.IO.Path.GetFileName(sourceFullFileName), "");
            //Prevent rooted paths of the form "\directory\file"
            if (relativepath.StartsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                relativepath = relativepath.TrimStart(System.IO.Path.DirectorySeparatorChar);
            if (relativepath.StartsWith(System.IO.Path.AltDirectorySeparatorChar.ToString()))
                relativepath = relativepath.TrimStart(System.IO.Path.AltDirectorySeparatorChar);

            string destinationfilename = System.IO.Path.Combine(DestinationDirectory, relativepath, System.IO.Path.GetFileName(sourceFullFileName));




            return destinationfilename;
        }

        //Returns a list of directory names based off a path
        /// <summary>
        /// Returns a list of directory names based off a path
        /// </summary>
        /// <param name="directorypath">Full directory path</param>
        /// <returns>List of directories.  Does NOT contain root directory ( "C:\" or "\\servername\" )</returns>
        public List<string> GetDirectoryNames(string directorypath)
        {
            List<string> FullList = new List<string>();
            //Chop off initial file
            if (!String.IsNullOrWhiteSpace(Path.GetFileName(directorypath)))
                directorypath = Path.GetDirectoryName(directorypath);

            while (true)
            {
                if (String.IsNullOrEmpty(directorypath))
                    break;
                if (directorypath.EndsWith(Path.DirectorySeparatorChar.ToString()) || directorypath.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
                {
                    directorypath = Path.GetDirectoryName(directorypath);   //Will trim trailing /\
                    if (String.IsNullOrWhiteSpace(directorypath))
                        break;
                    FullList.Add(Path.GetFileName(directorypath));
                }
                else
                {
                    if (String.IsNullOrWhiteSpace(directorypath))
                        break;
                    FullList.Add(Path.GetFileName(directorypath));
                    directorypath = Path.GetDirectoryName(directorypath);
                }

            }
            FullList.Reverse();
            return FullList;
        }

    }
}
