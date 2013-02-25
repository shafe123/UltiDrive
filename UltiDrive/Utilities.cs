using RestSharp;
using RestSharp.Serializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.XPath;
using FileManagement;
using UltiDrive.SkyDrive;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UltiDrive.Dropbox.Api;

namespace UltiDrive
{
    class Utilities
    {
        public static string ParseSourceForIdentifier(string identifier, Uri source)
        {
            return ParseSourceForIdentifier(identifier, source.ToString());
        }
        public static string ParseSourceForIdentifier(string identifier, string source)
        {
            //looks through the URL to find the specified identifier and returns the value of the identifier
            //i.e. if given string moneymaker = "?source=desktop&view=mobile"
            //method(view, moneymaker) returns mobile
            string result = "";

            if (source.IndexOf(identifier) == -1)
                return null;

            int NextEqualSign = source.IndexOf('=', source.IndexOf(identifier));

            if (source.LastIndexOf('&') > source.IndexOf(identifier))
            {
                int NextAmpersand = source.IndexOf('&', NextEqualSign);
                result = source.Substring(NextEqualSign + 1, NextAmpersand - NextEqualSign - 1);
            }
            else
            {
                result = source.Substring(NextEqualSign + 1, source.Length - NextEqualSign-1);
            }

            return result;
        }

        public static void WriteDropboxSettings()
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(DropboxApi.Api.AccessToken);
            System.IO.File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\UltiDrive\\Dropbox.json", json);
        }

        public static StorageServices GetStorageService(string value)
        {
            StorageServices service = StorageServices.Empty;

            switch (value)
            {
                case "Box":
                    service = StorageServices.Box;
                    break;
                case "Dropbox":
                    service = StorageServices.Dropbox;
                    break;
                case "SkyDrive":
                    service = StorageServices.SkyDrive;
                    break;
                case "GoogleDrive":
                    service = StorageServices.GoogleDrive;
                    break;
                case "UbuntuOne":
                    service = StorageServices.UbuntuOne;
                    break;
                default:
                    service = StorageServices.Empty;
                    break;
            }

            return service;
        }
    }
}
