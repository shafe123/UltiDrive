using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UltiDrive.Dropbox
{
    public class DropboxProperties
    {
        private static DropboxProperties properties;

        private DropboxProperties() 
        { 
        }

        public static DropboxProperties Properties
        {
            get
            {
                if (properties == null)
                {
                    properties = new DropboxProperties();
                }

                return properties;
            }
        }

        public static string consumerKey = "p6qe2ckxfj8yzwp";
        public static string consumerSecret = "fu5jgon7e4qu46s";

        private string _oauthToken;
        private string _oauthTokenSecret;
        private string _accessToken;
        private string _accessTokenSecret;

        public string oauthToken
        {
            get
            {
                if (_oauthToken == null)
                {
                    _oauthToken = null;
                }

                return _oauthToken;
            }

            set
            {
                _oauthToken = value;
            }
        }
        public string oauthTokenSecret
        {
            get
            {
                return _oauthTokenSecret;
            }

            set
            {
                _oauthTokenSecret = value;
            }
        }
        public string accessToken
        {
            get
            {
                return _accessToken;
            }

            set
            {
                _accessToken = value;
            }
        }
        public string accessTokenSecret
        {
            get
            {
                return _accessTokenSecret;
            }

            set
            {
                _accessTokenSecret = value;
            }
        }
    }
}
