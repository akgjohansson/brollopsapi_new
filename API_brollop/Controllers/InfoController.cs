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
    
    [RoutePrefix("info"), EnableCors(origins: "http://localhost:9000", headers: "*", methods: "*")]
    public class InfoController : ApiController
    {
        public InfoController()
        {

        }
        [Route(""), HttpGet]
        public IHttpActionResult Get()
        {
            using (var helper = new DataBaseHelper())
            {
                var infoArray = helper.GetInfos();
                if (infoArray == null)
                    return NotFound();
                return Ok(infoArray);
            }
        }

        [Route(""), HttpPost]
        public IHttpActionResult Post(InfoDto info)
        {
            if (string.IsNullOrWhiteSpace(info.Name) || string.IsNullOrWhiteSpace(info.Swedish) || string.IsNullOrWhiteSpace(info.English))
            {
                return BadRequest("Fields cannot be emtpy");
            }
            using (var helper = new DataBaseHelper())
            {
                helper.CreateOrUpdateInfoItem(info);
                return Ok();
            }
        }
    }
}