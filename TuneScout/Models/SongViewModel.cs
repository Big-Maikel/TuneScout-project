using System;
using Logic.Models;

namespace TuneScout.Models
{
    public class SongViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string SpotifyUri { get; set; } = string.Empty;
        public string? PreviewUrl { get; set; }
        public bool? Explicit { get; set; }
        public double Valence { get; set; }

        public static SongViewModel FromTrack(Track t) => new SongViewModel
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