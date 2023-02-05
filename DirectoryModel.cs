using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DedicatedFTPServerValheim
{
    internal class DirectoryModel
    {
        public string PathDirectory { get; }
        public List<DirectoryModel> Directories { get; set; } = new List<DirectoryModel>();
        public List<string> Files { get; set; } = new List<string>();

        public DirectoryModel(string path)
        {
            PathDirectory = path;
        }
        public static List<string> GetFilesInDirectoryRecursive(DirectoryModel dir)
        {
            List<string> files = new List<string>();
            files.AddRange(dir.Files);
            foreach (var recDir in dir.Directories)
            {
                files.AddRange(GetFilesInDirectoryRecursive(recDir));
            }
            
            return files;
        }
    }
}
