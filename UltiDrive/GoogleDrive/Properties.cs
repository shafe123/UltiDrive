using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Runtime.Serialization.Json;
using System.Threading;
using Mjollnir.Live;
using FileManagement;
using RestSharp;

namespace UltiDrive.GoogleDrive
{
    class Requests
    {
        public static RestRequest BaseRequest(Method method)
        {
            RestRequest request = new RestRequest(method);
            //request.AddParameter("access_token", GoogleDriveRefreshInfo.Instance.AccessToken);
            request.RequestFormat = DataFormat.Json;
            request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };

            return request;
        }

        public static RestRequest FilesRequest(Method method)
        {
            RestRequest request = new RestRequest(method);
            request.RequestFormat = DataFormat.Json;
            request.Resource = "files";
            request.AddParameter("key", GoogleDriveRefreshInfo.Instance.AccessToken);
            request.AddParameter("setModifiedDate", "true");
            request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
            

            return request;
        }
    }

    class Clients
    {
        public static RestClient BaseClient()
        {
            RestClient client = new RestClient("https://www.googleapis.com/drive/v2/");
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(GoogleDrive.GoogleDriveRefreshInfo.Instance.AccessToken);
            return client;
        }

        public static RestClient UploadClient()
        {
            RestClient client = new RestClient("https://www.googleapis.com/upload/drive/v2/");
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(GoogleDrive.GoogleDriveRefreshInfo.Instance.AccessToken);
            return client;
        }
    }

    class DriveLogIn 
    {
        public static string ClientID = "80494111421.apps.googleusercontent.com";
        public static string ClientSecret = "_1yZl7NqbshOvVFhj4jU01rQ";
    }

    class GoogleDriveRefreshInfo
    {
        public bool LoggedIn
        {
            get
            {
                return AccessToken != null;
            }
        }

        private string _AccessToken;
        private int _ExpiresIn;
        private string _TokenType;
        private string _RefreshToken;

        public string AccessToken
        {
            get
            {
                return _Instance._AccessToken;
            }
            set
            {
                if (_Instance == null)
                    _Instance = new GoogleDriveRefreshInfo();
                _Instance._AccessToken = value;
            }
        }
        public string TokenType
        {
            get
            {
                return _Instance._TokenType;
            }
            set
            {
                if (_Instance == null)
                    _Instance = new GoogleDriveRefreshInfo();
                _Instance._TokenType = value;
            }
        }
        public string RefreshToken
        {
            get
            {
                return _Instance._RefreshToken;
            }
            set
            {
                if (_Instance == null)
                    _Instance = new GoogleDriveRefreshInfo();
                _Instance._RefreshToken = value;
            }
        }
        public int ExpiresIn
        {
            get
            {
                return _Instance._ExpiresIn;
            }
            set
            {
                if (_Instance == null)
                    _Instance = new GoogleDriveRefreshInfo();
                _Instance._ExpiresIn = value;
            }
        }

        private static GoogleDriveRefreshInfo _Instance;
        public static GoogleDriveRefreshInfo Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new GoogleDriveRefreshInfo();
                return _Instance;
            }
            set
            {
                _Instance = value;
            }
        }

        public GoogleDriveRefreshInfo(RootObject data)
        {
            if (_Instance == null)
                _Instance = new GoogleDriveRefreshInfo();
            _Instance._AccessToken = data.access_token;
            _Instance._ExpiresIn = data.expires_in;
            _Instance._TokenType = data.token_type;
            _Instance._RefreshToken = data.refresh_token;

            GoogleDrive.Utilities.WriteDriveSettings();
        }
        public GoogleDriveRefreshInfo()
        {
        }

        public class RootObject
        {
            public string access_token { get; set; }
            public int expires_in { get; set; }
            public string token_type { get; set; }
            public string refresh_token { get; set; }
        }
    }
}