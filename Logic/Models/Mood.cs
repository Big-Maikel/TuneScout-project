using System;
using System.Collections.Generic;

namespace Logic.Models;

public partial class Mood
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Preference> Preferences { get; set; } = new List<Preference>();

    public virtual ICollection<Track> Tracks { get; set; } = new List<Track>();
}
