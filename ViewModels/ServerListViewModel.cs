using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    private ApiService? _apiService;

    [ObservableProperty]
    private ObservableCollection<Server> _servers = new();

    public bool HasServers => Servers.Count > 0;
    public bool ShowEmptyState => IsConnected && !HasServers;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isAdmin;

    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private bool _isConnected;

    public ServerListViewModel(AuthService authService, NavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        
        IsConnected = _authService.IsAuthenticated;
        
        if (IsConnected)
        {
            _apiService = new ApiService(_authService.ServerAddress);
            _apiService.SetAuthToken(_authService.AccessToken);
            IsAdmin = _authService.IsAdmin;
            UserName = _authService.CurrentUser?.Name ?? "User";
        }
        else
        {
            IsAdmin = false;
            UserName = string.Empty;
        }
        
        Servers.CollectionChanged += (s, e) => 
        {
            OnPropertyChanged(nameof(HasServers));
            OnPropertyChanged(nameof(ShowEmptyState));
        };
    }

    public async Task LoadServersAsync()
    {
        if (!IsConnected || _apiService == null) return;
        
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var servers = await _apiService.GetServersAsync(_authService.AccessToken);
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
        if (!IsAdmin) return;
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
        IsConnected = false;
        OnPropertyChanged(nameof(ShowEmptyState));
    }

    [RelayCommand]
    private void ManageUsers()
    {
        if (!IsAdmin) return;
        _navigationService.NavigateTo<UsersViewModel>(_authService, _navigationService);
    }

    [RelayCommand]
    private void Connect()
    {
        _navigationService.NavigateTo<ConnectViewModel>(_authService, _navigationService);
    }
}
