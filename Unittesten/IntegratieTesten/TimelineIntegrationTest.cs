using k8s.KubeConfigModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace InMemoryIntegrationTestExample
{
 
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Track> Tracks => Set<Track>();
        public DbSet<Swipe> Swipes => Set<Swipe>();
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

    public class Track
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

    public class Swipe
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int TrackId { get; set; }
        public Track Track { get; set; } = null!;

        public SwipeDirection Direction { get; set; }
    }

    public enum SwipeDirection
    {
        Like,
        Dislike
    }

    public class TimelineRepository
    {
        private readonly TestDbContext _context;

        public TimelineRepository(TestDbContext context)
        {
            _context = context;
        }

        public int GetLikedTrackCount(int userId)
        {
            return _context.Swipes
                .Where(s => s.UserId == userId && s.Direction == SwipeDirection.Like)
                .Count();
        }
    }

    public class TimelineService
    {
        private readonly TimelineRepository _repository;

        public TimelineService(TimelineRepository repository)
        {
            _repository = repository;
        }

        public int GetTimelineLikeCount(int userId)
        {
            return _repository.GetLikedTrackCount(userId);
        }
    }

    public class TimelineIntegrationTest
    {
        [Fact]
        public void WhenSongIsLiked_ThenTimelineReturnsOneLike()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new TestDbContext(options);

            var user = new User { Name = "Test User" };
            var track = new Track { Name = "Test Song" };

            context.Users.Add(user);
            context.Tracks.Add(track);
            context.SaveChanges();

            var swipe = new Swipe
            {
                UserId = user.Id,
                TrackId = track.Id,
                Direction = SwipeDirection.Like
            };

            context.Swipes.Add(swipe);
            context.SaveChanges();

            var repository = new TimelineRepository(context);
            var service = new TimelineService(repository);

            var result = service.GetTimelineLikeCount(user.Id);

            Assert.Equal(1, result);
        }
    }
}
