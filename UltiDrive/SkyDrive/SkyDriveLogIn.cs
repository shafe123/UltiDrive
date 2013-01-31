using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UltiDrive.SkyDrive
{
    class SkyDriveLogIn
    {
        public static string SkyDriveClientID = "00000000400D5C53";
        public static string SkyDriveClientSecret = "uUeKp0LCHniERA-61ORCo6WcmQjMopCJ";
    }

    class RefreshInfo
    {
        public string token_type;
        public string expires_in;
        public string scope;
        public string access_token;
        public string refresh_token;
        public string authentication_token;
    }
}
