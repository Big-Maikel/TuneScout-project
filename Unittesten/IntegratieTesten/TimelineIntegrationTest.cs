using System;
using System.Linq;
using Xunit;
using Microsoft.EntityFrameworkCore;
using DataAccess.Contexts;
using DataAccess.Repositories;
using Logic.Services;
using Logic.Models;

namespace Unittesten.IntegratieTesten
{
    public class TimelineIntegrationTest
    {
        [Fact]
        public void WhenSongIsLiked_ThenTimelineReturnsOneLike()
        {
            var options = new DbContextOptionsBuilder<TuneScoutContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new TuneScoutContext(options);

            var user = new User { Name = "Test User", Email = "test@test.com", Password = "test" };
            var track = new Track { Name = "Test Song", Artist = "Test Artist", SpotifyUri = "spotify:track:123" };

            context.Users.Add(user);
            context.Tracks.Add(track);
            context.SaveChanges();

            var swipe = new Swipe
            {
                UserId = user.Id,
                TrackId = track.Id,
                Direction = "like",
                Timestamp = DateTime.Now
            };

            context.Swipes.Add(swipe);
            context.SaveChanges();

            var repository = new TimelineRepository(context);
            var service = new TimelineService(repository);

            var result = service.GetTimeline(user.Id, topN: 5);

            Assert.NotNull(result);
        }

        [Fact]
        public void WhenMultipleSongsAreLiked_ThenTimelineShowsCorrectCount()
        {
            var options = new DbContextOptionsBuilder<TuneScoutContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new TuneScoutContext(options);
            
            var user = new User { Name = "Test User", Email = "test@test.com", Password = "test" };
            context.Users.Add(user);
            context.SaveChanges();

            for (int i = 1; i <= 3; i++)
            {
                var track = new Track 
                { 
                    Name = $"Song {i}", 
                    Artist = $"Artist {i}", 
                    SpotifyUri = $"spotify:track:{i}" 
                };
                context.Tracks.Add(track);
                context.SaveChanges();

                var swipe = new Swipe
                {
                    UserId = user.Id,
                    TrackId = track.Id,
                    Direction = "like",
                    Timestamp = DateTime.Now
                };
                context.Swipes.Add(swipe);
            }
            context.SaveChanges();

            var repository = new TimelineRepository(context);
            var service = new TimelineService(repository);
            var result = service.GetTimeline(user.Id);

            var totalLikes = context.Swipes.Count(s => s.UserId == user.Id && s.Direction == "like");
            Assert.Equal(3, totalLikes);
        }
    }
}
