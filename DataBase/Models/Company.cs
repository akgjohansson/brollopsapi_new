using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Models
{
    public class Company
    {
        public virtual Guid Id { get; set; }
        public virtual ICollection<Person> Persons { get; set; }
        public virtual string Comment { get; set; }
        public virtual string AccessCode { get; set; }
    }
}
