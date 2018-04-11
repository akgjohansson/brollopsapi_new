using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace API_brollop.Controllers
{
    [RoutePrefix("validation"), EnableCors(origins: "http://localhost:9000", headers: "*", methods: "*")]
    public class ValidationController : ApiController
    {
        [Route("email"), HttpGet]
        public IHttpActionResult GetEmailValidation(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return Ok(addr.Address == email);
            } catch {
                return Ok(false);
            }
        }
    }
}