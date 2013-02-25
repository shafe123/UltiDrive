using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
using FileManagement;
using RestSharp;

namespace UltiDrive.Box
{
    class Api
    {
        public static void Login(bool asDialog)
        {
            try
            {
                LoginPage.Site = StorageServices.Box;
                LoginPage.SigningIn = true;
                LoginPage newWindow = new LoginPage();
                if (asDialog)
                    newWindow.ShowDialog();
                else
                    newWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Problem!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void RefreshTokens()
        {
            throw new NotImplementedException();
        }

        public static Quota GetQuota()
        {
            RestClient client = BaseClients.BaseClient();
            RestRequest request = BaseRequests.BaseRequest(Method.GET);
            request.Resource = "users/me";

            Quota test = MakeRequest<Quota>(client, request);
            return test;
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
    }
}
