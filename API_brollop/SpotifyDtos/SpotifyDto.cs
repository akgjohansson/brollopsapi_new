using System.Collections.Generic;

namespace API_brollop.Controllers
{
    public class SpotifyDto
    {
        public Dictionary<string, dynamic> Artists { get; set; }
        public Dictionary<string, dynamic> Tracks { get; set; }
    }

    public class SpotifyQueueDto
    {
        public string Id { get; set; }
    }
}