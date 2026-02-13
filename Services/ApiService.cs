using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Lattice.Models;

namespace Lattice.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private string _baseUrl;

    public ApiService(string baseUrl)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _httpClient = new HttpClient();
    }

    public void SetBaseUrl(string baseUrl)
    {
        _baseUrl = baseUrl.TrimEnd('/');
    }

    public void SetAuthToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<ConnectResult?> ConnectAsync(string token)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(new { token }),
            Encoding.UTF8,
            "application/json");

        try
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/connect", content);
            
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<ConnectResult>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<Server>?> GetServersAsync(string accessToken)
    {
        SetAuthToken(accessToken);
        return await _httpClient.GetFromJsonAsync<List<Server>>($"{_baseUrl}/api/servers");
    }

    public async Task<Server?> GetServerAsync(string id, string accessToken)
    {
        SetAuthToken(accessToken);
        return await _httpClient.GetFromJsonAsync<Server>($"{_baseUrl}/api/servers/{id}");
    }

    public async Task<Server?> CreateServerAsync(CreateServerRequest request, string accessToken)
    {
        SetAuthToken(accessToken);
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/api/servers", content);
        
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<Server>();
    }

    public async Task<bool> StartServerAsync(string id, string accessToken)
    {
        SetAuthToken(accessToken);
        var response = await _httpClient.PostAsync($"{_baseUrl}/api/servers/{id}/start", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> StopServerAsync(string id, string accessToken)
    {
        SetAuthToken(accessToken);
        var response = await _httpClient.PostAsync($"{_baseUrl}/api/servers/{id}/stop", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RestartServerAsync(string id, string accessToken)
    {
        SetAuthToken(accessToken);
        var response = await _httpClient.PostAsync($"{_baseUrl}/api/servers/{id}/restart", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SendCommandAsync(string id, string command, string accessToken)
    {
        SetAuthToken(accessToken);
        var content = new StringContent(
            JsonSerializer.Serialize(new { command }),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/api/servers/{id}/command", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteServerAsync(string id, string accessToken)
    {
        SetAuthToken(accessToken);
        var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/servers/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<List<string>?> GetServerTypesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<string>>($"{_baseUrl}/api/servers/types");
    }

    public async Task<List<string>?> GetVersionsAsync(string type)
    {
        return await _httpClient.GetFromJsonAsync<List<string>>($"{_baseUrl}/api/servers/versions/{type}");
    }

    public async Task<List<PluginInfo>?> GetPluginsAsync(string serverId, string accessToken, string type = "plugin")
    {
        SetAuthToken(accessToken);
        return await _httpClient.GetFromJsonAsync<List<PluginInfo>>($"{_baseUrl}/api/servers/{serverId}/plugins?type={type}");
    }

    public async Task<bool> EnablePluginAsync(string serverId, string path, string accessToken)
    {
        SetAuthToken(accessToken);
        var content = new StringContent(
            JsonSerializer.Serialize(new { path }),
            Encoding.UTF8,
            "application/json");
        var response = await _httpClient.PostAsync($"{_baseUrl}/api/servers/{serverId}/plugins/plugin/enable", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DisablePluginAsync(string serverId, string path, string accessToken)
    {
        SetAuthToken(accessToken);
        var content = new StringContent(
            JsonSerializer.Serialize(new { path }),
            Encoding.UTF8,
            "application/json");
        var response = await _httpClient.PostAsync($"{_baseUrl}/api/servers/{serverId}/plugins/plugin/disable", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeletePluginAsync(string serverId, string path, string accessToken)
    {
        SetAuthToken(accessToken);
        var content = new StringContent(
            JsonSerializer.Serialize(new { path }),
            Encoding.UTF8,
            "application/json");
        var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"{_baseUrl}/api/servers/{serverId}/plugins") { Content = content });
        return response.IsSuccessStatusCode;
    }

    public async Task<List<BackupInfo>?> GetBackupsAsync(string serverId, string accessToken)
    {
        SetAuthToken(accessToken);
        return await _httpClient.GetFromJsonAsync<List<BackupInfo>>($"{_baseUrl}/api/servers/{serverId}/backups");
    }

    public async Task<BackupInfo?> CreateBackupAsync(string serverId, string accessToken, string? name = null)
    {
        SetAuthToken(accessToken);
        var content = new StringContent(
            JsonSerializer.Serialize(new { name }),
            Encoding.UTF8,
            "application/json");
        var response = await _httpClient.PostAsync($"{_baseUrl}/api/servers/{serverId}/backups", content);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<BackupInfo>();
    }

    public async Task<List<MarketItem>?> SearchMarketAsync(string serverId, string accessToken, string source, string query, string type)
    {
        SetAuthToken(accessToken);
        var encodedQuery = Uri.EscapeDataString(query);
        return await _httpClient.GetFromJsonAsync<List<MarketItem>>(
            $"{_baseUrl}/api/servers/{serverId}/market/{source}/search?q={encodedQuery}&type={type}");
    }

    public async Task<List<MarketVersion>?> GetMarketVersionsAsync(string serverId, string accessToken, string source, string projectId)
    {
        SetAuthToken(accessToken);
        return await _httpClient.GetFromJsonAsync<List<MarketVersion>>(
            $"{_baseUrl}/api/servers/{serverId}/market/{source}/versions/{projectId}");
    }

    public async Task<DownloadResult?> DownloadFromMarketAsync(
        string serverId, 
        string accessToken,
        string source,
        string projectId,
        string versionId,
        string type,
        string namingTemplate,
        string? category)
    {
        SetAuthToken(accessToken);
        var content = new StringContent(
            JsonSerializer.Serialize(new { 
                source,
                projectId,
                versionId,
                type,
                namingTemplate,
                category
            }),
            Encoding.UTF8,
            "application/json");
        var response = await _httpClient.PostAsync($"{_baseUrl}/api/servers/{serverId}/market/download", content);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<DownloadResult>();
    }

    public async Task<List<TokenUser>?> GetUsersAsync(string accessToken)
    {
        SetAuthToken(accessToken);
        return await _httpClient.GetFromJsonAsync<List<TokenUser>>($"{_baseUrl}/api/users");
    }

    public async Task<TokenUser?> CreateUserAsync(string name, string accessToken)
    {
        SetAuthToken(accessToken);
        var content = new StringContent(
            JsonSerializer.Serialize(new { name }),
            Encoding.UTF8,
            "application/json");
        var response = await _httpClient.PostAsync($"{_baseUrl}/api/users", content);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<TokenUser>();
    }

    public async Task<bool> DeleteUserAsync(string userId, string accessToken)
    {
        SetAuthToken(accessToken);
        var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/users/{userId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<string?> RegenerateUserTokenAsync(string userId, string accessToken)
    {
        SetAuthToken(accessToken);
        var response = await _httpClient.PostAsync($"{_baseUrl}/api/users/{userId}/regenerate", null);
        if (!response.IsSuccessStatusCode) return null;
        var result = await response.Content.ReadFromJsonAsync<RegenerateResult>();
        return result?.Token;
    }

    public async Task<bool> GrantAccessAsync(string userId, string serverId, string accessToken)
    {
        SetAuthToken(accessToken);
        var content = new StringContent(
            JsonSerializer.Serialize(new { userId, serverId }),
            Encoding.UTF8,
            "application/json");
        var response = await _httpClient.PostAsync($"{_baseUrl}/api/users/grant", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RevokeAccessAsync(string userId, string serverId, string accessToken)
    {
        SetAuthToken(accessToken);
        var content = new StringContent(
            JsonSerializer.Serialize(new { userId, serverId }),
            Encoding.UTF8,
            "application/json");
        var response = await _httpClient.PostAsync($"{_baseUrl}/api/users/revoke", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<string>?> GetUserServersAsync(string userId, string accessToken)
    {
        SetAuthToken(accessToken);
        return await _httpClient.GetFromJsonAsync<List<string>>($"{_baseUrl}/api/users/{userId}/servers");
    }
}

public class ConnectResult
{
    [JsonPropertyName("user")]
    public UserInfo User { get; set; } = new();
    
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; } = string.Empty;
}

public class PluginInfo
{
    public string Name { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public string? Version { get; set; }
    public long Size { get; set; }
    public string Type { get; set; } = "plugin";
}

public class BackupInfo
{
    public string Id { get; set; } = string.Empty;
    public string ServerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Type { get; set; } = "manual";
}

public class MarketItem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public long Downloads { get; set; }
    public string Source { get; set; } = "modrinth";
}

public class MarketVersion
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string VersionNumber { get; set; } = string.Empty;
    public string GameVersion { get; set; } = string.Empty;
    public string Loader { get; set; } = string.Empty;
}

public class DownloadResult
{
    public bool Success { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? OriginalName { get; set; }
}

public class TokenUser
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
}

public class RegenerateResult
{
    public string Token { get; set; } = string.Empty;
}

public class CreateServerRequest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "vanilla";
    public string Version { get; set; } = string.Empty;
    public int Port { get; set; } = 25565;
    public string? MaxMemory { get; set; }
}
