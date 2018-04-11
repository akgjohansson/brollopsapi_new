using API_brollop.Common;
using API_brollop.Extensions;
using ChamberOfSecrets;
using DataBase.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using API_brollop.SpotifyDtos;
using System.Collections;
using System.Linq;
using System.Threading;
using DataBase;

namespace API_brollop.Client
{
    public class SpotifyApi : ISpotifyApi
    {
        private Uri _baseUri;
        private string _clientID;
        private string _clientSecret;
        private HttpClient _client;
        private Token _token;
        public bool IsExpired => _token?.IsExpired ?? false;
        public bool IsInitiated => _token != null && DefaultDeviceId != null;
        public byte[] ClientByteArray { get { return new UTF8Encoding().GetBytes($"{_clientID}:{_clientSecret}"); } }
        public string DefaultDeviceId { get; set; }
        public List<Track> Queue { get; set; }
        public string BasicPlaylistId { get; set; }
        public string LandingPlaylistId { get; set; }
        public string RuntimeQueuePlaylistId { get; set; }
        public List<Track> BasicPlaylist { get; set; }
        public List<Track> LandingPlaylist { get; set; }
        public int LandingListSize { get; set; }
        public bool TakeFromQueue { get; set; }
        public SecretContainer _secretContainer { get; set; }
        public SpotifyApi()
        {
            var spotifyCredentials = new SecretContainer("spotify");
            _secretContainer = new SecretContainer("brollop_mail");
            _baseUri = new Uri("https://api.spotify.com");
            _clientID = spotifyCredentials.ClientId;
            _clientSecret = spotifyCredentials.ClientSecret;
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
            Queue = new List<Track>();
            LandingListSize = 4;
            TakeFromQueue = true;
            LandingPlaylist = new List<Track>();
            BasicPlaylistId = "0cWW0sr3Hu48SFdtdCcP6K";
            GetTokensFromDatabase();
        }

        private void GetTokensFromDatabase()
        {
            using (var helper = new DataBaseHelper())
            {
                var lastSession = helper.GetLastSession();
                Token token = null;
                if (lastSession != null)
                {
                    DefaultDeviceId = lastSession.DeviceId;
                    token = lastSession.Token;
                }
                if (token != null)
                {
                    _token = token;
                    if (token.IsExpired)
                    {
                        try
                        {
                            Task.WaitAll(RefreshTokens());
                        }
                        catch
                        {
                            _token = null;
                        }
                    }
                }
            }
        }

        public HttpResponseMessage Get(string searchString)
        {
            var uri = _baseUri.Append($"v1/search?q={searchString}&type=track%2Cartist&market=SE");
            var response = _client.GetAsync(uri).Result;
            return response;
        }

        public async Task<IEnumerable<Playlist>> GetPlaylists()
        {
            var request = ConstructRequestMessage("GET", "v1/me/playlists");
            var response = await _client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            var items = JsonConvert.DeserializeObject<ItemsCover>(content);
            var playlists = new List<Playlist>();
            foreach (var item in items.Items)
            {
                playlists.Add(JsonConvert.DeserializeObject<Playlist>(Convert.ToString(item)));
            }
            return playlists;
        }

        public async Task<string> GetDevices()
        {
            var request = ConstructRequestMessage("GET", "v1/me/player/devices");
            var response = await _client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        private HttpRequestMessage ConstructRequestMessage(string methodName, string path)
        {
            var uri = _baseUri.Append(path);
            var method = new HttpMethod(methodName);
            var request = new HttpRequestMessage(method, uri);
            request.Headers.Add("Authorization", $"Bearer {_token.Access_token}");
            return request;
        }

        public async Task<string> GetAuthorizationTokens(string authorization_code)
        {
            //var redirect_uri = "http://localhost:9000/";
            var redirect_uri = "http://launchpadmcquack-001-site3.htempurl.com/";

            var body = new SpotifyAuthorizationPostDto
            {
                code = authorization_code,
                grant_type = "authorization_code",
                scope = "user-read-private",
                redirect_uri = redirect_uri
            };
            var method = new HttpMethod("POST");
            var requestUri = new Uri("https://accounts.spotify.com/api/token");
            var request = new HttpRequestMessage(method, requestUri);
            request.Headers.Add("Authorization", $"Basic {Convert.ToBase64String(ClientByteArray)}");
            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string> ("code", authorization_code),
                new KeyValuePair<string, string> ("grant_type", "authorization_code"),
                new KeyValuePair<string, string> ("scope", "user-read-playback-state"),
                new KeyValuePair<string, string> ("redirect_uri", redirect_uri)
            });
            var response = await _client.SendAsync(request);
            var statusCode = response.StatusCode;
            var content = await response.Content.ReadAsStringAsync();
            UpdateToken(content);
            await SetPlaylistIds();
            await RepopulateBasicPlaylistIds();
            LandingPlaylist = await GetPlaylistTracks(LandingPlaylistId);
            return content;
        }

        private async Task SetPlaylistIds()
        {
            var request = ConstructRequestMessage("GET", "https://api.spotify.com/v1/me/playlists");
            var response = await _client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            var items = JsonConvert.DeserializeObject<ItemsCover>(content);
            var playLists = new List<Playlist>();
            foreach (var item in items.Items)
            {
                var playlist = JsonConvert.DeserializeObject<Playlist>(Convert.ToString(item));
                playLists.Add(playlist);
            }
            //BasicPlaylistId = playLists.First(p => p.Name == "Bröllop_playlist").Id;
            LandingPlaylistId = playLists.First(p => p.Name == "Bröllop_kö").Id;
            RuntimeQueuePlaylistId = playLists.First(p => p.Name == "Bröllop_runtime_queue").Id;
        }

        public async Task RefreshTokens()
        {
            try
            {
                if (_token == null) return;
                var method = new HttpMethod("POST");
                var requestUri = new Uri("https://accounts.spotify.com/api/token");
                var request = new HttpRequestMessage(method, requestUri);
                //request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                request.Headers.Add("Authorization", $"Basic {Convert.ToBase64String(ClientByteArray)}");
                //request.Headers.Authorization = new AuthenticationHeaderValue("Authorization", $"Basic {Convert.ToBase64String(ClientByteArray)}");
                request.Headers.Add("Cache-Control", "no-cache");
                //request.Content = new StringContent(JsonConvert.SerializeObject(new { grant_type = "refresh_token", refresh_token = _token.Refresh_token }));
                request.Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                {
                new KeyValuePair<string, string> ("grant_type", "refresh_token"),
                new KeyValuePair<string, string> ("refresh_token", _token.Refresh_token)
            });
                var response = await _client.SendAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    UpdateToken(content);
                }
            }
            catch (Exception error)
            {

            }
        }

        private void SaveToken()
        {
            using (var helper = new DataBaseHelper())
            {
                helper.SaveToken(_token);
            }
        }

        private void UpdateToken(string content)
        {
            _token = JsonConvert.DeserializeObject<Token>(content);
            _token.Expiration_date = DateTime.Now.AddSeconds(_token.Expires_in - 10);
            if (_client.DefaultRequestHeaders.Contains("Authorization"))
                _client.DefaultRequestHeaders.Remove("Authorization");
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token.Access_token}");
            SaveToken();
        }

        public async Task<string> UpdateDevice(DevicesPostDto device)
        {
            var request = ConstructRequestMessage("PUT", "https://api.spotify.com/v1/me/player");
            request.Content = new StringContent(JsonConvert.SerializeObject(new { device_ids = new List<string> { device.Device_Id }, play = device.Play }));
            //    new FormUrlEncodedContent(new[]
            //{
            //    new KeyValuePair<string, string> ("device_ids", $"[{device.Device_Ids}]"),
            //    new KeyValuePair<string, string> ("play", device.Play.ToString())
            //});
            var response = await _client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        public async Task<CurrentStateResponseDto> GetCurrentPlaybackState()
        {
            await MakeSureIdsAreSet();
            var currentPlaybackState = await GetState();

            await UpdatePlaybackState(currentPlaybackState);

            LandingPlaylist = await GetPlaylistTracks(LandingPlaylistId);
            Queue = await GetPlaylistTracks(RuntimeQueuePlaylistId);
            var doublettes = Queue.Where(t => LandingPlaylist.Any(p => p.Id == t.Id));
            foreach (var track in doublettes)
            {
                await RemoveSong(track, RuntimeQueuePlaylistId);
            }
            var playingTrackIndex = GetPlayingIndex(currentPlaybackState, LandingPlaylist);
            if (playingTrackIndex < 0)
            {
                IssueWarning("Starta om spellistan Bröllop_kö!");
            }
            else
            {
                if (playingTrackIndex > 1)
                {
                    await RemoveFirstLandingTrack();
                    if (LandingPlaylist.Count <= LandingListSize)
                    {
                        if (TakeFromQueue && Queue.Count > 0)
                            await AddQueuedSongToLandingList();
                        else
                            await AddBasicSongToLandingList();
                    }
                    playingTrackIndex = GetPlayingIndex(currentPlaybackState, LandingPlaylist);
                }
            }
            //await RepopulateLandingListIfNeeded(LandingPlaylist);
            var response = currentPlaybackState.ToDto(LandingPlaylist[playingTrackIndex + 1]);
            return response;
        }

        private async Task RemoveFirstLandingTrack()
        {
            var landingTracks = await GetPlaylistTracks(LandingPlaylistId);
            await RemoveSong(landingTracks[0], LandingPlaylistId);
        }

        private void IssueWarning(string warning)
        {
            using (var smtpClient = new MailManager.MailManager("smtp.gmail.com", 587, _secretContainer.UserName, _secretContainer.UserName, _secretContainer.Password))
            {
                smtpClient.SendMail(new List<string> { "johansson.mcquack@gmail.com" }, "Spotifyvarning!", warning);
            }
        }

        private static int GetPlayingIndex(CurrentPlayback currentPlaybackState, List<Track> landingTracks)
        {
            var playingTrackIndex = -1;
            for (int i = 0; i < landingTracks.Count; i++)
            {
                if (landingTracks[i].Id == currentPlaybackState.Id)
                {
                    playingTrackIndex = i;
                    break;
                }
            }
            return playingTrackIndex;
        }

        private async Task MakeSureIdsAreSet()
        {
            if (LandingPlaylistId == null)
                await SetPlaylistIds();
        }

        private async Task RepopulateLandingListIfNeeded(List<Track> landingTracks)
        {
            if (landingTracks.Count < LandingListSize)
            {
                for (int i = landingTracks.Count; i < LandingListSize; i++)
                {
                    await AddBasicSongToLandingList(i);
                }
            }
        }

        private async Task UpdatePlaybackState(CurrentPlayback currentPlaybackState)
        {
            if (!currentPlaybackState.Is_Playing)
            {
                if (DefaultDeviceId != null)
                {
                    await UpdateDevice(new DevicesPostDto
                    {
                        Device_Id = DefaultDeviceId,
                        Play = true
                    });
                    
                }
            }
            if (currentPlaybackState.Shuffle_State)
            {
                await ToggleShuffle(false);
            }
        }

        private async Task<CurrentPlayback> GetState()
        {
            var request = ConstructRequestMessage("GET", "https://api.spotify.com/v1/me/player");
            var response = await _client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            var currentPlaybackState = JsonConvert.DeserializeObject<CurrentPlayback>(content);
            var item = JsonConvert.DeserializeObject<DynamicItem>(content);
            NameAndId track = JsonConvert.DeserializeObject<NameAndId>(Convert.ToString(item.Item));
            currentPlaybackState.Name = track.Name;
            currentPlaybackState.Id = track.Id;
            return currentPlaybackState;
        }

        private async Task ToggleShuffle(bool shuffleState)
        {
            var request = ConstructRequestMessage("PUT", $"https://api.spotify.com/v1/me/player/shuffle?state={shuffleState}&device_id={DefaultDeviceId}");
            var response = await _client.SendAsync(request);
        }

        public async Task<string> QueueSong(string id)
        {
            var queue = await GetPlaylistTracks(RuntimeQueuePlaylistId);
            if (Queue.Any(t => t.Id == id) || LandingPlaylist.Any(p => p.Id == id) || queue.Any(t => t.Id == id))
                return "Låten är redan köad";
            var trackRequest = ConstructRequestMessage("GET", $"https://api.spotify.com/v1/tracks/{id}");
            var trackResponse = await _client.SendAsync(trackRequest);
            var content = await trackResponse.Content.ReadAsStringAsync();
            var track = ConvertJsonTrack(content);
            Queue.Add(track);
            await AddSongToPlaylist(track, RuntimeQueuePlaylistId);
            return $"Låten {track.Name} lades till i kön";
        }

        public async Task<TrackBase> GetCurrentTrack()
        {
            var request = ConstructRequestMessage("GET", "https://api.spotify.com/v1/me/player/currently-playing");
            var response = await _client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            var track = JsonConvert.DeserializeObject<TrackBase>(content);
            return track;
        }

        public async Task<IEnumerable<TrackBase>> NextSongInLine()
        {
            Track nextSong;
            if (Queue.Count == 0) // No song in queue
            {
                if (BasicPlaylist == null || BasicPlaylist.Count == 0)
                    await RepopulateBasicPlaylistIds();
                nextSong = BasicPlaylist[new Random().Next(BasicPlaylist.Count)];
            }
            else
                nextSong = Queue[0];

            if (LandingPlaylist == null || LandingPlaylist.Count < LandingListSize)
                await RefreshLandingList();
            else
            {
                if (LandingPlaylist.Count <= LandingListSize)
                {
                    if (TakeFromQueue && Queue.Count > 0)
                        await AddQueuedSongToLandingList();
                    else
                        await AddBasicSongToLandingList();
                    await RemoveRecentlyPlayedSong();
                    LandingPlaylist.RemoveAt(0);
                }
            }

            //TakeFromQueue = !TakeFromQueue;
            var landingSongs = await GetLandingSongs();
            return landingSongs;
        }

        private async Task<IEnumerable<TrackBase>> GetLandingSongs()
        {
            var request = ConstructRequestMessage("GET", $"https://api.spotify.com/v1/users/akgjohansson/playlists/{LandingPlaylistId}/tracks");
            var response = await _client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            var items = JsonConvert.DeserializeObject<ItemsCover>(content);
            var tracks = new List<TrackBase>();
            foreach (var item in items.Items)
            {
                var track = JsonConvert.DeserializeObject<TrackBase>(Convert.ToString(item));
                tracks.Add(track);
            }
            return tracks;
        }

        private async Task RefreshLandingList()
        {
            LandingPlaylist = await GetPlaylistTracks(LandingPlaylistId);
            //else
            //{
            //    foreach (var track in LandingPlaylist)
            //    {
            //        await RemoveSong(track);
            //    }
            //    LandingPlaylist = new List<Track>();
            //}
            if (LandingPlaylist.Count < LandingListSize)
            {
                var count = (await GetPlaylistTracks(LandingPlaylistId)).Count;
                for (int i = 0; i < LandingListSize - count; i++)
                {
                    if (Queue.Count > 0)
                        await AddQueuedSongToLandingList();
                    else
                        await AddBasicSongToLandingList();
                }
            }
        }

        private async Task AddBasicSongToLandingList(int i = 0)
        {
            Track song;
            bool duplicate;
            do
            {
                if (i == 0)
                    song = BasicPlaylist[new Random().Next(BasicPlaylist.Count)];
                else
                    song = BasicPlaylist[new Random(i++).Next(BasicPlaylist.Count)];
                duplicate = LandingPlaylist.Any(t => t.Id == song.Id);
            } while (duplicate);
            await AddSongToPlaylist(song, LandingPlaylistId);
        }

        private async Task AddSongToPlaylist(Track song, string playlistId)
        {
            var request = ConstructRequestMessage("POST", $"https://api.spotify.com/v1/users/akgjohansson/playlists/{playlistId}/tracks");
            request.Content = new StringContent(JsonConvert.SerializeObject(new { uris = new List<string> { song.Uri } }));
            var response = await _client.SendAsync(request);
            LandingPlaylist.Add(song);
        }

        private async Task AddQueuedSongToLandingList()
        {
            var song = Queue[0];
            await AddSongToPlaylist(song, LandingPlaylistId);
            Queue.Remove(song);
            await RemoveSong(song, RuntimeQueuePlaylistId);
        }

        public async Task RemoveRecentlyPlayedSong()
        {
            var recentlyPlayed = LandingPlaylist.First();
            await RemoveSong(recentlyPlayed, LandingPlaylistId);
        }

        private async Task RemoveSong(Track track, string playlistId)
        {
            var request = ConstructRequestMessage("DELETE", $"https://api.spotify.com/v1/users/akgjohansson/playlists/{playlistId}/tracks");
            var test = JsonConvert.SerializeObject(new { tracks = new List<UriModel> { new UriModel { uri = track.Uri } } });
            request.Content = new StringContent(JsonConvert.SerializeObject(new { tracks = new List<UriModel> { new UriModel { uri = track.Uri } } }));
            var response = await _client.SendAsync(request);
        }

        private async Task RepopulateBasicPlaylistIds()
        {

            var tracks = await GetPlaylistTracks(BasicPlaylistId);
            BasicPlaylist = tracks;

        }

        private async Task<List<Track>> GetPlaylistTracks(string playlistId)
        {
            var request = ConstructRequestMessage("GET", $"https://api.spotify.com/v1/users/akgjohansson/playlists/{playlistId}/tracks");
            var response = await _client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            var items = JsonConvert.DeserializeObject<ItemsCover>(content);
            var playlist = new List<Track>();
            foreach (var item in items.Items)
            {
                Dictionary<string, dynamic> itemDictionary = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(Convert.ToString(item));
                var trackString = Convert.ToString(itemDictionary["track"]);
                Track track = ConvertJsonTrack(trackString);
                playlist.Add(track);
            }
            return playlist;
        }

        private static Track ConvertJsonTrack(dynamic trackString)
        {
            TrackCover trackCover = JsonConvert.DeserializeObject<TrackCover>(trackString);
            var track = new Track
            {
                Duration_ms = trackCover.Duration_ms,
                Href = trackCover.Href,
                Id = trackCover.Id,
                Name = trackCover.Name,
                Type = trackCover.Type,
                Uri = trackCover.Uri
            };
            Dictionary<string, dynamic> albumDictionary = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(Convert.ToString(trackCover.Album));
            List<Dictionary<string, dynamic>> artistsDictionary = JsonConvert.DeserializeObject<List<Dictionary<string, dynamic>>>(Convert.ToString(trackCover.Artists));
            var artistName = "";
            for (var i = 0; i < artistsDictionary.Count; i++)
            {
                var artist = artistsDictionary[i];
                var thisArtist = (Convert.ToString(artist["name"]));
                artistName += thisArtist;
                if (i < artistsDictionary.Count - 2)
                    artistName += ", ";
                else if (i == artistsDictionary.Count - 2)
                    artistName += " och ";
            }
            track.Album = Convert.ToString(albumDictionary["name"]);
            track.Artist = artistName;
            return track;
        }

        public void SaveSession()
        {
            using (var helper = new DataBaseHelper())
            {
                helper.SaveSession(_token, DefaultDeviceId);
            }
        }
    }
}