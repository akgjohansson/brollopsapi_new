using DataBase.Models;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBase.Dtos;
using API_brollop.Common;

namespace DataBase
{
    public class DataBaseHelper : DataBaseHandler
    {
        public DataBaseHelper(string connectionString = "") : base(connectionString)
        {
        }

        public object GetMenuItems()
        {
            var query = Session.Query<MenuItem>().ToFuture();
            if (!query.Any())
                return null;
            return query;
        }

        public void RegisterLoadingOfPage()
        {
            using (var transaction = Session.BeginTransaction())
            {
                var loading = new Loadings
                {
                    Time = DateTime.Now
                };
                Session.Save(loading);
                transaction.Commit();
            }
        }

        public int GetNumberOfLoadings()
        {
            var loadings = Session.Query<Loadings>().ToList().Count;
            return loadings;
        }

        public IEnumerable<string> GetEmailAddresses()
        {
            var emailAddresses = Session.Query<Person>().Select(p => p.Email).Distinct();
            return emailAddresses;
        }

        public IEnumerable<LodgingType> GetLodgningTypes()
        {
            var lodgingTypes = Session.Query<LodgingType>().ToFuture();
            return lodgingTypes;
        }

        public Company GetCompanyIdByReferenceCode(string referenceCode)
        {
            var companyQuery = Session.Query<Company>().Where(c => c.AccessCode == referenceCode).FirstOrDefault();
            return companyQuery;
        }

        public Guid RegisterCompany(CompanyPostDto company, string accessCode)
        {
            
            Company newCompany = new Company();
            newCompany = new Company
            {
                Comment = company.Comment,
                AccessCode = accessCode
            };

            using (var companyTransaction = Session.BeginTransaction())
            {
                Session.Save(newCompany);
                companyTransaction.Commit();
            }
            foreach (var person in company.Persons)
            {
                var newPerson = new Person
                {
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    Phone = person.Phone,
                    Email = person.Email,
                    FoodPreferences = new List<FoodPreference>(),
                    Going = person.Going,
                    Company = newCompany
                };
                if (person.FoodPreferences != null)
                {
                    foreach (var foodPreference in person.FoodPreferences)
                    {
                        Guid foodPreferenceId;
                        var foodPreferenceQuery = Session.Query<FoodPreference>().Where(f => (f.SwedishName.ToLower() == foodPreference.SwedishName.ToLower() && !string.IsNullOrWhiteSpace(foodPreference.SwedishName)) || (f.EnglishName.ToLower() == foodPreference.EnglishName.ToLower() && !string.IsNullOrWhiteSpace(foodPreference.EnglishName)));
                        if (foodPreferenceQuery.Any())
                            foodPreferenceId = foodPreferenceQuery.First().Id;
                        else
                        {
                            var newFoodPreference = new FoodPreference { SwedishName = foodPreference.SwedishName, EnglishName = foodPreference.EnglishName };
                            using (var foodPreferenceTransaction = Session.BeginTransaction())
                            {
                                Session.Save(newFoodPreference);
                                foodPreferenceTransaction.Commit();
                            }
                            foodPreferenceId = newFoodPreference.Id;
                        }
                        newPerson.FoodPreferences.Add(new FoodPreference { Id = foodPreferenceId });
                    }
                }
                using (var personTransaction = Session.BeginTransaction())
                {
                    Session.Save(newPerson);
                    personTransaction.Commit();
                }
            }
            return newCompany.Id;
        }

        public SpotifySession GetLastSession()
        {
            var token = Session.Query<SpotifySession>().OrderByDescending(x => x.Time).FirstOrDefault();
            return token;
        }

        public List<Company> GetAllCompanies()
        {
            var companies = Session.Query<Company>().ToList();
            return companies;
        }

        public bool IsAccessCodeExist(string accessCode)
        {
            return Session.Query<Company>().Any(x => x.AccessCode.ToLower() == accessCode.ToLower());
        }

        private void EmptyPersonsFromDatabase(ICollection<Person> persons)
        {
            foreach (var person in persons)
            {
                var existingPerson = Session.Query<Person>().Where(p => p.FirstName == person.FirstName && p.LastName == person.LastName).FirstOrDefault();
                if (existingPerson != null)
                    Session.Delete(existingPerson);
            }
        }

        public bool UpdateCompany(Guid id, CompanyPostDto company)
        {
            var existCompany = Session.Query<Company>().Where(c => c.Id == id).FirstOrDefault();
            if (existCompany == null)
                return false;
            try
            {
                using (var transaction = Session.BeginTransaction())
                {
                    Session.Delete(existCompany);
                    transaction.Commit();
                }
                var output = RegisterCompany(company, existCompany.AccessCode);
                return true;


            } catch (Exception e)
            {
                using (var transaction = Session.BeginTransaction())
                {
                    Session.Save(existCompany);
                    transaction.Commit();
                    return false;
                }
            }
        }

        public bool UpdateFoodPreference(FoodPreference foodPreference)
        {
            var existingFoodPreference = Session.Query<FoodPreference>().Where(f => f.Id == foodPreference.Id).FirstOrDefault();
            if (existingFoodPreference == null) return false;
            existingFoodPreference.SwedishName = foodPreference.SwedishName;
            existingFoodPreference.EnglishName = foodPreference.EnglishName;
            using (var transaction = Session.BeginTransaction())
            {
                Session.Update(existingFoodPreference);
                transaction.Commit();
            }
            return true;
        }

        public List<FoodPreference> GetFoodPreferences()
        {
            var foodPreferences = Session.Query<FoodPreference>().Select(x => new FoodPreference
            {
                Id = x.Id,
                EnglishName = x.EnglishName,
                SwedishName = x.SwedishName
            }).ToList();
            return foodPreferences;
        }

        public List<ContactResponseDto> GetContactList()
        {
            var report = new List<ContactResponseDto>();
            var contacts = Session.Query<Contact>().ToFuture();
            if (!contacts.Any())
                return null;

            foreach (var contact in contacts)
            {
                var thisContact = new ContactFlatDto
                {
                    FirstName = contact.FirstName,
                    LastName = contact.LastName,
                    Email = contact.Email,
                    Phone = contact.Phone
                };
                var existingRole = report.Where(c => c.SwedishRole == contact.SwedishRole).FirstOrDefault();
                if (existingRole == null)
                {
                    report.Add(new ContactResponseDto
                    {
                        SwedishRole = contact.SwedishRole,
                        EnglishRole = contact.EnglishRole,
                        Contacts = new List<ContactFlatDto> { thisContact }
                    });
                }
                else
                {
                    for (int i = 0; i < report.Count; i++)
                    {
                        if (report[i].SwedishRole == contact.SwedishRole)
                        {
                            report[i].Contacts.Add(thisContact);
                        }
                    }
                }
            }
            return report;
        }

        public List<InfoDto> GetInfos()
        {
            var query = Session.Query<Info>().ToFuture();
            if (!query.Any())
                return null;
            var output = new List<InfoDto>();
            foreach (var info in query)
            {
                output.Add(new InfoDto
                {
                    English = info.English,
                    Swedish = info.Swedish,
                    Name = info.Name
                });
            }
            return output;
        }

        public void SaveToken(Token token)
        {
            var existingToken = Session.Query<Token>().FirstOrDefault(t => t.Expiration_date == token.Expiration_date);
            if (existingToken == null)
            {
                using (var transaction = Session.BeginTransaction())
                {
                    Session.Save(token);
                    transaction.Commit();
                }
            }
        }
        public void SaveSession(Token token, string deviceId) {
            var existingToken = Session.Query<Token>().First(t => t.Expiration_date == token.Expiration_date);
            using (var transaction = Session.BeginTransaction())
            {
                var spotifySession = new SpotifySession
                {
                    DeviceId = deviceId,
                    Token = token,
                    Time = DateTime.Now
                };
                Session.Save(spotifySession);
                transaction.Commit();
            }
        }

        public void AddOrUpdateContact(ContactPostDto newContact)
        {
            using (var transaction = Session.BeginTransaction())
            {
                Contact contact;
                var existContact = Session.Query<Contact>().Where(c => c.FirstName.ToLower() == newContact.FirstName.ToLower() && c.LastName.ToLower() == newContact.LastName.ToLower()).ToFuture();
                if (existContact.Any())
                {
                    contact = existContact.First();
                }
                else
                {
                    contact = new Contact();
                }
                contact.FirstName = newContact.FirstName;
                contact.LastName = newContact.LastName;
                contact.Email = newContact.Email;
                contact.Phone = newContact.Phone;
                contact.SwedishRole = newContact.SwedishRole;
                contact.EnglishRole = newContact.EnglishRole;
                if (existContact.Any())
                    Session.Update(contact);
                else
                    Session.Save(contact);
                transaction.Commit();
            }
        }

        public void CreateOrUpdateInfoItem(InfoDto newInfo)
        {
            using (var transaction = Session.BeginTransaction())
            {
                Info info;
                var existInfo = Session.Query<Info>().Where(i => i.Name.ToLower() == newInfo.Name.ToLower()).ToFuture();
                if (existInfo.Any())
                {
                    info = existInfo.First();
                    info.Name = newInfo.Name;
                    info.Swedish = newInfo.Swedish;
                    info.English = newInfo.English;
                }
                else
                {
                    info = new Info
                    {
                        Name = newInfo.Name,
                        Swedish = newInfo.Swedish,
                        English = newInfo.English
                    };
                }
                Session.SaveOrUpdate(info);
                transaction.Commit();
            }
        }

        public Guid CreateOrUpdateMenuItem(MenuItem menu)
        {
            using (var transaction = Session.BeginTransaction())
            {
                var exists = Session.Query<MenuItem>().Where(m => m.Swedish.ToLower() == menu.Swedish.ToLower() || m.English.ToLower() == menu.English.ToLower() || m.Navigation == menu.Navigation).ToList();
                if (exists.Count == 0)
                {
                    if (!string.IsNullOrWhiteSpace(menu.Swedish) && !string.IsNullOrWhiteSpace(menu.English) && !string.IsNullOrWhiteSpace(menu.Navigation))
                    {
                        Session.Save(menu);
                        transaction.Commit();
                        return menu.Id;
                    }
                    return Guid.Empty;
                }
                else if (exists.Count == 1)
                {
                    var dbMenu = exists[0];
                    var update = new MenuItem { Swedish = dbMenu.Swedish, English = dbMenu.English, Navigation = dbMenu.Navigation, Id = dbMenu.Id };
                    Session.Update(update);
                    transaction.Commit();
                    return update.Id;
                }
                else
                {
                    return Guid.Empty;
                }
            }
        }
    }
}
