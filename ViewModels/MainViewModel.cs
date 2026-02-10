using CommunityToolkit.Mvvm.ComponentModel;
using Lattice.Services;

namespace Lattice.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private ObservableObject? _currentView;

    public MainViewModel(AuthService authService, NavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        _navigationService.CurrentViewChanged += OnCurrentViewChanged;
        CurrentView = _navigationService.CurrentView;
    }

    private void OnCurrentViewChanged()
    {
        CurrentView = _navigationService.CurrentView;
    }
}
