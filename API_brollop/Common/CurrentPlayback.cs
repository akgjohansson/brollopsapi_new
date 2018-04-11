using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_brollop.Common
{
    public class CurrentPlayback
    {
        public string TimeStamp { get; set; }
        public Device Device { get; set; }
        public int Progress_ms { get; set; }
        public bool Is_Playing { get; set; }
        public TrackBase Item { get; set; }
        public bool Shuffle_State { get; set; }
        public string Repeat_State { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public int TimeLeft => Item.Duration_ms - Progress_ms;
    }
}