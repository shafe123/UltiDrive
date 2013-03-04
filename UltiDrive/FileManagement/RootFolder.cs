using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
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

        public bool HasSubFolders()
        {
            return subFolders.Count > 0;
        }

        public RootFolder(string location)
        {
            if (location != "")
            {
                RootFolderName = location;
                subFolders = new List<DirectoryInfo>();

                DirectoryInfo thisDir = new DirectoryInfo(location);
                subFolders.Add(thisDir);

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
                    if (db.files.Where(f => f.rootFolder + f.relativeFilePath == file.FullName).Count() == 0)
                    {
                        string service = Enum.GetName(typeof(StorageServices), FileStructure.Index.algo.SortingHat(file));

                        UltiDrive.file newFile = new UltiDrive.file()
                        {
                            lastModified = file.LastWriteTime,
                            origFileName = file.Name,
                            relativeFilePath = file.FullName.Replace(this.RootFolderName, ""),
                            rootFolder = this.RootFolderName,
                            service = service,
                        };


                        Exception error = null;
                        int count = 0;
                        do
                        {
                            error = null;
                            newFile.guid = Guid.NewGuid().ToString();
                            try
                            {
                                db.files.Add(newFile);
                                db.SaveChanges();
                            }
                            catch (Exception e)
                            {
                                IEnumerable<DbEntityValidationResult> results = db.GetValidationErrors();

                                error = e;
                                count++;
                                if (count > 2)
                                {
                                    FileStructure.Index.UnManagedFiles.Add(file.FullName);
                                    break;
                                }
                            }
                        } while (error != null);
                    }
                }
                catch (Exception e)
                {
                    FileStructure.Index.UnManagedFiles.Add(file.FullName);
                }
            }
        }
    }
}
