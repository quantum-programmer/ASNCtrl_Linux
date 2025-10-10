using ARM.ViewModels;
//ing Avalonia;
using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ARM.Models;

namespace ARM.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        AddHandler(Button.PointerEnteredEvent,
            (_, e) => (DataContext as MainViewModel)?.HiddenButtonPointerEntered(e),
            RoutingStrategies.Tunnel);
    }

    private void RevealSection(object? sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem &&
            menuItem.Parent is MenuItem &&
            (menuItem.Parent as MenuItem)?.Parent is ContextMenu contextMenu &&
            contextMenu.PlacementTarget is Button button)
        {
            button.Opacity = 1;
            var tag = button.Tag?.ToString();
            if (!string.IsNullOrEmpty(tag))
            {
                var names = tag.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var name in names)
                {
                    var tb = this.FindControl<TextBox>(name.Trim());
                    if (tb != null)
                    {
                        tb.Opacity = 1;
                        tb.IsHitTestVisible = true;
                    }
                }
            }
        }
    }

    private void ReportMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is MenuItem { DataContext: ARMReport report })
        {
            var command = report.OpenCommand;
            if (command?.CanExecute(null) == true)
            {
                command.Execute(null);
            }
        }
    }

}