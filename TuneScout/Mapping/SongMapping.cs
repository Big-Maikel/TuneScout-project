using Logic.Models;
using TuneScout.Models;

namespace TuneScout.Mappings
{
    public static class SongMapping
    {
        public static SongViewModel ToViewModel(this Track t)
        {
            return new SongViewModel
            {
                Id = t.Id,
                Name = t.Name ?? string.Empty,
                Artist = t.Artist ?? string.Empty,
                SpotifyUri = t.SpotifyUri ?? string.Empty,
                PreviewUrl = t.PreviewUrl,
                Explicit = t.Explicit,
                Valence = t.Valence
            };
        }
    }
}
