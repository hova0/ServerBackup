using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServerBackupTest
{
    [TestClass]
    public class CommandRunnerTests
    {
        [TestMethod]
        public void CommandRunnerSimpleTest()
        {
            ServerBackup.CommandRunner cr = new ServerBackup.CommandRunner(new StubLogger());
            cr.Threads = 5;
            DateTime starttime = DateTime.Now;

            SetupTestDirectory("CommandRunnerSimpleTest");
            Console.WriteLine("Took {0} to set up test directory", DateTime.Now.Subtract(starttime).TotalMilliseconds);
            ServerBackup.FileSelector fs = new ServerBackup.FileSelector(@"C:\temp\CommandRunnerSimpleTestdir1\");
            fs.IncludeMatchers.Add(new ServerBackup.FileMaskMatcher(".*"));
            Assert.IsTrue(fs.FileList().Count() == 50);

            CleanupTestDirectory("CommandRunnerSimpleTest");

        }
        [TestMethod]
        public void CommandRunnerGetDestinationFileNameTest()
        {
            ServerBackup.CommandRunner cr = new ServerBackup.CommandRunner(new StubLogger());
            Assert.IsTrue(cr.GetDestinationFileName(
                @"C:\temp\dir1\test1.txt",
                @"C:\temp\dir1\",
                @"C:\temp\dir2\"
                ) == @"C:\temp\dir2\test1.txt");
            Assert.IsTrue(cr.GetDestinationFileName(
    @"C:\temp\dir1\test1.txt",
    @"C:\temp\dir1",
    @"C:\temp\dir2"
    ) == @"C:\temp\dir2\test1.txt");
            Assert.IsTrue(cr.GetDestinationFileName(
              @"\\172.21.1.43\e\newprogram\EWR\Dirwatcher\test1.txt",
              @"\\172.21.1.43\e\newprogram\",
              @"\\172.21.1.44\Archive\EWRBackup\"
              ) == @"\\172.21.1.44\Archive\EWRBackup\EWR\Dirwatcher\test1.txt");
            Assert.IsTrue(cr.GetDestinationFileName(
              @"\\172.21.1.43\e\newprogram\EWR\Dirwatcher\test1.txt",
              @"\\172.21.1.43\e\newprogram",
              @"\\172.21.1.44\Archive\EWRBackup\"
              ) == @"\\172.21.1.44\Archive\EWRBackup\EWR\Dirwatcher\test1.txt");

        }


        private void SetupTestDirectory(string prefix)
        {
            if (!System.IO.Directory.Exists(@"C:\temp\" + prefix + @"dir1\"))
                System.IO.Directory.CreateDirectory(@"C:\temp\" + prefix + @"dir1");
            if (!System.IO.Directory.Exists(@"C:\temp\" + prefix + @"dir2\"))
                System.IO.Directory.CreateDirectory(@"C:\temp\" + prefix + @"dir2");
            for (int i = 0; i < 50; i++)
            {
                System.IO.File.WriteAllLines(@"C:\temp\" + prefix +@"dir1\test" + i.ToString() + ".txt", new string[] { "File " + i.ToString(), "." });
            }
        }

        private void CleanupTestDirectory(string prefix)
        {
            System.IO.Directory.Delete(@"C:\temp\" + prefix + @"dir1\", true);
            System.IO.Directory.Delete(@"C:\temp\" + prefix + @"dir2\", true);
        }

        [TestMethod]
        public void CommandRunnerSchedulingOneThreadTest()
        {
            StubCommand s = new StubCommand();
            ServerBackup.CommandRunner cr = new ServerBackup.CommandRunner(new StubLogger());
            cr.BaseDirectory = @"C:\temp\";
            cr.DestinationDirectory = @"C:\temp\dir2\";
            ServerBackup.FileSelector fs = new ServerBackup.FileSelector(@"C:\temp\");
            fs.IncludeMatchers.Add(new ServerBackup.FileMaskMatcher(".*"));
            System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
            cr.RunCommands(s, fs, cts.Token);
        }

        [TestMethod]
        public void CommandRunnerSchedulingMultipleThreadsTest()
        {
            StubCommand s = new StubCommand();
            ServerBackup.CommandRunner cr = new ServerBackup.CommandRunner(new StubLogger());
            SetupTestDirectory("CommandRunnerSchedulingMultipleThreadsTest");
            cr.BaseDirectory = @"C:\temp\CommandRunnerSchedulingMultipleThreadsTestdir1\";
            cr.DestinationDirectory = @"C:\temp\CommandRunnerSchedulingMultipleThreadsTestdir2\";
            ServerBackup.FileSelector fs = new ServerBackup.FileSelector(@"C:\temp\CommandRunnerSchedulingMultipleThreadsTestdir1\");
            fs.IncludeMatchers.Add(new ServerBackup.FileMaskMatcher(".*"));
            fs.Recurse = true;
            cr.Threads = 25;
            System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
            cr.RunCommands(s, fs, cts.Token);

            Console.WriteLine("Successfull Files:");
            cr.SuccessfulFiles.ForEach(x => Console.WriteLine(x));
            Assert.IsTrue(cr.SuccessfulFiles.Count == 50);
            Console.WriteLine("Failed Files:");
            cr.FailedFiles.ForEach(x => Console.WriteLine(x));
            Assert.IsTrue(cr.FailedFiles.Count == 0);
            CleanupTestDirectory("CommandRunnerSchedulingMultipleThreadsTest");
        }

        [TestMethod]
        public void CommandRunnerFailureThreadsTest()
        {
            FailureCommand s = new FailureCommand();
            ServerBackup.CommandRunner cr = new ServerBackup.CommandRunner(new StubLogger());
            SetupTestDirectory("CommandRunnerFailureThreadsTest");
            cr.BaseDirectory = @"C:\temp\CommandRunnerFailureThreadsTestdir1\";
            cr.DestinationDirectory = @"C:\temp\CommandRunnerFailureThreadsTestdir2\";
            ServerBackup.FileSelector fs = new ServerBackup.FileSelector(@"C:\temp\CommandRunnerFailureThreadsTestdir1\");
            fs.IncludeMatchers.Add(new ServerBackup.FileMaskMatcher(".*"));
            fs.Recurse = true;
            cr.Threads = 5;
            System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
            cr.RunCommands(s, fs, cts.Token);

            Console.WriteLine("Successfull Files: {0} ", cr.SuccessfulFiles.Count);
            cr.SuccessfulFiles.ForEach(x => Console.WriteLine(x));
            Assert.IsTrue(cr.SuccessfulFiles.Count == 0);
            Console.WriteLine("Failed Files: {0} ", cr.FailedFiles.Count);
            cr.FailedFiles.ForEach(x => Console.WriteLine(x));
            Assert.IsTrue(cr.FailedFiles.Count < 50);
            CleanupTestDirectory("CommandRunnerFailureThreadsTest");
        }


        [TestMethod]
        public void CommandRunnerCancellationThreadsTest()
        {
            StubCommand2 s = new StubCommand2(100);
            ServerBackup.CommandRunner cr = new ServerBackup.CommandRunner(new StubLogger());
            SetupTestDirectory("CommandRunnerCancellationThreadsTest");
            cr.BaseDirectory = @"C:\temp\CommandRunnerCancellationThreadsTestdir1\";
            cr.DestinationDirectory = @"C:\temp\CommandRunnerCancellationThreadsTestdir2\";
            ServerBackup.FileSelector fs = new ServerBackup.FileSelector(@"C:\temp\CommandRunnerCancellationThreadsTestdir1\");
            fs.IncludeMatchers.Add(new ServerBackup.FileMaskMatcher(".*"));
            fs.Recurse = true;
            cr.Threads = 5;
            System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
            
            Task t = Task.Run(() =>  cr.RunCommands(s, fs, cts.Token) );
            cts.CancelAfter(10000);
            //cts.Cancel();
            t.Wait();

            Console.WriteLine("Successfull Files: {0} ", cr.SuccessfulFiles.Count);
            cr.SuccessfulFiles.ForEach(x => Console.WriteLine(x));
            //Assert.IsTrue(cr.SuccessfulFiles.Count != 0 );
            Console.WriteLine("Failed Files: {0} ", cr.FailedFiles.Count);
            cr.FailedFiles.ForEach(x => Console.WriteLine(x));
            //Assert.IsTrue(cr.FailedFiles.Count != 50);
            CleanupTestDirectory("CommandRunnerCancellationThreadsTest");
        }


        public class StubCommand : ServerBackup.ICommand
        {
            public String CommandResult { get; set; }
            public bool Simulate { get; set; }
            public void Initialize() { }
            public void Close() { }
            public bool ContinueOnError { get; set; }
            public Boolean Run(String sourcefilename, String destinationfilename)
            {
                int delayms = Math.Abs(sourcefilename.GetHashCode() % 100);
                //Console.WriteLine("{3}\t{0} ==>> {1} ", sourcefilename, destinationfilename, System.Threading.Thread.CurrentThread.ManagedThreadId);
                System.Threading.Thread.Sleep(delayms); // Simulate IO
                return true;
            }

            public Task<Boolean> RunAsync(String sourcefilename, String destinationfilename, System.Threading.CancellationToken cts)
            {
                Task<bool> t = new Task<bool> (() => Run(sourcefilename, destinationfilename));
                return t;
            }
        }

        public class StubCommand2 : ServerBackup.ICommand
        {
            public String CommandResult { get; set; }
            private int delayms = 0;
            public bool Simulate { get; set; }
            public void Initialize() { }
            public void Close() { }
            public bool ContinueOnError { get; set; }
            public StubCommand2(int delay)
            {
                delayms = delay;
            }
            public Boolean Run(String sourcefilename, String destinationfilename)
            {
                System.Threading.Thread.Sleep(delayms); // Simulate IO
                return true;
            }

            public Task<Boolean> RunAsync(String sourcefilename, String destinationfilename, System.Threading.CancellationToken cts)
            {
                if (cts.IsCancellationRequested)
                    return new Task<bool>(() => false);
                Task<bool> t = new Task<bool>(() => Run(sourcefilename, destinationfilename));
                return t;
            }
        }

        public class FailureCommand : ServerBackup.ICommand
        {
            public String CommandResult { get; set; }
            public bool Simulate { get; set; }
            public void Initialize() { }
            public void Close() { }
            public bool ContinueOnError { get; set; }
            public Boolean Run(String sourcefilename, String destinationfilename)
            {
                throw new Exception("Failure");
            }

            public Task<Boolean> RunAsync(String sourcefilename, String destinationfilename, System.Threading.CancellationToken cts)
            {
                Task<bool> t = new Task<bool>(() => Run(sourcefilename, destinationfilename));
                return t;
            }
        }


    }
}
