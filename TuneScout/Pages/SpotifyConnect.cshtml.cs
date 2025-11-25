using System;
using System.Threading;
using System.Threading.Tasks;
using Logic.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace TuneScout.Pages
{
    public class SpotifyConnectModel : PageModel
    {
        private readonly ISpotifyRepository _spotifyRepository;
        private readonly ILogger<SpotifyConnectModel> _logger;

        public SpotifyConnectModel(ISpotifyRepository spotifyRepository, ILogger<SpotifyConnectModel> logger)
        {
            _spotifyRepository = spotifyRepository;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _spotifyRepository.GetRecommendationsAsync("pop", 1, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // log and expose the full message so you can paste it here
                _logger.LogError(ex, "Spotify connect attempt failed.");
                TempData["SpotifyError"] = ex.Message;
            }

            return RedirectToPage("/Spotify");
        }
    }
}