using API_brollop.Client;
using API_brollop.Common;
using API_brollop.SpotifyDtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace API_brollop.SpotifyControllers
{
    [RoutePrefix("spotify/devices"), EnableCors(origins: "http://localhost:9000", headers: "*", methods: "*")]
    public class DevicesController : ApiController
    {
        private ISpotifyApi _api;
        public DevicesController(ISpotifyApi spotifyApi)
        {
            _api = spotifyApi;
            if (_api.IsExpired)
                _api.RefreshTokens();
        }

        [Route(""), HttpGet]
        public async Task<IHttpActionResult> GetDevices()
        {
            var response = await _api.GetDevices();
            var devicesCover = JsonConvert.DeserializeObject<DevicesCover>(response);
            var report = new DevicesResponseDto { Devices = new List<Device>() };
            foreach (var device in devicesCover.Devices)
            {
                report.Devices.Add(JsonConvert.DeserializeObject<Device>(Convert.ToString(device)));
            }
            return Ok(report);
        }

        [Route(""), HttpPut]
        public  async Task<IHttpActionResult> Put([FromBody] DevicesPostDto device)
        {
            var response = await _api.UpdateDevice(device);
            return Ok(response);
        }

        [Route(""), HttpPost]
        public async Task<IHttpActionResult> Post(DefaultDevicePostDto dto)
        {
            _api.DefaultDeviceId = dto.DeviceId;
            await _api.UpdateDevice(new DevicesPostDto
            {
                Device_Id = dto.DeviceId,
                Play = true
            });
            _api.SaveSession();
            return Ok("Success");
        }
    }
}