using Xunit;
using Moq;
using Logic.Services;
using Logic.Interfaces;
using Logic.Models;
using System.Collections.Generic;
using System.Linq;

namespace Unittesten
{
    public class TimelineServiceTests
    {
        private readonly Mock<ITimelineRepository> _mockTimelineRepo;
        private readonly TimelineService _service;

        public TimelineServiceTests()
        {
            _mockTimelineRepo = new Mock<ITimelineRepository>();

            _service = new TimelineService(_mockTimelineRepo.Object);
        }

        [Fact]
        public void GetTimeline_CallsRepositoryAndReturnsResult()
        {
            var topGenres = new List<TopItem> { new TopItem(1, "Pop", 5) };
            var topMoods = new List<TopItem> { new TopItem(2, "Happy", 3) };
            var topLanguages = new List<TopItem> { new TopItem(3, "English", 4) };

            var expectedTimeline = new TimelineResult(topGenres, topMoods, topLanguages);

            _mockTimelineRepo
                .Setup(r => r.GetTimeline(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(expectedTimeline);

            var result = _service.GetTimeline(1, 5);

            Assert.NotNull(result);
            Assert.Equal(1, result.TopGenres.First().Id);
            Assert.Equal("Pop", result.TopGenres.First().Name);
            Assert.Equal(2, result.TopMoods.First().Id);
            Assert.Equal("Happy", result.TopMoods.First().Name);
            Assert.Equal(3, result.TopLanguages.First().Id);
            Assert.Equal("English", result.TopLanguages.First().Name);

            _mockTimelineRepo.Verify(r => r.GetTimeline(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }
    }
}
