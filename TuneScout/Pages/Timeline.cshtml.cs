using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Logic.Services;
using TuneScout.Models;
using System;
using System.Linq;
using System.Globalization;

namespace TuneScout.Pages
{
    public class TimelineModel : PageModel
    {
        private readonly TimelineService _timelineService;
        private readonly ILogger<TimelineModel> _logger;

        public TimelineViewModel ViewModel { get; set; } = new();
        
        [BindProperty(SupportsGet = true)]
        public string Period { get; set; } = "all";

        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }

        public string? ValidationMessage { get; set; }

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

            if (FromDate.HasValue && ToDate.HasValue && ToDate.Value < FromDate.Value)
            {
                ValidationMessage = "De einddatum kan niet eerder zijn dan de begindatum. Selecteer een geldige periode.";
                _logger?.LogWarning("Invalid date range: FromDate={FromDate}, ToDate={ToDate}", FromDate, ToDate);
                
                Period = "all";
                FromDate = null;
                ToDate = null;
            }

            if (FromDate.HasValue && FromDate.Value > DateTime.Now.Date)
            {
                ValidationMessage = "De begindatum kan niet in de toekomst liggen.";
                _logger?.LogWarning("FromDate is in the future: {FromDate}", FromDate);
                FromDate = null;
                ToDate = null;
                Period = "all";
            }

            if (ToDate.HasValue && ToDate.Value > DateTime.Now.Date)
            {
                ValidationMessage = "De einddatum kan niet in de toekomst liggen.";
                _logger?.LogWarning("ToDate is in the future: {ToDate}", ToDate);
                ToDate = null;
                Period = "all";
            }

            DateTime? calculatedFromDate = null;
            DateTime? calculatedToDate = ToDate;

            if (FromDate.HasValue && string.IsNullOrEmpty(ValidationMessage))
            {
                Period = "custom";
                calculatedFromDate = FromDate.Value.Date;
                
                if (ToDate.HasValue)
                {
                    calculatedToDate = ToDate.Value.Date.AddDays(1).AddTicks(-1); 
                }
                else
                {
                    calculatedToDate = DateTime.Now.Date.AddDays(1).AddTicks(-1);
                }
            }
            else if (string.IsNullOrEmpty(ValidationMessage))
            {
                calculatedFromDate = Period switch
                {
                    "week" => GetStartOfWeek(DateTime.Now),
                    "month" => new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                    _ => null
                };
            }

            var result = _timelineService.GetTimeline(userId.Value, topN: 10, calculatedFromDate, calculatedToDate);

            ViewModel.TopGenres = result.TopGenres.Select(x => new TimelineViewModel.TopItemViewModel(x.Id, x.Name, x.Count)).ToList();
            ViewModel.TopMoods = result.TopMoods.Select(x => new TimelineViewModel.TopItemViewModel(x.Id, x.Name, x.Count)).ToList();
            ViewModel.TopLanguages = result.TopLanguages.Select(x => new TimelineViewModel.TopItemViewModel(x.Id, x.Name, x.Count)).ToList();

            return Page();
        }

        private DateTime GetStartOfWeek(DateTime date)
        {
            var culture = CultureInfo.GetCultureInfo("nl-NL");
            var dayOfWeek = culture.Calendar.GetDayOfWeek(date);
            
            int daysToSubtract = ((int)dayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            
            return date.Date.AddDays(-daysToSubtract);
        }
    }
}