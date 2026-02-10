using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lattice.Services;

namespace Lattice.ViewModels;

public partial class CreateServerViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly NavigationService _navigationService;
    private readonly ApiService _apiService;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _selectedType = "vanilla";

    [ObservableProperty]
    private string _selectedVersion = string.Empty;

    [ObservableProperty]
    private int _port = 25565;

    [ObservableProperty]
    private string _maxMemory = "2G";

    [ObservableProperty]
    private ObservableCollection<string> _types = new();

    [ObservableProperty]
    private ObservableCollection<string> _versions = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isCreating;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public CreateServerViewModel(AuthService authService, NavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        _apiService = new ApiService(_authService);
    }

    public async Task InitializeAsync()
    {
        IsLoading = true;
        try
        {
            var types = await _apiService.GetServerTypesAsync();
            Types.Clear();
            if (types != null)
            {
                foreach (var type in types)
                {
                    Types.Add(type);
                }
            }

            await LoadVersionsAsync();
        }
        catch
        {
            ErrorMessage = "Failed to load server types";
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSelectedTypeChanged(string value)
    {
        _ = LoadVersionsAsync();
    }

    private async Task LoadVersionsAsync()
    {
        if (string.IsNullOrEmpty(SelectedType)) return;

        try
        {
            var versions = await _apiService.GetVersionsAsync(SelectedType);
            Versions.Clear();
            if (versions != null)
            {
                foreach (var version in versions)
                {
                    Versions.Add(version);
                }
                if (Versions.Count > 0)
                {
                    SelectedVersion = Versions[0];
                }
            }
        }
        catch
        {
            // Ignore version load errors
        }
    }

    [RelayCommand]
    private async Task Create()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Please enter a server name";
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedVersion))
        {
            ErrorMessage = "Please select a version";
            return;
        }

        IsCreating = true;
        ErrorMessage = string.Empty;

        try
        {
            var request = new CreateServerRequest
            {
                Name = Name,
                Type = SelectedType,
                Version = SelectedVersion,
                Port = Port,
                MaxMemory = MaxMemory
            };

            var server = await _apiService.CreateServerAsync(request);
            if (server != null)
            {
                _navigationService.NavigateTo<ServerListViewModel>(_authService, _navigationService);
            }
            else
            {
                ErrorMessage = "Failed to create server";
            }
        }
        catch
        {
            ErrorMessage = "Connection error";
        }
        finally
        {
            IsCreating = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _navigationService.NavigateTo<ServerListViewModel>(_authService, _navigationService);
    }
}
