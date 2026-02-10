using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lattice.Models;
using Lattice.Services;

namespace Lattice.ViewModels;

public partial class ServerListViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly NavigationService _navigationService;
    private readonly ApiService _apiService;

    [ObservableProperty]
    private ObservableCollection<Server> _servers = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private User? _currentUser;

    public ServerListViewModel(AuthService authService, NavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        _apiService = new ApiService(_authService);
        CurrentUser = _authService.CurrentUser;
    }

    public async Task LoadServersAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var servers = await _apiService.GetServersAsync();
            Servers.Clear();
            if (servers != null)
            {
                foreach (var server in servers)
                {
                    Servers.Add(server);
                }
            }
        }
        catch
        {
            ErrorMessage = "Failed to load servers";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void CreateServer()
    {
        _navigationService.NavigateTo<CreateServerViewModel>(_authService, _navigationService);
    }

    [RelayCommand]
    private void SelectServer(Server server)
    {
        _navigationService.NavigateTo<ServerDetailViewModel>(_authService, _navigationService, server.Id);
    }

    [RelayCommand]
    private void Logout()
    {
        _authService.Clear();
        _navigationService.NavigateTo<LoginViewModel>(_authService, _navigationService);
    }
}
