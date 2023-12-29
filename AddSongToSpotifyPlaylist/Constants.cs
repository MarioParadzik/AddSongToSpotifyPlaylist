namespace AddSongToSpotifyPlaylist;

internal static class Endpoints
{
    internal const string AuthorizationEndpoint = "https://accounts.spotify.com/authorize";
    internal const string TokenEndpoint = "https://accounts.spotify.com/api/token";
    internal const string CurrentlyPlaying = "https://api.spotify.com/v1/me/player/currently-playing";
}

internal static class EnviornmentVariables
{
    internal const string ClientId = nameof(ClientId);
    internal const string ClientSecret = nameof(ClientSecret);
    internal const string VaultUri = nameof(VaultUri);
    internal const string Passphrase = nameof(Passphrase);
    internal const string PlaylistId = nameof(PlaylistId);
    internal const string RedirectUri = nameof(RedirectUri);
}

internal static class FunctionNames
{
    internal const string SpotifyAddSongToPlaylistFunction = nameof(SpotifyAddSongToPlaylistFunction);
    internal const string SpotifyAuthorizationFunction = nameof(SpotifyAuthorizationFunction);
    internal const string SpotifyCallbackFunction = nameof(SpotifyCallbackFunction);
}
internal static class KeyVaultName
{
    internal const string AccessToken = nameof(AccessToken);
    internal const string RefreshToken = nameof(RefreshToken);
    internal const string ExpiresIn = nameof(ExpiresIn);
}
