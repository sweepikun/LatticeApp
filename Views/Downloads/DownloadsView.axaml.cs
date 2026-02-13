using Avalonia.Controls;
using Avalonia.Input;
using Lattice.ViewModels;

namespace Lattice.Views.Downloads;

public partial class DownloadsView : UserControl
{
    public DownloadsView()
    {
        InitializeComponent();
    }

    private void OnSearchKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is DownloadsViewModel vm)
        {
            vm.SearchCommand.Execute(null);
        }
    }
}
