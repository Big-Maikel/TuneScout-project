using System.Collections.Generic;

namespace TuneScout.Models
{
    public class TimelineViewModel
    {
        public List<TopItemViewModel> TopGenres { get; set; } = new();
        public List<TopItemViewModel> TopMoods { get; set; } = new();
        public List<TopItemViewModel> TopLanguages { get; set; } = new();

        public record TopItemViewModel(int Id, string Name, int Count);
    }
}