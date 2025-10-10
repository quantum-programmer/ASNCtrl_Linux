using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ARM.Services
{
    public class SubprocessManager : IDisposable
    {
        private Process? _process;
        private readonly string _executablePath;
        private readonly string? _arguments;
        private bool _isDisposed;

        public bool IsRunning => _process != null && !_process.HasExited;
        public event EventHandler<string>? OutputReceived;
        public event EventHandler<string>? ErrorReceived;
        public event EventHandler<int>? ProcessExited;

        public SubprocessManager(string executablePath, string? arguments = null)
        {
            if (string.IsNullOrWhiteSpace(executablePath))
                throw new ArgumentException("Путь к исполняемому файлу не может быть пустым", nameof(executablePath));

            _executablePath = executablePath;
            _arguments = arguments;
        }

        /// <summary>
        /// Запускает подпроцесс
        /// </summary>
        public async Task<bool> StartAsync()
        {
            if (IsRunning)
                return true;

            if (!File.Exists(_executablePath))
                throw new FileNotFoundException($"Исполняемый файл не найден: {_executablePath}");

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = _executablePath,
                    Arguments = _arguments ?? string.Empty,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true
                };


                _process = new Process { StartInfo = startInfo };
                
                _process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        OutputReceived?.Invoke(this, e.Data);
                };

                _process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        ErrorReceived?.Invoke(this, e.Data);
                };

                _process.Exited += (sender, e) =>
                {
                    ProcessExited?.Invoke(this, _process.ExitCode);
                };

                _process.EnableRaisingEvents = true;

                bool started = _process.Start();
                
                if (started)
                {
                    _process.BeginOutputReadLine();
                    _process.BeginErrorReadLine();
                }

                return started;
            }
            catch (Exception ex)
            {
                ErrorReceived?.Invoke(this, $"Ошибка запуска процесса: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Останавливает подпроцесс
        /// </summary>
        public async Task StopAsync(int timeoutMs = 5000)
        {
            if (_process == null || _process.HasExited)
                return;

            try
            {
                // Пытаемся корректно завершить процесс
                _process.CloseMainWindow();
                
                // Ждем завершения
                bool exited = await Task.Run(() => _process.WaitForExit(timeoutMs));
                
                if (!exited)
                {
                    // Принудительное завершение
                    _process.Kill();
                    await Task.Run(() => _process.WaitForExit(1000));
                }
            }
            catch (Exception ex)
            {
                ErrorReceived?.Invoke(this, $"Ошибка остановки процесса: {ex.Message}");
            }
        }

        /// <summary>
        /// Отправляет данные в стандартный ввод процесса
        /// </summary>
        public async Task WriteInputAsync(string input)
        {
            if (_process?.StandardInput != null && IsRunning)
            {
                await _process.StandardInput.WriteLineAsync(input);
                await _process.StandardInput.FlushAsync();
            }
        }

        /// <summary>
        /// Перезапускает подпроцесс
        /// </summary>
        public async Task<bool> RestartAsync()
        {
            await StopAsync();
            await Task.Delay(500); // Небольшая задержка перед перезапуском
            return await StartAsync();
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            StopAsync().Wait();
            
            _process?.Dispose();
            _process = null;
            
            _isDisposed = true;
        }
    }
}
