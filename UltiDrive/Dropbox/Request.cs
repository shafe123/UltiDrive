using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UltiDrive.Dropbox;
using RestSharp.Contrib;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Windows;
using OAuthProtocol;
using UltiDrive.Dropbox.Api;
using FileManagement;

namespace UltiDrive.Dropbox
{
    public class Request
    {
        public OAuthToken GetAccessToken()
        {
            var oauth = new OAuth();

            var requestToken = oauth.GetRequestToken(new Uri(DropboxRestApi.BaseUri), DropboxProperties.consumerKey, DropboxProperties.consumerSecret);
            DropboxProperties.Properties.oauthToken = requestToken.Token;
            DropboxProperties.Properties.oauthTokenSecret = requestToken.Secret;

            var authorizeUri = oauth.GetAuthorizeUri(new Uri(DropboxRestApi.AuthorizeBaseUri), requestToken);

            LoginPage.Site = StorageServices.Dropbox;
            LoginPage.SigningIn = true;

            LoginPage window = new LoginPage();
            window.ShowDialog();

            return oauth.GetAccessToken(new Uri(DropboxRestApi.BaseUri), DropboxProperties.consumerKey, DropboxProperties.consumerSecret, requestToken);
        }
    }
}
