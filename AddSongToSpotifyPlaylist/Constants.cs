namespace AddSongToSpotifyPlaylist;

internal static class Endpoints
{
    internal const string AuthorizationEndpoint = "https://accounts.spotify.com/authorize";
    internal const string TokenEndpoint = "https://accounts.spotify.com/api/token";
    internal const string CurrentlyPlaying = "https://api.spotify.com/v1/me/player/currently-playing";
}

internal static class EnviornmentVariables
{
    internal const string ClientId = "ClientId";
    internal const string ClientSecret = "ClientSecret";
    internal const string VaultUri = "VaultUri";
    internal const string Passphrase = "Passphrase";
    internal const string PlaylistId = "PlaylistId";
    internal const string RedirectUri = "RedirectUri";
}

internal static class FunctionNames
{
    internal const string SpotifyAddSongToPlaylistFunction = "SpotifyAddSongToPlaylistFunction";
    internal const string SpotifyAuthorizationFunction = "SpotifyAuthorizationFunction";
    internal const string SpotifyCallbackFunction = "SpotifyCallbackFunction";
}
internal static class KeyVaultName
{
    internal const string AccessToken = "AccessToken";
    internal const string RefreshToken = "RefreshToken";
    internal const string ExpiresIn = "ExpiresIn";
}
