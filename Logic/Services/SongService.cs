using Logic.Interfaces;
using Logic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Logic.Services
{
    public class SongService
    {
        private readonly ISongRepository _songRepository;
        private readonly ISpotifyRepository? _spotifyRepository;

        public SongService(ISongRepository songRepository, ISpotifyRepository? spotifyRepository = null)
        {
            _songRepository = songRepository ?? throw new ArgumentNullException(nameof(songRepository));
            _spotifyRepository = spotifyRepository;
        }

        public IReadOnlyList<Track> GetAll() => _songRepository.GetAll();

        public List<Track> Recommend(IEnumerable<Swipe>? userSwipes, bool noExplicit = false, int? languageId = null, int max = 50)
        {
            userSwipes ??= Array.Empty<Swipe>();

            var swipedIds = new HashSet<int>(userSwipes.Select(s => s.TrackId));
            var likes = userSwipes.Where(s => string.Equals(s.Direction, "like", StringComparison.OrdinalIgnoreCase)).Select(s => s.TrackId).ToList();
            var dislikes = userSwipes.Where(s => string.Equals(s.Direction, "dislike", StringComparison.OrdinalIgnoreCase)).Select(s => s.TrackId).ToList();

            var allTracks = _songRepository.GetAll().ToList();

            var genreScores = new Dictionary<int, double>();
            foreach (var id in likes)
            {
                var t = allTracks.FirstOrDefault(x => x.Id == id);
                if (t == null) continue;
                var gid = t.GenreId ?? 0;
                genreScores[gid] = genreScores.GetValueOrDefault(gid) + 1.0;
            }
            foreach (var id in dislikes)
            {
                var t = allTracks.FirstOrDefault(x => x.Id == id);
                if (t == null) continue;
                var gid = t.GenreId ?? 0;
                genreScores[gid] = genreScores.GetValueOrDefault(gid) - 0.8;
            }

            var candidates = allTracks
                .Where(t => !swipedIds.Contains(t.Id))
                .Where(t => !(noExplicit && (t.Explicit ?? false)))
                .Where(t => !languageId.HasValue || t.LanguageId == languageId.Value)
                .ToList();

            var rnd = Random.Shared;

            if (!likes.Any() && !dislikes.Any())
            {
                return candidates
                    .OrderBy(_ => rnd.NextDouble())
                    .Take(Math.Min(max, candidates.Count))
                    .ToList();
            }

            var scored = candidates.Select(t =>
            {
                double score = 0;
                if (t.GenreId.HasValue && genreScores.TryGetValue(t.GenreId.Value, out var g)) score += g;
                score += (t.Valence - 0.5) * 0.5;
                score += (rnd.NextDouble() - 0.5) * 0.18;
                return (track: t, score);
            })
            .OrderByDescending(x => x.score)
            .ThenByDescending(x => x.track.Valence)
            .ToList();

            var result = new List<Track>();
            var seen = new HashSet<int>();
            foreach (var item in scored)
            {
                if (item.track == null) continue;
                if (seen.Add(item.track.Id))
                {
                    result.Add(item.track);
                    if (result.Count >= max) break;
                }
            }

            if (result.Count > 3)
            {
                var top = result.Take(3).ToList();
                var rest = result.Skip(3).OrderBy(_ => rnd.NextDouble()).ToList();
                top.AddRange(rest);
                return top;
            }

            return result;
        }
    }
}