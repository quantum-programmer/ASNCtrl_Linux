using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
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
        AddHandler(Button.PointerEnteredEvent, HiddenButton_PointerEntered, RoutingStrategies.Tunnel);
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

    private static bool TryResolveMenuContext(object? sender, out ContextMenu? contextMenu, out Button? button)
    {
        contextMenu = null;
        button = null;

        if (sender is not MenuItem menuItem)
        {
            return false;
        }

        contextMenu = menuItem.GetLogicalAncestors().OfType<ContextMenu>().FirstOrDefault()
                       ?? menuItem.GetVisualAncestors().OfType<ContextMenu>().FirstOrDefault();

        if ((contextMenu?.Tag ?? contextMenu?.PlacementTarget) is Button resolvedButton)
        {
            button = resolvedButton;
            return true;
        }
        return false;
    }

        private static void UpdateSiblingTextBoxes(Button button, bool show)
        {
            var parent = button.GetLogicalParent();
            if (parent is null)
            {
                return;
            }
            var updatedCount = 0;
            foreach (var sibling in parent.GetLogicalChildren()
                                          .SkipWhile(c => !ReferenceEquals(c, button))
                                          .Skip(1))
            {   
                if (sibling is not TextBox siblingTextBox)
                {
                    continue;
                }

                if (show)
                {
                    siblingTextBox.Classes.Add("shown");
                    siblingTextBox.IsHitTestVisible = true;
                    siblingTextBox.Opacity = 1;
                }
                else
                {
                    siblingTextBox.Classes.Remove("shown");
                    siblingTextBox.IsHitTestVisible = false;
                    siblingTextBox.Opacity = 0;
                }

                if (++updatedCount >= 2)
                {
                    break;
                }
            }
        }

        private void HiddenMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (!TryResolveMenuContext(sender, out var contextMenu, out var button) || contextMenu is null || button is null)
            {
                return;
            }

            contextMenu.Close();

            button.Classes.Add("shown");
            button.Classes.Remove("menu-open");
            button.ClearValue(OpacityProperty);

        UpdateSiblingTextBoxes(button, show: true);
        }

        private void HiddenMenuEmpty_Click(object? sender, RoutedEventArgs e)
        {
            if (!TryResolveMenuContext(sender, out var contextMenu, out var button) || contextMenu is null || button is null)
            {
                return;
            }

            contextMenu.Close();

            button.Classes.Remove("shown");
            button.Classes.Remove("menu-open");
            button.Opacity = 0;

            UpdateSiblingTextBoxes(button, show: false);
        }

        private void HiddenButton_PointerEntered(object? sender, PointerEventArgs e)
        {
            if (e.Source is Button button && button.Classes.Contains("hidden-button"))
            {
                // Remove any explicit opacity value so the style-based hover logic can
                // take effect after the button was temporarily hidden via the menu.
                if (!button.Classes.Contains("shown"))
                {
                    button.ClearValue(OpacityProperty);
                }
            }
        }
}