using DataBase;
using DataBase.Dtos;
using DataBase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace API_brollop.Controllers
{
    [RoutePrefix("menu"), EnableCors(origins: "http://localhost:9000", headers: "*", methods: "*")]
    public class MenuController : ApiController
    {
        [Route(""), HttpGet]
        public IHttpActionResult Get()
        {
            using (var helper = new DataBaseHelper())
            {
                //var tja = User;
                //var hej = RequestContext.Principal;
                
                helper.RegisterLoadingOfPage();
                var menuItems = helper.GetMenuItems();
                if (menuItems == null)
                    return NotFound();
                return Ok(menuItems);
            }
        }

        [Route(""), HttpPost]
        public IHttpActionResult Post(MenuPostDto menu)
        {
            using (var helper = new DataBaseHelper())
            {
                var id = helper.CreateOrUpdateMenuItem(new MenuItem { Swedish = menu.Swedish, English = menu.English, Navigation = menu.Navigation });
                if (id == Guid.Empty)
                    return BadRequest();
                return Ok(id);
            }
        }
    }
}