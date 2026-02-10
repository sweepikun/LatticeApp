using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lattice.Services;

namespace Lattice.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly NavigationService _navigationService;
    private readonly ApiService _apiService;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    public LoginViewModel()
    {
        _authService = new AuthService();
        _navigationService = new NavigationService();
        _apiService = new ApiService(_authService);
    }

    public LoginViewModel(AuthService authService, NavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        _apiService = new ApiService(_authService);
    }

    [RelayCommand]
    private async Task Login()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please enter username and password";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _apiService.LoginAsync(Username, Password);
            
            if (result != null)
            {
                _authService.SetTokens(result.AccessToken, result.RefreshToken);
                _authService.SetUser(result.User);
                _navigationService.NavigateTo<ServerListViewModel>(_authService, _navigationService);
            }
            else
            {
                ErrorMessage = "Invalid username or password";
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
    private void GoToRegister()
    {
        _navigationService.NavigateTo<RegisterViewModel>(_authService, _navigationService);
    }
}
