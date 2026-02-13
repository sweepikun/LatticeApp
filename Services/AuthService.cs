using System;

namespace Lattice.Services;

public class AuthService
{
    public UserInfo? CurrentUser { get; private set; }
    public string AccessToken { get; private set; } = string.Empty;
    public string ServerAddress { get; private set; } = string.Empty;
    public WebSocketApiService? Api { get; private set; }
    
    public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken) && CurrentUser != null;
    public bool IsAdmin => CurrentUser?.Role == "admin";

    public event Action? AuthStateChanged;

    public void SetConnection(string serverAddress, string accessToken, UserInfo user)
    {
        ServerAddress = serverAddress;
        AccessToken = accessToken;
        CurrentUser = user;
        
        var wsUrl = serverAddress
            .Replace("http://", "ws://")
            .Replace("https://", "wss://") + "/ws";
        Api = new WebSocketApiService(wsUrl);
        
        AuthStateChanged?.Invoke();
    }

    public void Clear()
    {
        Api?.Dispose();
        Api = null;
        ServerAddress = string.Empty;
        AccessToken = string.Empty;
        CurrentUser = null;
        AuthStateChanged?.Invoke();
    }
}
