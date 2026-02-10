using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lattice.Models;
using Lattice.Services;

namespace Lattice.ViewModels;

public partial class ServerDetailViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly NavigationService _navigationService;
    private readonly ApiService _apiService;
    private readonly string _serverId;

    [ObservableProperty]
    private Server? _server;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public ServerDetailViewModel(AuthService authService, NavigationService navigationService, string serverId)
    {
        _authService = authService;
        _navigationService = navigationService;
        _apiService = new ApiService(_authService);
        _serverId = serverId;
    }

    public async Task LoadServerAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            Server = await _apiService.GetServerAsync(_serverId);
        }
        catch
        {
            ErrorMessage = "Failed to load server";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task Start()
    {
        if (Server == null) return;
        
        try
        {
            await _apiService.StartServerAsync(_serverId);
            await LoadServerAsync();
        }
        catch
        {
            ErrorMessage = "Failed to start server";
        }
    }

    [RelayCommand]
    private async Task Stop()
    {
        if (Server == null) return;
        
        try
        {
            await _apiService.StopServerAsync(_serverId);
            await LoadServerAsync();
        }
        catch
        {
            ErrorMessage = "Failed to stop server";
        }
    }

    [RelayCommand]
    private async Task Restart()
    {
        if (Server == null) return;
        
        try
        {
            await _apiService.RestartServerAsync(_serverId);
            await LoadServerAsync();
        }
        catch
        {
            ErrorMessage = "Failed to restart server";
        }
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (Server == null) return;
        
        try
        {
            await _apiService.DeleteServerAsync(_serverId);
            _navigationService.NavigateTo<ServerListViewModel>(_authService, _navigationService);
        }
        catch
        {
            ErrorMessage = "Failed to delete server";
        }
    }

    [RelayCommand]
    private void OpenConsole()
    {
        _navigationService.NavigateTo<ConsoleViewModel>(_authService, _navigationService, _serverId);
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigationService.NavigateTo<ServerListViewModel>(_authService, _navigationService);
    }
}
