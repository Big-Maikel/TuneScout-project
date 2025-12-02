using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Logic.Interfaces;
using DataAccess.Contexts;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class TimelineRepository : ITimelineRepository
    {
        private readonly TuneScoutContext _context;

        public TimelineRepository(TuneScoutContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public TimelineResult GetTimeline(int userId, int topN = 5)
        {
            var likes = _context.Swipes
                .Where(s => s.UserId == userId && EF.Functions.Like(s.Direction, "like"));

            var genreCounts = (from s in likes
                               join t in _context.Tracks on s.TrackId equals t.Id
                               where t.GenreId.HasValue
                               group s by t.GenreId.Value into grp
                               orderby grp.Count() descending
                               select new { Id = grp.Key, Count = grp.Count() })
                              .Take(topN)
                              .ToList();

            var topGenres = new List<TopItem>(genreCounts.Count);
            if (genreCounts.Count > 0)
            {
                var genreIds = genreCounts.Select(g => g.Id).ToList();
                var genreNames = _context.Genres
                                         .AsNoTracking()
                                         .Where(g => genreIds.Contains(g.Id))
                                         .ToDictionary(g => g.Id, g => g.Name ?? string.Empty);

                foreach (var g in genreCounts)
                {
                    var name = genreNames.TryGetValue(g.Id, out var n) ? n : $"Genre {g.Id}";
                    topGenres.Add(new TopItem(g.Id, name, g.Count));
                }
            }

            var moodCounts = (from s in likes
                              join t in _context.Tracks on s.TrackId equals t.Id
                              where t.MoodId.HasValue
                              group s by t.MoodId.Value into grp
                              orderby grp.Count() descending
                              select new { Id = grp.Key, Count = grp.Count() })
                             .Take(topN)
                             .ToList();

            var topMoods = new List<TopItem>(moodCounts.Count);
            if (moodCounts.Count > 0)
            {
                var moodIds = moodCounts.Select(m => m.Id).ToList();
                var moodNames = _context.Moods
                                        .AsNoTracking()
                                        .Where(m => moodIds.Contains(m.Id))
                                        .ToDictionary(m => m.Id, m => m.Name ?? string.Empty);

                foreach (var m in moodCounts)
                {
                    var name = moodNames.TryGetValue(m.Id, out var n) ? n : $"Mood {m.Id}";
                    topMoods.Add(new TopItem(m.Id, name, m.Count));
                }
            }

            var languageCounts = (from s in likes
                                  join t in _context.Tracks on s.TrackId equals t.Id
                                  where t.LanguageId.HasValue
                                  group s by t.LanguageId into grp
                                  orderby grp.Count() descending
                                  select new { Id = grp.Key.Value, Count = grp.Count() })
                                 .Take(topN)
                                 .ToList();

            var topLanguages = new List<TopItem>(languageCounts.Count);
            foreach (var item in languageCounts)
            {
                var name = GetLanguageName(item.Id) ?? $"Language {item.Id}";
                topLanguages.Add(new TopItem(item.Id, name, item.Count));
            }

            return new TimelineResult(topGenres, topMoods, topLanguages);
        }

        private string? GetLanguageName(int id)
        {
            try
            {
                var conn = _context.Database.GetDbConnection();
                if (conn.State != ConnectionState.Open)
                    conn.Open();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT name FROM [language] WHERE id = @id";
                var param = cmd.CreateParameter();
                param.ParameterName = "@id";
                param.Value = id;
                cmd.Parameters.Add(param);

                var result = cmd.ExecuteScalar();
                return result == null || result == DBNull.Value ? null : Convert.ToString(result);
            }
            catch
            {
                return null;
            }
        }
    }
}