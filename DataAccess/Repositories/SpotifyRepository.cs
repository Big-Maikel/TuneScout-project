using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Logic.Interfaces;
using Logic.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories
{
    public class SpotifyRepository : ISpotifyRepository
    {
        private readonly HttpClient _httpClient;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _redirectUri;
        private readonly ILogger<SpotifyRepository> _logger;

        private CachedSpotifyToken? _appToken;
        private CachedSpotifyToken? _userToken;
        private readonly SemaphoreSlim _tokenLock = new(1, 1);

        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public SpotifyRepository(HttpClient httpClient, IConfiguration config, ILogger<SpotifyRepository> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _clientId = config["Spotify:ClientId"] ?? throw new ArgumentNullException("Spotify:ClientId missing");
            _clientSecret = config["Spotify:ClientSecret"] ?? throw new ArgumentNullException("Spotify:ClientSecret missing");
            _redirectUri = config["Spotify:RedirectUri"] ?? throw new ArgumentNullException("Spotify:RedirectUri missing");
        }

        private async Task EnsureAppTokenAsync(CancellationToken cancellationToken = default)
        {
            if (_appToken != null && _appToken.ExpiresAt > DateTimeOffset.UtcNow.AddSeconds(30))
                return;

            await _tokenLock.WaitAsync(cancellationToken);
            try
            {
                if (_appToken != null && _appToken.ExpiresAt > DateTimeOffset.UtcNow.AddSeconds(30))
                    return;

                var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));
                using var req = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
                req.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);
                req.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" }
                });

                using var resp = await _httpClient.SendAsync(req, cancellationToken);
                resp.EnsureSuccessStatusCode();

                var json = await resp.Content.ReadAsStringAsync(cancellationToken);
                var tokenResp = JsonSerializer.Deserialize<SpotifyTokenResponse>(json, _jsonOptions)!;

                _appToken = new CachedSpotifyToken
                {
                    AccessToken = tokenResp.AccessToken!,
                    ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(tokenResp.ExpiresIn)
                };
            }
            finally { _tokenLock.Release(); }
        }

        public string GetAuthorizationUrl()
        {
            var scope = "user-read-playback-state user-modify-playback-state streaming";

            return
                $"https://accounts.spotify.com/authorize" +
                $"?client_id={_clientId}" +
                $"&response_type=code" +
                $"&redirect_uri={Uri.EscapeDataString(_redirectUri)}" +
                $"&scope={Uri.EscapeDataString(scope)}";
        }

        public async Task<SpotifyUserToken> ExchangeCodeForTokenAsync(string code, CancellationToken cancellationToken = default)
        {
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));

            using var req = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
            req.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);

            req.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", Uri.EscapeDataString(_redirectUri) } 
            });

            using var resp = await _httpClient.SendAsync(req, cancellationToken);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync(cancellationToken);
            var tokenResp = JsonSerializer.Deserialize<SpotifyTokenResponse>(json, _jsonOptions)!;

            _userToken = new CachedSpotifyToken
            {
                AccessToken = tokenResp.AccessToken!,
                RefreshToken = tokenResp.RefreshToken!,
                ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(tokenResp.ExpiresIn)
            };

            return new SpotifyUserToken
            {
                AccessToken = tokenResp.AccessToken!,
                RefreshToken = tokenResp.RefreshToken!,
                ExpiresIn = tokenResp.ExpiresIn
            };
        }

        public string? GetUserAccessToken() => _userToken?.AccessToken;

        public async Task<IEnumerable<Track>> GetRecommendationsAsync(string seedGenres, int limit = 20, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(seedGenres)) seedGenres = "pop";
            await EnsureAppTokenAsync(cancellationToken);
            var token = _appToken!.AccessToken;

            var url = $"https://api.spotify.com/v1/recommendations?seed_genres={Uri.EscapeDataString(seedGenres)}&limit={limit}";
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var resp = await _httpClient.SendAsync(req, cancellationToken);
            if (!resp.IsSuccessStatusCode) return Enumerable.Empty<Track>();

            var json = await resp.Content.ReadAsStringAsync(cancellationToken);
            var doc = JsonSerializer.Deserialize<SpotifyRecommendationsResponse>(json, _jsonOptions);
            if (doc?.Tracks == null) return Enumerable.Empty<Track>();

            return doc.Tracks.Select(t => new Track
            {
                Id = 0,
                Name = t.Name ?? string.Empty,
                Artist = t.Artists?.FirstOrDefault()?.Name ?? string.Empty,
                SpotifyUri = t.Uri ?? $"spotify:track:{t.Id}",
                Explicit = t.Explicit,
                Valence = 0.5,
                GenreId = null
            }).ToList();
        }

        private class CachedSpotifyToken
        {
            public string AccessToken { get; init; } = string.Empty;
            public string? RefreshToken { get; init; }
            public DateTimeOffset ExpiresAt { get; init; }
        }

        private class SpotifyTokenResponse
        {
            public string? AccessToken { get; init; }
            public string? RefreshToken { get; init; }
            public int ExpiresIn { get; init; }
            public string? TokenType { get; init; }
        }

        private class SpotifyRecommendationsResponse
        {
            public SpotifyTrack[]? Tracks { get; init; }
        }

        private class SpotifyTrack
        {
            public string? Id { get; init; }
            public string? Name { get; init; }
            public bool Explicit { get; init; }
            public SpotifyArtist[]? Artists { get; init; }
            public string? Uri { get; init; }
        }

        private class SpotifyArtist
        {
            public string? Name { get; init; }
        }
    }
}
