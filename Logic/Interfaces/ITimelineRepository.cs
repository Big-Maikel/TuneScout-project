using System;
using System.Collections.Generic;

namespace Logic.Interfaces
{
    public record TopItem(int Id, string Name, int Count);

    public record TimelineResult(IReadOnlyList<TopItem> TopGenres, IReadOnlyList<TopItem> TopMoods, IReadOnlyList<TopItem> TopLanguages);

    public interface ITimelineRepository
    {
        TimelineResult GetTimeline(int userId, int topN = 5, DateTime? fromDate = null, DateTime? toDate = null);
    }
}