using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DataAccess.Contexts;
using Logic.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System;

namespace TuneScout.Pages
{
    public class TimelineDetailsModel : PageModel
    {
        private readonly TuneScoutContext _context;

        public string CategoryType { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string Period { get; set; } = "all";
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<LikedTrack> LikedTracks { get; set; } = new();

        public TimelineDetailsModel(TuneScoutContext context)
        {
            _context = context;
        }

        public IActionResult OnGet(string type, int id, string name, string period = "all", DateTime? fromDate = null, DateTime? toDate = null)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            CategoryType = type ?? "genre";
            CategoryName = name ?? "Onbekend";
            Period = period ?? "all";
            FromDate = fromDate;
            ToDate = toDate;

            if (fromDate.HasValue && toDate.HasValue && toDate.Value < fromDate.Value)
            {
                return RedirectToPage("/Timeline", new { period = "all" });
            }

            if (fromDate.HasValue && fromDate.Value > DateTime.Now.Date)
            {
                return RedirectToPage("/Timeline", new { period = "all" });
            }

            if (toDate.HasValue && toDate.Value > DateTime.Now.Date)
            {
                return RedirectToPage("/Timeline", new { period = "all" });
            }

            DateTime? calculatedFromDate = null;
            DateTime? calculatedToDate = null;

            if (fromDate.HasValue)
            {
                calculatedFromDate = fromDate.Value.Date;
                calculatedToDate = toDate.HasValue ? toDate.Value.Date.AddDays(1).AddTicks(-1) : DateTime.Now.Date.AddDays(1).AddTicks(-1);
            }
            else
            {
                calculatedFromDate = Period switch
                {
                    "week" => DateTime.Now.AddDays(-7),
                    "month" => DateTime.Now.AddMonths(-1),
                    _ => null
                };
            }

            var likedSwipesQuery = _context.Swipes
                .Where(s => s.UserId == userId.Value && EF.Functions.Like(s.Direction, "like"));

            if (calculatedFromDate.HasValue)
            {
                likedSwipesQuery = likedSwipesQuery.Where(s => s.Timestamp >= calculatedFromDate.Value);
            }

            if (calculatedToDate.HasValue)
            {
                likedSwipesQuery = likedSwipesQuery.Where(s => s.Timestamp <= calculatedToDate.Value);
            }

            var likedSwipes = likedSwipesQuery.Select(s => s.TrackId).ToList();

            IQueryable<Track> tracksQuery = _context.Tracks.AsQueryable();

            switch (CategoryType.ToLower())
            {
                case "genre":
                    tracksQuery = tracksQuery.Where(t => t.GenreId == id);
                    break;
                case "mood":
                    tracksQuery = tracksQuery.Where(t => t.MoodId == id);
                    break;
                case "language":
                    tracksQuery = tracksQuery.Where(t => t.LanguageId == id);
                    break;
                default:
                    return RedirectToPage("/Timeline");
            }

            var tracks = tracksQuery
                .Where(t => likedSwipes.Contains(t.Id))
                .Include(t => t.Genre)
                .Include(t => t.Mood)
                .Include(t => t.Language)
                .ToList();

            var swipeDataQuery = _context.Swipes
                .Where(s => s.UserId == userId.Value && likedSwipes.Contains(s.TrackId));

            if (calculatedFromDate.HasValue)
            {
                swipeDataQuery = swipeDataQuery.Where(s => s.Timestamp >= calculatedFromDate.Value);
            }

            if (calculatedToDate.HasValue)
            {
                swipeDataQuery = swipeDataQuery.Where(s => s.Timestamp <= calculatedToDate.Value);
            }

            var swipeData = swipeDataQuery
                .Select(s => new { s.TrackId, s.Timestamp })
                .ToDictionary(s => s.TrackId, s => s.Timestamp);

            LikedTracks = tracks.Select(t => new LikedTrack
            {
                Id = t.Id,
                Name = t.Name,
                Artist = t.Artist,
                GenreName = t.Genre?.Name ?? "Onbekend",
                MoodName = t.Mood?.Name ?? "Onbekend",
                LanguageName = t.Language?.Name ?? "Onbekend",
                PreviewUrl = t.PreviewUrl,
                LikedAt = swipeData.ContainsKey(t.Id) ? swipeData[t.Id] : DateTime.MinValue
            })
            .OrderByDescending(t => t.LikedAt)
            .ToList();

            return Page();
        }

        public class LikedTrack
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Artist { get; set; } = string.Empty;
            public string GenreName { get; set; } = string.Empty;
            public string MoodName { get; set; } = string.Empty;
            public string LanguageName { get; set; } = string.Empty;
            public string? PreviewUrl { get; set; }
            public DateTime LikedAt { get; set; }
        }
    }
}
