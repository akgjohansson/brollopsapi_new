using ChamberOfSecrets;
using DataBase;
using DataBase.Dtos;
using DataBase.Models;
using MailManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace API_brollop.Controllers
{
    [RoutePrefix("person"), EnableCors(origins: "http://localhost:9000", headers: "*", methods: "*")]
    public class PersonController : ApiController
    {
        private SecretContainer _secretContainer;
        public PersonController()
        {
            _secretContainer = new SecretContainer("brollop_mail");
        }
        [Route("{referenceCode}"),HttpGet]
        public IHttpActionResult Get(string referenceCode)
        {
            using (var helper = new DataBaseHelper())
            {
                var company = helper.GetCompanyIdByReferenceCode(referenceCode);
                if (company == null)
                    return BadRequest("Anmälan hittades inte");
                var returnPersons = new List<Person>();
                company.Persons.ToList().ForEach(p => {
                    returnPersons.Add(new Person
                    {
                        FirstName = p.FirstName,
                        LastName = p.LastName,
                        Phone = p.Phone,
                        Email = p.Email,
                        Going = p.Going,
                        FoodPreferences = p.FoodPreferences.Select(f => new FoodPreference { EnglishName = f.EnglishName, SwedishName = f.SwedishName }).ToList()
                    });
                });
                var returnCompany = new Company
                {
                    Id = company.Id,
                    Comment = company.Comment,
                    Persons = returnPersons
                };
                return Ok(returnCompany);
            }
        }

        [Route("guestlist"), HttpGet]
        public IHttpActionResult GetGuestList()
        {
            using (var helper = new DataBaseHelper())
            {
                var companiesFromDb = helper.GetAllCompanies();
                var companies = new List<Company>();
                foreach (var company in companiesFromDb)
                {
                    companies.Add(new Company
                    {
                        Comment = company.Comment,
                        Persons = company.Persons.Select<Person, Person>(p =>
                        {
                            {
                                return new Person
                                {
                                    FirstName = p.FirstName,
                                    LastName = p.LastName,
                                    Email = p.Email,
                                    Phone = p.Phone,
                                    FoodPreferences = p.FoodPreferences.Select<FoodPreference, FoodPreference>(f =>
                                    {
                                        return new FoodPreference
                                        {
                                            SwedishName = f.SwedishName,
                                            EnglishName = f.EnglishName
                                        };
                                    }).ToList(),
                                    Going = p.Going
                                };
                            }
                        }).ToList()
                    });
                }
                return Ok(companies);
            }
        }

        [Route("registration"),HttpPost]
        public IHttpActionResult Post(CompanyPostDto persons)
        {
            using (var helper = new DataBaseHelper())
            {
                var accessCode = $"{persons.Persons[0].FirstName.Substring(0, 2)}{persons.Persons[0].LastName.Substring(0, 2)}";
                while (helper.IsAccessCodeExist(accessCode))
                {
                    var random = new Random();
                    accessCode += random.Next(10);
                }
                var id = helper.RegisterCompany(persons, accessCode);
                var emails = new List<string>();
                persons.Persons.ForEach(x => emails.Add(x.Email));
                var going = false;
                persons.Persons.ForEach(x => { if (x.Going) going = true; });
                var goingTextSwe = going ? $"Vad roligt det ska bli att se {GetAckusativePronoun(persons.Persons.Count)}" : $"Vad synd att {GetPronoun(persons.Persons.Count)} inte kan komma";
                var goingTextEng = going ? "It's nice to know you'll be at the" : "Too bad too hear you cannot attend our";
                var byeTextSwe = going ? "Vi ses på bröllopet!" : "Ha det bra!";
                var byeTextEng = going ? "See you at the wedding!" : "Cheers!";
                using (var mailManager = new MailManager.MailManager("smtp.gmail.com", 587, _secretContainer.UserName,_secretContainer.UserName,_secretContainer.Password))
                {
                    var text = $"==> English below\n\nHej {FormatGuestList(persons.Persons)}!\n\n{goingTextSwe} " +
                        $"på vårt bröllop! Håll gärna koll på hemsidan framöver, för där kommer all nödvändig information att vara med. Tveka inte " +
                        $"att höra av {GetAckusativePronoun(persons.Persons.Count)} om det är något {GetPronoun(persons.Persons.Count)} undrar över!\n\nOm " +
                        $"{GetPronoun(persons.Persons.Count)} vill redigera {GetPossessivePronoun(persons.Persons.Count)} anmälan, kan " +
                        $"{GetPronoun(persons.Persons.Count)} göra det genom att gå in på anmälningssidan hos hemsidan och där trycka " +
                        $"\"redigera anmälning\". Där fyller {GetPronoun(persons.Persons.Count)} i koden {accessCode} och trycker enter.\n\n" +
                        $"{byeTextSwe}\n\nJohanna och Andreas\n\n\n\n\n\n\n\n\n======================\n\n\n\n\n\n\n\n\n" +
                        $"Dear {FormatGuestList(persons.Persons)},\n\n{goingTextEng} " +
                        $"wedding! Please keep an eye at the website, we will post all necessary information there. Don't hesitate to contact" +
                        $" us if there is anything that is unclear.\n\nYou can edit the registration by going to the registration page at the " +
                        $"website and select 'edit'. There you fill the code {accessCode} and press enter to edit\n\n" +
                        $"{byeTextEng}\n\nJohanna and Andreas";
                    mailManager.SendMail(emails, "Välkommen på bröllop!", text);
                    string textToUs = GenerateTextToUs(persons);

                    mailManager.SendMail(new List<string> { _secretContainer.UserName }, "Ny anmälan", textToUs);
                }

                return Ok(accessCode);
            }
        }

        [Route("registration/{id:Guid}"),HttpPut]
        public IHttpActionResult Put(Guid id, CompanyPostDto company)
        {
            using (var helper = new DataBaseHelper())
            {
                bool success = helper.UpdateCompany(id, company);
                if (success)
                {
                    using (var mailManager = new MailManager.MailManager("smtp.gmail.com", 587, _secretContainer.UserName, _secretContainer.UserName, _secretContainer.Password))
                    {
                        var text = $"Hej {FormatGuestList(company.Persons)}!\n\n{GetPossessivePronoun(company.Persons.Count, true)} anmälan är uppdaterad. Vi ses på bröllopet!\n\nVarma hälsningar,\n" +
                            $"Johanna och Andreas";
                        mailManager.SendMail(company.Persons.Select(c => c.Email), "Bröllopsanmälan är uppdaterad", text);

                        var textToUs = GenerateTextToUs(company);
                        mailManager.SendMail(new List<string> { _secretContainer.UserName }, "Anmälan har uppdaterats", textToUs);
                    }
                        return Ok();
                }
                return BadRequest();
            }
        }

        private string GenerateTextToUs(CompanyPostDto persons)
        {
            var guestList = "";
            foreach (var guest in persons.Persons)
            {
                guestList += $"{guest.FirstName}: \n";
                guestList += $"Kommer: ";
                guestList += guest.Going ? "Ja\n" : "Nej\n";
                guest.FoodPreferences.ForEach(f => guestList += $"{f.SwedishName} ");
                guestList += $"E-post: {guest.Email}\n";
                guestList += $"Telefon: {guest.Phone}\n\n\n";

            }

            var textToUs = $"Anmälan har kommit in\n\n Gäster: {FormatGuestList(persons.Persons)}\n\n" +
                guestList;
            return textToUs;
        }


        private string FormatGuestList(List<PersonDto> persons)
        {
            string output = "";
            for (int i = 0; i < persons.Count; i++)
            {
                output += persons[i].FirstName;
                if (i == persons.Count - 2)
                    output += " & ";
                else if (i < persons.Count - 2)
                    output += ", ";
            }
            return output;
        }

        private string GetAckusativePronoun(int count)
        {
            return count == 1 ? "dig": "er";
                
        }

        private string GetPossessivePronoun(int count, bool capitalLetter = false)
        {
            if (count == 1)
                return capitalLetter?"Din":"din";
            return capitalLetter?"Er":"er";
        }

        private string GetPronoun(int count)
        {
            if (count == 1)
                return "du";
            return "ni";
        }
    }
}