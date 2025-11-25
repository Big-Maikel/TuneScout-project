using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Logic.Services;
using TuneScout.Models;

namespace TuneScout.Pages
{
    public class TimelineModel : PageModel
    {
        private readonly TimelineService _timelineService;
        private readonly ILogger<TimelineModel> _logger;

        public TimelineViewModel ViewModel { get; set; } = new();

        public TimelineModel(TimelineService timelineService, ILogger<TimelineModel> logger)
        {
            _timelineService = timelineService;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            var result = _timelineService.GetTimeline(userId.Value, topN: 10);

            ViewModel.TopGenres = result.TopGenres.Select(x => new TimelineViewModel.TopItemViewModel(x.Id, x.Name, x.Count)).ToList();
            ViewModel.TopMoods = result.TopMoods.Select(x => new TimelineViewModel.TopItemViewModel(x.Id, x.Name, x.Count)).ToList();
            ViewModel.TopLanguages = result.TopLanguages.Select(x => new TimelineViewModel.TopItemViewModel(x.Id, x.Name, x.Count)).ToList();

            return Page();
        }
    }
}