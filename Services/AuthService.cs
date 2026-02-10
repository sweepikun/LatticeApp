using System;
using Lattice.Models;

namespace Lattice.Services;

public class AuthService
{
    public User? CurrentUser { get; private set; }
    public string AccessToken { get; private set; } = string.Empty;
    public string RefreshToken { get; private set; } = string.Empty;
    public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken);

    public event Action? AuthStateChanged;

    public void SetUser(User user)
    {
        CurrentUser = user;
        AuthStateChanged?.Invoke();
    }

    public void SetTokens(string accessToken, string refreshToken)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        AuthStateChanged?.Invoke();
    }

    public void Clear()
    {
        CurrentUser = null;
        AccessToken = string.Empty;
        RefreshToken = string.Empty;
        AuthStateChanged?.Invoke();
    }
}
