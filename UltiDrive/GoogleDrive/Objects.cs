using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UltiDrive.GoogleDrive
{
    class Quota
    {
        public long quotaBytesTotal { get; set; }
        public long quotaBytesUsed { get; set; }
        public long quotaBytesUsedInTrash { get; set; }
    }

    class File
    {
        public string id { get; set; }
        public string title { get; set; }
        public string originalFilename { get; set; }
        public string downloadUrl { get; set; }
        public DateTime modifiedDate { get; set; }
        public List<Parent> parents { get; set; }
    }

    public class Parent
    {
        public string id { get; set; }
        public bool isRoot { get; set; }
    }
}
