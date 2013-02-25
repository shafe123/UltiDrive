using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UltiDrive.Box
{
    public class BaseClients
    {
        public static RestClient BaseClient()
        {
            RestClient client = new RestClient("https://api.box.com/2.0/");
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator("Bearer " + BoxProperties.refreshInfo.access_token);

            return client;
        }
    }
    public class BaseRequests
    {
        public static RestRequest BaseRequest(Method method)
        {
            RestRequest request = new RestRequest(method);

            return request;
        }
    }

    public class Quota
    {
        public long space_amount { get; set; }
        public long space_used { get; set; }
        public long max_upload_size { get; set; }
    }

    public class FileResponse
    {
        public int total_count { get; set; }
        public List<File> entries { get; set; }
    }
    public class Folder
    {
        public File BaseInfo { get; set; }
        public Items item_collection { get; set; }
    }
    public class File
    {
        public string type { get; set; }
        public long id { get; set; }
        public int sequence_id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public long size { get; set; }
        public string path { get; set; }
        public string path_id { get; set; }
        public string created_at { get; set; }
        public string modified_at { get; set; }
        public object etag { get; set; }
        public CreatedBy created_by { get; set; }
        public ModifiedBy modified_by { get; set; }
        public OwnedBy owned_by { get; set; }
        public string shared_link { get; set; }
        public Parent parent { get; set; }
    }

    public class Items
    {
        public int total_count { get; set; }
        public List<Entry> entries { get; set; }
    }

    public class Entry
    {
        public string type { get; set; }
        public long id { get; set; }
        public string name { get; set; }
    }

    public class CreatedBy
    {
        public string type { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string login { get; set; }
    }

    public class ModifiedBy
    {
        public string type { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string login { get; set; }
    }

    public class OwnedBy
    {
        public string type { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string login { get; set; }
    }

    public class Parent
    {
        public string type { get; set; }
        public string id { get; set; }
        public string sequence_id { get; set; }
        public string name { get; set; }
    }
}
