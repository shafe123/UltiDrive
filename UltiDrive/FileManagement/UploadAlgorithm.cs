using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltiDrive.FileManagement;

namespace FileManagement
{
    public enum StorageServices { Box, SkyDrive, Dropbox, GoogleDrive, UbuntuOne, Empty };

    public class UploadAlgorithm
    {
        private SortedSet<StorageInformation> sortedStorage;
        public StorageInformation GetServiceStorageInformation(StorageServices service)
        {
            return sortedStorage.First(info => info.service == service);
        }

        public StorageServices SortingHat(long fileSize, string filePath, string guid)
        {
            try
            {
                StorageInformation info = sortedStorage.First(val => val.storageLeft > fileSize);

                if (!Unity.UploadFile(guid, filePath, info.service) && sortedStorage.Where(val => val.storageLeft > fileSize).Count() > 1)
                {
                    //try the next option if the first doesn't work.
                    info = sortedStorage.Where(val => val.storageLeft > fileSize).Skip(1).First();
                    Unity.UploadFile(guid, filePath, info.service);
                }

                if (info.service != StorageServices.Empty)
                    info.storageLeft -= fileSize;
                info.AddFileToList(filePath);
                return info.service;
            }
            // Occurs if there is not ANY storage left
            catch (InvalidOperationException e)
            { //throw new NotImplementedException("No Storage Available"); 
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
        private List<String> ManagedFiles;

        public StorageServices service;
        public long storageLeft;
        public long storageTotal;

        public StorageInformation(StorageServices service, long storageUsed, long storageTotal)
        {
            this.service = service;
            this.storageLeft = storageTotal - storageUsed;
            this.storageTotal = storageTotal;
            this.ManagedFiles = new List<string>();
        }

        public void AddFileToList(string filePath)
        {
            ManagedFiles.Add(filePath);
        }
        public List<String> GetManagedFiles()
        {
            return ManagedFiles;
        }
        public void RemoveFileFromList(string filePath)
        {
            ManagedFiles.Remove(filePath);
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
