using Avalonia.Controls;
using Avalonia.Input;
using Lattice.ViewModels;

namespace Lattice.Views.AI;

public partial class AIView : UserControl
{
    public AIView()
    {
        InitializeComponent();
    }

    private void OnInputKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is AIViewModel vm)
        {
            vm.SendMessageCommand.Execute(null);
        }
    }
}
