using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using System.Linq;

namespace ARM.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void HiddenButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.ContextMenu is { } menu)
        {
            btn.Classes.Add("menu-open");

            // Remember which button opened the menu so menu item clicks can
            // easily find the associated control even if placement information
            // becomes unavailable once the menu closes.
            menu.Tag = btn;

            void OnMenuClosed(object? _, RoutedEventArgs args)
            {
                menu.Closed -= OnMenuClosed;
                btn.Classes.Remove("menu-open");
            }

            menu.Closed += OnMenuClosed;
            menu.Open(btn);
        }
    }

    private void HiddenMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem)
        {
            return;
        }

        var contextMenu = menuItem.GetLogicalAncestors().OfType<ContextMenu>().FirstOrDefault()
                   ?? menuItem.GetVisualAncestors().OfType<ContextMenu>().FirstOrDefault();

        if ((contextMenu?.Tag ?? contextMenu?.PlacementTarget) is not Button button)
        {
            return;
        }

        contextMenu.Close();

        button.Classes.Add("shown");
        button.Classes.Remove("menu-open");
        button.Opacity = 1;

        var parent = button.GetLogicalParent();
        if (parent is null)
        {
            return;
        }

        var revealedCount = 0;
        foreach (var sibling in parent.GetLogicalChildren()
                                      .SkipWhile(c => !ReferenceEquals(c, button))
                                      .Skip(1))
        {
            if (sibling is not TextBox siblingTextBox)
            {
                continue;
            }

            siblingTextBox.Classes.Add("shown");
            siblingTextBox.IsHitTestVisible = true;
            siblingTextBox.Opacity = 1;

            if (++revealedCount >= 2)
            {
                break;
            }
        }
    }
}