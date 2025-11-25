using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Logic.Models;

namespace Logic.Interfaces
{
    public interface ISpotifyRepository
    {
        Task<IEnumerable<Track>> GetRecommendationsAsync(
            string seedGenres,
            int limit = 20,
            CancellationToken cancellationToken = default);

        string GetAuthorizationUrl(); 
        Task<SpotifyUserToken> ExchangeCodeForTokenAsync(
            string code,
            CancellationToken cancellationToken = default);
        string? GetUserAccessToken();
    }
}
