using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lattice.Services;

namespace Lattice.ViewModels;

public partial class ChatMessage : ObservableObject
{
    [ObservableProperty]
    private string _role = string.Empty;

    [ObservableProperty]
    private string _content = string.Empty;

    [ObservableProperty]
    private DateTime _timestamp;
}

public partial class AIViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly NavigationService _navigationService;
    private readonly ApiService _apiService;

    [ObservableProperty]
    private ObservableCollection<ChatMessage> _messages = new();

    [ObservableProperty]
    private string _inputText = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _selectedProvider = string.Empty;

    [ObservableProperty]
    private string _providerKey = string.Empty;

    [ObservableProperty]
    private string _selectedModel = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _providers = new();

    [ObservableProperty]
    private ObservableCollection<string> _models = new();

    public AIViewModel(AuthService authService, NavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        _apiService = new ApiService(_authService);
        
        _ = LoadProvidersAsync();
    }

    private async Task LoadProvidersAsync()
    {
        try
        {
            var providers = await _apiService.GetAIProvidersAsync();
            Providers.Clear();
            if (providers != null)
            {
                foreach (var p in providers)
                {
                    Providers.Add(p);
                }
            }
        }
        catch
        {
            // Ignore
        }
    }

    [RelayCommand]
    private async Task ConfigureProvider()
    {
        if (string.IsNullOrEmpty(SelectedProvider) || string.IsNullOrEmpty(ProviderKey) || string.IsNullOrEmpty(SelectedModel))
        {
            ErrorMessage = "Please fill in all fields";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _apiService.ConfigureAIAsync(SelectedProvider, ProviderKey, SelectedModel);
            if (result != null)
            {
                SelectedProvider = result;
                ErrorMessage = "";
            }
        }
        catch
        {
            ErrorMessage = "Failed to configure AI";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(InputText) || string.IsNullOrEmpty(SelectedProvider)) return;

        var userMessage = new ChatMessage
        {
            Role = "user",
            Content = InputText,
            Timestamp = DateTime.Now
        };
        Messages.Add(userMessage);
        
        var input = InputText;
        InputText = string.Empty;
        IsLoading = true;

        try
        {
            var response = await _apiService.ChatAIAsync(SelectedProvider, input);
            if (response != null)
            {
                Messages.Add(new ChatMessage
                {
                    Role = "assistant",
                    Content = response,
                    Timestamp = DateTime.Now
                });
            }
        }
        catch
        {
            ErrorMessage = "Failed to get response";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ClearChat()
    {
        Messages.Clear();
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigationService.NavigateTo<ServerListViewModel>(_authService, _navigationService);
    }
}
