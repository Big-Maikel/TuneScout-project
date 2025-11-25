using Logic.Interfaces;
using System;

namespace Logic.Services
{
    public class TimelineService
    {
        private readonly ITimelineRepository _timelineRepository;

        public TimelineService(ITimelineRepository timelineRepository)
        {
            _timelineRepository = timelineRepository ?? throw new ArgumentNullException(nameof(timelineRepository));
        }

        public TimelineResult GetTimeline(int userId, int topN = 5)
        {
            return _timelineRepository.GetTimeline(userId, topN);
        }
    }
}