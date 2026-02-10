using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Lattice.Models;

namespace Lattice.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly AuthService _authService;
    private const string BaseUrl = "http://localhost:3000/api";

    public ApiService(AuthService authService)
    {
        _authService = authService;
        _httpClient = new HttpClient();
    }

    private void AddAuthHeader()
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authService.AccessToken);
    }

    public async Task<AuthResult?> LoginAsync(string username, string password)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(new { username, password }),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync($"{BaseUrl}/auth/login", content);
        
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<AuthResult>();
    }

    public async Task<AuthResult?> RegisterAsync(string username, string email, string password)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(new { username, email, password }),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync($"{BaseUrl}/auth/register", content);
        
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<AuthResult>();
    }

    public async Task<bool> RefreshTokenAsync()
    {
        var content = new StringContent(
            JsonSerializer.Serialize(new { refreshToken = _authService.RefreshToken }),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync($"{BaseUrl}/auth/refresh", content);
        
        if (!response.IsSuccessStatusCode) return false;

        var result = await response.Content.ReadFromJsonAsync<AuthResult>();
        if (result != null)
        {
            _authService.SetTokens(result.AccessToken, result.RefreshToken);
            return true;
        }
        
        return false;
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        AddAuthHeader();
        return await _httpClient.GetFromJsonAsync<User>($"{BaseUrl}/auth/me");
    }

    public async Task<List<Server>?> GetServersAsync()
    {
        AddAuthHeader();
        return await _httpClient.GetFromJsonAsync<List<Server>>($"{BaseUrl}/servers");
    }

    public async Task<Server?> GetServerAsync(string id)
    {
        AddAuthHeader();
        return await _httpClient.GetFromJsonAsync<Server>($"{BaseUrl}/servers/{id}");
    }

    public async Task<Server?> CreateServerAsync(CreateServerRequest request)
    {
        AddAuthHeader();
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync($"{BaseUrl}/servers", content);
        
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<Server>();
    }

    public async Task<bool> StartServerAsync(string id)
    {
        AddAuthHeader();
        var response = await _httpClient.PostAsync($"{BaseUrl}/servers/{id}/start", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> StopServerAsync(string id)
    {
        AddAuthHeader();
        var response = await _httpClient.PostAsync($"{BaseUrl}/servers/{id}/stop", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RestartServerAsync(string id)
    {
        AddAuthHeader();
        var response = await _httpClient.PostAsync($"{BaseUrl}/servers/{id}/restart", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SendCommandAsync(string id, string command)
    {
        AddAuthHeader();
        var content = new StringContent(
            JsonSerializer.Serialize(new { command }),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync($"{BaseUrl}/servers/{id}/command", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteServerAsync(string id)
    {
        AddAuthHeader();
        var response = await _httpClient.DeleteAsync($"{BaseUrl}/servers/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<List<string>?> GetServerTypesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<string>>($"{BaseUrl}/servers/types");
    }

    public async Task<List<string>?> GetVersionsAsync(string type)
    {
        return await _httpClient.GetFromJsonAsync<List<string>>($"{BaseUrl}/servers/versions/{type}");
    }

    // Plugins
    public async Task<List<PluginInfo>?> GetPluginsAsync(string serverId, string type = "plugin")
    {
        AddAuthHeader();
        return await _httpClient.GetFromJsonAsync<List<PluginInfo>>($"{BaseUrl}/servers/{serverId}/plugins?type={type}");
    }

    public async Task<bool> EnablePluginAsync(string serverId, string path)
    {
        AddAuthHeader();
        var content = new StringContent(
            JsonSerializer.Serialize(new { path }),
            Encoding.UTF8,
            "application/json");
        var response = await _httpClient.PostAsync($"{BaseUrl}/servers/{serverId}/plugins/plugin/enable", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DisablePluginAsync(string serverId, string path)
    {
        AddAuthHeader();
        var content = new StringContent(
            JsonSerializer.Serialize(new { path }),
            Encoding.UTF8,
            "application/json");
        var response = await _httpClient.PostAsync($"{BaseUrl}/servers/{serverId}/plugins/plugin/disable", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeletePluginAsync(string serverId, string path)
    {
        AddAuthHeader();
        var content = new StringContent(
            JsonSerializer.Serialize(new { path }),
            Encoding.UTF8,
            "application/json");
        var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/servers/{serverId}/plugins") { Content = content });
        return response.IsSuccessStatusCode;
    }

    // AI
    public async Task<List<string>?> GetAIProvidersAsync()
    {
        AddAuthHeader();
        return await _httpClient.GetFromJsonAsync<List<string>>($"{BaseUrl}/ai/providers");
    }

    public async Task<string?> ConfigureAIAsync(string type, string apiKey, string model)
    {
        AddAuthHeader();
        var content = new StringContent(
            JsonSerializer.Serialize(new { type, apiKey, model }),
            Encoding.UTF8,
            "application/json");
        var response = await _httpClient.PostAsync($"{BaseUrl}/ai/configure", content);
        if (!response.IsSuccessStatusCode) return null;
        var result = await response.Content.ReadFromJsonAsync<ConfigureResult>();
        return result?.ProviderId;
    }

    public async Task<string?> ChatAIAsync(string providerId, string message)
    {
        AddAuthHeader();
        var content = new StringContent(
            JsonSerializer.Serialize(new { providerId, message }),
            Encoding.UTF8,
            "application/json");
        var response = await _httpClient.PostAsync($"{BaseUrl}/ai/chat", content);
        if (!response.IsSuccessStatusCode) return null;
        var result = await response.Content.ReadFromJsonAsync<ChatResult>();
        return result?.Response;
    }

    // Backups
    public async Task<List<BackupInfo>?> GetBackupsAsync(string serverId)
    {
        AddAuthHeader();
        return await _httpClient.GetFromJsonAsync<List<BackupInfo>>($"{BaseUrl}/servers/{serverId}/backups");
    }

    public async Task<BackupInfo?> CreateBackupAsync(string serverId, string? name = null)
    {
        AddAuthHeader();
        var content = new StringContent(
            JsonSerializer.Serialize(new { name }),
            Encoding.UTF8,
            "application/json");
        var response = await _httpClient.PostAsync($"{BaseUrl}/servers/{serverId}/backups", content);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<BackupInfo>();
    }
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

public class ConfigureResult
{
    public string ProviderId { get; set; } = string.Empty;
}

public class ChatResult
{
    public string Response { get; set; } = string.Empty;
}

public class CreateServerRequest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "vanilla";
    public string Version { get; set; } = string.Empty;
    public int Port { get; set; } = 25565;
    public string? MaxMemory { get; set; }
}
