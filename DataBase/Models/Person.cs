using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Models
{
    public class Person
    {
        public virtual Guid Id { get; set; }
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual string Phone { get; set; }
        public virtual string Email { get; set; }
        public virtual Company Company { get; set; }
        public virtual bool Going { get; set; }
        public virtual ICollection<FoodPreference> FoodPreferences { get; set; }
    }
}
