using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_brollop.Common
{
    public class Track : TrackLongBase
    {
        public string Album { get; set; }
        public string Artist { get; set; }
    }

    public class TrackBase
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Duration_ms { get; set; }
    }

    public class TrackLongBase : TrackBase
    {
        public string Href { get; set; }
        public string Type { get; set; }
        public string Uri { get; set; }
    }

    public class TrackCover : TrackLongBase
    {
        public dynamic Album { get; set; }
        public dynamic Artists { get; set; }
    }
}