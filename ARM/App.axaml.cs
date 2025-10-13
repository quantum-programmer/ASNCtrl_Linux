using ARM.Services;
using ARM.ViewModels;
using ARM.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using MsBox.Avalonia;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ARM
{
    public partial class App : Application
    {
        private SubprocessManager? _opcLoggerProcess;

        private static readonly string MetrologyFileName =
#if VERSION_2B
            "denscalc.dll";
#elif VERSION_2C
            "oildenscalc.dll";
#else
            "default.dll";
#endif

        private static readonly string ValidHash =
#if VERSION_2B
            "C27BD1A545D27B2FAE0A9B81E2AB7CD7";
#elif VERSION_2C
            "99E992D40A2E7FEA5B4C7F3BBE815AC9";
#else
            "DEFAULT_HASH";
#endif
        public static IDialogService DialogService { get; private set; }

        private static string GetOpcLoggerPath()
        {
            string fileName;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                fileName = "opc_logger.exe";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                fileName = "opc_logger";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                fileName = "opc_logger";
            else
                fileName = "opc_logger";

            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(appDirectory, fileName);
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override async void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Проверка файла метрологии
                bool metrologyOk = await CheckMetrologyFile();
                if (!metrologyOk)
                {
                    desktop.Shutdown();
                    return;
                }

                // Удаляем встроенный Avalonia data validator
                BindingPlugins.DataValidators.RemoveAt(0);

                DialogService = new DialogService();

                var mainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };

#if DEBUG || VERSION_2B || VERSION_2C
                DevToolsExtensions.AttachDevTools(mainWindow);
#endif

                desktop.MainWindow = mainWindow;

                // Запуск OPC Logger
                bool opcLoggerStarted = await StartOpcLogger();
                if (!opcLoggerStarted)
                {
                    await ShowError("Не удалось запустить OPC Logger. Приложение будет закрыто.");
                    await Dispatcher.UIThread.InvokeAsync(() => mainWindow.Close());
                    return;
                }

                // Подписка на завершение приложения
                desktop.Exit += OnApplicationExit;

                mainWindow.Show();
            }

            base.OnFrameworkInitializationCompleted();
        }

        private async Task<bool> StartOpcLogger()
        {
            try
            {
                string opcLoggerPath = GetOpcLoggerPath();

                if (!File.Exists(opcLoggerPath))
                {
                    await ShowError($"Файл opc_logger не найден по пути: {opcLoggerPath}");
                    return false;
                }

                _opcLoggerProcess = new SubprocessManager(opcLoggerPath);

                _opcLoggerProcess.OutputReceived += (sender, output) =>
                {
                    Console.WriteLine($"[OPC Logger] {output}");
                };

                _opcLoggerProcess.ErrorReceived += (sender, error) =>
                {
                    Console.WriteLine($"[OPC Logger Error] {error}");
                };

                _opcLoggerProcess.ProcessExited += (sender, exitCode) =>
                {
                    Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        Console.WriteLine($"[OPC Logger] Завершился с кодом: {exitCode}");
                        if (exitCode != 0)
                        {
                            await ShowError($"OPC Logger завершился с ошибкой {exitCode}");
                        }
                    });
                };

                bool started = await _opcLoggerProcess.StartAsync();
                if (started)
                {
                    Console.WriteLine("[OPC Logger] успешно запущен");
                    await Task.Delay(500);
                }

                return started;
            }
            catch (Exception ex)
            {
                await ShowError($"Ошибка при запуске OPC Logger: {ex.Message}");
                return false;
            }
        }

        private void OnApplicationExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            _opcLoggerProcess?.Dispose();
        }

        private async Task<bool> CheckMetrologyFile()
        {
            if (!File.Exists(MetrologyFileName))
            {
                await ShowError("Файл метрологии не найден!");
                return false;
            }

            var fileHash = GetFileHash(MetrologyFileName);
            if (!string.Equals(fileHash, ValidHash, StringComparison.OrdinalIgnoreCase))
            {
                await ShowError("Ошибка проверки метрологии: файл повреждён или подменён.");
                return false;
            }

            return true;
        }

        private string GetFileHash(string filePath)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filePath);
            var hashBytes = md5.ComputeHash(stream);
            return BitConverter.ToString(hashBytes).Replace("-", "");
        }

        private async Task ShowError(string message)
        {
            // Обеспечиваем вызов на UI-потоке всегда
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var messageBox = MessageBoxManager.GetMessageBoxStandard("Ошибка", message);
                await messageBox.ShowAsync();
            });
        }
    }
}

