using BlazorBootstrap;

namespace arna.HRMS.Services.Common;

public sealed class NotificationService
{
    private readonly List<ToastMessage> _messages = new();
    private readonly object _lock = new();

    public event Func<Task>? OnChange;

    // Must return List because Toasts requires List
    public List<ToastMessage> Messages
    {
        get
        {
            lock (_lock)
            {
                return _messages;
            }
        }
    }

    public async Task Success(string message)
    {
        await ShowAsync(ToastType.Success, "Success", message, "bi-check-circle-fill"); 
    }

    public async Task Error(string message)
    {
        await ShowAsync(ToastType.Danger, "Error", message, "bi-x-circle-fill");
    }

    public async Task Warning(string message)
    {
        await ShowAsync(ToastType.Warning, "Warning", message, "bi-exclamation-triangle-fill"); 
    }

    public async Task Info(string message)
    {
        await ShowAsync(ToastType.Info, "Information", message, "bi-info-circle-fill"); 
    }

    private async Task ShowAsync(
        ToastType type,
        string title,
        string message,
        string icon)
    {
        lock (_lock)
        {
            _messages.Add(new ToastMessage
            {
                Type = type,
                Title = title,
                Message = message,
                CustomIconName = icon,
                AutoHide = true
            });
        }

        if (OnChange is not null)
        {
            await OnChange.Invoke();
        }
    }
}