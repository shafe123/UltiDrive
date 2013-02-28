using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltiDrive.FileManagement;

namespace FileManagement
{
    public enum StorageServices { Box, SkyDrive, Dropbox, GoogleDrive, UbuntuOne, Empty };

    public class UploadAlgorithm
    {
        private static SortedSet<StorageInformation> sortedStorage;
        public static SortedSet<StorageInformation> Services { get { return sortedStorage; } }
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
