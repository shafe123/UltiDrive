﻿using System;
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
        public UploadAlgorithm algo;
        public UltiDriveSystemWatcher watcher;
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

            _Index.algo = new UploadAlgorithm(info);
            _Index.InstantiateIndex(rootFolders.ToList<String>());
        }
        public FileStructure(List<String> rootFolders, List<StorageInformation> info)
        {
            if (_Index == null)
                _Index = new FileStructure();
            _Index.IndexRoots = new List<RootFolder>();

            _Index.algo = new UploadAlgorithm(info);
            _Index.InstantiateIndex(rootFolders);
        }
        public void AddRootFolder(string dirPath)
        {
            List<string> roots = new List<string>();
            foreach (RootFolder rf in _Index.IndexRoots)
                roots.Add(rf.RootFolderName);

            foreach (string root in roots)
            {
                if (dirPath.Contains(root))
                    return;
            }

            _Index.IndexRoots.Add(new RootFolder(dirPath));
            _Index.watcher.AddWatcher(dirPath);

            indexEntities db = new indexEntities();
            List<file> files = db.files.Where(f => f.rootFolder == dirPath).ToList();
            UpdateFiles(files);

            StringWriter stringWriter = new StringWriter(new System.Text.StringBuilder());
            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(List<string>));
            writer.Serialize(stringWriter, roots);
            System.IO.File.WriteAllText(App.AppFolder + "\\Watcher.dat", stringWriter.ToString());
        }
        public void RemoveRootFolder(string dirPath)
        {
            indexEntities db = new indexEntities();
            List<file> files = db.files.Where(f => f.rootFolder == dirPath).ToList();
            foreach (file fi in files)
            {
                Unity.DeleteFile(fi.fullpath);
            }

            _Index.IndexRoots.Remove(_Index.IndexRoots.Single(rf => rf.RootFolderName == dirPath));
            _Index.watcher.RemoveWatcher(dirPath);
        }

        private void InstantiateIndex(List<String> roots)
        {
            int count = roots.Count;

            RemoveDuplicates(ref roots);
            StringWriter stringWriter = new StringWriter(new System.Text.StringBuilder());
            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(List<string>));
            writer.Serialize(stringWriter, roots);
            System.IO.File.WriteAllText(App.AppFolder + "\\Watcher.dat", stringWriter.ToString());

            Task[] taskArray = new Task[count];
            for (int i = 0; i < count; i++)
            {
                int value = i;
                taskArray[i] = new Task(() => StartRoot(value, roots), TaskCreationOptions.AttachedToParent);
                taskArray[i].Start();
            }
            Task.WaitAll(taskArray);

            indexEntities db = new indexEntities();
            List<file> files = db.files.ToList();
            _Index.watcher = new UltiDriveSystemWatcher(roots);
            UpdateFiles(files);

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
        private void UpdateFiles(List<file> files)
        {
            indexEntities db = new indexEntities();
            foreach (file file in files)
            {
                if (file.serviceFileId != null)
                {
                    FileInfo fileInfo = new FileInfo(file.fullpath);

                    if (fileInfo.LastWriteTime.Date.Ticks > file.lastModified.Date.Ticks)
                    {
                        Unity.UpdateFile(file.fullpath);
                    }
                    db.files.Single(f => f.guid == file.guid).lastModified = fileInfo.LastWriteTime;
                }
                else
                {
                    file.serviceFileId = Unity.UploadFile(file);
                }
                
                db.files.Single(f => f.guid == file.guid).serviceFileId = file.serviceFileId;

                if (db.ChangeTracker.Entries().Count() > 500)
                    db.SaveChanges();
            }
            db.SaveChanges();
        }
    }
}
