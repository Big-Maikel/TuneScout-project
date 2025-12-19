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
        public void GetAll_ReturnsEmptyList_WhenNoTracks()
        {
            _mockSongRepo.Setup(r => r.GetAll()).Returns(new List<Track>());

            var result = _service.GetAll();

            Assert.Empty(result);
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

        [Fact]
        public void Recommend_NoExplicit_FiltersExplicitTracks()
        {
            var tracks = new List<Track>
            {
                new Track { Id = 1, Name = "Clean Song", Explicit = false, Valence = 0.5 },
                new Track { Id = 2, Name = "Explicit Song", Explicit = true, Valence = 0.5 },
                new Track { Id = 3, Name = "Another Clean", Explicit = false, Valence = 0.6 },
                new Track { Id = 4, Name = "Another Explicit", Explicit = true, Valence = 0.7 }
            };
            _mockSongRepo.Setup(r => r.GetAll()).Returns(tracks);

            var result = _service.Recommend(null, noExplicit: true, max: 10);

            Assert.DoesNotContain(result, t => t.Explicit == true);
            Assert.All(result, t => Assert.False(t.Explicit ?? false));
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void Recommend_NoExplicitFalse_IncludesExplicitTracks()
        {
            var tracks = new List<Track>
            {
                new Track { Id = 1, Name = "Clean Song", Explicit = false, Valence = 0.5 },
                new Track { Id = 2, Name = "Explicit Song", Explicit = true, Valence = 0.5 }
            };
            _mockSongRepo.Setup(r => r.GetAll()).Returns(tracks);

            var result = _service.Recommend(null, noExplicit: false, max: 10);

            Assert.Equal(2, result.Count);
            Assert.Contains(result, t => t.Explicit == true);
        }

        [Fact]
        public void Recommend_MaxParameter_LimitsResultCount()
        {
            var tracks = new List<Track>();
            for (int i = 1; i <= 20; i++)
            {
                tracks.Add(new Track { Id = i, Name = $"Song{i}", Valence = 0.5 });
            }
            _mockSongRepo.Setup(r => r.GetAll()).Returns(tracks);

            var result = _service.Recommend(null, noExplicit: false, max: 10);

            Assert.Equal(10, result.Count);
        }

        [Fact]
        public void Recommend_MaxGreaterThanCandidates_ReturnsAllCandidates()
        {
            var tracks = new List<Track>
            {
                new Track { Id = 1, Name = "Song1", Valence = 0.5 },
                new Track { Id = 2, Name = "Song2", Valence = 0.6 },
                new Track { Id = 3, Name = "Song3", Valence = 0.7 }
            };
            _mockSongRepo.Setup(r => r.GetAll()).Returns(tracks);

            var result = _service.Recommend(null, noExplicit: false, max: 100);

            Assert.Equal(3, result.Count);
        }

        [Fact]
        public void Recommend_ExcludesAlreadySwipedTracks()
        {
            var tracks = new List<Track>
            {
                new Track { Id = 1, Name = "Song1", Valence = 0.5, GenreId = 1 },
                new Track { Id = 2, Name = "Song2", Valence = 0.6, GenreId = 1 },
                new Track { Id = 3, Name = "Song3", Valence = 0.7, GenreId = 2 },
                new Track { Id = 4, Name = "Song4", Valence = 0.8, GenreId = 2 }
            };
            _mockSongRepo.Setup(r => r.GetAll()).Returns(tracks);

            var swipes = new List<Swipe>
            {
                new Swipe { TrackId = 1, Direction = "like" },
                new Swipe { TrackId = 3, Direction = "dislike" }
            };

            var result = _service.Recommend(swipes, noExplicit: false, max: 10);

            Assert.DoesNotContain(result, t => t.Id == 1);
            Assert.DoesNotContain(result, t => t.Id == 3);
            Assert.Contains(result, t => t.Id == 2 || t.Id == 4);
        }

        [Fact]
        public void Recommend_WithGenrePreference_PrioritizesSameGenre()
        {
            var tracks = new List<Track>
            {
                new Track { Id = 1, Name = "Pop Song 1", GenreId = 1, Valence = 0.5 },
                new Track { Id = 2, Name = "Pop Song 2", GenreId = 1, Valence = 0.5 },
                new Track { Id = 3, Name = "Rock Song", GenreId = 2, Valence = 0.5 },
                new Track { Id = 4, Name = "Jazz Song", GenreId = 3, Valence = 0.5 }
            };
            _mockSongRepo.Setup(r => r.GetAll()).Returns(tracks);

            var swipes = new List<Swipe>
            {
                new Swipe { TrackId = 1, Direction = "like" }
            };

            var result = _service.Recommend(swipes, noExplicit: false, max: 2);

            Assert.Contains(result, t => t.Id == 2);
        }

        [Fact]
        public void Recommend_WithDislikedGenre_LowersPriorityForThatGenre()
        {
            var tracks = new List<Track>
            {
                new Track { Id = 1, Name = "Rock Song 1", GenreId = 1, Valence = 0.5 },
                new Track { Id = 2, Name = "Rock Song 2", GenreId = 1, Valence = 0.5 },
                new Track { Id = 3, Name = "Pop Song", GenreId = 2, Valence = 0.5 }
            };
            _mockSongRepo.Setup(r => r.GetAll()).Returns(tracks);

            var swipes = new List<Swipe>
            {
                new Swipe { TrackId = 1, Direction = "dislike" }
            };

            var result = _service.Recommend(swipes, noExplicit: false, max: 10);

            Assert.DoesNotContain(result, t => t.Id == 1);
            Assert.Contains(result, t => t.Id == 2 || t.Id == 3);
        }

        [Fact]
        public void Recommend_EmptySwipesList_ReturnsSameAsNullSwipes()
        {
            var tracks = new List<Track>
            {
                new Track { Id = 1, Name = "Song1", Valence = 0.5 },
                new Track { Id = 2, Name = "Song2", Valence = 0.6 }
            };
            _mockSongRepo.Setup(r => r.GetAll()).Returns(tracks);

            var result = _service.Recommend(new List<Swipe>(), noExplicit: false, max: 10);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void Recommend_CaseInsensitiveSwipeDirection()
        {
            var tracks = new List<Track>
            {
                new Track { Id = 1, Name = "Song1", GenreId = 1, Valence = 0.5 },
                new Track { Id = 2, Name = "Song2", GenreId = 1, Valence = 0.6 }
            };
            _mockSongRepo.Setup(r => r.GetAll()).Returns(tracks);

            var swipes = new List<Swipe>
            {
                new Swipe { TrackId = 1, Direction = "LIKE" },
                new Swipe { TrackId = 2, Direction = "Like" }
            };

            var result = _service.Recommend(swipes, noExplicit: false, max: 10);

            Assert.DoesNotContain(result, t => t.Id == 1 || t.Id == 2);
        }

        [Fact]
        public void Recommend_WithNullGenreId_HandlesGracefully()
        {
            var tracks = new List<Track>
            {
                new Track { Id = 1, Name = "Song with Genre", GenreId = 1, Valence = 0.5 },
                new Track { Id = 2, Name = "Song without Genre", GenreId = null, Valence = 0.6 }
            };
            _mockSongRepo.Setup(r => r.GetAll()).Returns(tracks);

            var result = _service.Recommend(null, noExplicit: false, max: 10);

            Assert.Equal(2, result.Count);
            Assert.Contains(result, t => t.GenreId == null);
        }

        [Fact]
        public void Recommend_AllTracksExplicitWithNoExplicit_ReturnsEmpty()
        {
            var tracks = new List<Track>
            {
                new Track { Id = 1, Name = "Explicit 1", Explicit = true, Valence = 0.5 },
                new Track { Id = 2, Name = "Explicit 2", Explicit = true, Valence = 0.6 }
            };
            _mockSongRepo.Setup(r => r.GetAll()).Returns(tracks);

            var result = _service.Recommend(null, noExplicit: true, max: 10);

            Assert.Empty(result);
        }

        [Fact]
        public void Recommend_AllTracksSwipedAlready_ReturnsEmpty()
        {
            var tracks = new List<Track>
            {
                new Track { Id = 1, Name = "Song1", Valence = 0.5 },
                new Track { Id = 2, Name = "Song2", Valence = 0.6 }
            };
            _mockSongRepo.Setup(r => r.GetAll()).Returns(tracks);

            var swipes = new List<Swipe>
            {
                new Swipe { TrackId = 1, Direction = "like" },
                new Swipe { TrackId = 2, Direction = "dislike" }
            };

            var result = _service.Recommend(swipes, noExplicit: false, max: 10);

            Assert.Empty(result);
        }

        [Fact]
        public void Recommend_MixedLikesAndDislikesForSameGenre_CalculatesNetScore()
        {
            var tracks = new List<Track>
            {
                new Track { Id = 1, Name = "Rock 1", GenreId = 1, Valence = 0.5 },
                new Track { Id = 2, Name = "Rock 2", GenreId = 1, Valence = 0.6 },
                new Track { Id = 3, Name = "Rock 3", GenreId = 1, Valence = 0.7 },
                new Track { Id = 4, Name = "Pop 1", GenreId = 2, Valence = 0.5 }
            };
            _mockSongRepo.Setup(r => r.GetAll()).Returns(tracks);

            var swipes = new List<Swipe>
            {
                new Swipe { TrackId = 1, Direction = "like" },
                new Swipe { TrackId = 2, Direction = "like" },
                new Swipe { TrackId = 3, Direction = "dislike" }
            };

            var result = _service.Recommend(swipes, noExplicit: false, max: 10);

            Assert.Contains(result, t => t.Id == 4);
        }
    }
}
