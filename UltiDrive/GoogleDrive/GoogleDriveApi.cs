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

        public static File UploadFile(string fileName, string filePath)
        {
            RestClient client = Clients.UploadClient();
            RestRequest request = Requests.FilesRequest(Method.POST);
            request.Resource = "files";

            File newFile = new File() { title = fileName, originalFilename = fileName };
            request.AddBody(newFile);

            File response = MakeRequest<File>(client, request);

            client = Clients.UploadClient();
            request = Requests.FilesRequest(Method.PUT);
            request.Resource += "/" + response.id;
            request.AddFile(fileName, filePath);

            IRestResponse response2 = MakeRequest(client, request);

            return response;
        }

        public static void DownloadAllFiles(string downloadLocation)
        {

        }

        public static void DownloadFile(string downloadUrl, string filePath)
        {

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
