using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_brollop.Common
{
    public class Owner
    {
        public string Display_Name { get; set; }
        public IEnumerable<External_Url> External_Urls { get; set; }
        public string Href { get; set; }
        public string Id { get; set; }
        public string Uri { get; set; }
    }
}