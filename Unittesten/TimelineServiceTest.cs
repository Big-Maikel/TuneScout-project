using Xunit;
using Moq;
using Logic.Services;
using Logic.Interfaces;
using Logic.Models;
using System;
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
                .Setup(r => r.GetTimeline(It.IsAny<int>(), It.IsAny<int>(), null, null))
                .Returns(expectedTimeline);

            var result = _service.GetTimeline(1, 5);

            Assert.NotNull(result);
            Assert.Equal(1, result.TopGenres.First().Id);
            Assert.Equal("Pop", result.TopGenres.First().Name);
            Assert.Equal(2, result.TopMoods.First().Id);
            Assert.Equal("Happy", result.TopMoods.First().Name);
            Assert.Equal(3, result.TopLanguages.First().Id);
            Assert.Equal("English", result.TopLanguages.First().Name);
            Assert.Equal(5, result.TopGenres.First().Count);
            Assert.Equal(3, result.TopMoods.First().Count);
            Assert.Equal(4, result.TopLanguages.First().Count);

            _mockTimelineRepo.Verify(r => r.GetTimeline(1, 5, null, null), Times.Once);
        }

        [Fact]
        public void GetTimeline_WithDateRange_PassesDatesToRepository()
        {
            var fromDate = new DateTime(2025, 1, 1);
            var toDate = new DateTime(2025, 1, 31);

            var expectedTimeline = new TimelineResult(
                new List<TopItem>(),
                new List<TopItem>(),
                new List<TopItem>()
            );

            _mockTimelineRepo
                .Setup(r => r.GetTimeline(It.IsAny<int>(), It.IsAny<int>(), fromDate, toDate))
                .Returns(expectedTimeline);

            var result = _service.GetTimeline(1, 10, fromDate, toDate);

            Assert.NotNull(result);
            _mockTimelineRepo.Verify(r => r.GetTimeline(1, 10, fromDate, toDate), Times.Once);
        }

        [Fact]
        public void GetTimeline_WithOnlyFromDate_PassesCorrectParameters()
        {
            var fromDate = new DateTime(2025, 1, 1);

            var expectedTimeline = new TimelineResult(
                new List<TopItem> { new TopItem(1, "Rock", 10) },
                new List<TopItem>(),
                new List<TopItem>()
            );

            _mockTimelineRepo
                .Setup(r => r.GetTimeline(1, 5, fromDate, null))
                .Returns(expectedTimeline);

            var result = _service.GetTimeline(1, 5, fromDate, null);

            Assert.NotNull(result);
            Assert.Single(result.TopGenres);
            Assert.Equal("Rock", result.TopGenres.First().Name);
            _mockTimelineRepo.Verify(r => r.GetTimeline(1, 5, fromDate, null), Times.Once);
        }

        [Fact]
        public void GetTimeline_ReturnsEmptyLists_WhenNoData()
        {
            var emptyTimeline = new TimelineResult(
                new List<TopItem>(),
                new List<TopItem>(),
                new List<TopItem>()
            );

            _mockTimelineRepo
                .Setup(r => r.GetTimeline(It.IsAny<int>(), It.IsAny<int>(), null, null))
                .Returns(emptyTimeline);

            var result = _service.GetTimeline(1, 10);

            Assert.NotNull(result);
            Assert.Empty(result.TopGenres);
            Assert.Empty(result.TopMoods);
            Assert.Empty(result.TopLanguages);
        }

        [Fact]
        public void GetTimeline_WithCustomTopN_PassesCorrectValue()
        {
            var expectedTimeline = new TimelineResult(
                new List<TopItem>(),
                new List<TopItem>(),
                new List<TopItem>()
            );

            _mockTimelineRepo
                .Setup(r => r.GetTimeline(1, 15, null, null))
                .Returns(expectedTimeline);

            _service.GetTimeline(1, 15);

            _mockTimelineRepo.Verify(r => r.GetTimeline(1, 15, null, null), Times.Once);
        }

        [Fact]
        public void GetTimeline_WithMultipleItems_ReturnsAllItems()
        {
            var topGenres = new List<TopItem>
            {
                new TopItem(1, "Pop", 10),
                new TopItem(2, "Rock", 8),
                new TopItem(3, "Jazz", 5)
            };

            var topMoods = new List<TopItem>
            {
                new TopItem(1, "Happy", 7),
                new TopItem(2, "Energetic", 6)
            };

            var topLanguages = new List<TopItem>
            {
                new TopItem(1, "English", 12),
                new TopItem(2, "Dutch", 4),
                new TopItem(3, "Spanish", 3)
            };

            var expectedTimeline = new TimelineResult(topGenres, topMoods, topLanguages);

            _mockTimelineRepo
                .Setup(r => r.GetTimeline(1, 10, null, null))
                .Returns(expectedTimeline);

            var result = _service.GetTimeline(1, 10);

            Assert.Equal(3, result.TopGenres.Count);
            Assert.Equal(2, result.TopMoods.Count);
            Assert.Equal(3, result.TopLanguages.Count);
            Assert.Equal("Pop", result.TopGenres.First().Name);
            Assert.Equal(10, result.TopGenres.First().Count);
        }

        [Fact]
        public void GetTimeline_DefaultTopNValue_UsesDefaultOf5()
        {
            var expectedTimeline = new TimelineResult(
                new List<TopItem>(),
                new List<TopItem>(),
                new List<TopItem>()
            );

            _mockTimelineRepo
                .Setup(r => r.GetTimeline(1, 5, null, null))
                .Returns(expectedTimeline);

            _service.GetTimeline(1);

            _mockTimelineRepo.Verify(r => r.GetTimeline(1, 5, null, null), Times.Once);
        }

        [Fact]
        public void GetTimeline_WithDifferentUserIds_CallsRepositoryWithCorrectUserId()
        {
            var expectedTimeline = new TimelineResult(
                new List<TopItem>(),
                new List<TopItem>(),
                new List<TopItem>()
            );

            _mockTimelineRepo
                .Setup(r => r.GetTimeline(42, 5, null, null))
                .Returns(expectedTimeline);

            _service.GetTimeline(42);

            _mockTimelineRepo.Verify(r => r.GetTimeline(42, 5, null, null), Times.Once);
        }
    }
}
