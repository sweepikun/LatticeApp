using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Lattice.Services;

public class NavigationService
{
    private ObservableObject? _currentView;
    public ObservableObject? CurrentView => _currentView;

    public event Action? CurrentViewChanged;

    public void NavigateTo<T>(params object[] args) where T : ObservableObject
    {
        _currentView = (T?)Activator.CreateInstance(typeof(T), args);
        CurrentViewChanged?.Invoke();
    }

    public void NavigateTo(ObservableObject viewModel)
    {
        _currentView = viewModel;
        CurrentViewChanged?.Invoke();
    }
}
