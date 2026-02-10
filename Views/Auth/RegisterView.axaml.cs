using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Lattice.ViewModels;

namespace Lattice.Views.Auth;

public partial class RegisterView : UserControl
{
    public RegisterView()
    {
        InitializeComponent();
    }

    private void OnPasswordChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox && DataContext is RegisterViewModel vm)
        {
            vm.Password = passwordBox.Password;
        }
    }

    private void OnConfirmPasswordChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox && DataContext is RegisterViewModel vm)
        {
            vm.ConfirmPassword = passwordBox.Password;
        }
    }
}
