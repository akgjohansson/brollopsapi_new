using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using API_brollop.SpotifyDtos;
using API_brollop.Common;

namespace API_brollop.Client
{
    public interface ISpotifyApi
    {
        byte[] ClientByteArray { get; }
        HttpResponseMessage Get(string searchString);
        Task<IEnumerable<Playlist>> GetPlaylists();
        Task<string> GetAuthorizationTokens(string authorization_code);
        bool IsExpired { get; }
        bool IsInitiated { get; }
        Task RefreshTokens();
        Task<string> GetDevices();
        Task<string> UpdateDevice(DevicesPostDto device);
        Task<CurrentStateResponseDto> GetCurrentPlaybackState();
        string DefaultDeviceId { get; set; }

        Task<string> QueueSong(string id);
        Task<TrackBase> GetCurrentTrack();
        Task<IEnumerable<TrackBase>> NextSongInLine();
        void SaveSession();
    }
}