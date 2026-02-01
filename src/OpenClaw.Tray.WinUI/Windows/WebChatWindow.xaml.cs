using Microsoft.UI.Xaml;
using Microsoft.Web.WebView2.Core;
using OpenClaw.Shared;
using OpenClawTray.Helpers;
using OpenClawTray.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using WinUIEx;
using Windows.Foundation;

namespace OpenClawTray.Windows;

public sealed partial class WebChatWindow : WindowEx
{
    private readonly string _gatewayUrl;
    private readonly string _token;
    private bool _initialized;
    
    // Store event handlers for cleanup
    private TypedEventHandler<CoreWebView2, CoreWebView2NavigationCompletedEventArgs>? _navigationCompletedHandler;
    private TypedEventHandler<CoreWebView2, CoreWebView2NavigationStartingEventArgs>? _navigationStartingHandler;
    
    public bool IsClosed { get; private set; }

    public WebChatWindow(string gatewayUrl, string token)
    {
        Logger.Info($"WebChatWindow: Constructor called, gateway={gatewayUrl}");
        _gatewayUrl = gatewayUrl;
        _token = token;
        
        InitializeComponent();
        
        // Window configuration
        this.SetWindowSize(520, 750);
        this.MinWidth = 380;
        this.MinHeight = 450;
        this.CenterOnScreen();
        this.SetIcon(IconHelper.GetStatusIconPath(ConnectionStatus.Connected));
        
        Closed += OnWindowClosed;
        
        Logger.Info("WebChatWindow: Starting InitializeWebViewAsync");
        _ = InitializeWebViewAsync();
    }

    private void OnWindowClosed(object sender, WindowEventArgs e)
    {
        IsClosed = true;
        
        // Cleanup WebView2 event handlers
        if (WebView.CoreWebView2 != null)
        {
            if (_navigationCompletedHandler != null)
                WebView.CoreWebView2.NavigationCompleted -= _navigationCompletedHandler;
            if (_navigationStartingHandler != null)
                WebView.CoreWebView2.NavigationStarting -= _navigationStartingHandler;
        }
    }

    private async Task InitializeWebViewAsync()
    {
        try
        {
            Logger.Info("WebChatWindow: Initializing WebView2...");
            
            // Set up user data folder
            var userDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "OpenClawTray", "WebView2");
            
            Directory.CreateDirectory(userDataFolder);
            Logger.Info($"WebChatWindow: User data folder: {userDataFolder}");

            // Set environment variable for user data folder (WinUI workaround)
            Environment.SetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER", userDataFolder);
            
            // Just call EnsureCoreWebView2Async without creating environment first
            Logger.Info("WebChatWindow: Calling EnsureCoreWebView2Async...");
            await WebView.EnsureCoreWebView2Async();
            Logger.Info("WebChatWindow: CoreWebView2 initialized successfully");
            
            // Configure WebView2
            WebView.CoreWebView2.Settings.IsStatusBarEnabled = false;
            WebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
            WebView.CoreWebView2.Settings.IsZoomControlEnabled = true;

            // Handle navigation events (store for cleanup)
            _navigationCompletedHandler = (s, e) =>
            {
                Logger.Info($"WebChatWindow: Navigation completed, success={e.IsSuccess}, status={e.WebErrorStatus}");
                LoadingRing.IsActive = false;
                LoadingRing.Visibility = Visibility.Collapsed;
            };
            WebView.CoreWebView2.NavigationCompleted += _navigationCompletedHandler;

            _navigationStartingHandler = (s, e) =>
            {
                Logger.Info($"WebChatWindow: Navigation starting to {e.Uri}");
                LoadingRing.IsActive = true;
                LoadingRing.Visibility = Visibility.Visible;
            };
            WebView.CoreWebView2.NavigationStarting += _navigationStartingHandler;

            // Navigate to chat
            NavigateToChat();
            _initialized = true;
        }
        catch (Exception ex)
        {
            Logger.Error($"WebView2 initialization failed: {ex.Message}");
            Logger.Error($"WebView2 stack trace: {ex.StackTrace}");
            LoadingRing.IsActive = false;
            
            // Fallback: open in browser
            Logger.Info("Falling back to browser...");
            OnPopout(this, new RoutedEventArgs());
            Close();
        }
    }

    private void NavigateToChat()
    {
        if (WebView.CoreWebView2 == null) return;

        var baseUrl = _gatewayUrl
            .Replace("ws://", "http://")
            .Replace("wss://", "https://");
        
        var url = $"{baseUrl}?token={Uri.EscapeDataString(_token)}";
        Logger.Info($"WebChatWindow: Navigating to {baseUrl} (token hidden)");
        WebView.CoreWebView2.Navigate(url);
    }

    private void OnHome(object sender, RoutedEventArgs e)
    {
        NavigateToChat();
    }

    private void OnRefresh(object sender, RoutedEventArgs e)
    {
        WebView.CoreWebView2?.Reload();
    }

    private void OnPopout(object sender, RoutedEventArgs e)
    {
        var baseUrl = _gatewayUrl
            .Replace("ws://", "http://")
            .Replace("wss://", "https://");
        var url = $"{baseUrl}?token={Uri.EscapeDataString(_token)}";
        
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to open in browser: {ex.Message}");
        }
    }

    private void OnDevTools(object sender, RoutedEventArgs e)
    {
        WebView.CoreWebView2?.OpenDevToolsWindow();
    }
}
