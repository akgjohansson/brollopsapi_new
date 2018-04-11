using API_brollop.Client;
using API_brollop.SpotifyDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace API_brollop.PlayBackWorker
{
    public class Worker
    {
        private readonly ISpotifyApi _api;
        public Worker(ISpotifyApi api)
        {
            _api = api;
        }

        public async Task DoWork()
        {
            while (true)
            {
                try
                {
                    if (!_api.IsInitiated)
                        Thread.Sleep(10000);
                    else
                    {
                        if (_api.IsExpired)
                            await _api.RefreshTokens();
                        var currentState = await _api.GetCurrentPlaybackState();

                        if (!currentState.Is_Playing)
                        {
                            if (_api.DefaultDeviceId != null)
                                await _api.UpdateDevice(new DevicesPostDto { Device_Id = _api.DefaultDeviceId, Play = true });
                        }

                        var currentSong = await _api.GetCurrentTrack();
                        var secondsLeft = (currentSong.Duration_ms - currentState.Progress_ms) / 1000;
                        if (secondsLeft > 60)
                            Thread.Sleep(30000);
                        else if (secondsLeft > 30)
                            Thread.Sleep(15000);
                        else
                        {
                            Thread.Sleep((secondsLeft - 5) * 1000);
                            await _api.NextSongInLine();
                            Thread.Sleep(1000);
                        }
                    }
                } catch { Thread.Sleep(1000); }
            }
        }
    }
}