using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Lattice.ViewModels;

namespace Lattice.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        PointerPressed += OnPointerPressed;
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is Visual source)
        {
            var titleBar = this.FindControl<Border>("TitleBar");
            if (titleBar != null && source == titleBar)
            {
                BeginMoveDrag(e);
            }
        }
    }
}
