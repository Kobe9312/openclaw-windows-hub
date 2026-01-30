using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Updatum;

namespace MoltbotTray.Dialogs;

public sealed class DownloadProgressDialog
{
    private Window? _window;
    private readonly UpdatumManager? _updater;

    public DownloadProgressDialog(UpdatumManager updater)
    {
        _updater = updater;
    }

    public void ShowAsync()
    {
        _window = new Window { Title = "Downloading Update..." };
        _window.SystemBackdrop = new MicaBackdrop(); // Apply Mica backdrop
        
        var panel = new StackPanel { Padding = new Thickness(20) };
        var progressText = new TextBlock { Text = "Downloading update...", Margin = new Thickness(0, 0, 0, 10) };
        var progressBar = new ProgressBar { IsIndeterminate = true };
        
        panel.Children.Add(progressText);
        panel.Children.Add(progressBar);
        _window.Content = panel;
        
        _window.Activate();
    }

    public void Close() => _window?.Close();
}
