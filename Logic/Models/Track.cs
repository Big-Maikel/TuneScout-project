using System;
using System.Collections.Generic;

namespace Logic.Models;

public partial class Track
{
    public int Id { get; set; }

    public int? GenreId { get; set; }

    public int? MoodId { get; set; }

    public int? LanguageId { get; set; }

    public string SpotifyUri { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Artist { get; set; } = null!;

    public bool? Explicit { get; set; }

    public string? PreviewUrl { get; set; }

    public double Valence { get; set; }

    public virtual Genre? Genre { get; set; }

    public virtual Mood? Mood { get; set; }

    public virtual ICollection<Swipe> Swipes { get; set; } = new List<Swipe>();
}
