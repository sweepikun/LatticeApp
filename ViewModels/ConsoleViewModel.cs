using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lattice.Models;
using Lattice.Services;

namespace Lattice.ViewModels;

public partial class ConsoleViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly NavigationService _navigationService;
    private readonly ApiService _apiService;
    private readonly WebSocketService _wsService;
    private readonly string _serverId;

    [ObservableProperty]
    private Server? _server;

    [ObservableProperty]
    private ObservableCollection<LogEntry> _logs = new();

    [ObservableProperty]
    private string _commandInput = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _autoScroll = true;

    public ConsoleViewModel(AuthService authService, NavigationService navigationService, string serverId)
    {
        _authService = authService;
        _navigationService = navigationService;
        _apiService = new ApiService(_authService.ServerAddress);
        
        var wsUrl = _authService.ServerAddress.Replace("http://", "ws://").Replace("https://", "wss://") + "/ws";
        _wsService = new WebSocketService(wsUrl);
        _serverId = serverId;

        _wsService.LogReceived += OnLogReceived;
        _wsService.StatusChanged += OnStatusChanged;
    }

    public async Task InitializeAsync()
    {
        IsLoading = true;
        try
        {
            Server = await _apiService.GetServerAsync(_serverId, _authService.AccessToken);
            await _wsService.ConnectAsync();
            _wsService.Subscribe(_serverId);
        }
        catch
        {
            ErrorMessage = "Failed to connect to server";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OnLogReceived(LogEntry log)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            Logs.Add(log);
            if (Logs.Count > 1000)
            {
                Logs.RemoveAt(0);
            }
        });
    }

    private void OnStatusChanged(string status)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            if (Server != null)
            {
                Server.Status = status;
                OnPropertyChanged(nameof(Server));
            }
        });
    }

    [RelayCommand]
    private void SendCommand()
    {
        if (string.IsNullOrWhiteSpace(CommandInput)) return;

        _wsService.SendCommand(_serverId, CommandInput);
        CommandInput = string.Empty;
    }

    [RelayCommand]
    private void ClearLogs()
    {
        Logs.Clear();
    }

    [RelayCommand]
    private async Task Start()
    {
        if (Server == null) return;
        await _apiService.StartServerAsync(_serverId, _authService.AccessToken);
    }

    [RelayCommand]
    private async Task Stop()
    {
        if (Server == null) return;
        await _apiService.StopServerAsync(_serverId, _authService.AccessToken);
    }

    [RelayCommand]
    private void GoBack()
    {
        _wsService.Unsubscribe();
        _wsService.Dispose();
        _navigationService.NavigateTo<ServerDetailViewModel>(_authService, _navigationService, _serverId);
    }
}
