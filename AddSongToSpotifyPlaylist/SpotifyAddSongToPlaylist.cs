using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace AddSongToSpotifyPlaylist;
public static class SpotifyAddSongToPlaylist
{

    [Function(FunctionNames.SpotifyAddSongToPlaylistFunction)]
    public static async Task<HttpResponseData> SpotifyAddSongToPlaylistFunction(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req, FunctionContext executionContext)
    {
        var response = req.CreateResponse();
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

        var _logger = executionContext.GetLogger(FunctionNames.SpotifyAddSongToPlaylistFunction);

        try
        {
            // Read the query string
            string passphrase = req.Query["passphrase"] ?? throw new NullReferenceException("Passphrase is missing!");

            string env_passphrase = HelperMethods.GetEnvironmentVariable("Passphrase");

            // Simple Auth
            if (!passphrase.Equals(env_passphrase, StringComparison.Ordinal))
            {
                _logger.LogError("[-] Passphrase not matching!");
                response.StatusCode = HttpStatusCode.Unauthorized;
                response.WriteString("Unauthorized: Access Denied. Please provide valid credentials.!");
                return response;
            }

            // Check if access token expired.
            if (DateTime.Parse(await HelperMethods.GetSecretAsync(KeyVaultName.ExpiresIn)).ToUniversalTime().CompareTo(DateTime.Now.ToUniversalTime()) <= 0)
            {
                _logger.LogInformation("[+] Refreshing token.");
                await RefreshAccessToken(_logger);
            }

            if (await AddSongToPlaylist(_logger))
            {
                response.StatusCode = HttpStatusCode.OK;
                response.WriteString("Song successfully added!");
                return response;
            }

            _logger.LogError("[-] An error occurred while trying to add song to playlist.");
            response.WriteString("Internal server error!");
            response.StatusCode = HttpStatusCode.InternalServerError;
            return response;

        } catch (Exception ex)
        {
            _logger.LogError("[-] An error occurred while adding song to playlist.", ex);
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.WriteString("Internal server error!");
            return response;
        }
        
    }

    private static async Task<bool> AddSongToPlaylist(ILogger _logger)
    {
        // Preparing client by example - https://developer.spotify.com/documentation/web-api/reference/get-recently-played
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await HelperMethods.GetSecretAsync("AccessToken"));
        HttpResponseMessage res = await client.GetAsync(Endpoints.CurrentlyPlaying);

        if (!res.IsSuccessStatusCode)
        {
            _logger.LogError("[-] Error Getting current playing song ({StatusCode})", res.StatusCode);
            return false;
        }

        // Extract track ID from the response
        string? trackId = JObject.Parse(await res.Content.ReadAsStringAsync())?.SelectToken("item.uri")?.Value<string>();
        if (string.IsNullOrEmpty(trackId))
        {
            _logger.LogError("[-] Unable to extract track ID from the response.");
            return false;
        }

        _logger.LogInformation("[+] Track ID: {trackId}", trackId);

        // Add the song to the playlist.
        var jsonPayload = JsonConvert.SerializeObject(new { uris = new[] { trackId } });
        string uri = $"https://api.spotify.com/v1/playlists/{HelperMethods.GetEnvironmentVariable(EnviornmentVariables.PlaylistId)}/tracks";
        res = await client.PostAsync(uri, new StringContent(jsonPayload, Encoding.UTF8, "application/json"));

        if (!res.IsSuccessStatusCode)
        {
            _logger.LogError("[-] Error posting song to playlist ({StatusCode})", res.StatusCode);
            return false;
        }
            
        _logger.LogInformation("[+] Song posted to playlist!");
        return true;
    }

    private static async Task RefreshAccessToken(ILogger _logger)
    {
        var refreshToken = await HelperMethods.GetSecretAsync(KeyVaultName.RefreshToken);
        string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", HelperMethods.GetEnvironmentVariable(EnviornmentVariables.ClientId),
            HelperMethods.GetEnvironmentVariable(EnviornmentVariables.ClientSecret))));

        // Preparing client by example - https://developer.spotify.com/documentation/web-api/tutorials/refreshing-tokens
        using var client = new HttpClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);

        List<KeyValuePair<string, string>> requestData = new()
        {
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("refresh_token", refreshToken)
        };

        HttpResponseMessage response = await client.PostAsync(Endpoints.TokenEndpoint, new FormUrlEncodedContent(requestData));
        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            if (responseContent != null)
            {
                JObject tokenData = JObject.Parse(responseContent);
                await HelperMethods.SetSecretAsync(KeyVaultName.AccessToken, (string)tokenData["access_token"]!);
                await HelperMethods.SetSecretAsync(KeyVaultName.ExpiresIn, DateTime.Now.AddMinutes(SpotiyAuthorization.RefreshTokenExpirationMinutes).ToString());
                _logger.LogError("[+] Token refreshed and updated.");
            }
        } else
        {
            _logger.LogError("[-] Error while trying to refresh token.");
        }
        
    }

}
