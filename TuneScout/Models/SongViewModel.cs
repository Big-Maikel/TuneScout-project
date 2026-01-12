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
    }
}
