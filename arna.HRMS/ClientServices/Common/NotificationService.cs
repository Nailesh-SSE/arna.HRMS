using BlazorBootstrap;

namespace arna.HRMS.ClientServices.Common;

public class NotificationService
{ 
    public List<ToastMessage> Messages { get; } = new();

    public void Success(string message)
        => Show(ToastType.Success, "Success", message, "bi-check-circle-fill");

    public void Error(string message)
        => Show(ToastType.Danger, "Error", message, "bi-x-circle-fill");

    public void Warning(string message)
        => Show(ToastType.Warning, "Warning", message, "bi-exclamation-triangle-fill");

    public void Info(string message)
        => Show(ToastType.Info, "Information", message, "bi-info-circle-fill");

    private void Show(ToastType type, string title, string message, string icon)
    {
        Messages.Add(new ToastMessage
        {
            Type = type,
            Title = title,
            Message = message,
            CustomIconName = icon,
            AutoHide = true
        });
    }
}
