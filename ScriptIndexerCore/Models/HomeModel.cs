using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScriptIndexerCore.Models
{
    public class HomeModel
    {
        public string Mongodb_path { get; set; }
        public string Mongodb_port { get; set; }
        public string Database_name { get; set; }
        public string Movie_collection_name { get; set; }
        public string Show_collection_name { get; set; }
        public string Misc_collection_name { get; set; }

    }
}
