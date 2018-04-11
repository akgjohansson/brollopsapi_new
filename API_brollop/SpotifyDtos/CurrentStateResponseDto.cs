using API_brollop.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_brollop.SpotifyDtos
{
	public class CurrentStateResponseDto : CurrentPlayback
	{
        public Track NextSong { get; set; }
    }

    public static class PlaybackDtoConverters {
        public static CurrentStateResponseDto ToDto(this CurrentPlayback playback, Track nextSong)
        {
            var report = new CurrentStateResponseDto
            {
                Device = playback.Device,
                Id = playback.Id,
                Is_Playing = playback.Is_Playing,
                Item = playback.Item,
                Name = playback.Name,
                Progress_ms = playback.Progress_ms,
                Repeat_State = playback.Repeat_State,
                Shuffle_State = playback.Shuffle_State,
                TimeStamp = playback.TimeStamp,
                NextSong = nextSong
            };
            return report;
        }
    }
}