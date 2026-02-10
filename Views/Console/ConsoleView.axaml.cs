using Avalonia.Controls;
using Avalonia.Input;
using Lattice.ViewModels;

namespace Lattice.Views.Console;

public partial class ConsoleView : UserControl
{
    public ConsoleView()
    {
        InitializeComponent();
    }

    private void OnCommandKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is ConsoleViewModel vm)
        {
            vm.SendCommandCommand.Execute(null);
        }
    }
}
