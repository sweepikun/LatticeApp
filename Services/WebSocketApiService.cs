using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;

namespace Lattice.Services;

public class WsMessage
{
    public string Action { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public JsonElement? Data { get; set; }
    public string? Error { get; set; }
}

public class WsRequest
{
    public string Action { get; set; } = string.Empty;
    public object? Data { get; set; }
    public string RequestId { get; set; } = string.Empty;
}

public class UserInfo
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
}

public class ServerInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class NodeInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class TunnelInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int LocalPort { get; set; }
    public int RemotePort { get; set; }
    public string Type { get; set; } = "tcp";
}

public class FrpConfig
{
    public bool Enabled { get; set; }
    public string ServerAddr { get; set; } = string.Empty;
    public int ServerPort { get; set; } = 7000;
    public TunnelInfo[] Tunnels { get; set; } = Array.Empty<TunnelInfo>();
}

public class DashboardStats
{
    public int TotalServers { get; set; }
    public int RunningServers { get; set; }
    public int TotalUsers { get; set; }
    public int Nodes { get; set; }
    public long HeapUsed { get; set; }
    public long HeapTotal { get; set; }
    public double Uptime { get; set; }
}

public class MarketProject
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public long Downloads { get; set; }
}

public class MarketVersionInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string VersionNumber { get; set; } = string.Empty;
    public string[] GameVersions { get; set; } = Array.Empty<string>();
    public string[] Loaders { get; set; } = Array.Empty<string>();
}

public class WebSocketApiService : IDisposable
{
    private WebsocketClient? _client;
    private readonly string _serverUrl;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<WsMessage>> _pendingRequests = new();
    private int _requestIdCounter;

    public bool IsConnected => _client?.IsRunning ?? false;
    public event Action? Connected;
    public event Action? Disconnected;
    public event Action<string, string>? ServerLogReceived;
    public event Action<string, string>? ServerStatusChanged;

    public WebSocketApiService(string serverUrl)
    {
        _serverUrl = serverUrl;
    }

    public async Task ConnectAsync()
    {
        if (_client?.IsRunning == true) return;

        _client = new WebsocketClient(new Uri(_serverUrl));
        
        _client.ReconnectionHappened.Subscribe(_ => 
        {
            Connected?.Invoke();
        });

        _client.DisconnectionHappened.Subscribe(_ => 
        {
            Disconnected?.Invoke();
        });

        _client.MessageReceived.Subscribe(msg => 
        {
            try
            {
                if (string.IsNullOrEmpty(msg.Text)) return;
                var message = JsonSerializer.Deserialize<WsMessage>(msg.Text);
                if (message == null) return;

                if (!string.IsNullOrEmpty(message.RequestId) && 
                    _pendingRequests.TryRemove(message.RequestId, out var tcs))
                {
                    tcs.SetResult(message);
                }
                else if (message.Action == "servers/log" && message.Data.HasValue)
                {
                    var data = message.Data.Value;
                    var serverId = data.GetProperty("serverId").GetString();
                    var log = data.GetProperty("log").GetString();
                    if (serverId != null && log != null)
                    {
                        ServerLogReceived?.Invoke(serverId, log);
                    }
                }
                else if (message.Action == "servers/status" && message.Data.HasValue)
                {
                    var data = message.Data.Value;
                    var serverId = data.GetProperty("serverId").GetString();
                    var status = data.GetProperty("status").GetString();
                    if (serverId != null && status != null)
                    {
                        ServerStatusChanged?.Invoke(serverId, status);
                    }
                }
            }
            catch { }
        });

        await _client.Start();
    }

    private string GenerateRequestId()
    {
        return Interlocked.Increment(ref _requestIdCounter).ToString();
    }

    public async Task<WsMessage> SendRequestAsync(string action, object? data = null, int timeoutMs = 30000)
    {
        if (_client == null || !_client.IsRunning)
        {
            throw new Exception("Not connected");
        }

        var requestId = GenerateRequestId();
        var tcs = new TaskCompletionSource<WsMessage>();
        _pendingRequests[requestId] = tcs;

        var request = new WsRequest
        {
            Action = action,
            Data = data,
            RequestId = requestId
        };

        var json = JsonSerializer.Serialize(request);
        _client.Send(json);

        using var cts = new CancellationTokenSource(timeoutMs);
        cts.Token.Register(() => 
        {
            _pendingRequests.TryRemove(requestId, out _);
            tcs.TrySetCanceled();
        });

        return await tcs.Task;
    }

    private T? GetData<T>(WsMessage response)
    {
        if (!response.Success || !response.Data.HasValue)
            return default;
        
        return JsonSerializer.Deserialize<T>(response.Data.Value.GetRawText());
    }

    public async Task<ConnectResult?> AuthConnectAsync(string token)
    {
        var response = await SendRequestAsync("auth/connect", new { token });
        if (!response.Success) return null;
        return GetData<ConnectResult>(response);
    }

    public async Task<UserInfo?> AuthValidateAsync(string accessToken)
    {
        var response = await SendRequestAsync("auth/validate", new { token = accessToken });
        if (!response.Success) return null;
        return GetData<UserInfo>(response);
    }

    public async Task<ServerInfo[]?> GetServersAsync()
    {
        var response = await SendRequestAsync("servers/list");
        if (!response.Success) return null;
        return GetData<ServerInfo[]>(response);
    }

    public async Task<ServerInfo?> GetServerAsync(string id)
    {
        var response = await SendRequestAsync("servers/get", new { id });
        if (!response.Success) return null;
        return GetData<ServerInfo>(response);
    }

    public async Task<ServerInfo?> CreateServerAsync(object data)
    {
        var response = await SendRequestAsync("servers/create", data);
        if (!response.Success) return null;
        return GetData<ServerInfo>(response);
    }

    public async Task<bool> StartServerAsync(string id)
    {
        var response = await SendRequestAsync("servers/start", new { id });
        return response.Success;
    }

    public async Task<bool> StopServerAsync(string id)
    {
        var response = await SendRequestAsync("servers/stop", new { id });
        return response.Success;
    }

    public async Task<bool> RestartServerAsync(string id)
    {
        var response = await SendRequestAsync("servers/restart", new { id });
        return response.Success;
    }

    public async Task<bool> DeleteServerAsync(string id)
    {
        var response = await SendRequestAsync("servers/delete", new { id });
        return response.Success;
    }

    public async Task<bool> SendCommandAsync(string id, string command)
    {
        var response = await SendRequestAsync("servers/command", new { id, command });
        return response.Success;
    }

    public async Task<bool> SubscribeServerAsync(string id)
    {
        var response = await SendRequestAsync("servers/subscribe", new { id });
        return response.Success;
    }

    public async Task<bool> UnsubscribeServerAsync()
    {
        var response = await SendRequestAsync("servers/unsubscribe");
        return response.Success;
    }

    public async Task<string[]?> GetServerTypesAsync()
    {
        var response = await SendRequestAsync("servers/types");
        if (!response.Success) return null;
        return GetData<string[]>(response);
    }

    public async Task<string[]?> GetVersionsAsync(string type)
    {
        var response = await SendRequestAsync("servers/versions", new { type });
        if (!response.Success) return null;
        return GetData<string[]>(response);
    }

    public async Task<TokenUser[]?> GetUsersAsync()
    {
        var response = await SendRequestAsync("users/list");
        if (!response.Success) return null;
        return GetData<TokenUser[]>(response);
    }

    public async Task<TokenUser?> CreateUserAsync(string name)
    {
        var response = await SendRequestAsync("users/create", new { name });
        if (!response.Success) return null;
        return GetData<TokenUser>(response);
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        var response = await SendRequestAsync("users/delete", new { userId });
        return response.Success;
    }

    public async Task<string?> RegenerateTokenAsync(string userId)
    {
        var response = await SendRequestAsync("users/regenerate", new { userId });
        if (!response.Success || !response.Data.HasValue) return null;
        return response.Data.Value.GetProperty("token").GetString();
    }

    public async Task<bool> GrantAccessAsync(string userId, string serverId)
    {
        var response = await SendRequestAsync("users/grant", new { userId, serverId });
        return response.Success;
    }

    public async Task<bool> RevokeAccessAsync(string userId, string serverId)
    {
        var response = await SendRequestAsync("users/revoke", new { userId, serverId });
        return response.Success;
    }

    public async Task<NodeInfo[]?> GetNodesAsync()
    {
        var response = await SendRequestAsync("nodes/list");
        if (!response.Success) return null;
        return GetData<NodeInfo[]>(response);
    }

    public async Task<NodeInfo?> AddNodeAsync(string name, string address)
    {
        var response = await SendRequestAsync("nodes/add", new { name, address });
        if (!response.Success) return null;
        return GetData<NodeInfo>(response);
    }

    public async Task<bool> RemoveNodeAsync(string id)
    {
        var response = await SendRequestAsync("nodes/remove", new { id });
        return response.Success;
    }

    public async Task<FrpConfig?> GetFrpConfigAsync()
    {
        var response = await SendRequestAsync("frp/get");
        if (!response.Success) return null;
        return GetData<FrpConfig>(response);
    }

    public async Task<bool> UpdateFrpConfigAsync(bool enabled, string serverAddr, int serverPort)
    {
        var response = await SendRequestAsync("frp/update", new { enabled, serverAddr, serverPort });
        return response.Success;
    }

    public async Task<TunnelInfo?> AddTunnelAsync(string name, int localPort, int remotePort, string type = "tcp")
    {
        var response = await SendRequestAsync("frp/tunnel/add", new { name, localPort, remotePort, type });
        if (!response.Success) return null;
        return GetData<TunnelInfo>(response);
    }

    public async Task<bool> RemoveTunnelAsync(string id)
    {
        var response = await SendRequestAsync("frp/tunnel/remove", new { id });
        return response.Success;
    }

    public async Task<DashboardStats?> GetDashboardStatsAsync()
    {
        var response = await SendRequestAsync("dashboard/stats");
        if (!response.Success) return null;
        return GetData<DashboardStats>(response);
    }

    public async Task<MarketProject[]?> SearchProjectsAsync(string query, string type = "mod")
    {
        var response = await SendRequestAsync("downloads/search", new { query, type });
        if (!response.Success) return null;
        return GetData<MarketProject[]>(response);
    }

    public async Task<MarketVersion[]?> GetVersionsAsync(string slug, string? gameVersion = null, string? loader = null)
    {
        var response = await SendRequestAsync("downloads/versions", new { slug, gameVersion, loader });
        if (!response.Success) return null;
        return GetData<MarketVersion[]>(response);
    }

    public async Task<bool> DownloadAsync(string serverId, string slug, string versionId, string gameVersion, string loader)
    {
        var response = await SendRequestAsync("downloads/download", new { serverId, slug, versionId, gameVersion, loader }, 60000);
        return response.Success;
    }

    public async Task<PluginInfo[]?> GetServersPluginsAsync(string serverId, string type = "plugin")
    {
        var response = await SendRequestAsync("servers/plugins", new { id = serverId, type });
        if (!response.Success) return null;
        return GetData<PluginInfo[]>(response);
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}
