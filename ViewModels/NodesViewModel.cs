using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using Lattice.Services;

namespace Lattice.ViewModels;

public partial class NodeItem : ObservableObject
{
    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _address = string.Empty;

    [ObservableProperty]
    private string _status = string.Empty;

    [ObservableProperty]
    private bool _isOnline;

    [ObservableProperty]
    private double _cpuUsage;

    [ObservableProperty]
    private double _memoryUsage;

    [ObservableProperty]
    private long _memoryUsed;

    [ObservableProperty]
    private long _memoryTotal;

    [ObservableProperty]
    private int _serverCount;

    public ObservableCollection<ObservableValue> CpuHistory { get; } = new();
    public ObservableCollection<ObservableValue> MemoryHistory { get; } = new();
    
    public ISeries[] CpuSeries { get; set; }
    public ISeries[] MemorySeries { get; set; }

    public NodeItem()
    {
        for (int i = 0; i < 20; i++)
        {
            CpuHistory.Add(new ObservableValue(0));
            MemoryHistory.Add(new ObservableValue(0));
        }
        
        CpuSeries = new ISeries[]
        {
            new LineSeries<ObservableValue>
            {
                Values = CpuHistory,
                Fill = null,
                Stroke = new SolidColorPaint(new SKColor(96, 205, 255)) { StrokeThickness = 2 },
                LineSmoothness = 0.5,
                GeometrySize = 0
            }
        };
        
        MemorySeries = new ISeries[]
        {
            new LineSeries<ObservableValue>
            {
                Values = MemoryHistory,
                Fill = null,
                Stroke = new SolidColorPaint(new SKColor(108, 203, 95)) { StrokeThickness = 2 },
                LineSmoothness = 0.5,
                GeometrySize = 0
            }
        };
    }

    public void UpdateStats(double cpu, double memory)
    {
        CpuUsage = cpu;
        MemoryUsage = memory;
        
        CpuHistory.RemoveAt(0);
        CpuHistory.Add(new ObservableValue(cpu));
        
        MemoryHistory.RemoveAt(0);
        MemoryHistory.Add(new ObservableValue(memory));
    }
}

public partial class NodesViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<NodeItem> _nodes = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _newNodeName = string.Empty;

    [ObservableProperty]
    private string _newNodeAddress = string.Empty;

    [ObservableProperty]
    private bool _showAddForm;

    [ObservableProperty]
    private bool _hasNodes;

    public NodesViewModel(AuthService authService, NavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        
        Nodes.CollectionChanged += (s, e) => HasNodes = Nodes.Count > 0;
    }

    public async Task LoadAsync()
    {
        if (_authService.Api == null) return;

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            await _authService.Api.ConnectAsync();
            var nodes = await _authService.Api.GetNodesAsync();
            
            Nodes.Clear();
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var item = new NodeItem
                    {
                        Id = node.Id,
                        Name = node.Name,
                        Address = node.Address,
                        IsOnline = node.Status == "online"
                    };
                    Nodes.Add(item);
                }
            }
        }
        catch
        {
            ErrorMessage = "加载节点失败";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ShowAdd()
    {
        NewNodeName = string.Empty;
        NewNodeAddress = string.Empty;
        ShowAddForm = true;
    }

    [RelayCommand]
    private void CancelAdd()
    {
        ShowAddForm = false;
    }

    [RelayCommand]
    private async Task AddNode()
    {
        if (_authService.Api == null) return;
        if (string.IsNullOrWhiteSpace(NewNodeName) || string.IsNullOrWhiteSpace(NewNodeAddress)) return;

        IsLoading = true;

        try
        {
            var node = await _authService.Api.AddNodeAsync(NewNodeName, NewNodeAddress);
            if (node != null)
            {
                Nodes.Add(new NodeItem
                {
                    Id = node.Id,
                    Name = node.Name,
                    Address = node.Address,
                    IsOnline = node.Status == "online"
                });
                ShowAddForm = false;
            }
        }
        catch
        {
            ErrorMessage = "添加节点失败";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RemoveNode(NodeItem node)
    {
        if (_authService.Api == null || node == null) return;

        IsLoading = true;

        try
        {
            var success = await _authService.Api.RemoveNodeAsync(node.Id);
            if (success)
            {
                Nodes.Remove(node);
            }
        }
        catch
        {
            ErrorMessage = "删除节点失败";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ConnectNode(NodeItem node)
    {
        if (node == null) return;
        
    }
}
