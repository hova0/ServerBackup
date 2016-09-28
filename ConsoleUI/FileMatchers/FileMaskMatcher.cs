using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;
namespace ServerBackup
{
    /// <summary>
    /// Used to determine if a filename matches a specified regex
    /// </summary>
    public class FileMaskMatcher : IFileMatcher
    {
        private string _mask;
        public string Mask
        {
            get
            {
                return _mask;
            }
            private set
            {
                _mask = value;
                try
                {
                    regexmask = new System.Text.RegularExpressions.Regex(_mask, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                }
                catch (System.ArgumentException ae)
                {
                    _mask = null;
                    regexmask = null;
                    throw;
                }
            }
        }
        private System.Text.RegularExpressions.Regex regexmask;

        public FileMaskMatcher(string mask)
        {
            this.Mask = mask;
        }

        public bool IsMatch(string filename)
        {
            if (regexmask != null)
                return regexmask.IsMatch(filename);
            return false;
        }
        public bool IsMatch(System.IO.FileInfo filename)
        {
            if (regexmask != null)
                return regexmask.IsMatch(filename.FullName);
            return false;
        }

        public override String ToString()
        {
            string returnstring = "{ FileMaskMatcher { ";
            returnstring += " FileMask : " + Mask + " } } ";
            return returnstring;
        }


    }
}
