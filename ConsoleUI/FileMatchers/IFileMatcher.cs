using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerBackup
{
   public  interface IFileMatcher
    {
        bool IsMatch(string filename);
        bool IsMatch(System.IO.FileInfo file);

    }
}
