using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using FileManagement;
using RestSharp;
using System.Xml;
using System.Data;
using System.IO;


namespace UltiDrive.Box
{
    class BoxFileProperties
    {
        public string file_name { get; set; }
        public double file_size { get; set; }
        public long file_id { get; set; }
        public string hlbb { get; set; }
    }

    class BoxProperties
    {
        // UserName = UltiDrive@live.com
        // Password = Ult1Dr1ve
        public static bool LoggedIn
        {
            get
            {
                return _refreshInfo.access_token != null;
            }
        }


        public static readonly string APIKey = "5wvicd6ece1ghi01flwdkfi94zywy5al";
        public static readonly string ClientSecret = "M7rPpcSgniaHcZN2ARVnrLx2JWorLFxD";
        private static RefreshInfo _refreshInfo;
        public static RefreshInfo refreshInfo
        {
            get
            {
                return _refreshInfo;
            }
            set
            {
                _refreshInfo = value;
                Utilities.WriteBoxSettings();
            }
        }

        public class RefreshInfo
        {
            public string access_token { get; set; }
            public string expires_in { get; set; }
            public string token_type { get; set; }
            public string refresh_token { get; set; }

            public long UltiDriveFolderId { get; set; }
        }
    }
}
