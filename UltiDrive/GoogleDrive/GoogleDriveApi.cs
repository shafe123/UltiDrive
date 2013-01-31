using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FileManagement;
using RestSharp;

namespace UltiDrive.GoogleDrive
{
    class Api
    {
        public static void Login(bool asDialog)
        {
            LoginPage.SigningIn = true;
            LoginPage.Site = StorageServices.GoogleDrive;
            if (asDialog)
                new LoginPage().ShowDialog();
            else
                new LoginPage().Show();

            if (!GoogleDriveRefreshInfo.Instance.LoggedIn)
            {
                FancyMsgBoxWindow.Show("Google Drive", "Please log into Google Drive to use this service");
            }
        }

        public static Quota GetQuota()
        {
            RestClient client = Clients.BaseClient();

            RestRequest request = Requests.BaseRequest(Method.GET);
            request.Resource = "about";

            return MakeRequest<Quota>(client, request);
        }

        public static void RefreshTokens()
        {
            RestClient client = new RestClient("https://accounts.google.com/");
            RestRequest request = Requests.BaseRequest(Method.POST);
            request.Resource = "o/oauth2/token";

            request.AddParameter("client_id", DriveLogIn.ClientID);
            request.AddParameter("client_secret", DriveLogIn.ClientSecret);
            request.AddParameter("refresh_token", GoogleDriveRefreshInfo.Instance.RefreshToken);
            request.AddParameter("grant_type", "refresh_token");

            IRestResponse<RefreshResponse> response = client.Execute<RefreshResponse>(request);
            if (response.Data != null)
                GoogleDriveRefreshInfo.Instance.AccessToken = response.Data.access_token;
            else
            {
                FancyMsgBoxWindow.Show("Error refreshing", "Google Drive had problems refreshing its access token, please log-in again.");
                Login(true);
            }
        }

        public static void Logout()
        {
            System.IO.File.Delete(Utilities.JSONFile);
            GoogleDriveRefreshInfo.Instance = null;
        }

        public static void GetAllFiles()
        {
            RestClient client = GoogleDrive.Clients.BaseClient();
            RestRequest request = Requests.BaseRequest(Method.GET);
            request.Resource = "files";
            request.AddParameter("fields", "items(downloadUrl,id,title),nextPageToken");

            Utilities.FileIds ids = MakeRequest<Utilities.FileIds>(client, request);

            while (ids != null && ids.nextPageToken != null && ids.nextPageToken != "")
            {
                request = Requests.BaseRequest(Method.GET);
                request.Resource = "files";
                request.AddParameter("fields", "items(id,title),nextPageToken");
                request.AddParameter("pageToken", ids.nextPageToken);
                Utilities.FileIds tempIds = MakeRequest<Utilities.FileIds>(client, request);

                if (tempIds == null || tempIds.items == null)
                    break;
                
                ids.items.AddRange(tempIds.items);
                ids.nextPageToken = tempIds.nextPageToken;
            }

            Utilities.fileIds = ids;
        }

        public static File UploadFile(string fileName, string filePath)
        {
            RestClient client = Clients.UploadClient();
            RestRequest request = Requests.BaseRequest(Method.POST);
            request.Resource = "files";
            request.AddParameter("uploadType", "media");

            //request.RequestFormat = DataFormat.Json;
            //File uFile = new File()
            //{
            //    title = fileName
            //};
            //request.AddBody(uFile);

            byte[] file = System.IO.File.ReadAllBytes(filePath);

            request.AddFile("file", file, fileName);

            File result = MakeRequest<File>(client, request);
            return result;
        }

        public static void DownloadAllFiles(string downloadLocation)
        {
            GetAllFiles();

            foreach (File f in Utilities.fileIds.items)
            {
                if (f.downloadUrl != null)
                    DownloadFile(f.downloadUrl, downloadLocation + "\\" + f.title);
            }
        }

        public static void DownloadFile(string downloadUrl, string filePath)
        {
            RestClient client = new RestClient();
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(GoogleDrive.GoogleDriveRefreshInfo.Instance.AccessToken);
            RestRequest request = Requests.BaseRequest(Method.GET);
            request.Resource = downloadUrl;

            IRestResponse response = MakeRequest(client, request);
            System.IO.File.WriteAllBytes(filePath, response.RawBytes);
        }

        private static IRestResponse MakeRequest(RestClient client, RestRequest request)
        {
            IRestResponse response;

            try
            {
                response = client.Execute(request);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new AccessViolationException();
            }
            catch (Exception)
            {
                RefreshTokens();
                response = client.Execute(request);
            }

            return response;
        }

        private static T MakeRequest<T>(RestClient client, RestRequest request)
        {
            IRestResponse response = MakeRequest(client, request);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(response.Content);
        }

        private class RefreshResponse
        {
            public string access_token { get; set; }
            public long expires_in { get; set; }
            public string token_type { get; set; }
            public RefreshResponse() { }
        }
    }
}
