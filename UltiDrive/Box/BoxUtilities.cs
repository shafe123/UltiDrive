using RestSharp;
using RestSharp.Serializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UltiDrive.Box
{
    class Utilities
    {
        public static void WriteBoxSettings()
        {
            FileStream file = GetJSONFile();

            JsonSerializer ser = new JsonSerializer();
            string values = ser.Serialize(BoxProperties.refreshInfo);

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
                return App.AppFolder + "\\Box.json";
            }
        }

        public static void SetUpUltiDriveFolder()
        {
            //set up/find UltiDrive folder
            RestClient client = Box.BaseClients.BaseClient();
            RestRequest request = Box.BaseRequests.BaseRequest(Method.GET);
            request.Resource = "folders/0";

            Folder rootFolder = client.Execute<Box.Folder>(request).Data;
            long UltiDrive = 0;
            try
            {
                UltiDrive = (from f in rootFolder.item_collection.entries
                             where f.name == "UltiDrive"
                             select f.id).First();
            }
            catch
            {
                RestClient client2 = Box.BaseClients.BaseClient();
                RestRequest request2 = Box.BaseRequests.BaseRequest(Method.POST);
                
            }
            Box.BoxProperties.refreshInfo.UltiDriveFolderId = UltiDrive;
        }
    }
}
