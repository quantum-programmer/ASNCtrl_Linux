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
using Serilog;
using System.Threading.Tasks;
using MsBox.Avalonia;
using System.Collections.Generic;
using System.Diagnostics;

namespace ARM.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    public ObservableCollection<ARMReport> reports = new();

    public ICommand OpenSettingsViewCommand => new RelayCommand(OpenSettings);
    public ICommand OpenInfoViewCommand => new RelayCommand(OpenInfo);
    public ICommand OpenSpravocnikCommand => new RelayCommand(OpenSpravocnik);
    // public ICommand OpenReportCommand => new RelayCommand(OpenReport);
    //public ICommand OpenReportsCommand => new RelayCommand(OpenReports);

    //public IAsyncRelayCommand<ARMReport> OpenReportCommand => new AsyncRelayCommand<ARMReport>(OpenReport);

    private PostModel? _selectedPost;

    private readonly IDBService _dbService;

    public ObservableCollection<PostGroupModel> AutoCisternGroups { get; } = new();
    public ObservableCollection<PostGroupModel> DispenserGroups { get; } = new();

    [ObservableProperty]
    private ObservableCollection<Bushe> _controlButtons = new();

    public MainViewModel(IDBService dbService)
    {
        _dbService = dbService;
        LoadPostsAsync();
        LoadListReports();
        InitializeControlButtons();
    }

    private void InitializeControlButtons()
    {
        var buttonTitles = new[]
        {
            "Разрешение",
            "Продолжить",
            "Минимальная доза",
            "Предельная разница",
            "Режим управления",
            "Способ загрузки"
        };

        for (int i = 0; i < 6; i++)
        {
            ControlButtons.Add(new Bushe
            {
                ButtonContent = buttonTitles[i],
                PlaceholderText = $"Значение {i + 1}"
            });
        }
    }

    [RelayCommand]
    private void HideControlButton(Bushe buttonModel)
    {
        buttonModel.IsHiddenButtonVisible = false;
        buttonModel.IsInputPairVisible = true;
    }

    [RelayCommand]
    private void SaveControlValue(Bushe buttonModel)
    {
        // Логика сохранения введенных данных
        Console.WriteLine($"Сохранено: {buttonModel.Text} для {buttonModel.ButtonContent}");

        // Здесь можно добавить логику обработки введенных значений
        // Например, обновление состояния системы
    }

    //генерация постов + сайд
    private async void LoadPostsAsync()
    {
        var postList = await (_dbService as PostgresDBService)?.GetPostsAsync();
        if (postList != null)
        {
            AutoCisternGroups.Clear();
            DispenserGroups.Clear();

            var autoGroups = postList
                .Where(p => p.MachineType == 0)
                .GroupBy(p => p.Side)
                .Select(g => new PostGroupModel
                {
                    Side = g.Key,
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
                    Side = g.Key,
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


    private void OpenSettings()
    {
        var window = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        var modal = new SettingsView { DataContext = new SettingsViewModel() };
        modal.ShowDialog(window);
    }
    
    private void OpenInfo()
    {
        var window = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        var modal = new InfoPageView { DataContext = new InfoPageViewModel() };
        modal.ShowDialog(window);
    }

    private void OpenSpravocnik()
    { 
        var window = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        var modal = new SpravocnikPostovView { DataContext = new SpravocnikPostovViewModel(_dbService) };
        modal.ShowDialog(window);
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




}

