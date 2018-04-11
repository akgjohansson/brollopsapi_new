using ChamberOfSecrets;
using DataBase;
using DataBase.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace API_brollop.Controllers
{
    [RoutePrefix("admin"), EnableCors(origins: "http://localhost:9000", headers: "*", methods: "*")]
    public class AdminController : ApiController
    {
        private SecretContainer _secretContainer;
        public AdminController()
        {
            _secretContainer = new SecretContainer("brollop_mail");
        }
        [Route("{password}")]
        public IHttpActionResult Get(string password)
        {
            if (password == "JA2018-pass")
                return Ok();
            return BadRequest();
        }

        [Route("loading"),HttpGet]
        public IHttpActionResult GetLoadings()
        {
            using (var helper = new DataBaseHelper())
            {
                var loadings = helper.GetNumberOfLoadings();
                return Ok(loadings);
            }
        }

        [Route("mail"), HttpPost]
        public IHttpActionResult SendMail(MailPostDto emailDto)
        {
            using (var helper = new DataBaseHelper())
            {
                var emails = helper.GetEmailAddresses();
                using (var client = new MailManager.MailManager("smtp.gmail.com", 587, _secretContainer.UserName, _secretContainer.UserName, _secretContainer.Password))
                {
                    //client.SendMail(new List<string> { "johansson.mcquack@gmail.com", "sossatina@gmail.com" }, emailDto.Subject, emailDto.Message);
                    client.SendMail(emails, emailDto.Subject, emailDto.Message);
                    //var report = JsonConvert.SerializeObject(new { Content = emails.Select(m => new { Address = m }) });
                    //return Ok(report);
                }
            }
            return Ok();
        }
    }
}