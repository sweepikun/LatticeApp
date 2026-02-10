using Avalonia.Controls;
using Avalonia.Input;
using Lattice.ViewModels;

namespace Lattice.Views.Auth;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
    }

    private void OnPasswordChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox && DataContext is LoginViewModel vm)
        {
            vm.Password = passwordBox.Password;
        }
    }
}
