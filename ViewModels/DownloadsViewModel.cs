using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lattice.Services;

namespace Lattice.ViewModels;

public partial class DownloadItem : ObservableObject
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

public partial class DownloadsViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<DownloadItem> _results = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private int _selectedCategory = 1;

    [ObservableProperty]
    private bool _isSearching;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _selectedType = "mod";

    [ObservableProperty]
    private ObservableCollection<string> _categories = new() { "核心", "插件", "模组" };

    public bool HasResults => Results.Count > 0;
    public bool ShowEmptyState => !IsSearching && Results.Count == 0 && !string.IsNullOrEmpty(SearchQuery);

    public DownloadsViewModel(AuthService authService, NavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        
        Results.CollectionChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(HasResults));
            OnPropertyChanged(nameof(ShowEmptyState));
        };
    }

    partial void OnSelectedCategoryChanged(int value)
    {
        SelectedType = value switch
        {
            0 => "core",
            1 => "plugin",
            2 => "mod",
            _ => "mod"
        };
        
        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            SearchCommand.Execute(null);
        }
    }

    [RelayCommand]
    private async Task Search()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery)) return;
        if (_authService.Api == null) return;

        IsSearching = true;
        ErrorMessage = string.Empty;
        Results.Clear();

        try
        {
            await _authService.Api.ConnectAsync();
            
            if (SelectedCategory == 0)
            {
                await LoadCores();
            }
            else
            {
                var type = SelectedCategory == 1 ? "plugin" : "mod";
                var projects = await _authService.Api.SearchProjectsAsync(SearchQuery, type);
                
                if (projects != null)
                {
                    foreach (var p in projects)
                    {
                        Results.Add(new DownloadItem
                        {
                            Id = p.Id,
                            Title = p.Title,
                            Description = p.Description,
                            Icon = p.Icon,
                            Author = p.Author,
                            Downloads = p.Downloads,
                            Source = "modrinth"
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "搜索失败: " + ex.Message;
        }
        finally
        {
            IsSearching = false;
        }
    }

    private async Task LoadCores()
    {
        if (_authService.Api == null) return;
        
        var types = await _authService.Api.GetServerTypesAsync();
        if (types != null)
        {
            foreach (var type in types)
            {
                Results.Add(new DownloadItem
                {
                    Id = type,
                    Title = type.ToUpper(),
                    Description = GetCoreDescription(type),
                    Source = "core"
                });
            }
        }
    }

    private string GetCoreDescription(string type)
    {
        return type.ToLower() switch
        {
            "vanilla" => "Minecraft 官方服务端",
            "paper" => "高性能 Spigot 分支，优化 TPS",
            "spigot" => "流行的模组服务端平台",
            "forge" => "Minecraft 模组加载器",
            "fabric" => "轻量级模组加载器",
            _ => "Minecraft 服务端核心"
        };
    }

    [RelayCommand]
    private Task SelectItem(DownloadItem item)
    {
        if (item == null) return Task.CompletedTask;
        return Task.CompletedTask;
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchQuery = string.Empty;
        Results.Clear();
    }
}
