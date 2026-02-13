using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Lattice.Services;

public class SavedConnection
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime LastUsed { get; set; }
}

public class ConnectionConfig
{
    public List<SavedConnection> Connections { get; set; } = new();
    public string? LastConnectionId { get; set; }
    public Dictionary<string, object> Settings { get; set; } = new();
}

public class ConnectionManager
{
    private static ConnectionManager? _instance;
    public static ConnectionManager Instance => _instance ??= new ConnectionManager();

    private readonly string _configPath;
    private ConnectionConfig _config;

    public event Action? ConnectionsChanged;

    public IReadOnlyList<SavedConnection> Connections => _config.Connections;
    public SavedConnection? LastConnection => 
        string.IsNullOrEmpty(_config.LastConnectionId) 
            ? null 
            : _config.Connections.Find(c => c.Id == _config.LastConnectionId);

    private ConnectionManager()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var latticeDir = Path.Combine(appData, "Lattice");
        Directory.CreateDirectory(latticeDir);
        _configPath = Path.Combine(latticeDir, "connections.json");
        _config = LoadConfig();
    }

    private ConnectionConfig LoadConfig()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                var json = File.ReadAllText(_configPath);
                return JsonSerializer.Deserialize<ConnectionConfig>(json) ?? new ConnectionConfig();
            }
        }
        catch { }
        
        return new ConnectionConfig();
    }

    private void SaveConfig()
    {
        try
        {
            var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configPath, json);
            ConnectionsChanged?.Invoke();
        }
        catch { }
    }

    public SavedConnection AddConnection(string name, string address, string token)
    {
        var connection = new SavedConnection
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Address = address,
            Token = token,
            LastUsed = DateTime.Now
        };
        
        _config.Connections.Add(connection);
        SaveConfig();
        
        return connection;
    }

    public void UpdateConnection(string id, string name, string address, string? token = null)
    {
        var connection = _config.Connections.Find(c => c.Id == id);
        if (connection == null) return;
        
        connection.Name = name;
        connection.Address = address;
        if (token != null) connection.Token = token;
        connection.LastUsed = DateTime.Now;
        
        SaveConfig();
    }

    public void RemoveConnection(string id)
    {
        var index = _config.Connections.FindIndex(c => c.Id == id);
        if (index >= 0)
        {
            _config.Connections.RemoveAt(index);
            if (_config.LastConnectionId == id)
            {
                _config.LastConnectionId = null;
            }
            SaveConfig();
        }
    }

    public void SetLastConnection(string id)
    {
        var connection = _config.Connections.Find(c => c.Id == id);
        if (connection != null)
        {
            connection.LastUsed = DateTime.Now;
            _config.LastConnectionId = id;
            SaveConfig();
        }
    }

    public SavedConnection? GetConnection(string id)
    {
        return _config.Connections.Find(c => c.Id == id);
    }

    public T GetSetting<T>(string key, T defaultValue)
    {
        if (_config.Settings.TryGetValue(key, out var value))
        {
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch { }
        }
        return defaultValue;
    }

    public void SetSetting<T>(string key, T value)
    {
        if (value == null)
        {
            _config.Settings.Remove(key);
        }
        else
        {
            _config.Settings[key] = value!;
        }
        SaveConfig();
    }
}
