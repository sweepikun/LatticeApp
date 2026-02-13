using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lattice.Services;

namespace Lattice.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly NavigationService _navigationService;
    private Window? _window;

    [ObservableProperty]
    private ObservableObject? _currentView;

    [ObservableProperty]
    private string _currentPage = "dashboard";

    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private bool _isAdmin;

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private bool _isNavExpanded = true;

    [ObservableProperty]
    private string _connectedNodeName = "未连接节点";

    [ObservableProperty]
    private bool _hasConnectedNode;

    [ObservableProperty]
    private ObservableCollection<NodeItem> _availableNodes = new();

    [ObservableProperty]
    private NodeItem? _selectedNode;

    public MainViewModel(AuthService authService, NavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        _navigationService.CurrentViewChanged += OnCurrentViewChanged;
        
        IsConnected = _authService.IsAuthenticated;
        UserName = _authService.CurrentUser?.Name ?? "User";
        IsAdmin = _authService.IsAdmin;
        
        UpdateConnectionStatus();
        
        NavigateTo("dashboard");
    }

    public void SetWindow(Window window)
    {
        _window = window;
    }

    private void OnCurrentViewChanged()
    {
        CurrentView = _navigationService.CurrentView;
    }

    public void UpdateConnectionStatus()
    {
        HasConnectedNode = _authService.Api != null && _authService.IsAuthenticated;
        
        if (HasConnectedNode && !string.IsNullOrEmpty(_authService.ServerAddress))
        {
            ConnectedNodeName = _authService.ServerAddress
                .Replace("http://", "")
                .Replace("https://", "")
                .Split('/')[0];
        }
        else
        {
            ConnectedNodeName = "未连接节点";
        }
    }

    [RelayCommand]
    public void NavigateTo(string page)
    {
        CurrentPage = page;
        
        switch (page)
        {
            case "dashboard":
                _navigationService.NavigateTo<DashboardViewModel>(_authService, _navigationService);
                break;
            case "instances":
                _navigationService.NavigateTo<InstancesViewModel>(_authService, _navigationService);
                break;
            case "downloads":
                _navigationService.NavigateTo<DownloadsViewModel>(_authService, _navigationService);
                break;
            case "nodes":
                _navigationService.NavigateTo<NodesViewModel>(_authService, _navigationService);
                break;
            case "users":
                _navigationService.NavigateTo<UsersViewModel>(_authService, _navigationService);
                break;
            case "frp":
                _navigationService.NavigateTo<FRPViewModel>(_authService, _navigationService);
                break;
            case "settings":
                _navigationService.NavigateTo<SettingsViewModel>(_authService, _navigationService);
                break;
        }
    }

    [RelayCommand]
    private void ToggleNav()
    {
        IsNavExpanded = !IsNavExpanded;
    }

    [RelayCommand]
    private void Logout()
    {
        _authService.Clear();
        _navigationService.NavigateTo<ConnectViewModel>(_authService, _navigationService);
    }

    [RelayCommand]
    private void Minimize()
    {
        if (_window != null)
        {
            _window.WindowState = WindowState.Minimized;
        }
    }

    [RelayCommand]
    private void Maximize()
    {
        if (_window != null)
        {
            _window.WindowState = _window.WindowState == WindowState.Maximized 
                ? WindowState.Normal 
                : WindowState.Maximized;
        }
    }

    [RelayCommand]
    private void Close()
    {
        _window?.Close();
    }
}
