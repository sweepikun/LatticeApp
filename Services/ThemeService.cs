using System;
using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Lattice.Services;

public enum ThemeVariant
{
    Light,
    Dark
}

public class ThemeService : ObservableObject
{
    private readonly Application _app;
    private ThemeVariant _currentTheme;

    public ThemeVariant CurrentTheme
    {
        get => _currentTheme;
        set
        {
            if (SetProperty(ref _currentTheme, value))
            {
                ApplyTheme(value);
            }
        }
    }

    public ThemeService()
    {
        _app = Application.Current ?? throw new InvalidOperationException("Application not initialized");
        _currentTheme = ThemeVariant.Dark;
    }

    public void ToggleTheme()
    {
        CurrentTheme = CurrentTheme == ThemeVariant.Dark ? ThemeVariant.Light : ThemeVariant.Dark;
    }

    private void ApplyTheme(ThemeVariant theme)
    {
        _app.RequestedThemeVariant = theme switch
        {
            ThemeVariant.Light => Avalonia.Styling.ThemeVariant.Light,
            ThemeVariant.Dark => Avalonia.Styling.ThemeVariant.Dark,
            _ => Avalonia.Styling.ThemeVariant.Dark
        };
    }
}
