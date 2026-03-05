using System;
using System.Windows;

namespace ScreenDrop.Services;

public class NotificationService
{
    public void ShowSuccess(string url)
    {
        try
        {
            ShowBalloonTip("Screenshot Uploaded!", "URL copied to clipboard");
        }
        catch
        {
        }
    }

    public void ShowError(string message)
    {
        try
        {
            ShowBalloonTip("Upload Failed", message);
        }
        catch
        {
        }
    }

    public void ShowInfo(string message)
    {
        try
        {
            ShowBalloonTip("ScreenDrop", message);
        }
        catch
        {
        }
    }

    private void ShowBalloonTip(string title, string message)
    {
        // Notifications will be shown via the tray icon
        // This is a placeholder - the actual notification is handled by TaskbarIcon
    }
}
