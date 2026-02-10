using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lattice.Services;

namespace Lattice.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly NavigationService _navigationService;
    private readonly ApiService _apiService;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    public RegisterViewModel()
    {
        _authService = new AuthService();
        _navigationService = new NavigationService();
        _apiService = new ApiService(_authService);
    }

    public RegisterViewModel(AuthService authService, NavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        _apiService = new ApiService(_authService);
    }

    [RelayCommand]
    private async Task Register()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email) || 
            string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please fill in all fields";
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match";
            return;
        }

        if (Password.Length < 6)
        {
            ErrorMessage = "Password must be at least 6 characters";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _apiService.RegisterAsync(Username, Email, Password);
            
            if (result != null)
            {
                _authService.SetTokens(result.AccessToken, result.RefreshToken);
                _authService.SetUser(result.User);
                _navigationService.NavigateTo<ServerListViewModel>(_authService, _navigationService);
            }
            else
            {
                ErrorMessage = "Registration failed. Username or email may already exist.";
            }
        }
        catch
        {
            ErrorMessage = "Connection error. Please try again.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void GoToLogin()
    {
        _navigationService.NavigateTo<LoginViewModel>(_authService, _navigationService);
    }
}
