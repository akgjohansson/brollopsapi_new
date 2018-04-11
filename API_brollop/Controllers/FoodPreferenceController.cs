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
    [RoutePrefix("foodpreference"), EnableCors(origins: "http://localhost:9000", headers: "*", methods: "*")]
    public class FoodPreferenceController: ApiController
    {
        [Route(""),HttpGet]
        public IHttpActionResult Get()
        {
            using (var helper = new DataBaseHelper())
            {
                var foodPreferences = helper.GetFoodPreferences();
                return Ok(foodPreferences);
            }
        }

        [Route(""), HttpPut]
        public IHttpActionResult Put(FoodPreference foodPreference)
        {
            using (var helper = new DataBaseHelper())
            {
                var response = helper.UpdateFoodPreference(foodPreference);
                if (response)
                    return Ok("Uppdateringen lyckades");
                else return BadRequest("Något gick fel");
            }
        }
    }
}