using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lattice.Services;

namespace Lattice.ViewModels;

public partial class MarketItem : ObservableObject
{
    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _icon = string.Empty;

    [ObservableProperty]
    private string _author = string.Empty;

    [ObservableProperty]
    private long _downloads;

    [ObservableProperty]
    private string _source = "modrinth";
}

public partial class MarketVersion : ObservableObject
{
    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _versionNumber = string.Empty;

    [ObservableProperty]
    private string _gameVersion = string.Empty;

    [ObservableProperty]
    private string _loader = string.Empty;
}

public partial class PluginMarketViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly NavigationService _navigationService;
    private readonly ApiService _apiService;
    private readonly string _serverId;

    [ObservableProperty]
    private ObservableCollection<MarketItem> _results = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _selectedSource = "modrinth";

    [ObservableProperty]
    private string _selectedType = "mod";

    [ObservableProperty]
    private bool _isSearching;

    [ObservableProperty]
    private bool _isDownloading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private MarketItem? _selectedItem;

    [ObservableProperty]
    private ObservableCollection<MarketVersion> _versions = new();

    [ObservableProperty]
    private bool _showVersions;

    [ObservableProperty]
    private string _namingTemplate = "original";

    [ObservableProperty]
    private string _category = string.Empty;

    public PluginMarketViewModel(AuthService authService, NavigationService navigationService, string serverId)
    {
        _authService = authService;
        _navigationService = navigationService;
        _apiService = new ApiService(_authService);
        _serverId = serverId;
    }

    [RelayCommand]
    private async Task Search()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery)) return;

        IsSearching = true;
        ErrorMessage = string.Empty;
        Results.Clear();
        ShowVersions = false;

        try
        {
            var items = await _apiService.SearchMarketAsync(_serverId, SelectedSource, SearchQuery, SelectedType);
            if (items != null)
            {
                foreach (var item in items)
                {
                    Results.Add(item);
                }
            }
        }
        catch
        {
            ErrorMessage = "Failed to search";
        }
        finally
        {
            IsSearching = false;
        }
    }

    [RelayCommand]
    private async Task SelectItem(MarketItem item)
    {
        SelectedItem = item;
        Versions.Clear();
        ShowVersions = true;
        IsSearching = true;

        try
        {
            var versions = await _apiService.GetMarketVersionsAsync(_serverId, item.Source, item.Id);
            if (versions != null)
            {
                foreach (var v in versions)
                {
                    Versions.Add(v);
                }
            }
        }
        catch
        {
            ErrorMessage = "Failed to load versions";
        }
        finally
        {
            IsSearching = false;
        }
    }

    [RelayCommand]
    private async Task Download(MarketVersion version)
    {
        if (SelectedItem == null) return;

        IsDownloading = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _apiService.DownloadFromMarketAsync(
                _serverId,
                SelectedItem.Source,
                SelectedItem.Id,
                version.Id,
                SelectedType,
                NamingTemplate,
                Category
            );

            if (result != null)
            {
                ErrorMessage = $"Downloaded: {result.FileName}";
            }
            else
            {
                ErrorMessage = "Download failed";
            }
        }
        catch
        {
            ErrorMessage = "Download failed";
        }
        finally
        {
            IsDownloading = false;
        }
    }

    [RelayCommand]
    private void CloseVersions()
    {
        ShowVersions = false;
        SelectedItem = null;
        Versions.Clear();
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigationService.NavigateTo<ServerDetailViewModel>(_authService, _navigationService, _serverId);
    }
}
