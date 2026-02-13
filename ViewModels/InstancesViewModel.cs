using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lattice.Services;

namespace Lattice.ViewModels;

public partial class InstanceItem : ObservableObject
{
    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _type = string.Empty;

    [ObservableProperty]
    private string _version = string.Empty;

    [ObservableProperty]
    private string _status = string.Empty;

    [ObservableProperty]
    private string _nodeId = string.Empty;

    [ObservableProperty]
    private string _nodeName = string.Empty;

    [ObservableProperty]
    private int _port;

    [ObservableProperty]
    private int _playerCount;

    [ObservableProperty]
    private int _maxPlayers;

    public bool IsRunning => Status == "running";
    public bool IsStopped => Status == "stopped";
}

public partial class InstancesViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<InstanceItem> _instances = new();

    [ObservableProperty]
    private ObservableCollection<InstanceItem> _filteredInstances = new();

    [ObservableProperty]
    private ObservableCollection<NodeItem> _nodes = new();

    [ObservableProperty]
    private NodeItem? _selectedNode;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isAdmin;

    [ObservableProperty]
    private bool _showCreateWizard;

    public bool HasInstances => FilteredInstances.Count > 0;
    public bool ShowEmptyState => !IsLoading && FilteredInstances.Count == 0;

    public InstancesViewModel(AuthService authService, NavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        IsAdmin = _authService.IsAdmin;
        
        Instances.CollectionChanged += (s, e) => ApplyFilter();
        FilteredInstances.CollectionChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(HasInstances));
            OnPropertyChanged(nameof(ShowEmptyState));
        };
    }

    public async Task LoadAsync()
    {
        if (_authService.Api == null) return;

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            await _authService.Api.ConnectAsync();
            
            var servers = await _authService.Api.GetServersAsync();
            var nodes = await _authService.Api.GetNodesAsync();
            
            Nodes.Clear();
            Nodes.Add(new NodeItem { Id = "", Name = "全部节点", Address = "", Status = "" });
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    Nodes.Add(new NodeItem
                    {
                        Id = node.Id,
                        Name = node.Name,
                        Address = node.Address,
                        Status = node.Status
                    });
                }
            }
            SelectedNode = Nodes.FirstOrDefault();
            
            Instances.Clear();
            if (servers != null)
            {
                foreach (var server in servers)
                {
                    Instances.Add(new InstanceItem
                    {
                        Id = server.Id,
                        Name = server.Name,
                        Type = server.Type,
                        Version = server.Version,
                        Status = server.Status,
                        Port = server.Port
                    });
                }
            }
            
            ApplyFilter();
        }
        catch (Exception ex)
        {
            ErrorMessage = "加载实例失败: " + ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSelectedNodeChanged(NodeItem? value)
    {
        ApplyFilter();
    }

    partial void OnSearchQueryChanged(string value)
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var filtered = Instances.AsEnumerable();
        
        if (SelectedNode != null && !string.IsNullOrEmpty(SelectedNode.Id))
        {
            filtered = filtered.Where(i => i.NodeId == SelectedNode.Id);
        }
        
        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            filtered = filtered.Where(i => 
                i.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                i.Type.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
        }
        
        FilteredInstances.Clear();
        foreach (var instance in filtered)
        {
            FilteredInstances.Add(instance);
        }
    }

    [RelayCommand]
    private async Task StartInstance(InstanceItem instance)
    {
        if (_authService.Api == null || instance == null) return;
        try
        {
            await _authService.Api.StartServerAsync(instance.Id);
            instance.Status = "starting";
            await Task.Delay(1000);
            await LoadAsync();
        }
        catch { }
    }

    [RelayCommand]
    private async Task StopInstance(InstanceItem instance)
    {
        if (_authService.Api == null || instance == null) return;
        try
        {
            await _authService.Api.StopServerAsync(instance.Id);
            instance.Status = "stopping";
            await Task.Delay(1000);
            await LoadAsync();
        }
        catch { }
    }

    [RelayCommand]
    private async Task RestartInstance(InstanceItem instance)
    {
        if (_authService.Api == null || instance == null) return;
        try
        {
            await _authService.Api.RestartServerAsync(instance.Id);
            instance.Status = "restarting";
            await Task.Delay(1000);
            await LoadAsync();
        }
        catch { }
    }

    [RelayCommand]
    private void CreateInstance()
    {
        _navigationService.NavigateTo<CreateServerViewModel>(_authService, _navigationService);
    }

    [RelayCommand]
    private void OpenInstance(InstanceItem instance)
    {
        if (instance == null) return;
        _navigationService.NavigateTo<ServerDetailViewModel>(_authService, _navigationService, instance.Id);
    }
}
