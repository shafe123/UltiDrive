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

namespace UltiDrive.SkyDrive
{
    [Serializable]
    class Files
    {
        public class From
        {
            public string name { get; set; }
            public string id { get; set; }
        }
        public class SharedWith
        {
            public string access { get; set; }
        }
        public class Datum
        {
            public string id { get; set; }
            public From from { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string parent_id { get; set; }
            public long size { get; set; }
            public string upload_location { get; set; }
            public bool is_embeddable { get; set; }
            public long count { get; set; }
            public string link { get; set; }
            public string type { get; set; }
            public SharedWith shared_with { get; set; }
            public string created_time { get; set; }
            public string updated_time { get; set; }
        }
        public class RootObject
        {
            public List<Datum> data { get; set; }
        }
    }

    [Serializable]
    public static class Properties
    {
        public static string SkyDriveClientID = "00000000400D5C53";
        public static string SkyDriveClientSecret = "uUeKp0LCHniERA-61ORCo6WcmQjMopCJ";
        public static LiveConnectSession session = null;

        //Session data
        public static string token_type = null;
        public static int expires_in = 0;
        public static string scope = null;
        public static string access_token = null;
        public static string refresh_token = null;
        //END session data

        public static string UltiDriveFolderID = null;

        public class Quota
        {
            public long quota { get; set; }
            public long available { get; set; }
        }

        public static bool LoggedIn
        {
            get
            {
                return session != null;
            }
        }
    }

    //Need to clean up this code below...
    [Serializable]
    public class RefreshInfo
    {
        public class RootObject
        {
            public string token_type { get; set; }
            public int expires_in { get; set; }
            public string scope { get; set; }
            public string access_token { get; set; }
            public string refresh_token { get; set; }
        }

        private string token_type { get; set; }
        private int expires_in { get; set; }
        private string[] scopes { get; set; }
        private string access_token { get; set; }
        private string refresh_token { get; set; }

        public RefreshInfo() { }
        private RefreshInfo(RootObject obj)
        {
            this.token_type = obj.token_type;
            this.expires_in = obj.expires_in;
            this.scopes = obj.scope.Split(' ');
            this.access_token = obj.access_token;
            this.refresh_token = obj.refresh_token;
        }
    }
}
