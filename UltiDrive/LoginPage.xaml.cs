using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using RestSharp;
using System.Windows.Navigation;
using System.Threading;
using UltiDrive.SkyDrive;
using UltiDrive.Dropbox;
using UltiDrive.Box;
using System.Diagnostics;
using FileManagement;
using Mjollnir.Live;
using System.Xml;
using UltiDrive.GoogleDrive;

namespace UltiDrive
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Window
    {
        private delegate void LogInDelegate();

        public static StorageServices Site;
        public static bool SigningIn;

        public LoginPage()
        {
            InitializeComponent();
            LoginBrowser.LoadCompleted += new LoadCompletedEventHandler(LoginBrowser_LoadCompleted);

            if (SigningIn)
            {
                if (Site == StorageServices.SkyDrive)
                {
                    this.Title = "SkyDrive Login";

                    LoginBrowser.Dispatcher.BeginInvoke(new LogInDelegate(SkyDriveLogIn), null);
                }

                if (Site == StorageServices.GoogleDrive)
                {
                    this.Title = "Google Drive Login";

                    LoginBrowser.Dispatcher.BeginInvoke(new LogInDelegate(GoogleDriveLogIn), null);
                }

                if (Site == StorageServices.Dropbox)
                {
                    this.Title = "Dropbox Login";

                    LoginBrowser.Dispatcher.BeginInvoke(new LogInDelegate(DropboxLogIn), null);
                }

                if (Site == StorageServices.Box)
                {
                    this.Title = "Box Login";

                    LoginBrowser.Dispatcher.BeginInvoke(new LogInDelegate(BoxLogIn), null);
                }
            }

            this.Width = 1024;
            this.Height = 800;
        }

        private void SkyDriveLogIn()
        {
            RestClient client = new RestClient("https://login.live.com/");

            RestRequest request = new RestRequest(Method.GET);
            request.Resource = "oauth20_authorize.srf";
            request.AddParameter("client_id", SkyDrive.Properties.SkyDriveClientID);
            request.AddParameter("scope", "wl.signin wl.offline_access wl.skydrive wl.skydrive_update wl.photos");
            request.AddParameter("response_type", "code");
            request.AddParameter("redirect_uri", "https://login.live.com/oauth20_desktop.srf");

            IRestResponse response = client.Execute(request);
            LoginBrowser.Navigate(client.BuildUri(request));
        }
        private void SkyDriveLogInPart2()
        {
            RestClient client = new RestClient("https://login.live.com/");

            RestRequest request = new RestRequest(Method.GET);
            request.Resource = "oauth20_token.srf";
            request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
            request.AddParameter("client_id", SkyDrive.Properties.SkyDriveClientID);
            request.AddParameter("client_secret", SkyDrive.Properties.SkyDriveClientSecret);
            request.AddParameter("code", Application.Current.Resources["SkyDriveAuthCode"]);
            request.AddParameter("redirect_uri", "https://login.live.com/oauth20_desktop.srf");
            request.AddParameter("grant_type", "authorization_code");

            IRestResponse<SkyDrive.RefreshInfo.RootObject> response = client.Execute<SkyDrive.RefreshInfo.RootObject>(request);
            //new SkyDriveRefreshInfo(response);

            this.Close();
            SkyDrive.Properties.session = new LiveConnectSession();

            SkyDrive.Properties.session.AccessToken = response.Data.access_token;
            SkyDrive.Properties.session.RefreshToken = response.Data.refresh_token;
        }

        private void GoogleDriveLogIn()
        {
            RestClient client = new RestClient("https://accounts.google.com");
            string scopes = "https://www.googleapis.com/auth/drive";
            scopes += " https://www.googleapis.com/auth/drive.file";
            scopes += " https://www.googleapis.com/auth/drive.metadata.readonly";
            scopes += " https://www.googleapis.com/auth/drive.readonly";
            scopes += " https://www.googleapis.com/auth/userinfo.profile";

            RestRequest request = new RestRequest(Method.GET);
            request.Resource = "/o/oauth2/auth";
            request.AddParameter("client_id", DriveLogIn.ClientID);
            request.AddParameter("scope", scopes);
            request.AddParameter("response_type", "code");
            request.AddParameter("redirect_uri", "urn:ietf:wg:oauth:2.0:oob");
            request.AddParameter("access_type", "offline");
            request.AddParameter("state", "");

            IRestResponse response = client.Execute(request);
            LoginBrowser.Navigate(client.BuildUri(request));
        }
        private void GoogleDriveLogIn_2()
        {

            RestClient client = new RestClient("https://accounts.google.com");

            RestRequest request = new RestRequest(Method.POST);
            request.Resource = "/o/oauth2/token";
            request.AddParameter("code", Application.Current.Resources["GoogleDriveAuthCode"]);
            request.AddParameter("grant_type", "authorization_code");
            request.AddParameter("redirect_uri", "urn:ietf:wg:oauth:2.0:oob");
            request.AddParameter("client_id", DriveLogIn.ClientID);
            request.AddParameter("scope", "");
            request.AddParameter("client_secret", DriveLogIn.ClientSecret);

            IRestResponse<GoogleDriveRefreshInfo.RootObject> response = client.Execute<GoogleDriveRefreshInfo.RootObject>(request);
            new GoogleDriveRefreshInfo(response.Data);

            this.Close();
        }

        private void DropboxLogIn()
        {
            string authString = String.Format("oauth_token={0}", DropboxProperties.Properties.oauthToken);
            string authorizeUrl = "https://www.dropbox.com/1/oauth/authorize?" + authString;

            LoginBrowser.Navigate(authorizeUrl);
        }

        private void BoxLogIn()
        {
                try
                {
                    LoginBrowser.Navigate("https://api.box.com/oauth2/authorize?response_type=code&client_id=" + BoxProperties.APIKey);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Problem!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
        }
        private void BoxLogIn2()
        {
            RestClient client = new RestClient("https://api.box.com/oauth2/");
            RestRequest request = new RestRequest(Method.POST);
            request.Resource = "token";

            request.AddParameter("grant_type", "authorization_code");
            request.AddParameter("code", Application.Current.Resources["BoxAuthCode"].ToString());
            request.AddParameter("client_id", BoxProperties.APIKey);
            request.AddParameter("client_secret", BoxProperties.ClientSecret);

            IRestResponse<BoxProperties.RefreshInfo> response = client.Execute<BoxProperties.RefreshInfo>(request);
            BoxProperties.refreshInfo = response.Data;

            Box.Utilities.SetUpUltiDriveFolder();
        }

        void LoginBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            WebBrowser browser = (WebBrowser)sender;

            string source = browser.Source.ToString();
            if (browser.Source != null)
            {
                if (Site == StorageServices.SkyDrive)
                {
                    if (source.Contains("code") && !source.Contains("client_id"))
                    {
                        // skydrive.login.com/source.srf?code=thisissomecode&yes=no
                        Application.Current.Resources["SkyDriveAuthCode"] = Utilities.ParseSourceForIdentifier("code=", browser.Source.Query);
                        SkyDriveLogInPart2();
                    }
                }

                if (Site == StorageServices.Dropbox)
                {
                    if (source == "https://www.dropbox.com/1/oauth/authorize")
                    {
                        this.Close();
                    }
                }

                if (Site == StorageServices.GoogleDrive)
                {
                    mshtml.IHTMLDocument2 doc = (mshtml.IHTMLDocument2)browser.Document;
                    source = doc.title;
                    if (source.Contains("Success code="))
                    {
                        Application.Current.Resources["GoogleDriveAuthCode"] = source.Substring(source.IndexOf('=')+1, source.Length-source.IndexOf('=')-1);
                        //Utilities.ParseSourceForIdentifier("Success code", source);
                        GoogleDriveLogIn_2();
                    }
                }

                if (Site == StorageServices.Box)
                {
                    if (source.Contains("ultidrive"))
                    {
                        Application.Current.Resources["BoxAuthCode"] = Utilities.ParseSourceForIdentifier("code", source);
                        BoxLogIn2();
                        this.Close();
                        //MessageBox.Show("Logged In!", "UltiDrive", MessageBoxButton.OK);
                    }
                }
            }
        }
    }
}
