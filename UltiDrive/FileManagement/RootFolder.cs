using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UltiDrive.FileManagement;

namespace FileManagement
{
    public class RootFolder
    {
        public string RootFolderName;
        public List<FileInfo> includedFiles;
        public List<DirectoryInfo> subFolders;

        public bool HasFiles()
        {
            return includedFiles.Count > 0;
        }
        public bool HasSubFolders()
        {
            return subFolders.Count > 0;
        }

        public RootFolder(string location)
        {
            if (location != "")
            {
                RootFolderName = location;
                includedFiles = new List<FileInfo>();
                subFolders = new List<DirectoryInfo>();

                DirectoryInfo thisDir = new DirectoryInfo(location);

                BuildDirectories(thisDir);
                foreach (DirectoryInfo dir in subFolders)
                {
                    BuildFiles(dir);
                }
            }
        }

        public void BuildDirectories(DirectoryInfo curDir)
        {
            DirectoryInfo info = curDir;
            DirectoryInfo[] dirs = info.GetDirectories();
            foreach (DirectoryInfo dir in dirs)
            {
                //Console.WriteLine(dir.FullName);
                subFolders.Add(dir);
                if (dir.GetDirectories().Count() > 0)
                {
                    BuildDirectories(dir);
                }
            }
        }

        public void BuildFiles(DirectoryInfo curDir)
        {
            UltiDrive.indexEntities db = new UltiDrive.indexEntities();

            FileInfo[] files = curDir.GetFiles();
            foreach (FileInfo file in files)
            {
                //Console.WriteLine(file.FullName);
                try
                {
                    string service = Enum.GetName(typeof(StorageServices), FileStructure.algo.SortingHat(file));

                    UltiDrive.file newFile = new UltiDrive.file()
                    {
                        guid = Guid.NewGuid().ToString(),
                        lastModified = file.LastWriteTime,
                        origFileName = file.Name,
                        relativeFilePath = file.FullName.Replace(this.RootFolderName, ""),
                        rootFolder = this.RootFolderName,
                        service = service,
                    };

                    db.files.Add(newFile);
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    FileStructure.Index.UnManagedFiles.Add(file.FullName);
                }
            }
        }
    }
}
