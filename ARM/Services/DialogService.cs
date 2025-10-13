using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System.Threading.Tasks;

public interface IDialogService
{
    Task ShowErrorAsync(string message);
    Task ShowInfoAsync(string message);
    Task<bool> ShowConfirmationAsync(string message);
}

public class DialogService : IDialogService
{
    public async Task ShowErrorAsync(string message)
    {
        await ShowMessageAsync("Ошибка", message);
    }

    public async Task ShowInfoAsync(string message)
    {
        await ShowMessageAsync("Информация", message);
    }

    public async Task<bool> ShowConfirmationAsync(string message)
    {
        // Обеспечиваем вызов на UI-потоке всегда
        return await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandard(
                "Подтверждение",
                message,
                ButtonEnum.YesNo);

            var result = await messageBox.ShowAsync();
            return result == ButtonResult.Yes;
        });
    }

    private async Task ShowMessageAsync(string title, string message)
    {
        // Обеспечиваем вызов на UI-потоке всегда
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandard(title, message);
            await messageBox.ShowAsync();
        });
    }
}