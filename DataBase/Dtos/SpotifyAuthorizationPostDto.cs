using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Dtos
{
    public class SpotifyAuthorizationBaseDto
    {
        public string grant_type { get; set; }
    }
    public class SpotifyAuthorizationPostDto : SpotifyAuthorizationBaseDto
    {
        public string code { get; set; }
        public string redirect_uri { get; set; }
        public string scope { get; set; }
    }

    public class SpotifyRefreshAuthorizationPostDto : SpotifyAuthorizationBaseDto
    {
        public string refresh_token { get; set; }
    }
}
