using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_brollop.Common
{
    public class Playlist
    {
        public bool Collaborative { get; set; }
        //public IEnumerable<dynamic> External_Urls { get; set; }
        public string Href { get; set; }
        public string Id { get; set; }
        //public IEnumerable<dynamic> Images { get; set; }
        public string Name { get; set; }
        public bool Public { get; set; }

    }
}