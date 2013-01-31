using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using FileManagement;
using Mjollnir.Live;
using RestSharp;
using UltiDrive.SkyDrive;
using System.Collections;

namespace UltiDrive.SkyDrive
{
    public class Api
    {
        static Queue guidToDeleteQueue = Queue.Synchronized(new Queue());
        static Queue guidToDownloadQueue = Queue.Synchronized(new Queue());

        public static void Login(bool asDialog)
        {
            if (!Properties.LoggedIn)
            {
                LoginPage.Site = StorageServices.SkyDrive;
                LoginPage.SigningIn = true;

                LoginPage newWindow = new LoginPage();

                if (asDialog)
                    newWindow.ShowDialog();
                else
                    newWindow.Show();

                Utilities.WriteSkyDriveSettings();
            }
        }

        public static void Logout()
        {
            SkyDrive.Properties.session = null;
            System.IO.File.Delete(SkyDrive.Utilities.JSONFile);
        }

        public static Properties.Quota GetQuota()
        {
            validateSession();
            RestClient client = new RestClient("https://apis.live.net/v5.0/");
            RestRequest request = new RestRequest(Method.GET);
            request.Resource = "me/skydrive/quota";
            request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
            request.AddParameter("access_token", Properties.session.AccessToken);

            IRestResponse response = client.Execute(request);

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Properties.Quota data = serializer.Deserialize<Properties.Quota>(response.Content);
            return data;
        }
        private static void refresh_tokens()
        {
            //Refresh the tokens
            RestClient client = new RestClient("https://login.live.com");

            RestRequest request = new RestRequest(Method.POST);
            request.Resource = "oauth20_token.srf";
            request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
            request.AddParameter("client_id", Properties.SkyDriveClientID);
            request.AddParameter("client_secret", Properties.SkyDriveClientSecret);
            request.AddParameter("refresh_token", Properties.session.RefreshToken);
            request.AddParameter("grant_type", "refresh_token");

            IRestResponse<RefreshInfo.RootObject> response = client.Execute<RefreshInfo.RootObject>(request);

            Properties.session = new LiveConnectSession();

            Properties.session.AccessToken = response.Data.access_token;
            Properties.session.RefreshToken = response.Data.refresh_token;
            //END Refresh the tokens
        }
        private static void validateSession()
        {
            RestClient client = new RestClient("https://apis.live.net/v5.0/");
            RestRequest request = new RestRequest(Method.GET);
            request.Resource = "me/skydrive/quota";
            request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
            request.AddParameter("access_token", Properties.session.AccessToken);

            IRestResponse response = client.Execute(request);

            if (response.Content.ToString().Contains("request_token_invalid"))
            {
                refresh_tokens();
            }
        }

        private static void DownloadFileHelper(object sender, LiveOperationCompletedEventArgs e)
        {
            string clientDirectory = "C:\\UltiDrive\\";

            indexEntities db = new indexEntities();
            string guidToDownload = (string)guidToDownloadQueue.Dequeue();
            var request = from f in db.files
                          where f.guid == guidToDownload
                          select f.origFileName;

            string originalPath = request.First().ToString();

            string originalFileName = originalPath.Split('\\').Last();

            try
            {
                JavaScriptSerializer ser = new JavaScriptSerializer();
                Files.RootObject data = ser.Deserialize<Files.RootObject>(e.RawResult);

                foreach (Files.Datum listItem in data.data)
                {
                    if (listItem.name == guidToDownload)
                    {
                        LiveConnectClient client = new LiveConnectClient(Properties.session);

                        client.DownloadCompleted += UltiDrive.SkyDrive.Utilities.ConnectClient_DownloadCompleted;
                        string path = clientDirectory + originalFileName;
                        client.DownloadAsync(listItem.id + "/content", path);
                    }
                }

                
            }
            catch (Exception ex)
            {
                Console.WriteLine("SkyDriveApi.DownloadFile() failed. --> " + ex.ToString());
            }
        }

        public static bool DownloadFile(string guid)
        {
            validateSession();

            try
            {
                guidToDownloadQueue.Enqueue(guid);
                LiveConnectClient client = new LiveConnectClient(Properties.session);
                client.GetCompleted += DownloadFileHelper;
                client.GetAsync(SkyDrive.Properties.UltiDriveFolderID + "/files");

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }

        }

        public static bool UploadFile(string filePath)
        {
            validateSession();
            try
            {
                LiveConnectClient client = new LiveConnectClient(Properties.session);

                client.UploadCompleted += UltiDrive.SkyDrive.Utilities.ConnectClient_UploadCompleted;
                var stream = default(Stream);
                stream = File.OpenRead(filePath);
                client.UploadAsync(Properties.UltiDriveFolderID + "/files", filePath, stream, stream);
                //System.Windows.MessageBox.Show(stream.ToString());
                //stream.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("SkyDriveApi.UploadFile() failed. --> " + e.ToString());
                return false;
            }
        }

        public static bool UploadFile(string guid, string filePath)
        {
            validateSession();
            try
            {
                LiveConnectClient client = new LiveConnectClient(Properties.session);

                client.UploadCompleted += UltiDrive.SkyDrive.Utilities.ConnectClient_UploadCompleted;
                var stream = default(Stream);
                stream = File.OpenRead(filePath);
                string directoryName = Path.GetDirectoryName(filePath) + "\\";
                client.UploadAsync(Properties.UltiDriveFolderID + "/files", directoryName + guid, stream, stream);
                
                //System.Windows.MessageBox.Show(stream.ToString());
                //stream.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("SkyDriveApi.UploadFile() failed. --> " + e.ToString());
                return false;
            }
        }


        private static void DeleteFileHelper(object sender, LiveOperationCompletedEventArgs e)
        {
            try
            {
                JavaScriptSerializer ser = new JavaScriptSerializer();
                Files.RootObject data = ser.Deserialize<Files.RootObject>(e.RawResult);
                string guidToDelete = (string)guidToDownloadQueue.Dequeue();
                foreach (Files.Datum listItem in data.data)
                {
                    if (listItem.name == guidToDelete)
                    {
                        LiveConnectClient client = new LiveConnectClient(Properties.session);
                        client.DeleteCompleted += UltiDrive.SkyDrive.Utilities.ConnectClient_DeleteCompleted;
                        client.DeleteAsync(listItem.id);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static bool DeleteFile(string guid)
        {
            validateSession();
            try
            {
                guidToDeleteQueue.Enqueue(guid);
                LiveConnectClient client = new LiveConnectClient(Properties.session);
                client.GetCompleted += DeleteFileHelper;
                client.GetAsync(SkyDrive.Properties.UltiDriveFolderID + "/files");

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

    }
}
