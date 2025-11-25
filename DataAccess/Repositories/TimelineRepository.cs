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
                .Where(s => s.UserId == userId && s.Direction == "like");

            var topGenres = (from s in likes
                             join t in _context.Tracks on s.TrackId equals t.Id
                             where t.GenreId.HasValue
                             join g in _context.Genres on t.GenreId equals g.Id
                             group s by new { g.Id, g.Name } into grp
                             orderby grp.Count() descending
                             select new TopItem(grp.Key.Id, grp.Key.Name ?? string.Empty, grp.Count()))
                            .Take(topN)
                            .ToList();

            // Top moods
            var topMoods = (from s in likes
                            join t in _context.Tracks on s.TrackId equals t.Id
                            where t.MoodId.HasValue
                            join m in _context.Moods on t.MoodId equals m.Id
                            group s by new { m.Id, m.Name } into grp
                            orderby grp.Count() descending
                            select new TopItem(grp.Key.Id, grp.Key.Name ?? string.Empty, grp.Count()))
                           .Take(topN)
                           .ToList();

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