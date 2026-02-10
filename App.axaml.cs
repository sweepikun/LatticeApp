using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Lattice.Services;
using Lattice.ViewModels;
using Lattice.Views;
using Lattice.Views.Auth;

namespace Lattice;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var authService = new AuthService();
        var navigationService = new NavigationService();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainViewModel = new MainViewModel(authService, navigationService);
            
            if (authService.IsAuthenticated)
            {
                navigationService.NavigateTo<ServerListViewModel>(authService, navigationService);
            }
            else
            {
                navigationService.NavigateTo<LoginViewModel>();
            }

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
