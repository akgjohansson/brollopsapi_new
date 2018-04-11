using DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace API_brollop.Controllers
{
    [RoutePrefix("start"), EnableCors(origins: "http://localhost:9000", headers: "*", methods: "*")]
    public class StartController : ApiController
    {
        [Route("helloworld"), HttpGet]
        public IHttpActionResult HelloWorld()
        {
            return Ok("Hello world!");
        }

        [Route("remainingdays"),HttpGet]
        public IHttpActionResult GetRemainingDays()
        {
            var weddingDate = new DateTime(2018, 05, 19);
            var today = DateTime.Today;
            var totalDays = (weddingDate - today).TotalDays;
            return Ok(totalDays-1);
        }

        [Route("builddb/{id:Guid}"), HttpPost]
        public IHttpActionResult CreateDataBase(Guid id)
        {
            if (id == Guid.Parse("73f72bd4-e9ba-4941-bbb3-7e298c61b535"))
            {
                using (var helper = new DataBaseHelper())
                {
                    helper.BuildDatabase();
                    return Ok();
                }
            }
            else
            {
                return BadRequest();
            }
        }
    }
}