using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lattice.Models;
using Lattice.Services;

namespace Lattice.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private User? _currentUser;

    public DashboardViewModel()
    {
        _authService = new AuthService();
        _navigationService = new NavigationService();
        CurrentUser = _authService.CurrentUser;
    }

    public DashboardViewModel(AuthService authService, NavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        CurrentUser = _authService.CurrentUser;
    }

    [RelayCommand]
    private void Logout()
    {
        _authService.Clear();
        _navigationService.NavigateTo<LoginViewModel>(_authService, _navigationService);
    }
}
