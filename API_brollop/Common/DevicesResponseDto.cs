using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_brollop.Common
{
    public class Device
    {
        public string Id { get; set; }
        public bool IsActive { get; set; }
        public bool IsRestricted { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public float? Volume_Percent { get; set; }
    }

    public class DevicesResponseDto
    {
        public List<Device> Devices { get; set; }
    }
}