using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
namespace ServerBackup
{
    /// <summary>
    /// Class used for determining the source list of files
    /// </summary>
    public class FileSelector
    {
        //public List<string> IncludeMask { get; set; }
        // public List<string> ExcludeMask { get; set; }

        public List<IFileMatcher> IncludeMatchers { get; set; }
        public List<IFileMatcher> ExcludeMatchers { get; set; }

        //public string Source { get; set; }

        public bool Recurse { get; set; }

        //private long accumulatedFileSize = 0;

        private System.IO.DirectoryInfo baseDirectory;

        public FileSelector(string basePath)
        {
            //IncludeMask = new List<string>();
            // ExcludeMask = new List<string>();
            IncludeMatchers = new List<IFileMatcher>();
            ExcludeMatchers = new List<IFileMatcher>();
            if (System.IO.Directory.Exists(basePath))
            {
                baseDirectory = new System.IO.DirectoryInfo(basePath);
            }
            else
            {
                throw new Exception("Directory does not exist or access denied.");
            }
        }

        public IEnumerable<FileInfo> FileList()
        {
            foreach (FileInfo fi in FileList(baseDirectory, Recurse))
                yield return fi;
        }
        //Recursion method
        public IEnumerable<FileInfo> FileList(DirectoryInfo basepath, bool recurse)
        {
            FileSystemInfo[] AllEntries = null;
            try
            {
                AllEntries = basepath.GetFileSystemInfos();
            }
            catch { }
            if (AllEntries == null)
                yield break;

            //Process Files
            foreach (FileSystemInfo f in AllEntries)
            {
                if (f is FileInfo)
                {
                    //Test File for inclusion / exclusion 
                    FileInfo actualfileinfo = f as FileInfo;
                    bool skipfile = false;
                    foreach (IFileMatcher exl in this.ExcludeMatchers)
                        if (exl.IsMatch(actualfileinfo))
                        {
                            skipfile = true;
                            break;
                        }
                    foreach (IFileMatcher incl in this.IncludeMatchers)
                        if (!incl.IsMatch(actualfileinfo))
                        {
                            skipfile = true;
                            break;
                        }
                    if (skipfile)
                        continue;
                    yield return f as FileInfo;
                }
                // This is a directory to recurse into it if needed.
                if ((f.Attributes & FileAttributes.Directory) == FileAttributes.Directory && recurse)
                {
                    foreach (FileInfo insidefileinfo in FileList(f as DirectoryInfo, recurse))
                        yield return insidefileinfo;
                    //yield return FileList(f.FullName, recurse);
                    continue;
                }
            }
        }


        public FileSelector DeepClone()
        {
            FileSelector fs = new ServerBackup.FileSelector(baseDirectory.FullName);
            foreach (IFileMatcher ix in IncludeMatchers)
            {
                fs.IncludeMatchers.Add(ix);
            }
            foreach (IFileMatcher ix in ExcludeMatchers)
            {
                fs.ExcludeMatchers.Add(ix);
            }
            fs.Recurse = this.Recurse;

            return fs;
        }


    }


}
