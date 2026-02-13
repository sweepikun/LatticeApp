using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lattice.Services;

namespace Lattice.ViewModels;

public partial class ConnectViewModel : ObservableObject
{
    private readonly ConnectionManager _connectionManager;
    private readonly AuthService _authService;
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<SavedConnection> _savedConnections = new();

    [ObservableProperty]
    private string _address = string.Empty;

    [ObservableProperty]
    private string _token = string.Empty;

    [ObservableProperty]
    private string _connectionName = string.Empty;

    [ObservableProperty]
    private bool _isConnecting;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _showNewConnectionForm;

    [ObservableProperty]
    private SavedConnection? _selectedConnection;

    public ConnectViewModel(AuthService authService, NavigationService navigationService)
    {
        _connectionManager = ConnectionManager.Instance;
        _authService = authService;
        _navigationService = navigationService;
        
        RefreshConnections();
        _connectionManager.ConnectionsChanged += RefreshConnections;
    }

    private void RefreshConnections()
    {
        SavedConnections.Clear();
        foreach (var conn in _connectionManager.Connections)
        {
            SavedConnections.Add(conn);
        }
        
        if (SavedConnections.Count == 0)
        {
            ShowNewConnectionForm = true;
        }
    }

    [RelayCommand]
    private void SelectConnection(SavedConnection connection)
    {
        SelectedConnection = connection;
        Address = connection.Address;
        Token = connection.Token;
        ConnectionName = connection.Name;
        ShowNewConnectionForm = false;
    }

    [RelayCommand]
    private void ShowNewForm()
    {
        SelectedConnection = null;
        Address = string.Empty;
        Token = string.Empty;
        ConnectionName = string.Empty;
        ShowNewConnectionForm = true;
    }

    [RelayCommand]
    private void DeleteConnection(SavedConnection connection)
    {
        _connectionManager.RemoveConnection(connection.Id);
        if (SelectedConnection?.Id == connection.Id)
        {
            SelectedConnection = null;
            ShowNewConnectionForm = true;
        }
    }

    [RelayCommand]
    private async Task Connect()
    {
        if (string.IsNullOrWhiteSpace(Address) || string.IsNullOrWhiteSpace(Token))
        {
            ErrorMessage = "Please enter address and token";
            return;
        }

        IsConnecting = true;
        ErrorMessage = string.Empty;

        try
        {
            var wsUrl = Address
                .Replace("http://", "ws://")
                .Replace("https://", "wss://") + "/ws";
            
            using var tempClient = new WebSocketApiService(wsUrl);
            await tempClient.ConnectAsync();
            
            var result = await tempClient.AuthConnectAsync(Token);

            if (result != null)
            {
                _authService.SetConnection(Address, result.AccessToken, result.User);

                if (SelectedConnection != null)
                {
                    _connectionManager.SetLastConnection(SelectedConnection.Id);
                }
                else if (!string.IsNullOrWhiteSpace(ConnectionName))
                {
                    var newConn = _connectionManager.AddConnection(ConnectionName, Address, Token);
                    _connectionManager.SetLastConnection(newConn.Id);
                }

                _navigationService.NavigateTo<MainViewModel>(_authService, _navigationService);
            }
            else
            {
                ErrorMessage = "Invalid token or server unreachable";
            }
        }
        catch
        {
            ErrorMessage = "Connection failed. Please check the address and token.";
        }
        finally
        {
            IsConnecting = false;
        }
    }
}
