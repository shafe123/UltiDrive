using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mjollnir.Live;
using UltiDrive.Dropbox.Api;
using FileManagement;
using System.Data.SqlServerCe;
using System.Windows.Forms;

namespace UltiDrive.FileManagement
{
    public enum FileOperation { Delete, Rename, Update, Upload, Download }

    public class Unity
    {
        public static bool DeleteFile(string fileName)
        {
            try
            {
                indexEntities db = new indexEntities();
                var request = (from f in db.files
                               where f.origFileName == fileName
                               select f);
                file theFile = request.First();
                string service = theFile.service;
                string guid = theFile.guid;
                db.files.Remove(theFile);
                db.SaveChanges();

                if (service == StorageServices.Dropbox.ToString())
                {
                    throw new NotImplementedException();
                }
                else if (service == StorageServices.GoogleDrive.ToString())
                {
                    //Google Drive Upload code
                    throw new NotImplementedException();
                }
                else if (service == StorageServices.SkyDrive.ToString())
                {
                    SkyDrive.Api.DeleteFile(guid);
                }
                else if (service == StorageServices.UbuntuOne.ToString())
                {
                    //UbuntuOne Upload code
                    throw new NotImplementedException();
                }
                else if (service == StorageServices.Box.ToString())
                {
                    throw new NotImplementedException();
                }
                else
                {
                    Console.WriteLine("WTF did you do..?!");
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        public static bool RenameFile(string oldFileName, string newFileName)
        {
            try
            {
                indexEntities db = new indexEntities();

                DateTime newLastModified = System.IO.File.GetLastWriteTime(newFileName);

                var request = (from f in db.files 
                               where f.origFileName == oldFileName 
                               select f);

                file theFile = request.First();

                theFile.lastModified = newLastModified;
                theFile.origFileName = newFileName;

                db.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        public static bool UpdateFile(string filePath)
        {
            string[] paths = filePath.Split('\\');
            string filename = paths.Last();

            indexEntities db = new indexEntities();
            file uploadFile = (from f in db.files
                               where (f.rootFolder + f.relativeFilePath) == filePath
                               select f).First();

            bool result = true;
            try
            {
                switch (uploadFile.service)
                {
                    case "Dropbox":
                        var file = DropboxApi.Api.UploadFile("sandbox", uploadFile.guid, filePath);
                        break;
                    case "GoogleDrive":
                        throw new NotImplementedException();
                    case "SkyDrive":
                        SkyDrive.Api.UploadFile(uploadFile.guid, filePath);
                        break;
                    case "UbuntuOne":
                        throw new NotImplementedException();
                    case "Box":
                        throw new NotImplementedException();
                    default:
                        throw new Exception("how'd you manage that one?");
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        public static string UploadFile(string guid, string filePath, StorageServices service)
        {
            indexEntities db = new indexEntities();
            string rootFolder = FileStructure.Index.IndexRoots.Where(root => filePath.Contains(root.RootFolderName))
                                                                .First().RootFolderName;
            string serviceName = Enum.GetName(typeof(StorageServices), service);

            file newFile = new file()
            {
                origFileName = filePath,
                service = serviceName,
                lastModified = System.IO.File.GetLastWriteTime(filePath),
                rootFolder = rootFolder,
                relativeFilePath = filePath.Replace(rootFolder, ""),
            };

            string[] paths = filePath.Split('\\');

            try
            {
                switch (service)
                {
                    case StorageServices.Dropbox:
                        Dropbox.Api.FileSystemInfo file = DropboxApi.Api.UploadFile("sandbox", guid, filePath);
                        return file.Path;
                    case StorageServices.GoogleDrive:
                        var gfile = GoogleDrive.Api.UploadFile(guid, filePath);
                        break;
                    case StorageServices.SkyDrive:
                        SkyDrive.Api.UploadFile(guid, filePath);
                        break;
                    case StorageServices.UbuntuOne:
                        throw new NotImplementedException();
                    case StorageServices.Box:
                        throw new NotImplementedException();
                    default:
                        throw new Exception("how'd you manage that one?");

                }
                return "Test";
            }
            catch (Exception)
            {
                return "Failure";
            }
        }

        public static bool DownloadFile(string fileName, string downloadLocation)
        {
            //Aaron, look at this, much easier to read/write
            string guid = "";
            string serviceStr = "";
            indexEntities db = new indexEntities();
            file dbFile = (from f in db.files
                         where f.origFileName == fileName
                         select f).First();
            
            guid = dbFile.guid;
            serviceStr = dbFile.service;

            bool result = true;
            StorageServices service = Utilities.GetStorageService(serviceStr);
            if (guid != "" && service != StorageServices.Empty)
            {
                try
                {
                    switch (service)
                    {
                        case StorageServices.Dropbox:
                            var file = DropboxApi.Api.DownloadFile("sandbox", guid);
                            file.Save(dbFile.origFileName);
                            break;
                        case StorageServices.GoogleDrive:
                            throw new NotImplementedException();
                        case StorageServices.SkyDrive:
                            SkyDrive.Api.DownloadFile(guid);
                            break;
                        case StorageServices.UbuntuOne:
                            throw new NotImplementedException();
                        case StorageServices.Box:
                            throw new NotImplementedException();
                        default:
                            throw new Exception("how'd you manage that one?");
                    }
                }
                catch (Exception)
                {
                    result = false;
                }
            }
            else
            {
                MessageBox.Show("GUID does not exist!");
                return false;
            }

            return result;
        }

        private static void UpdateRemoteIndex(file file, FileOperation op)
        {

        }
    }
}
