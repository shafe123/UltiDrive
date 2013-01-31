using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileManagement
{
    class Folder
    {
        public string name;
        public string FolderLocation;
        public List<FileToManage> includedFiles;
        public List<Folder> subFolders;

        public bool HasFiles()
        {
            return includedFiles.Count > 0;
        }
        public bool HasSubFolders()
        {
            return subFolders.Count > 0;
        }

        public Folder(string location)
        {
            if (location != "")
            {
                FolderLocation = location;
                name = location.Substring(location.LastIndexOf("\\"));
                includedFiles = new List<FileToManage>();
                subFolders = new List<Folder>();

                DirectoryInfo thisDir = new DirectoryInfo(location);

                BuildFiles(thisDir);
                BuildDirectories(thisDir);
            }
        }

        public void BuildFiles(DirectoryInfo curDir)
        {
            DirectoryInfo info = curDir;
            foreach (FileInfo file in info.GetFiles())
            {
                //Console.WriteLine(file.FullName);
                try
                {
                    FileToManage ftm = new FileToManage(file);
                    if (ftm.service == StorageServices.Empty)
                        FileStructure.Index.UnManagedFiles.Add(file.FullName);
                    else
                        includedFiles.Add(ftm);
                }
                catch (Exception e)
                {
                    FileStructure.Index.UnManagedFiles.Add(file.FullName);
                }
            }
        }

        public void BuildDirectories(DirectoryInfo curDir)
        {
            DirectoryInfo info = curDir;
            foreach (DirectoryInfo dir in info.GetDirectories())
            {
                //Console.WriteLine(dir.FullName);
                Folder current = new Folder(dir.FullName);
                subFolders.Add(current);
            }
        }
    }
}
