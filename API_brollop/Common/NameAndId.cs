using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_brollop.Common
{
    public class HasName
    {
        public string Name { get; set; }
    }
    public class NameAndId : HasName
    {
        public string Id { get; set; }
    }
}