using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_brollop.SpotifyDtos
{
    public class DevicesPostDto
    {
        public string Device_Id { get; set; }
        public bool Play { get; set; }
    }

    public class DefaultDevicePostDto
    {
        public string DeviceId { get; set; }
    }
}