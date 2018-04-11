using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_brollop.Extensions
{

    public static class ClientExtensions
    {
        public static Uri Append(this Uri uri, string append)
        {
            return new Uri(uri, append);
        }
    }
}