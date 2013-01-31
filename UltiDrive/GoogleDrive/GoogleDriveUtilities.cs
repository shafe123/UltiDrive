using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RestSharp.Serializers;

namespace UltiDrive.GoogleDrive
{
    class Utilities
    {
        public static void WriteDriveSettings()
        {
            FileStream file = GetJSONFile();

            JsonSerializer ser = new JsonSerializer();
            string values = ser.Serialize(GoogleDriveRefreshInfo.Instance);

            file.Write(Encoding.ASCII.GetBytes(values), 0, Encoding.ASCII.GetByteCount(values));
            file.Close();
        }

        public static FileStream GetJSONFile()
        {
            return System.IO.File.Open(JSONFile, FileMode.OpenOrCreate);
        }

        public static string JSONFile
        {
            get
            {
                return App.AppFolder + "\\GoogleDrive.json";
            }
        }

        public static FileIds fileIds;

        public class FileIds
        {
            public List<File> items { get; set; }
            public string nextPageToken { get; set; }
        }
    }
}
