﻿using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace OllamaApiFacade.DemoWebApi;

public class KeyVaultHelper(string keyVaultUrl)
{
    private readonly SecretClient _secretClient = new(new Uri(keyVaultUrl), new DefaultAzureCredential());

    public async Task<string> GetSecretAsync(string secretName)
    {
        KeyVaultSecret secret = await _secretClient.GetSecretAsync(secretName);
        return secret.Value;
    }
}