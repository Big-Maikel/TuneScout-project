using Microsoft.AspNetCore.Mvc.RazorPages;
using Logic.Interfaces;

namespace TuneScout.Pages
{
    public class SpotifyModel : PageModel
    {
        private readonly ISpotifyRepository _spotifyRepository;

        public SpotifyModel(ISpotifyRepository spotifyRepository)
        {
            _spotifyRepository = spotifyRepository;
        }

        public bool SpotifyConnected { get; set; }

        public void OnGet()
        {
            SpotifyConnected = !string.IsNullOrEmpty(_spotifyRepository.GetUserAccessToken());
        }
    }
}
