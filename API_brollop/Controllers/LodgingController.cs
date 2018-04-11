using DataBase;
using DataBase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace API_brollop.Controllers
{
    [RoutePrefix("lodging"), EnableCors(origins: "http://localhost:9000", headers: "*", methods: "*")]
    public class LodgingController : ApiController
    {
        [Route(""), HttpGet]
        public IHttpActionResult Get()
        {
            using (var helper = new DataBaseHelper())
            {
                var lodgingTypes = helper.GetLodgningTypes();
                var output = new List<LodgingType>();
                foreach (var lodgingType in lodgingTypes)
                {
                    var lodgings = new List<Lodging>();
                    foreach (var lodging in lodgingType.Lodgings)
                    {
                        lodgings.Add(new Lodging
                        {
                            SwedishName = lodging.SwedishName,
                            EnglishName = lodging.EnglishName,
                            Url = lodging.Url
                        });
                    }
                    output.Add(new LodgingType
                    {
                        SwedishName = lodgingType.SwedishName,
                        EnglishName = lodgingType.EnglishName,
                        Lodgings = lodgings
                    });
                }
                return Ok(output);
            }
        }
    }
}