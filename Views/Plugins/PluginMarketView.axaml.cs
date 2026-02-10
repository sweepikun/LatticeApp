using Avalonia.Controls;
using Avalonia.Input;
using Lattice.ViewModels;

namespace Lattice.Views.Plugins;

public partial class PluginMarketView : UserControl
{
    public PluginMarketView()
    {
        InitializeComponent();
    }

    private void OnSearchKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is PluginMarketViewModel vm)
        {
            vm.SearchCommand.Execute(null);
        }
    }

    private void OnItemClick(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && border.DataContext is MarketItem item)
        {
            if (DataContext is PluginMarketViewModel vm)
            {
                vm.SelectItemCommand.Execute(item);
            }
        }
    }
}
