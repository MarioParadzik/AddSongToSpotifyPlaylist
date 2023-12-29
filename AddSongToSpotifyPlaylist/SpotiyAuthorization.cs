using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using System.Web;
using Microsoft.Extensions.Logging;

namespace AddSongToSpotifyPlaylist;
internal class SpotiyAuthorization
{
    public const int RefreshTokenExpirationMinutes = 55;
    [Function(FunctionNames.SpotifyAuthorizationFunction)]
    public static HttpResponseData Run(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        // Implementation of the Spotify authorization code flow 
        var state = HelperMethods.GenerateRandomString(16);
        var scope = "user-read-currently-playing playlist-modify-public playlist-modify-private";

        var authorizeUrl = $"{Endpoints.AuthorizationEndpoint}?response_type=code" +
            $"&client_id={HelperMethods.GetEnvironmentVariable(EnviornmentVariables.ClientId)}&scope={HttpUtility.UrlEncode(scope)}" +
            $"&redirect_uri={HttpUtility.UrlEncode(HelperMethods.GetEnvironmentVariable(EnviornmentVariables.RedirectUri))}&state={HttpUtility.UrlEncode(state)}";

        var response = req.CreateResponse(HttpStatusCode.Redirect);
        response.Headers.Add("Location", authorizeUrl);

        return response;
    }

    [Function(FunctionNames.SpotifyCallbackFunction)]
    public static async Task<HttpResponseData> Callback(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req, ILogger _logger)
    {
        var response = req.CreateResponse();
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        try
        {
            // Exchange authorization code for access token
            string code = req.Query["code"] ?? throw new NullReferenceException("Code is not retrieved!");
            string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", HelperMethods.GetEnvironmentVariable("ClientId"),
                HelperMethods.GetEnvironmentVariable("ClientSecret"))));

            // Preparing client by example - https://developer.spotify.com/documentation/web-api/tutorials/code-flow
            using var client = new HttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);

            //Prepare Request Body
            List<KeyValuePair<string, string>> requestData = new()
            {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("redirect_uri", HelperMethods.GetEnvironmentVariable(EnviornmentVariables.RedirectUri))
            };

            HttpResponseMessage res = await client.PostAsync(Endpoints.TokenEndpoint, new FormUrlEncodedContent(requestData));
            if (res.IsSuccessStatusCode)
            {
                string responseContent = await res.Content.ReadAsStringAsync();
                if (responseContent != null)
                {
                    JObject tokenData = JObject.Parse(responseContent);
                    await HelperMethods.SetSecretAsync(KeyVaultName.AccessToken, (string)tokenData["access_token"]!);
                    await HelperMethods.SetSecretAsync(KeyVaultName.RefreshToken, (string)tokenData["refresh_token"]!);
                    await HelperMethods.SetSecretAsync(KeyVaultName.ExpiresIn, DateTime.Now.AddMinutes(RefreshTokenExpirationMinutes).ToString());

                    _logger.LogInformation("[+] Secrets set.");
                    response.StatusCode = HttpStatusCode.OK;
                    response.WriteString("Authorization successful!!");
                    return response;
                }
            }

            _logger.LogError("[-] An error occurred while trying to retrieve access token.");
            response.WriteString("Internal server error!");
            response.StatusCode = HttpStatusCode.InternalServerError;
            return response;
        } catch (Exception ex)
        {
            _logger.LogError("[-] An error occurred during the callback.", ex);
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.WriteString("Internal server error!");
            return response;
        }
    }
}
