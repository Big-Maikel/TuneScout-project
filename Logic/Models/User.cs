using System;
using System.Collections.Generic;

namespace Logic.Models;

public partial class User
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public bool? NoExplicit { get; set; }

    public virtual ICollection<Preference> Preferences { get; set; } = new List<Preference>();

    public virtual ICollection<Swipe> Swipes { get; set; } = new List<Swipe>();
}
