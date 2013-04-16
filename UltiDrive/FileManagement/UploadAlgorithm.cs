using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltiDrive;
using UltiDrive.FileManagement;

namespace FileManagement
{
    public enum StorageServices { Box, SkyDrive, Dropbox, GoogleDrive, UbuntuOne, Empty };

    public class UploadAlgorithm
    {
        private static SortedSet<StorageInformation> sortedStorage;
        public SortedSet<StorageInformation> Services { get { return sortedStorage; } }
        public StorageInformation GetServiceStorageInformation(StorageServices service)
        {
            return sortedStorage.First(info => info.service == service);
        }

        public StorageServices SortingHat(FileInfo file)
        {
            try
            {
                StorageInformation info = sortedStorage.First(val => val.storageLeft > file.Length);

                if (info.service != StorageServices.Empty)
                    info.storageLeft -= file.Length;
                return info.service;
            }
            // Occurs if there is not ANY storage left
            catch (InvalidOperationException)
            {
                return StorageServices.Empty;
            }
        }
        public UploadAlgorithm(List<StorageInformation> storageInformation)
        {
            sortedStorage = new SortedSet<StorageInformation>(storageInformation);
            sortedStorage.Add(new StorageInformation(StorageServices.Empty, 0, long.MaxValue));
        }

        public bool AddService(StorageServices service)
        {
            if (Services.Count(s => s.service == service) > 0)
                return false;

            long storageUsed = 0;
            long storageTotal = 0;

            switch (service)
            {
                case StorageServices.SkyDrive:
                    UltiDrive.SkyDrive.Properties.Quota qt = UltiDrive.SkyDrive.Api.GetQuota();
                    storageUsed = qt.quota - qt.available;
                    storageTotal = qt.quota;
                    break;
                case StorageServices.GoogleDrive:
                    UltiDrive.GoogleDrive.Quota q = UltiDrive.GoogleDrive.Api.GetQuota();
                    storageUsed = q.quotaBytesUsed;
                    storageTotal = q.quotaBytesTotal;
                    break;
                case StorageServices.Dropbox:
                    UltiDrive.Dropbox.Api.Account acc = UltiDrive.Dropbox.Api.DropboxApi.Api.GetAccountInfo();
                    storageUsed = acc.Quota.Normal + acc.Quota.Shared;
                    storageTotal = acc.Quota.Total;
                    break;
                case StorageServices.Box:
                    UltiDrive.Box.Quota qu = UltiDrive.Box.Api.GetQuota();
                    storageUsed = qu.space_used;
                    storageTotal = qu.space_amount;
                    break;
            }

            Services.Add(new StorageInformation(service, storageUsed, storageTotal));

            return true;
        }
        public bool RemoveService(StorageServices service)
        {
            if (Services.Count(s => s.service == service) > 0)
            {
                Services.Remove(Services.Single(s => s.service == service));
            }

            indexEntities db = new indexEntities();
            foreach (file f in db.files)
            {
                StorageServices s = App.FolderStructure.algo.SortingHat(new FileInfo(f.fullpath));
                string message = Unity.MoveService(f, s);
                if (message != "Failure")
                {
                    f.serviceFileId = message;
                    f.service = Enum.GetName(typeof(StorageServices), s);
                }
                else
                {
                    db.unmanagedFiles.Add(new unmanagedFile() { filePath = f.fullpath, rootFolder = f.rootFolder });
                    db.files.Remove(f);
                }
            }

            db.SaveChanges();

            return false;
        }
    }

    public class StorageInformation : IComparable<StorageInformation>
    {
        public StorageServices service;
        public long storageLeft;
        public long storageTotal;

        public StorageInformation(StorageServices service, long storageUsed, long storageTotal)
        {
            this.service = service;
            this.storageLeft = storageTotal - storageUsed;
            this.storageTotal = storageTotal;
        }


        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (typeof(StorageInformation) == obj.GetType())
            {
                StorageInformation info = (StorageInformation)obj;
                if (info.service == this.service)
                    return true;
                return false;
            }
            else
                return false;
        }
        public int CompareTo(StorageInformation obj)
        {
            if (this < obj)
                return -1;
            else if (this == obj)
                return 0;
            else
                return 1;
        }
        public static bool operator <(StorageInformation obj1, StorageInformation obj2)
        {
            return (!obj1.Equals(obj2) && obj1.service == obj2.service && obj1.storageLeft < obj2.storageLeft);
        }
        public static bool operator >(StorageInformation obj1, StorageInformation obj2)
        {
            return (!obj1.Equals(obj2) && obj1.service == obj2.service && obj1.storageLeft > obj2.storageLeft);
        }
    }
}
