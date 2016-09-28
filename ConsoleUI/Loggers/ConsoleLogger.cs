using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerBackup.Loggers
{
    class ConsoleLogger : IInternalLogger
    {
        public void Error(String message, Exception e)
        {
           
            if (e != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[ERROR] ");
                Console.ResetColor();
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                SpamAggregateException(e, 0);
            } else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[ERROR] ");
                Console.ResetColor();
                Console.WriteLine(e.Message);
            }
        }

        public void Fatal(String message, Exception e)
        {
           
            if (e != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.Write("[FATAL] ");
                Console.ResetColor();
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                SpamAggregateException(e, 0);
            } else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.Write("[FATAL] ");
                Console.ResetColor();
                Console.WriteLine(e.Message);
            }


        }

        public void Warn(String message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("[WARN] ");
            Console.ResetColor();
            Console.WriteLine(message);
        }


        private void SpamAggregateException(Exception e, int depth)
        {
            string depthstring = "";
            if (depth > 0)
                depthstring = new string('>', depth);
            
            if (e is AggregateException)
            {
                AggregateException ae = (AggregateException)e;
                for (int i = 0; i < ae.InnerExceptions.Count; i++)
                {
                    Console.WriteLine(depthstring + ae.Message);
                    foreach (string s in ae.StackTrace.Split('\n'))
                        Console.WriteLine(depthstring + s);
                    if (ae.InnerException != null)
                        SpamAggregateException(ae.InnerException, ++depth);
                    Console.WriteLine("====================");
                }
            } else
            {
                Console.WriteLine(e.Message);
                foreach (string s in e.StackTrace.Split('\n'))
                    Console.WriteLine(depthstring + s);
                if (e.InnerException != null)
                    SpamAggregateException(e.InnerException, ++depth);
            }
        }

        public void Info(String message)
        {
            Console.Write("[INFO] ");
            Console.WriteLine(message);
        }

        public void Debug(string message)
        {
#if !DEBUG
            return;
#endif
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("[DEBUG] ");
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
