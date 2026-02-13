using System;
using System.Collections.ObjectModel;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lattice.Services;

namespace Lattice.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly NavigationService _navigationService;
    private readonly ConnectionManager _connectionManager;

    [ObservableProperty]
    private int _selectedTheme = 0;

    [ObservableProperty]
    private int _selectedLanguage = 0;

    [ObservableProperty]
    private bool _autoConnectOnStartup = true;

    [ObservableProperty]
    private bool _startMinimized;

    [ObservableProperty]
    private bool _autoStartWithSystem;

    [ObservableProperty]
    private ObservableCollection<SavedConnection> _savedNodes = new();

    [ObservableProperty]
    private bool _hasSavedNodes;

    [ObservableProperty]
    private string _appVersion = "1.0.0";

    public string[] ThemeOptions { get; } = { "跟随系统", "浅色", "深色" };
    public string[] LanguageOptions { get; } = { "简体中文", "English" };

    public SettingsViewModel(AuthService authService, NavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        _connectionManager = ConnectionManager.Instance;
        
        LoadSettings();
        RefreshNodes();
        _connectionManager.ConnectionsChanged += RefreshNodes;
    }

    private void LoadSettings()
    {
        AutoConnectOnStartup = _connectionManager.GetSetting("AutoConnectOnStartup", true);
        StartMinimized = _connectionManager.GetSetting("StartMinimized", false);
        AutoStartWithSystem = _connectionManager.GetSetting("AutoStartWithSystem", false);
        
        var theme = _connectionManager.GetSetting("Theme", "auto");
        SelectedTheme = theme switch
        {
            "light" => 1,
            "dark" => 2,
            _ => 0
        };
        
        var lang = _connectionManager.GetSetting("Language", "zh-CN");
        SelectedLanguage = lang == "en-US" ? 1 : 0;
        
        try
        {
            var version = GetType().Assembly.GetName().Version;
            AppVersion = version?.ToString(3) ?? "1.0.0";
        }
        catch
        {
            AppVersion = "1.0.0";
        }
    }

    partial void OnSelectedThemeChanged(int value)
    {
        var theme = value switch
        {
            1 => "light",
            2 => "dark",
            _ => "auto"
        };
        _connectionManager.SetSetting("Theme", theme);
        ApplyTheme(theme);
    }

    partial void OnSelectedLanguageChanged(int value)
    {
        var lang = value == 1 ? "en-US" : "zh-CN";
        _connectionManager.SetSetting("Language", lang);
    }

    partial void OnAutoConnectOnStartupChanged(bool value)
    {
        _connectionManager.SetSetting("AutoConnectOnStartup", value);
    }

    partial void OnStartMinimizedChanged(bool value)
    {
        _connectionManager.SetSetting("StartMinimized", value);
    }

    partial void OnAutoStartWithSystemChanged(bool value)
    {
        _connectionManager.SetSetting("AutoStartWithSystem", value);
    }

    private void ApplyTheme(string theme)
    {
        if (Application.Current == null) return;
        
        var requestedTheme = theme switch
        {
            "light" => Avalonia.Styling.ThemeVariant.Light,
            "dark" => Avalonia.Styling.ThemeVariant.Dark,
            _ => Avalonia.Styling.ThemeVariant.Default
        };
        Application.Current.RequestedThemeVariant = requestedTheme;
    }

    private void RefreshNodes()
    {
        SavedNodes.Clear();
        foreach (var conn in _connectionManager.Connections)
        {
            SavedNodes.Add(conn);
        }
        HasSavedNodes = SavedNodes.Count > 0;
    }

    [RelayCommand]
    private void DeleteNode(SavedConnection connection)
    {
        if (connection == null) return;
        _connectionManager.RemoveConnection(connection.Id);
    }
}
