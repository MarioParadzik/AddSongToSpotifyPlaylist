using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace AddSongToSpotifyPlaylist;
internal static class HelperMethods
{
    /// <summary>
    /// Asynchronously retrieves the value of a secret with the specified name from an Azure Key Vault.
    /// </summary>
    /// <param name="secretName">The name of the secret to be retrieved.</param>
    /// <returns>The secret value for the provided name.</returns>
    internal static async Task<string> GetSecretAsync(string secretName)
    {
        var client = new SecretClient(new Uri(GetEnvironmentVariable(EnviornmentVariables.VaultUri)), new DefaultAzureCredential());
        var secret = await client.GetSecretAsync(secretName);

        return secret.Value.Value;
    }

    /// <summary>
    /// Asynchronously sets a new secret value for the specified secret name in an Azure Key Vault.
    /// </summary>
    /// <param name="secretName">The name of the secret to be updated.</param>
    /// <param name="newSecretValue">The new value to set for the secret.</param>
    /// <returns></returns>
    internal static async Task SetSecretAsync(string secretName, string newSecretValue)
    {
        var client = new SecretClient(new Uri(GetEnvironmentVariable(EnviornmentVariables.VaultUri)), new DefaultAzureCredential());
        await client.SetSecretAsync(new KeyVaultSecret(secretName, newSecretValue));
    }

    /// <summary>
    /// Generates a random string of a specified length.
    /// </summary>
    /// <param name="length"></param>
    /// <returns>Random generated string of a specified length</returns>
    internal static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    /// <summary>
    /// Retrieves the value of an environment variable with the specified name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns>The value of the environment variable if it is set; otherwise, throws an <see cref="ArgumentNullException"/></returns>
    /// <exception cref="ArgumentNullException">Thrown if the environment variable with the given name is not set.</exception>
    internal static string GetEnvironmentVariable(string name) => Environment.GetEnvironmentVariable(name) ?? throw new ArgumentNullException($"Variable {name} not set!");
}

