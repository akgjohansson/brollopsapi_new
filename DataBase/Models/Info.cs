using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Models
{
    public class Info
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Swedish { get; set; }
        public virtual string English { get; set; }
    }
}
