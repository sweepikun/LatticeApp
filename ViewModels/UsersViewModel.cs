using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lattice.Services;

namespace Lattice.ViewModels;

public partial class UserItem : ObservableObject
{
    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _role = string.Empty;

    [ObservableProperty]
    private string _token = string.Empty;
}

public partial class UsersViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<UserItem> _users = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _newUserName = string.Empty;

    [ObservableProperty]
    private bool _showAddForm;

    public UsersViewModel(AuthService authService, NavigationService navigationService)
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
            var users = await _authService.Api.GetUsersAsync();
            
            Users.Clear();
            if (users != null)
            {
                foreach (var user in users)
                {
                    Users.Add(new UserItem
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Role = user.Role,
                        Token = user.Token
                    });
                }
            }
        }
        catch
        {
            ErrorMessage = "Failed to load users";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ShowAdd()
    {
        NewUserName = string.Empty;
        ShowAddForm = true;
    }

    [RelayCommand]
    private void CancelAdd()
    {
        ShowAddForm = false;
    }

    [RelayCommand]
    private async Task AddUser()
    {
        if (_authService.Api == null) return;
        if (string.IsNullOrWhiteSpace(NewUserName)) return;

        IsLoading = true;

        try
        {
            var user = await _authService.Api.CreateUserAsync(NewUserName);
            if (user != null)
            {
                Users.Add(new UserItem
                {
                    Id = user.Id,
                    Name = user.Name,
                    Role = user.Role,
                    Token = user.Token
                });
                ShowAddForm = false;
            }
        }
        catch
        {
            ErrorMessage = "Failed to create user";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteUser(UserItem user)
    {
        if (_authService.Api == null || user == null) return;

        IsLoading = true;

        try
        {
            var success = await _authService.Api.DeleteUserAsync(user.Id);
            if (success)
            {
                Users.Remove(user);
            }
        }
        catch
        {
            ErrorMessage = "Failed to delete user";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RegenerateToken(UserItem user)
    {
        if (_authService.Api == null || user == null) return;

        IsLoading = true;

        try
        {
            var newToken = await _authService.Api.RegenerateTokenAsync(user.Id);
            if (newToken != null)
            {
                user.Token = newToken;
            }
        }
        catch
        {
            ErrorMessage = "Failed to regenerate token";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
