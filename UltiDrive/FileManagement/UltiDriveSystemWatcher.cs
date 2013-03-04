using FileManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UltiDrive.FileManagement
{
    public class UltiDriveSystemWatcher
    {
        private List<FileSystemWatcher> _watchers;
        private List<DirectoryInfo> _directories;

        public UltiDriveSystemWatcher(List<string> directories)
        {
            _watchers = new List<FileSystemWatcher>();
            _directories = new List<DirectoryInfo>();

            foreach (string directory in directories)
            {
                AddWatcher(directory);
            }
        }

        private void AddWatcher(string path)
        {
            FileSystemWatcher watcher = new FileSystemWatcher();

            watcher.Created += new FileSystemEventHandler(fileCreated);
            watcher.Changed += new FileSystemEventHandler(fileChanged);
            watcher.Deleted += new FileSystemEventHandler(fileDeleted);
            watcher.Renamed += new RenamedEventHandler(fileRenamed);

            watcher.Path = @path;
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;
            watcher.BeginInit();

            _watchers.Add(watcher);
            _directories.Add(new DirectoryInfo(path));

            WriteXML();
        }

        private void RemoveWatcher(string path)
        {
            _watchers.Remove(_watchers.Single(w => w.Path == path));
            _directories.Remove(_directories.Single(d => d.FullName == path));

            WriteXML();
        }

        private void WriteXML()
        {
            StringWriter stringWriter = new StringWriter(new System.Text.StringBuilder());
            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(List<string>));
            writer.Serialize(stringWriter, _directories);
            System.IO.File.WriteAllText(App.AppFolder + "\\Watcher.dat", stringWriter.ToString());
        }

        void fileRenamed(object sender, RenamedEventArgs e)
        {
            bool success = Unity.RenameFile(e.OldFullPath, e.FullPath);
        }

        void fileDeleted(object sender, FileSystemEventArgs e)
        {
            bool success = Unity.DeleteFile(e.FullPath);
        }

        void fileChanged(object sender, FileSystemEventArgs e)
        {
            bool success = Unity.UpdateFile(e.FullPath);
        }

        void fileCreated(object sender, FileSystemEventArgs e)
        {
            indexEntities db = new indexEntities();

            FileInfo info = new FileInfo(e.FullPath);
            file newFile = new file()
            {
                lastModified = info.LastWriteTime,
                origFileName = info.Name
            };

            foreach (RootFolder folder in FileStructure.Index.IndexRoots)
            {
                if (e.FullPath.Contains(folder.RootFolderName))
                {
                    newFile.rootFolder = folder.RootFolderName;
                    newFile.relativeFilePath = info.FullName.Replace(folder.RootFolderName, "");
                    newFile.service = Enum.GetName(typeof(StorageServices), FileStructure.Index.algo.SortingHat(info));
                    break;
                }
            }

            Exception error;
            do
            {
                error = null;
                newFile.guid = Guid.NewGuid().ToString();
                try
                {
                    newFile = db.files.Add(newFile);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            } while (error != null);

            newFile.serviceFileId = Unity.UploadFile(newFile);
            db.SaveChanges();

            System.Windows.Forms.MessageBox.Show("A file has been uploaded");
        }

    }
}
