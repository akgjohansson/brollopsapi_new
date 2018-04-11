using DataBase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_brollop.Common
{
    public class Token
    {
        public virtual Guid Id { get; set; }
        public virtual string Access_token { get; set; }
        public virtual string Refresh_token { get; set; }
        public virtual int Expires_in { get; set; }
        public virtual DateTime Expiration_date { get; set; }
        public virtual bool IsExpired => DateTime.Now > Expiration_date;
        public virtual IEnumerable<SpotifySession> SpotifySession { get; set; }
    }
}