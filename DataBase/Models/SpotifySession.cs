using API_brollop.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Models
{
    public class SpotifySession
    {
        public virtual Guid Id { get; set; }
        public virtual Token Token { get; set; }
        public virtual string DeviceId { get; set; }
        public virtual DateTime Time { get; set; }
    }
}
