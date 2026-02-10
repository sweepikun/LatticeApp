using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Lattice.Converters;

public class StatusToColorConverter : IValueConverter
{
    public static readonly StatusToColorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            return status.ToLower() switch
            {
                "running" => new SolidColorBrush(Color.Parse("#4ade80")),
                "starting" => new SolidColorBrush(Color.Parse("#fbbf24")),
                "stopping" => new SolidColorBrush(Color.Parse("#fb923c")),
                "stopped" => new SolidColorBrush(Color.Parse("#6b7280")),
                _ => new SolidColorBrush(Color.Parse("#6b7280"))
            };
        }
        return new SolidColorBrush(Color.Parse("#6b7280"));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class StringEqualsConverter : IValueConverter
{
    public static readonly StringEqualsConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str && parameter is string param)
        {
            return string.Equals(str, param, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class MessageRoleToBrushConverter : IValueConverter
{
    public static readonly MessageRoleToBrushConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string role)
        {
            return role switch
            {
                "user" => new SolidColorBrush(Color.Parse("#FF6B9D")),
                "assistant" => new SolidColorBrush(Color.Parse("#2D2D2D")),
                _ => new SolidColorBrush(Color.Parse("#1A1A1A"))
            };
        }
        return new SolidColorBrush(Color.Parse("#1A1A1A"));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class RoleToLabelConverter : IValueConverter
{
    public static readonly RoleToLabelConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string role)
        {
            return role switch
            {
                "user" => "You",
                "assistant" => "AI Assistant",
                _ => role
            };
        }
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
