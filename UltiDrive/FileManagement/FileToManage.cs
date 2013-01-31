using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManagement
{
    class FileToManage
    {
        public string rootDirectory;
        public string fullPath;
        public string fileName;
        public string GUID;
        public StorageServices service;

        public FileToManage(FileInfo info)
        {
            GUID = Guid.NewGuid().ToString();
            rootDirectory = info.DirectoryName;
            fileName = info.Name;
            fullPath = info.FullName;
            service = FileStructure.algo.SortingHat(info.Length, fullPath, GUID);
        }
    }
}
