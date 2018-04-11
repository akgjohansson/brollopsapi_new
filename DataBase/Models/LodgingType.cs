using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Models
{
    public class LodgingType
    {
        public virtual Guid Id { get; set; }
        public virtual string SwedishName { get; set; }
        public virtual string EnglishName { get; set; }
        public virtual ICollection<Lodging> Lodgings { get; set; }
    }
}
