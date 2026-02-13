using Avalonia.Controls;
using Avalonia.Input;
using Lattice.ViewModels;

namespace Lattice.Views.Dashboard;

public partial class ServerDetailView : UserControl
{
    public ServerDetailView()
    {
        InitializeComponent();
    }

    private void OnCommandKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is ServerDetailViewModel vm)
        {
            vm.SendCommandCommand.Execute(null);
        }
    }
}
