using Logic.Models;

namespace TuneScout.Models
{
    public class SettingsViewModel
    {
        public bool NoExplicit { get; set; }
        public List<int> SelectedGenreIds { get; set; } = new();
        public List<int> SelectedMoodIds { get; set; } = new();
        public int? SelectedLanguageId { get; set; }
        
        public List<Genre> Genres { get; set; } = new();
        public List<Mood> Moods { get; set; } = new();
        public List<(int Id, string Name)> Languages { get; set; } = new();
        
        public string? Message { get; set; }
    }
}
