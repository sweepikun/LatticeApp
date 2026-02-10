using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Lattice.Models;
using Lattice.ViewModels;

namespace Lattice.Views.Dashboard;

public partial class ServerListView : UserControl
{
    public ServerListView()
    {
        InitializeComponent();
    }

    private void ServerItem_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && border.DataContext is Server server)
        {
            if (DataContext is ServerListViewModel vm)
            {
                vm.SelectServerCommand.Execute(server);
            }
        }
    }
}
