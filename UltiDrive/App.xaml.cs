using RestSharp;
using RestSharp.Deserializers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using UltiDrive.SkyDrive;
using Mjollnir.Live;
using System.Web.Script.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UltiDrive.Dropbox.Api;
using System.Net.NetworkInformation;
using FileManagement;
using UltiDrive.FileManagement;

namespace UltiDrive
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static double _TotalAvailable;
        private static double _TotalUsed;
        public static Task initialize;
        public static UltiDriveSystemWatcher _watcher;
        public static FileStructure _structure;

        public static double TotalAvailable
        {
            get
            {
                return _TotalAvailable;
            }
            set
            {
                _TotalAvailable = value;
                if (_TotalAvailable > 0.0)
                    UsedRatio = _TotalUsed / _TotalAvailable;
                else
                    UsedRatio = 0;
            }
        }
        public static double TotalUsed
        {
            get
            {
                return _TotalUsed;
            }
            set
            {
                _TotalUsed = value;
                if (_TotalAvailable > 0.0)
                    UsedRatio = _TotalUsed / _TotalAvailable;
                else
                    UsedRatio = 0;
            }
        }
        public static double UsedRatio { get; private set; }
        public static string AppFolder
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\UltiDrive";
            }
        }

        private void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            if (!e.IsAvailable)
            {
                FancyMsgBoxWindow.Show("Internet Lost", "We will wait in the background until you gain internet again.");
            }
            else
            {
                FancyMsgBoxWindow.Show("Internet Restored", "Yay! You have internet again.");
            }
        }

        void App_Startup(object sender, StartupEventArgs e)
        {
            List<StorageInformation> services = new List<StorageInformation>();
            TotalAvailable = 0;
            TotalUsed = 0;

            if (!Directory.Exists(AppFolder))
            {
                Directory.CreateDirectory(AppFolder);
                this.StartupUri = new Uri("Setup/InitialSetup.xaml", UriKind.Relative);
            }
            else
            {
                this.StartupUri = new Uri("FancyMainWindow.xaml", UriKind.Relative);

                if (System.IO.File.Exists(AppFolder + "\\Watcher.dat"))
                {
                    List<string> directories = new List<string>();

                    string xml = System.IO.File.ReadAllText(AppFolder + "\\Watcher.dat");
                    StringReader reader = new StringReader(xml);
                    System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(List<string>));
                    directories = (List<string>)serializer.Deserialize(reader);

                    InitializeServices(ref services);
                    _structure = new FileStructure(directories, services); 
                }
            }

            NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;
        }

        private void InitializeServices(ref List<StorageInformation> services)
        {
            FileStream file;

            #region UbuntuOne
            //UbuntuOne Settings
            //FileStream file = UbuntuOne.Utilities.GetJSONFile();
            //if (file.Length > 0)
            //{
            //    byte[] bytes = new byte[file.Length];
            //    file.Read(bytes, 0, (int)file.Length);
            //    string json = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
            //    //do work here
            //    new UbuntuOne.RefreshInfo(Newtonsoft.Json.JsonConvert.DeserializeObject<UbuntuOne.RefreshInfo>(json));

            //    UbuntuOne.Quota qt = UbuntuOne.Api.GetQuota();
            //    TotalAvailable += qt.total;
            //    TotalUsed += qt.used;

            //    services.Add(new StorageInformation(StorageServices.UbuntuOne, qt.used, qt.total));
            //}
            //file.Close();
            #endregion

            #region SkyDrive
            file = System.IO.File.Open(AppFolder + "\\SkyDrive.json", FileMode.OpenOrCreate, FileAccess.ReadWrite);

            if (file.Length > 0)
            {
                file.Close();

                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(App.AppFolder + "\\SkyDrive.json", FileMode.OpenOrCreate);
                SkyDrive.Properties.session = (LiveConnectSession)formatter.Deserialize(stream);

                //Refresh the tokens
                RestClient client = new RestClient("https://login.live.com");

                RestRequest request = new RestRequest(Method.POST);
                request.Resource = "oauth20_token.srf";
                request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
                request.AddParameter("client_id", SkyDrive.Properties.SkyDriveClientID);
                request.AddParameter("client_secret", SkyDrive.Properties.SkyDriveClientSecret);
                request.AddParameter("refresh_token", SkyDrive.Properties.session.RefreshToken);
                request.AddParameter("grant_type", "refresh_token");

                IRestResponse<SkyDrive.RefreshInfo.RootObject> response = client.Execute<SkyDrive.RefreshInfo.RootObject>(request);

                SkyDrive.Properties.session = new LiveConnectSession();

                SkyDrive.Properties.session.AccessToken = response.Data.access_token;
                SkyDrive.Properties.session.RefreshToken = response.Data.refresh_token;
                //END Refresh the tokens


                stream.Close();

                SkyDrive.Properties.Quota qt = SkyDrive.Api.GetQuota();
                TotalAvailable += qt.quota;
                TotalUsed += qt.quota - qt.available;

                //Check SkyDrive Initialized
                checkSkyDriveInitiatedStart(null, null);
                //END Check SkyDrive Initialized

                services.Add(new StorageInformation(StorageServices.SkyDrive, qt.quota - qt.available, qt.quota));
            }
            file.Close();
            #endregion

            #region GoogleDrive
            file = System.IO.File.Open(AppFolder + "\\GoogleDrive.json", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            if (file.Length > 0)
            {
                byte[] bytes = new byte[file.Length];
                file.Read(bytes, 0, (int)file.Length);
                string json = Encoding.ASCII.GetString(bytes, 0, bytes.Length);

                // do work here
                Newtonsoft.Json.JsonConvert.DeserializeObject<GoogleDrive.GoogleDriveRefreshInfo>(json);

                GoogleDrive.Quota qt = GoogleDrive.Api.GetQuota();
                TotalAvailable += qt.quotaBytesTotal;
                TotalUsed += qt.quotaBytesUsed;

                services.Add(new StorageInformation(StorageServices.GoogleDrive, qt.quotaBytesUsed, qt.quotaBytesTotal));
            }
            file.Close();
            #endregion

            #region Dropbox
            file = System.IO.File.Open(AppFolder + "\\Dropbox.json", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            if (file.Length > 0)
            {
                byte[] bytes = new byte[file.Length];
                file.Read(bytes, 0, (int)file.Length);
                string json = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                DropboxApi.Api.AccessToken = Newtonsoft.Json.JsonConvert.DeserializeObject<OAuthProtocol.OAuthToken>(json);

                Account act = DropboxApi.Api.GetAccountInfo();
                TotalAvailable += act.Quota.Total;
                TotalUsed += act.Quota.Normal + act.Quota.Shared;

                services.Add(new StorageInformation(StorageServices.Dropbox, act.Quota.Normal + act.Quota.Normal, act.Quota.Total));
            }
            file.Close();
            #endregion

            #region Box
            file = System.IO.File.Open(Box.Utilities.JSONFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            if (file.Length > 0)
            {
                byte[] bytes = new byte[file.Length];
                file.Read(bytes, 0, (int)file.Length);
                string json = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                file.Close();

                Box.BoxProperties.refreshInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<Box.BoxProperties.RefreshInfo>(json);

                Box.Quota qt = Box.Api.GetQuota();
                TotalAvailable += qt.space_amount;
                TotalUsed += qt.space_used;

                services.Add(new StorageInformation(StorageServices.Box, qt.space_used, qt.space_amount));
            }
            file.Close();
            #endregion
        }

        void App_Exit(object sender, ExitEventArgs e)
        {
            //System.IO.Directory.Delete(AppFolder, true);
        }

        private void skydrive_checkInitiated(object sender, LiveOperationCompletedEventArgs e)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            Files.RootObject data = ser.Deserialize<Files.RootObject>(e.RawResult);

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

        private void skydrive_setUltiDriveFolderID(object sender, LiveOperationCompletedEventArgs e)
        {
            LiveConnectClient client = new LiveConnectClient(SkyDrive.Properties.session);
            client.GetCompleted += skydrive_checkInitiated;
            client.GetAsync("me/skydrive/files");
        }

        private void checkSkyDriveInitiatedStart(object sender, RoutedEventArgs e)
        {
            //Check to see if user is logged in
            if (SkyDrive.Properties.LoggedIn)
            {
                //Check to see if SkyDrive is initiated for UltiDrive
                LiveConnectClient client = new LiveConnectClient(SkyDrive.Properties.session);
                client.GetCompleted += this.skydrive_checkInitiated;
                client.GetAsync("me/skydrive/files"); //Get list of files in root directory of SkyDrive.
                //END Check to see if SkyDrive is initiated for UltiDrive
            }
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {


            e.Handled = true;
        }
    }
}
