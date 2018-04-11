using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Models
{
    public class Loadings
    {
        public virtual DateTime Time { get; set; }
        public virtual Guid Id { get; set; }
    }
}
