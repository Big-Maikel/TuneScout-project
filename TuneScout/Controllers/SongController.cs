using Logic.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace TuneScout.Controllers
{
    //public class SongController : Controller
    //{
    //    private readonly ISpotifyRepository _spotifyRepository;

    //    public SongController(ISpotifyRepository spotifyRepository)
    //    {
    //        _spotifyRepository = spotifyRepository;
    //    }

    //    public IActionResult Connect()
    //    {
    //        var url = _spotifyRepository.GetAuthorizationUrl();
    //        return Redirect(url);
    //    }

    //    [HttpGet("callback")]
    //    public async Task<IActionResult> Callback(string code)
    //    {
    //        if (string.IsNullOrEmpty(code))
    //            return Content("Geen code ontvangen van Spotify.");

    //        var token = await _spotifyRepository.ExchangeCodeForTokenAsync(code);

    //        HttpContext.Session.SetString("SpotifyAccessToken", token.AccessToken);
    //        HttpContext.Session.SetString("SpotifyRefreshToken", token.RefreshToken);

    //        return RedirectToAction("Index", "Home");
    //    }

    //    public bool IsUserLoggedIn()
    //    {
    //        return !string.IsNullOrEmpty(HttpContext.Session.GetString("SpotifyAccessToken"));
    //    }

    //    public IActionResult Status()
    //    {
    //        if (IsUserLoggedIn())
    //            return Content("Spotify is verbonden");
    //        else
    //            return Content("Spotify is niet verbonden");
    //    }
    //}
}
