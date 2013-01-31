using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using UltiDrive;

namespace FileManagement
{
    public class FileStructure
    {
        public static UploadAlgorithm algo;
        public List<String> UnManagedFiles;
        private List<String> rootFolders;
        private List<Folder> IndexRoots;
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
            _Index.rootFolders = rootFolders.ToList<String>();
            _Index.IndexRoots = new List<Folder>();
            _Index.UnManagedFiles = new List<string>();

            algo = new UploadAlgorithm(info);
            _Index.InstantiateIndex();
        }
        public FileStructure(List<String> rootFolders, List<StorageInformation> info)
        {
            if (_Index == null)
                _Index = new FileStructure();
            _Index.rootFolders = rootFolders;
            _Index.IndexRoots = new List<Folder>();
            _Index.UnManagedFiles = new List<string>();

            algo = new UploadAlgorithm(info);
            _Index.InstantiateIndex();
        }

        private void InstantiateIndex()
        {
            RemoveDuplicates();

            Task[] taskArray = new Task[rootFolders.Count];
            for (int i = 0; i < rootFolders.Count; i++)
            {
                int value = i;
                taskArray[i] = new Task(() => StartRoot(value), TaskCreationOptions.AttachedToParent);
                taskArray[i].Start();
            }
            Task.WaitAll(taskArray);

        }
        private void StartRoot(int iValue)
        {
            IndexRoots.Add(new Folder(rootFolders[iValue]));
        }
        private void RemoveDuplicates()
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

        public string PrintStructure()
        {
            StringBuilder sb = new StringBuilder();

            foreach (Folder folder in IndexRoots)
            {
                sb.AppendLine(RecurseStructure("", folder));
            }

            sb.AppendLine("Unmanaged files: ");
            sb.AppendLine("=========================");
            foreach (string file in UnManagedFiles)
                sb.AppendLine(file);

            return sb.ToString();
        }
        private string RecurseStructure(string tabs, Folder currentFolder)
        {
            StringBuilder sb = new StringBuilder();

            if (currentFolder.HasSubFolders())
            {
                foreach (Folder folder in currentFolder.subFolders)
                {
                    sb.AppendLine(tabs + currentFolder.name);
                    sb.AppendLine(tabs + "==============================");
                    sb.AppendLine(RecurseStructure(tabs+"\t", folder));
                }
            }
            if (currentFolder.HasFiles())
            {
                foreach (FileToManage file in currentFolder.includedFiles)
                {
                    sb.Append(tabs + file.fileName);
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }
    }
}
