using DataBase;
using DataBase.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace API_brollop.Controllers
{
    [RoutePrefix("contact"), EnableCors(origins: "http://localhost:9000", headers: "*", methods: "*")]
    public class ContactController : ApiController
    {
        [Route(""), HttpGet]
        public IHttpActionResult Get()
        {
            using (var helper = new DataBaseHelper())
            {
                var contacts = helper.GetContactList();
                if (contacts == null)
                    return NotFound();
                var orderedContacts = contacts.OrderBy(c => c.SwedishRole).ToList();
                var output = new List<ContactResponseDto>();
                orderedContacts.ForEach(c =>
                {
                    if (c.SwedishRole.ToLower() == "brudparet")
                        output.Add(c);
                });
                orderedContacts.ForEach(c =>
                {
                    if (c.SwedishRole.ToLower() == "toast masters")
                        output.Add(c);
                });
                orderedContacts.ForEach(c =>
                {
                    if (c.SwedishRole.ToLower() != "brudparet" && c.SwedishRole.ToLower() != "toast masters")
                        output.Add(c);
                });
                return Ok(output);
            }
        }

        [Route(""), HttpPost]
        public IHttpActionResult Post(ContactPostDto contact)
        {
        //    if (string.IsNullOrWhiteSpace(contact.FirstName) || string.IsNullOrWhiteSpace(contact.LastName) || string.IsNullOrWhiteSpace(contact.Phone) || string.IsNullOrWhiteSpace(contact.Email) || string.IsNullOrWhiteSpace(contact.SwedishRole) || string.IsNullOrWhiteSpace(contact.EnglishRole))
        //        return BadRequest();
            using (var helper = new DataBaseHelper())
            {
                helper.AddOrUpdateContact(contact);
                return Ok();
            }
        }
    }
}