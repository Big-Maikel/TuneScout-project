using System;
using System.Collections.Generic;

namespace Logic.Models;

public partial class Preference
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? GenreId { get; set; }

    public int? MoodId { get; set; }

    public int? LanguageId { get; set; }

    public virtual Genre? Genre { get; set; }

    public virtual Mood? Mood { get; set; }

    public virtual Language? Language { get; set; }

    public virtual User User { get; set; } = null!;
}
