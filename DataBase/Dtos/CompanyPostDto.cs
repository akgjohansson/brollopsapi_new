using DataBase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Dtos
{
    public class CompanyPostDto
    {
        public List<PersonDto> Persons { get; set; }
        public string Comment { get; set; }
    }

    public class PersonDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public List<FoodPreferenceDto> FoodPreferences { get; set; }
        public bool Going { get; set; }
    }
}
