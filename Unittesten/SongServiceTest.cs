using Xunit;
using Moq;
using Logic.Services;
using Logic.Interfaces;
using Logic.Models;
using System.Collections.Generic;
using System.Linq;

namespace Unittesten
{
    public class SongServiceTests
    {
        private readonly Mock<ISongRepository> _mockSongRepo;
        private readonly SongService _service;

        public SongServiceTests()
        {
            _mockSongRepo = new Mock<ISongRepository>();

            _service = new SongService(_mockSongRepo.Object, null);
        }

        [Fact]
        public void GetAll_ReturnsAllTracks()
        {
            var tracks = new List<Track>
            {
                 new Track { Id = 1, Name = "Song1", Valence = 0.5, GenreId = 1 },
                 new Track { Id = 2, Name = "Song2", Valence = 0.8, GenreId = 2 }
            };
            _mockSongRepo.Setup(r => r.GetAll()).Returns(tracks);

            var result = _service.GetAll();

            Assert.Equal(2, result.Count);
            Assert.Contains(result, t => t.Name == "Song1");
            Assert.Contains(result, t => t.Name == "Song2");
        }

        [Fact]
        public void Recommend_NoSwipes_ReturnsRandomSubset()
        {
            var tracks = new List<Track>();
            for (int i = 1; i <= 10; i++)
            {
                tracks.Add(new Track { Id = i, Name = $"Song{i}", Valence = 0.5 });
            }
            _mockSongRepo.Setup(r => r.GetAll()).Returns(tracks);

            var result = _service.Recommend(null, noExplicit: false, max: 5);

            Assert.Equal(5, result.Count);
            Assert.All(result, t => Assert.Contains(tracks, x => x.Id == t.Id));
        }

        [Fact]
        public void Recommend_WithLikesAndDislikes_ScoresTracksCorrectly()
        {
            var tracks = new List<Track>
            {
                 new Track { Id = 1, Name = "Song1", GenreId = 1, Valence = 0.6 },
                 new Track { Id = 2, Name = "Song2", GenreId = 2, Valence = 0.4 },
                 new Track { Id = 3, Name = "Song3", GenreId = 1, Valence = 0.7 },
            };
            _mockSongRepo.Setup(r => r.GetAll()).Returns(tracks);

            var swipes = new List<Swipe>
            {
                 new Swipe { TrackId = 1, Direction = "like" },
                 new Swipe { TrackId = 2, Direction = "dislike" }
            };

            var result = _service.Recommend(swipes, noExplicit: false, max: 5);

            Assert.DoesNotContain(result, t => t.Id == 1);
            Assert.DoesNotContain(result, t => t.Id == 2);

            Assert.Contains(result, t => t.Id == 3);
        }

    }

}
