using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EWR.ServerBackup.Library
{
    public static class IOHelper
    {
        public static void DoRetryIO(Action a)
        {
            int retries = 5;
            int i = 0;
            while (true)
            {
                try
                {
                    a();
                    break;
                }
                catch (System.IO.IOException)
                {
                    if (i >= retries)
                        throw;
                    System.Threading.Thread.Sleep(50 * i);
                    i++;
                }
            }
        }
    }
}
