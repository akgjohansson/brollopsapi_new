using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Models
{
    public class FoodPreference
    {
        public virtual Guid Id { get; set; }
        public virtual string SwedishName { get; set; }
        public virtual string EnglishName { get; set; }
        public virtual ICollection<Person> Persons { get; set; }
    }
}
