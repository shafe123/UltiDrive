using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManagement
{
    public class FileToManage
    {
        public string rootDirectory;
        public string fullPath;
        public string fileName;
        public string GUID;
        public long fileSize;
        public StorageServices service;

        public FileToManage(FileInfo info, string root)
        {
            GUID = Guid.NewGuid().ToString();
            rootDirectory = root;
            fileName = info.Name;
            fullPath = info.FullName;
            fileSize = info.Length;
            service = FileStructure.algo.SortingHat(this);


        }
    }
}
