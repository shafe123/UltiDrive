using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows;
using Mjollnir.Live;
using RestSharp;
using UltiDrive.SkyDrive;

namespace UltiDrive.SkyDrive
{
    class Utilities
    {
        public static FileStream GetJSONFile()
        {
            return System.IO.File.Open(JSONFile, FileMode.OpenOrCreate);
        }

        public static string JSONFile
        {
            get
            {
                return App.AppFolder + "\\SkyDrive.json";
            }
        }

        public static void ConnectClient_GetQuotaCompleted(object sender, LiveDownloadCompletedEventArgs e)
        {
            Console.WriteLine(e.ToString());
        }

        public static void ConnectClient_UploadCompleted(object sender, LiveOperationCompletedEventArgs e)
        {
            Console.WriteLine("BOOM! Upload completed.");
        }

        public static void ConnectClient_DownloadCompleted(object sender, LiveDownloadCompletedEventArgs e)
        {
            if (e.Error == null && !e.Cancelled)
            {
                var fileName = (string)e.UserState;

                try
                {
                    File.WriteAllBytes(fileName, ((MemoryStream)e.Result).ToArray());
                }
                catch (Exception x)
                {
                    MessageBox.Show(x.Message);
                }
            }
            else
            {
                Console.WriteLine(e);
            }

            Console.WriteLine("BOOM! Download completed.");
        }

        public static void ConnectClient_DeleteCompleted(object sender, LiveOperationCompletedEventArgs e)
        {
            Console.WriteLine("BOOM! Delete completed.");
        }

        public static void WriteSkyDriveSettings()
        {
            Stream file = GetJSONFile();

            IFormatter formatter = new BinaryFormatter();

            if (!Properties.LoggedIn)
                throw new Exception("SkyDrive session not initialized");
            formatter.Serialize(file, Properties.session);

            file.Close();
        }

        public static void checkSkyDriveInitiatedStart(object sender, RoutedEventArgs e)
        {
            //Check to see if user is logged in
            if (SkyDrive.Properties.LoggedIn)
            {
                //Check to see if SkyDrive is initiated for UltiDrive
                LiveConnectClient client = new LiveConnectClient(SkyDrive.Properties.session);
                client.GetCompleted += skydrive_checkInitiated;
                client.GetAsync("me/skydrive/files"); //Get list of files in root directory of SkyDrive.
                //END Check to see if SkyDrive is initiated for UltiDrive
            }
        }

        private static void skydrive_checkInitiated(object sender, LiveOperationCompletedEventArgs e)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            Files.RootObject data = ser.Deserialize<Files.RootObject>(e.RawResult);

            bool foundTxt = false;
            bool foundFolder = false;
            foreach (Files.Datum listItem in data.data)
            {
                if (listItem.name == "UltiDrive") //Look for UltiDrive folder
                {
                    //System.Windows.MessageBox.Show("Found UltiDrive folder");
                    SkyDrive.Properties.UltiDriveFolderID = listItem.id; //Assign the UltiDrive folder ID to the SkyDrive Properties class.
                    foundFolder = true;
                }
            }

            if (foundFolder == false) //Not found, so create UltiDrive folder.
            {
                LiveConnectClient client = new LiveConnectClient(SkyDrive.Properties.session);
                var folderData = new Dictionary<string, object> { { "name", "UltiDrive" } };
                client.PostCompleted += skydrive_setUltiDriveFolderID;
                client.PostAsync("me/skydrive", folderData);
                //System.Windows.MessageBox.Show("Created UltiDrive folder!");
            }
        }

        private static void skydrive_setUltiDriveFolderID(object sender, LiveOperationCompletedEventArgs e)
        {
            LiveConnectClient client = new LiveConnectClient(SkyDrive.Properties.session);
            client.GetCompleted += skydrive_checkInitiated;
            client.GetAsync("me/skydrive/files");
        }
    }
}

