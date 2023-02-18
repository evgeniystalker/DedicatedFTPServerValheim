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
        public List<FileModel> Files { get; set; } = new List<FileModel>();

        public DirectoryModel(string path)
        {
            PathDirectory = path;
        }
        public static List<FileModel> GetFilesInDirectoryRecursive(DirectoryModel dir)
        {
            List<FileModel> files = new List<FileModel>();
            files.AddRange(dir.Files);
            foreach (var recDir in dir.Directories)
            {
                files.AddRange(GetFilesInDirectoryRecursive(recDir));
            }

            return files;
        }
        public static List<string> GetDirectoryRecursive(DirectoryModel dir)
        {
            List<string> directories = new List<string>();
            directories.AddRange(dir.Directories.Select(x => x.PathDirectory));
            foreach (var recDir in dir.Directories)
            {
                directories.AddRange(GetDirectoryRecursive(recDir));
            }
            return directories;
        }
    }
    public struct FileModel
    {
        public string FileName; 
        public string FilePath;
        public DateTime DateTimeChangedFile;
        public FileModel(string fileName, string filePath, DateTime dateOfChanged)
        {
            FileName= fileName;
            FilePath= filePath;
            DateTimeChangedFile= dateOfChanged;
        }
    }
}
