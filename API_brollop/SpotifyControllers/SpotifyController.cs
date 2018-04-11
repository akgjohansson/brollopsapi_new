using API_brollop.Client;
using API_brollop.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Net.Http;
using System.IO;
using System.Web.Script.Serialization;
using DataBase.Dtos;

namespace API_brollop.Controllers
{
    [RoutePrefix("spotify"), EnableCors(origins: "http://localhost:9000", headers: "*", methods: "*")]
    public class SpotifyController : ApiController
    {
        private ISpotifyApi _api;
        public SpotifyController(ISpotifyApi spotifyApi)
        {
            _api = spotifyApi;
            if (_api.IsExpired)
                _api.RefreshTokens();
        }

        [Route("state"), HttpGet]
        public async Task<IHttpActionResult> GetCurrentlyPlaying()
        {
            var response = await _api.GetCurrentPlaybackState();
            return Ok(response);
        }

        [Route("login"), HttpGet]
        public async Task<IHttpActionResult> Login(string authorization_code)
        {
            var request = Request;
            if (string.IsNullOrEmpty(authorization_code))
            {
                return ResponseMessage(new HttpResponseMessage(HttpStatusCode.NoContent));
            }
            var response = await _api.GetAuthorizationTokens(authorization_code);
            return Ok(response);
            //var response = await _api.InitiateAuthorizationFlow(authorization_code);
            //var content = await response.Content.ReadAsStringAsync();
            //return Ok(content);
        }

        [Route("bearer"), HttpGet]
        public IHttpActionResult GetBearer()
        {
            return Ok(_api.ClientByteArray);
        }



        [Route("playlists")]
        public async Task<IHttpActionResult> GetPlaylists()
        {
            var playlists = await _api.GetPlaylists();
            return Ok(playlists);
            
        }

        
        [Route("search")]
        public async Task<IHttpActionResult> Get(string s)
        {
            var response = _api.Get(s);
            if (response.StatusCode != HttpStatusCode.OK)
                return ResponseMessage(Request.CreateResponse(response.StatusCode, response.Content));
            var result = await response.Content.ReadAsStreamAsync();
            var data = JsonConvert.DeserializeObject<SpotifyDto>(await response.Content.ReadAsStringAsync());

            var spotify = new Spotify { Tracks = new List<Track>() };
            IEnumerable<Dictionary<string, dynamic>> jsonTracks = JsonConvert.DeserializeObject<IEnumerable<Dictionary<string, dynamic>>>(data.Tracks["items"].ToString());
            foreach (var item in jsonTracks)
            {
                List<Dictionary<string, dynamic>> artistList = JsonConvert.DeserializeObject<List<Dictionary<string, dynamic>>>(item["artists"].ToString());
                var halloj = artistList.Select(a => a["name"].ToString()).ToList();
                //var testtest = artistList.Select(a => Convert.ToString(a)).ToList();
                //var tjo = ((IEnumerable<string>)artistList.Select(a => a["name"].ToString())).ToList();
                //var hej = (List<string>)((IEnumerable<string>)artistList.Select(a => a["name"].ToString())).ToList();
                spotify.Tracks.Add(new Track
                {
                    Href = item["href"],
                    Name = item["name"],
                    Id = item["id"],
                    Duration_ms = Convert.ToInt32(item["duration_ms"]),
                    Type = item["type"],
                    Album = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(item["album"].ToString())["name"],
                    Artist = ConvertToReadableList(artistList.Select(a => a["name"].ToString()).ToList())
                });

            }

            if (response.StatusCode == HttpStatusCode.OK)
                return Ok(spotify);
            else
                return BadRequest(response.Content.ToString());
        }

        [Route("queue"), HttpPost]
        public async Task<IHttpActionResult> Queue(SpotifyQueueDto dto)
        {
            if (_api.DefaultDeviceId == null)
                return BadRequest("Default device måste sättas");
            var response = await _api.QueueSong(dto.Id);
            return Ok(response);
        }

        [Route("next"), HttpPost]
        public async Task<IHttpActionResult> Next()
        {
            var response = await _api.NextSongInLine();
            return Ok(response);
        }

        private string ConvertToReadableList(List<dynamic> list)
        {
            var output = "";
            for (int i = 0; i < list.Count; i++)
            {
                output += list[i].ToString();
                if (i < list.Count - 2)
                    output += ", ";
                else if (i == list.Count - 2)
                    output += " och ";
            }
            return output;
        }
    }
}