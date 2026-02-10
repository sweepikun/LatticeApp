using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using Websocket.Client;

namespace Lattice.Services;

public class LogEntry
{
    public string Text { get; set; } = string.Empty;
    public string Level { get; set; } = "info";
    public DateTime Timestamp { get; set; }
}

public class WebSocketService : IDisposable
{
    private WebsocketClient? _client;
    private readonly string _serverUrl;

    public event Action<LogEntry>? LogReceived;
    public event Action<string>? StatusChanged;
    public event Action? Connected;
    public event Action? Disconnected;

    public bool IsConnected => _client?.IsRunning ?? false;

    public WebSocketService(string serverUrl = "ws://localhost:3000/ws")
    {
        _serverUrl = serverUrl;
    }

    public async Task ConnectAsync()
    {
        if (_client?.IsRunning == true) return;

        _client = new WebsocketClient(new Uri(_serverUrl));
        
        _client.ReconnectionHappened.Subscribe(info => 
        {
            Connected?.Invoke();
        });

        _client.DisconnectionHappened.Subscribe(info => 
        {
            Disconnected?.Invoke();
        });

        _client.MessageReceived.Subscribe(msg => 
        {
            try
            {
                var message = JsonSerializer.Deserialize<JsonElement>(msg.Text);
                if (message == null) return;

                var type = message.GetProperty("type").GetString();
                
                switch (type)
                {
                    case "log":
                        var serverId = message.GetProperty("serverId").GetString();
                        var data = message.GetProperty("data").GetString();
                        var timestamp = message.GetProperty("timestamp").GetInt64();
                        
                        LogReceived?.Invoke(new LogEntry
                        {
                            Text = data ?? "",
                            Level = ParseLogLevel(data),
                            Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).LocalDateTime
                        });
                        break;
                    
                    case "status":
                        var status = message.GetProperty("status").GetString();
                        StatusChanged?.Invoke(status ?? "");
                        break;
                }
            }
            catch
            {
                // Ignore parse errors
            }
        });

        await _client.Start();
    }

    public void Subscribe(string serverId)
    {
        Send(new { type = "subscribe", serverId });
    }

    public void Unsubscribe()
    {
        Send(new { type = "unsubscribe" });
    }

    public void SendCommand(string serverId, string command)
    {
        Send(new { type = "command", serverId, command });
    }

    private void Send(object message)
    {
        if (_client?.IsRunning == true)
        {
            var json = JsonSerializer.Serialize(message);
            _client.Send(json);
        }
    }

    private string ParseLogLevel(string? log)
    {
        if (string.IsNullOrEmpty(log)) return "info";
        
        var lower = log.ToLower();
        if (lower.Contains("error") || lower.Contains("exception") || lower.Contains("fatal"))
            return "error";
        if (lower.Contains("warn") || lower.Contains("warning"))
            return "warn";
        if (lower.Contains("debug") || lower.Contains("trace"))
            return "debug";
        
        return "info";
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}
