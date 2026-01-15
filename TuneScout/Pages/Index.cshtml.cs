using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Logic.Services;
using Logic.Models;
using DataAccess.Contexts;
using TuneScout.Models;
using TuneScout.Mappings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace TuneScout.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly TuneScoutContext _context;
        private readonly SongService _songService;

        private const string SessionAnonSwipesKey = "AnonSwipesV1";

        public List<SongViewModel> Songs { get; set; } = new();
        public bool IsAuthenticated { get; set; }

        public IndexModel(
            ILogger<IndexModel> logger,
            TuneScoutContext context,
            SongService songService)
        {
            _logger = logger;
            _context = context;
            _songService = songService;
        }

        public void OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            IsAuthenticated = userId != null;

            var (userSwipes, noExplicit, languageId, genreIds, moodIds) = GetCombinedSwipesAndSettings();
            var recommendedTracks = RecommendSafe(userSwipes, noExplicit, languageId, genreIds, moodIds);

            if (!recommendedTracks.Any())
            {
                _logger?.LogInformation(
                    "No recommendations found for user {UserId}. Falling back to DB tracks.",
                    userId
                );

                recommendedTracks = _songService.GetAll()
                    .Where(t => !(noExplicit && (t.Explicit ?? false)))
                    .Where(t => !languageId.HasValue || t.LanguageId == languageId.Value)
                    .ToList();
            }

            Songs = recommendedTracks
                .Select(t => t.ToViewModel())
                .ToList();
        }

        [ValidateAntiForgeryToken]
        public IActionResult OnPostSwipe(int trackId, string direction)
        {
            if (!_context.Tracks.Any(t => t.Id == trackId))
            {
                _logger?.LogWarning(
                    "Swipe attempt for non-existent track {TrackId}",
                    trackId
                );

                return BadRequest(new
                {
                    success = false,
                    error = "Track not found"
                });
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return new JsonResult(new
                {
                    success = false,
                    error = "Not authenticated"
                })
                { StatusCode = 401 };
            }

            if (_context.Swipes.Any(s =>
                s.UserId == userId.Value &&
                s.TrackId == trackId))
            {
                return new JsonResult(new
                {
                    success = true,
                    message = "Already recorded"
                });
            }

            var swipe = new Swipe
            {
                UserId = userId.Value,
                TrackId = trackId,
                Direction = direction,
                Timestamp = DateTime.Now
            };

            _context.Swipes.Add(swipe);

            try
            {
                _context.SaveChanges();

                _logger?.LogInformation(
                    "Saved swipe for user {UserId}, track {TrackId}, direction {Direction}",
                    userId.Value,
                    trackId,
                    direction
                );
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "Failed to save swipe for user {UserId}, track {TrackId}",
                    userId,
                    trackId
                );

                return StatusCode(500, new
                {
                    success = false,
                    error = "Database error while saving swipe."
                });
            }

            return new JsonResult(new { success = true });
        }

        public IActionResult OnGetRecommendations()
        {
            var (userSwipes, noExplicit, languageId, genreIds, moodIds) = GetCombinedSwipesAndSettings();
            var recs = RecommendSafe(userSwipes, noExplicit, languageId, genreIds, moodIds);

            if (!recs.Any())
            {
                recs = _songService.GetAll()
                    .Where(t => !(noExplicit && (t.Explicit ?? false)))
                    .Where(t => !languageId.HasValue || t.LanguageId == languageId.Value)
                    .ToList();
            }

            var vm = recs
                .Select(t => t.ToViewModel())
                .ToList();

            return new JsonResult(vm);
        }

        private (IEnumerable<Swipe> userSwipes, bool noExplicit, int? languageId, List<int> genreIds, List<int> moodIds)
            GetCombinedSwipesAndSettings()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            IEnumerable<Swipe> dbSwipes = Array.Empty<Swipe>();
            bool noExplicit = false;
            int? languageId = null;
            var genreIds = new List<int>();
            var moodIds = new List<int>();

            if (userId != null)
            {
                dbSwipes = _context.Swipes
                    .Where(s => s.UserId == userId.Value)
                    .ToList();

                var user = _context.Users.Find(userId.Value);
                noExplicit = user?.NoExplicit ?? false;

                var preferences = _context.Preferences
                    .Where(p => p.UserId == userId.Value)
                    .ToList();
                
                var languagePreference = preferences.FirstOrDefault(p => p.LanguageId.HasValue);
                languageId = languagePreference?.LanguageId;

                genreIds = preferences
                    .Where(p => p.GenreId.HasValue)
                    .Select(p => p.GenreId!.Value)
                    .Distinct()
                    .ToList();

                moodIds = preferences
                    .Where(p => p.MoodId.HasValue)
                    .Select(p => p.MoodId!.Value)
                    .Distinct()
                    .ToList();
            }

            var sessionSwipes = GetSessionAnonSwipes()
                .Select(s => new Swipe
                {
                    UserId = userId ?? 0,
                    TrackId = s.TrackId,
                    Direction = s.Direction ?? "dislike",
                    Timestamp = s.Timestamp
                });

            return (dbSwipes.Concat(sessionSwipes), noExplicit, languageId, genreIds, moodIds);
        }

        private List<SessionSwipe> GetSessionAnonSwipes()
        {
            var json = HttpContext.Session.GetString(SessionAnonSwipesKey);
            if (string.IsNullOrEmpty(json))
                return new List<SessionSwipe>();

            try
            {
                return JsonSerializer.Deserialize<List<SessionSwipe>>(json)
                       ?? new List<SessionSwipe>();
            }
            catch
            {
                return new List<SessionSwipe>();
            }
        }

        private List<Track> RecommendSafe(
            IEnumerable<Swipe> swipes,
            bool noExplicit,
            int? languageId,
            List<int> genreIds,
            List<int> moodIds)
        {
            return _songService.Recommend(
                swipes ?? Array.Empty<Swipe>(),
                noExplicit,
                languageId,
                genreIds,
                moodIds,
                max: 100
            );
        }

        private class SessionSwipe
        {
            public int TrackId { get; set; }
            public string? Direction { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }
}
