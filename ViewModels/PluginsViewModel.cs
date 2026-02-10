using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lattice.Services;

namespace Lattice.ViewModels;

public partial class PluginItem : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    private bool _enabled;

    [ObservableProperty]
    private string _version = string.Empty;

    [ObservableProperty]
    private long _size;

    [ObservableProperty]
    private string _type = "plugin";
}

public partial class PluginsViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly NavigationService _navigationService;
    private readonly ApiService _apiService;
    private readonly string _serverId;

    [ObservableProperty]
    private ObservableCollection<PluginItem> _plugins = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _filterType = "plugin";

    public PluginsViewModel(AuthService authService, NavigationService navigationService, string serverId)
    {
        _authService = authService;
        _navigationService = navigationService;
        _apiService = new ApiService(_authService);
        _serverId = serverId;
    }

    public async Task LoadPluginsAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var plugins = await _apiService.GetPluginsAsync(_serverId, FilterType);
            Plugins.Clear();
            if (plugins != null)
            {
                foreach (var plugin in plugins)
                {
                    Plugins.Add(new PluginItem
                    {
                        Name = plugin.Name,
                        FileName = plugin.FileName,
                        Enabled = plugin.Enabled,
                        Version = plugin.Version ?? "",
                        Size = plugin.Size,
                        Type = plugin.Type
                    });
                }
            }
        }
        catch
        {
            ErrorMessage = "Failed to load plugins";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task TogglePlugin(PluginItem plugin)
    {
        try
        {
            if (plugin.Enabled)
            {
                await _apiService.DisablePluginAsync(_serverId, plugin.FileName);
            }
            else
            {
                await _apiService.EnablePluginAsync(_serverId, plugin.FileName);
            }
            plugin.Enabled = !plugin.Enabled;
        }
        catch
        {
            ErrorMessage = "Failed to toggle plugin";
        }
    }

    [RelayCommand]
    private async Task DeletePlugin(PluginItem plugin)
    {
        try
        {
            await _apiService.DeletePluginAsync(_serverId, plugin.FileName);
            Plugins.Remove(plugin);
        }
        catch
        {
            ErrorMessage = "Failed to delete plugin";
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigationService.NavigateTo<ServerListViewModel>(_authService, _navigationService);
    }
}
