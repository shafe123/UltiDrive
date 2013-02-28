using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using UltiDrive;
using UltiDrive.FileManagement;

namespace FileManagement
{
    public class FileStructure
    {
        public static UploadAlgorithm algo;
        public List<String> UnManagedFiles;
        public List<RootFolder> IndexRoots;
        private FileStructure() { }
        private static FileStructure _Index;
        
        public static FileStructure Index
        {
            get
            {
                if (_Index == null)
                    _Index = new FileStructure();
                return _Index;
            }
        }
        public FileStructure(String[] rootFolders, List<StorageInformation> info)
        {
            if (_Index == null)
                _Index = new FileStructure();
            _Index.IndexRoots = new List<RootFolder>();
            _Index.UnManagedFiles = new List<string>();

            algo = new UploadAlgorithm(info);
            _Index.InstantiateIndex(rootFolders.Count(), rootFolders.ToList<String>());
        }
        public FileStructure(List<String> rootFolders, List<StorageInformation> info)
        {
            if (_Index == null)
                _Index = new FileStructure();
            _Index.IndexRoots = new List<RootFolder>();
            _Index.UnManagedFiles = new List<string>();

            algo = new UploadAlgorithm(info);
            _Index.InstantiateIndex(rootFolders.Count, rootFolders);
        }

        private void InstantiateIndex(int count, List<String> roots)
        {
            RemoveDuplicates(ref roots);

            Task[] taskArray = new Task[count];
            for (int i = 0; i < count; i++)
            {
                int value = i;
                taskArray[i] = new Task(() => StartRoot(value, roots), TaskCreationOptions.AttachedToParent);
                taskArray[i].Start();
            }
            Task.WaitAll(taskArray);

            taskArray = new Task[8];
            indexEntities db = new indexEntities();
            List<file> files = db.files.ToList();
            UploadFiles(0, files);

            //int sections = files.Count / 8;

            //for (int i = 0; i < 8; i++)
            //{
            //    int ival = i;
            //    taskArray[ival] = new Task(() => UploadFiles(ival, files), TaskCreationOptions.AttachedToParent);
            //}
            //Task.WaitAll(taskArray);
        }
        private void StartRoot(int iValue, List<String> roots)
        {
            IndexRoots.Add(new RootFolder(roots[iValue]));
        }
        private void RemoveDuplicates(ref List<String> rootFolders)
        {
            List<string> toRemove = new List<string>();
            foreach (string firstfolder in rootFolders)
            {
                foreach (string secondfolder in rootFolders)
                {
                    if (firstfolder.Contains(secondfolder) && firstfolder != secondfolder)
                        toRemove.Add(firstfolder);
                }
            }
            foreach (string removal in toRemove)
                rootFolders.Remove(removal);
        }
        private void UploadFiles(int ival, List<file> files)
        {
            indexEntities db = new indexEntities();

            //if (ival == 7)
            //    files.RemoveRange(0, ival * files.Count / 8);
            //else
            //    files = files.GetRange(ival * files.Count / 8, files.Count / 8);

            foreach (file file in files)
            {
                StorageServices service = (StorageServices)Enum.Parse(typeof(StorageServices), file.service);
                file.serviceFileId = Unity.UploadFile(file);
                
                db.files.Single(f => f.guid == file.guid).serviceFileId = file.serviceFileId;
            }
            db.SaveChanges();
        }
    }
}
