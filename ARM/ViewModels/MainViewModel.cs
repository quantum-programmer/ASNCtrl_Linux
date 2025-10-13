using System.Windows.Input;
using ARM.ViewModels;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ARM.Views;
using System.Collections.ObjectModel;
using ARM.Models;
using ARM.Services;
using System.Linq;
using System;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using MsBox.Avalonia;
using System.Collections.Generic;
using Serilog;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace ARM.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    public ObservableCollection<ARMReport> reports = new();
    public ICommand OpenSettingsViewCommand => new RelayCommand(OpenSettings);
    public ICommand OpenInfoViewCommand => new RelayCommand(OpenInfo);
    public ICommand OpenDirectoryCommand => new RelayCommand<object>(OpenDirectory);
    private PostModel? _selectedPost;

    //private readonly IDBService _dbService;
    private readonly PostgresDBService _dbService;

    public ObservableCollection<PostGroupModel> AutoCisternGroups { get; } = new();
    public ObservableCollection<PostGroupModel> DispenserGroups { get; } = new();


    public MainViewModel(PostgresDBService dbService)
    {
        _dbService = dbService;
        LoadPostsAsync();
        LoadListReports();
    }



    //генерация постов + сайд
    private async void LoadPostsAsync()
    {
        var post123 = await _dbService.LoadPostsAsync();//удалить, оставил для коннекта до создания авторизации пользователя 
        List<PostModel> postList = null;
        try
        {
            postList = await _dbService.GetPostsAsync();
        }
        catch (Exception ex)
        {
            await App.DialogService.ShowErrorAsync($"Ошибка при загрузке постов: {ex.Message}");
        }
        if (postList != null)
        {
            AutoCisternGroups.Clear();
            DispenserGroups.Clear();

            var autoGroups = postList
                .Where(p => p.MachineType == 0)
                .GroupBy(p => p.Side)
                .Select(g => new PostGroupModel
                {
                    Side = (int)g.Key,
                    Posts = new ObservableCollection<PostModel>(
                        g.Select(post =>
                        {
                            post.SelectPostCommand = new RelayCommand(() => SelectedPost = post);
                            return post;
                        }))
                }).ToList();

            var dispenserGroups = postList
                .Where(p => p.MachineType == 1)
                .GroupBy(p => p.Side)
                .Select(g => new PostGroupModel
                {
                    Side = (int)g.Key,
                    Posts = new ObservableCollection<PostModel>(
                        g.Select(post =>
                        {
                            post.SelectPostCommand = new RelayCommand(() => SelectedPost = post);
                            return post;
                        }))
                }).ToList();

            foreach (var group in autoGroups)
                AutoCisternGroups.Add(group);

            foreach (var group in dispenserGroups)
                DispenserGroups.Add(group);
        }
    }

    private async void LoadListReports()
    {
        try
        {
            var reportsFromDb = (await _dbService.GetAllReportsAsync())
                ?.Where(r => r != null)
                .ToList() ?? new List<ARMReport>();

            foreach (var report in reportsFromDb)
            {
                var currentReport = report;
                currentReport.OpenCommand = new AsyncRelayCommand(() => OpenReport(currentReport));
            }

            Reports = new ObservableCollection<ARMReport>(reportsFromDb);
            Log.Information("Отчеты успешно загружены из базы данных");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Ошибка при загрузке отчетов из базы данных");
        }
    }

    public PostModel? SelectedPost
    {
        get => _selectedPost;
        set => SetProperty(ref _selectedPost, value);
    }
    public IRelayCommand<PostModel> SelectPostCommand => new RelayCommand<PostModel>(post => SelectedPost = post);


    private async void OpenSettings()
    {
        var window = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        var modal = new SettingsView { DataContext = new SettingsViewModel() };
        await modal.ShowDialog(window);
    }
    
    private void OpenInfo()
    {
        var window = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        var modal = new InfoPageView { DataContext = new InfoPageViewModel() };
        modal.ShowDialog(window);
    }

    private async void OpenDirectory(object parameter)
    {
        if (parameter is string directoryName)
        {
            var window = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

            var modal = new DirectoryMainView(_dbService, directoryName);

            await modal.ShowDialog(window);

            LoadPostsAsync(); // Обновляем данные после закрытия окна
        }
    }

    private async Task OpenReport(ARMReport? report)
    {
        if (report == null)
            return;

        string arguments = $"cmd=show id={report.ARMReportID}";

        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = @"RDesigner\RDesigner.exe",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();

            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            Log.Information($"RDesigner запущен с параметрами: {arguments}");
            Log.Information($"Вывод: {output}");

            if (!string.IsNullOrEmpty(error))
                Log.Warning($"Ошибки: {error}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Ошибка при запуске RDesigner.exe");
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Ошибка", $"Не удалось запустить RDesigner.exe: {ex.Message}");
            await box.ShowAsync();
        }
    }

    [RelayCommand]
    private void HiddenButtonClick(Button? button)
    {
        if (button?.ContextMenu is not { } menu)
        {
            return;
        }

        button.Classes.Add("menu-open");
        menu.Tag = button;

        void OnMenuClosed(object? _, RoutedEventArgs args)
        {
            menu.Closed -= OnMenuClosed;
            button.Classes.Remove("menu-open");
        }

        menu.Closed += OnMenuClosed;
        menu.Open(button);
    }

    [RelayCommand]
    private void HiddenMenuItemClick(MenuItem? menuItem)
    {
        if (!TryResolveMenuContext(menuItem, out var contextMenu, out var button) || contextMenu is null || button is null)
        {
            return;
        }

        contextMenu.Close();

        button.Classes.Add("shown");
        button.Classes.Remove("menu-open");
        button.ClearValue(Button.OpacityProperty);

        UpdateSiblingTextBoxes(button, show: true);
    }

    [RelayCommand]
    private void HiddenMenuEmptyClick(MenuItem? menuItem)
    {
        if (!TryResolveMenuContext(menuItem, out var contextMenu, out var button) || contextMenu is null || button is null)
        {
            return;
        }

        contextMenu.Close();

        button.Classes.Remove("shown");
        button.Classes.Remove("menu-open");
        button.ClearValue(Button.OpacityProperty);

        UpdateSiblingTextBoxes(button, show: false);
    }

    public void HiddenButtonPointerEntered(PointerEventArgs e)
    {
        if (e.Source is Button button && button.Classes.Contains("hidden-button") && !button.Classes.Contains("shown"))
        {
            button.ClearValue(Button.OpacityProperty);
        }
    }

    private static bool TryResolveMenuContext(MenuItem? menuItem, out ContextMenu? contextMenu, out Button? button)
    {
        contextMenu = null;
        button = null;

        if (menuItem is null)
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
                                      .SkipWhile(child => !ReferenceEquals(child, button))
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

}

