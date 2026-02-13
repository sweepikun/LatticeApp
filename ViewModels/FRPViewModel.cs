using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lattice.Services;

namespace Lattice.ViewModels;

public partial class TunnelItem : ObservableObject
{
    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private int _localPort;

    [ObservableProperty]
    private int _remotePort;

    [ObservableProperty]
    private string _type = "tcp";
}

public partial class FRPViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private bool _enabled;

    [ObservableProperty]
    private string _serverAddress = string.Empty;

    [ObservableProperty]
    private int _serverPort = 7000;

    [ObservableProperty]
    private ObservableCollection<TunnelItem> _tunnels = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _newTunnelName = string.Empty;

    [ObservableProperty]
    private int _newTunnelLocalPort;

    [ObservableProperty]
    private int _newTunnelRemotePort;

    [ObservableProperty]
    private bool _isAddTunnelVisible;

    public FRPViewModel(AuthService authService, NavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
    }

    public async Task LoadAsync()
    {
        if (_authService.Api == null) return;

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            await _authService.Api.ConnectAsync();
            var config = await _authService.Api.GetFrpConfigAsync();
            
            if (config != null)
            {
                Enabled = config.Enabled;
                ServerAddress = config.ServerAddr;
                ServerPort = config.ServerPort;
                
                Tunnels.Clear();
                foreach (var tunnel in config.Tunnels)
                {
                    Tunnels.Add(new TunnelItem
                    {
                        Id = tunnel.Id,
                        Name = tunnel.Name,
                        LocalPort = tunnel.LocalPort,
                        RemotePort = tunnel.RemotePort,
                        Type = tunnel.Type
                    });
                }
            }
        }
        catch
        {
            ErrorMessage = "Failed to load FRP config";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveConfig()
    {
        if (_authService.Api == null) return;

        IsLoading = true;

        try
        {
            await _authService.Api.UpdateFrpConfigAsync(Enabled, ServerAddress, ServerPort);
            ErrorMessage = "Saved successfully";
        }
        catch
        {
            ErrorMessage = "Failed to save config";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ShowAddTunnelForm()
    {
        NewTunnelName = string.Empty;
        NewTunnelLocalPort = 0;
        NewTunnelRemotePort = 0;
        IsAddTunnelVisible = true;
    }

    [RelayCommand]
    private void CancelAddTunnel()
    {
        IsAddTunnelVisible = false;
    }

    [RelayCommand]
    private async Task AddTunnel()
    {
        if (_authService.Api == null) return;
        if (string.IsNullOrWhiteSpace(NewTunnelName)) return;

        IsLoading = true;

        try
        {
            var tunnel = await _authService.Api.AddTunnelAsync(NewTunnelName, NewTunnelLocalPort, NewTunnelRemotePort);
            if (tunnel != null)
            {
                Tunnels.Add(new TunnelItem
                {
                    Id = tunnel.Id,
                    Name = tunnel.Name,
                    LocalPort = tunnel.LocalPort,
                    RemotePort = tunnel.RemotePort,
                    Type = tunnel.Type
                });
                IsAddTunnelVisible = false;
            }
        }
        catch
        {
            ErrorMessage = "Failed to add tunnel";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RemoveTunnel(TunnelItem tunnel)
    {
        if (_authService.Api == null || tunnel == null) return;

        IsLoading = true;

        try
        {
            var success = await _authService.Api.RemoveTunnelAsync(tunnel.Id);
            if (success)
            {
                Tunnels.Remove(tunnel);
            }
        }
        catch
        {
            ErrorMessage = "Failed to remove tunnel";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
