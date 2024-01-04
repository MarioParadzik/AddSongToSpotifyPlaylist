# Add Song To Spotify Playlist
Automating Spotify playlist management with an Azure Function and Siri

It often happens that I stumble upon a good new song that I would like to save for later, without having to remember the name.

Siri and Spotify already have some integrations, but the one I needed was not available.
> https://support.spotify.com/us/article/siri-and-spotify/

In order to add the currently playing song, I need to do a few steps to make it work, but I wanted to do it in 1.
As you'll see in this guide I decided to leverage a Siri Shortcut to trigger a simple Azure Function.

I created 3 Azure Functions:

1. SpotifyAuthorizationFunction 
2. SpotifyCallbackFunction
3. SpotifyAddSongToPlaylistFunction

The first 2 are for authorization purposes, and the 3rd is the function I call with Siri Shortcut.

The call to authorize the user

```bash
curl -X GET http://localhost:7071/api/SpotifyAuthorizationFunction
```

After this call, Spotify app calls the Callback function and you retrieve the access and refresh token.

When this is set and done you can call the function to add songs

```bash
curl -X GET http://localhost:7071/api/SpotifyAddSongToPlaylistFunction?passpharse=YourPasspharse
```
The resources I used in Azure : 
1. Azure Function App 
2. Azure storage (commes with Azure Functions)
3. Azure Key Vault (tokens)
4. Application insights (logging)

## Writeup

https://marioparadzik.github.io/
