using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lattice.Services;

namespace Lattice.ViewModels;

public partial class ServerDetailViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly NavigationService _navigationService;
    private readonly string _serverId;

    [ObservableProperty]
    private InstanceItem? _instance;

    [ObservableProperty]
    private int _selectedTab = 0;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isAdmin;

    [ObservableProperty]
    private string _consoleOutput = string.Empty;

    [ObservableProperty]
    private string _commandInput = string.Empty;

    [ObservableProperty]
    private ObservableCollection<InstalledPluginItem> _plugins = new();

    [ObservableProperty]
    private ObservableCollection<InstalledPluginItem> _mods = new();

    [ObservableProperty]
    private bool _showPlugins = true;

    public bool IsConsoleTab => SelectedTab == 0;
    public bool IsConfigTab => SelectedTab == 1;
    public bool IsPluginsTab => SelectedTab == 2;
    public bool HasInstance => Instance != null;

    public ServerDetailViewModel(AuthService authService, NavigationService navigationService, string serverId)
    {
        _authService = authService;
        _navigationService = navigationService;
        _serverId = serverId;
        _isAdmin = _authService.IsAdmin;
    }

    partial void OnSelectedTabChanged(int value)
    {
        OnPropertyChanged(nameof(IsConsoleTab));
        OnPropertyChanged(nameof(IsConfigTab));
        OnPropertyChanged(nameof(IsPluginsTab));
        
        if (value == 2)
        {
            LoadPluginsCommand.Execute(null);
        }
    }

    public async Task LoadAsync()
    {
        if (_authService.Api == null) return;

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            await _authService.Api.ConnectAsync();
            var server = await _authService.Api.GetServerAsync(_serverId);
            
            if (server != null)
            {
                Instance = new InstanceItem
                {
                    Id = server.Id,
                    Name = server.Name,
                    Type = server.Type,
                    Version = server.Version,
                    Status = server.Status,
                    Port = server.Port
                };
                OnPropertyChanged(nameof(HasInstance));
                
                // Subscribe to server logs
                await _authService.Api.SubscribeServerAsync(_serverId);
                
                // Listen for logs
                _authService.Api.ServerLogReceived += (id, log) =>
                {
                    if (id == _serverId)
                    {
                        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                        {
                            ConsoleOutput += log + "\n";
                        });
                    }
                };
                
                // Listen for status changes
                _authService.Api.ServerStatusChanged += (id, status) =>
                {
                    if (id == _serverId && Instance != null)
                    {
                        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                        {
                            Instance.Status = status;
                            OnPropertyChanged(nameof(Instance));
                        });
                    }
                };
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "加载失败: " + ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void SelectTab(string tab)
    {
        if (int.TryParse(tab, out var index))
        {
            SelectedTab = index;
        }
    }

    [RelayCommand]
    private async Task Start()
    {
        if (_authService.Api == null || Instance == null) return;
        try
        {
            await _authService.Api.StartServerAsync(Instance.Id);
            Instance.Status = "starting";
        }
        catch { }
    }

    [RelayCommand]
    private async Task Stop()
    {
        if (_authService.Api == null || Instance == null) return;
        try
        {
            await _authService.Api.StopServerAsync(Instance.Id);
            Instance.Status = "stopping";
        }
        catch { }
    }

    [RelayCommand]
    private async Task Restart()
    {
        if (_authService.Api == null || Instance == null) return;
        try
        {
            await _authService.Api.RestartServerAsync(Instance.Id);
            Instance.Status = "restarting";
        }
        catch { }
    }

    [RelayCommand]
    private async Task SendCommand()
    {
        if (_authService.Api == null || string.IsNullOrWhiteSpace(CommandInput)) return;
        
        try
        {
            await _authService.Api.SendCommandAsync(_serverId, CommandInput);
            ConsoleOutput += $"> {CommandInput}\n";
            CommandInput = string.Empty;
        }
        catch { }
    }

    [RelayCommand]
    private async Task LoadPlugins()
    {
        if (_authService.Api == null) return;
        
        try
        {
            var plugins = await _authService.Api.GetServersPluginsAsync(_serverId, ShowPlugins ? "plugin" : "mod");
            Plugins.Clear();
            
            if (plugins != null)
            {
                foreach (var p in plugins)
                {
                    Plugins.Add(new InstalledPluginItem
                    {
                        Name = p.Name,
                        Version = p.Version ?? "Unknown",
                        Enabled = p.Enabled
                    });
                }
            }
        }
        catch { }
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigationService.NavigateTo<InstancesViewModel>(_authService, _navigationService);
    }
}

public partial class InstalledPluginItem : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _version = string.Empty;

    [ObservableProperty]
    private bool _enabled = true;
}
