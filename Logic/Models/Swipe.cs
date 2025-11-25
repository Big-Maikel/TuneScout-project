using System;
using System.Collections.Generic;

namespace Logic.Models;

public partial class Swipe
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int TrackId { get; set; }

    public string Direction { get; set; } = null!;

    public DateTime Timestamp { get; set; }

    public virtual Track Track { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
