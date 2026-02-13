using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lattice.Services;

namespace Lattice.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private int _totalServers;

    [ObservableProperty]
    private int _runningServers;

    [ObservableProperty]
    private int _totalUsers;

    [ObservableProperty]
    private int _totalNodes;

    [ObservableProperty]
    private string _uptime = "0h 0m";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public DashboardViewModel(AuthService authService, NavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
    }

    public async Task LoadAsync()
    {
        if (_authService.Api == null) return;

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            await _authService.Api.ConnectAsync();
            var stats = await _authService.Api.GetDashboardStatsAsync();
            
            if (stats != null)
            {
                TotalServers = stats.TotalServers;
                RunningServers = stats.RunningServers;
                TotalUsers = stats.TotalUsers;
                TotalNodes = stats.Nodes;
                
                var ts = TimeSpan.FromSeconds(stats.Uptime);
                Uptime = $"{(int)ts.TotalHours}h {ts.Minutes}m";
            }
        }
        catch
        {
            ErrorMessage = "Failed to load statistics";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
