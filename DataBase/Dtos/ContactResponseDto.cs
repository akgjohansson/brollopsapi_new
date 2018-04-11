using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Dtos
{
    public class ContactResponseDto
    {
        public string SwedishRole { get; set; }
        public string EnglishRole { get; set; }
        public ICollection<ContactFlatDto> Contacts { get; set; }
    }

    public class ContactFlatDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}
