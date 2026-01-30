using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Threading.Tasks;

namespace MoltbotTray.Dialogs;

public enum UpdateDialogResult
{
    Download,
    Skip,
    RemindLater
}

/// <summary>
/// Dialog showing available update with release notes.
/// </summary>
public sealed class UpdateDialog
{
    private readonly string _version;
    private readonly string _changelog;
    private ContentDialog? _dialog;

    public UpdateDialogResult Result { get; private set; } = UpdateDialogResult.RemindLater;

    public UpdateDialog(string version, string changelog)
    {
        _version = version;
        _changelog = changelog;
    }

    public async Task<UpdateDialogResult> ShowAsync()
    {
        // Create a temporary window to host the dialog
        var window = new Window();
        window.Content = new Grid();
        window.SystemBackdrop = new MicaBackdrop(); // Apply Mica to host window
        window.Activate();

        // Build dialog content
        var content = new StackPanel
        {
            Spacing = 16,
            MaxWidth = 450
        };

        // Version header
        content.Children.Add(new TextBlock
        {
            Text = $"🎉 Version {_version} is available!",
            Style = (Style)Application.Current.Resources["SubtitleTextBlockStyle"]
        });

        // Current version
        var currentVersion = typeof(UpdateDialog).Assembly.GetName().Version?.ToString() ?? "Unknown";
        content.Children.Add(new TextBlock
        {
            Text = $"Current version: {currentVersion}",
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
        });

        // Changelog
        content.Children.Add(new TextBlock
        {
            Text = "What's New:",
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
        });

        var changelogScroll = new ScrollViewer
        {
            MaxHeight = 200,
            Content = new TextBlock
            {
                Text = _changelog,
                TextWrapping = TextWrapping.Wrap
            }
        };
        content.Children.Add(changelogScroll);

        // Create dialog
        _dialog = new ContentDialog
        {
            Title = "Update Available",
            Content = content,
            PrimaryButtonText = "Download & Install",
            SecondaryButtonText = "Remind Me Later",
            CloseButtonText = "Skip This Version",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = window.Content.XamlRoot
        };

        var dialogResult = await _dialog.ShowAsync();
        window.Close();

        Result = dialogResult switch
        {
            ContentDialogResult.Primary => UpdateDialogResult.Download,
            ContentDialogResult.Secondary => UpdateDialogResult.RemindLater,
            _ => UpdateDialogResult.Skip
        };

        return Result;
    }
}
