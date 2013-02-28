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
    public partial class file
    {
        public string fullpath { get { return this.rootFolder + this.relativeFilePath; } }
    }

    public abstract class API
    {
        public abstract void Login(bool asDialog);
        public abstract void Logout();
        public abstract file DownloadFile(string guid, string downloadLocation);
        public abstract file UpdateFile(string fullfilepath);
        public abstract file UploadFile(string guid, string fullfilepath);
        public abstract bool DeleteFile();
    }

    class Utilities
    {
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
    }
}
