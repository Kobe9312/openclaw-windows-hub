using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MoltbotTray;

/// <summary>
/// Embeds the Clawdbot WebChat UI via WebView2, matching the macOS native chat panel.
/// </summary>
public class WebChatForm : Form
{
    private WebView2? _webView;
    private readonly string _gatewayUrl;
    private readonly string _token;
    private ToolStrip? _toolbar;
    private bool _initialized;

    private static WebChatForm? _instance;

    /// <summary>
    /// Show or focus the singleton WebChat window.
    /// </summary>
    public static void ShowOrFocus(string gatewayUrl, string token)
    {
        if (_instance != null && !_instance.IsDisposed)
        {
            _instance.BringToFront();
            _instance.Focus();
            return;
        }

        _instance = new WebChatForm(gatewayUrl, token);
        _instance.Show();
    }

    private WebChatForm(string gatewayUrl, string token)
    {
        _gatewayUrl = gatewayUrl;
        _token = token;
        InitializeComponent();
        _ = InitializeWebViewAsync();
    }

    private void InitializeComponent()
    {
        Text = "Clawdbot Chat";
        Size = new Size(520, 750);
        MinimumSize = new Size(380, 450);
        StartPosition = FormStartPosition.CenterScreen;
        Icon = IconHelper.GetLobsterIcon();
        BackColor = Color.FromArgb(30, 30, 30);

        // Toolbar
        _toolbar = new ToolStrip
        {
            GripStyle = ToolStripGripStyle.Hidden,
            RenderMode = ToolStripRenderMode.System,
            BackColor = Color.FromArgb(45, 45, 45),
            ForeColor = Color.White
        };

        var homeBtn = new ToolStripButton("🏠 Home") { ForeColor = Color.White };
        homeBtn.Click += (_, _) => NavigateToChat();

        var refreshBtn = new ToolStripButton("↻ Refresh") { ForeColor = Color.White };
        refreshBtn.Click += (_, _) => _webView?.Reload();

        var popoutBtn = new ToolStripButton("↗ Browser") { ForeColor = Color.White };
        popoutBtn.Click += (_, _) =>
        {
            var url = _gatewayUrl.Replace("ws://", "http://").Replace("wss://", "https://");
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo($"{url}?token={Uri.EscapeDataString(_token)}") { UseShellExecute = true }); }
            catch { }
        };

        var devToolsBtn = new ToolStripButton("🔧 DevTools") { ForeColor = Color.White };
        devToolsBtn.Click += (_, _) => _webView?.CoreWebView2?.OpenDevToolsWindow();

        _toolbar.Items.Add(homeBtn);
        _toolbar.Items.Add(refreshBtn);
        _toolbar.Items.Add(popoutBtn);
        _toolbar.Items.Add(new ToolStripSeparator());
        _toolbar.Items.Add(devToolsBtn);

        // WebView2 fills remaining space
        _webView = new WebView2
        {
            Dock = DockStyle.Fill,
            DefaultBackgroundColor = Color.FromArgb(30, 30, 30)
        };

        // Controls layout — toolbar on top, webview fills rest
        Controls.Add(_webView);
        Controls.Add(_toolbar);
        _toolbar.Dock = DockStyle.Top;
    }

    private async Task InitializeWebViewAsync()
    {
        try
        {
            // Use a dedicated user data folder
            var userDataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MoltbotTray", "WebView2");

            var env = await CoreWebView2Environment.CreateAsync(
                userDataFolder: userDataDir);

            await _webView!.EnsureCoreWebView2Async(env);

            // Configure WebView2
            var settings = _webView.CoreWebView2.Settings;
            settings.IsStatusBarEnabled = false;
            settings.AreDefaultContextMenusEnabled = true;
            settings.IsZoomControlEnabled = true;

            _initialized = true;
            Logger.Info("WebView2 initialized");

            NavigateToChat();
        }
        catch (WebView2RuntimeNotFoundException)
        {
            Logger.Error("WebView2 runtime not found");
            var result = MessageBox.Show(
                "The Microsoft WebView2 Runtime is required for the chat panel.\n\n" +
                "Would you like to download it?",
                "WebView2 Required",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(
                    "https://developer.microsoft.com/en-us/microsoft-edge/webview2/")
                { UseShellExecute = true });
            }
            Close();
        }
        catch (Exception ex)
        {
            Logger.Error("WebView2 init failed", ex);
            MessageBox.Show($"Failed to initialize chat panel:\n{ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
        }
    }

    private void NavigateToChat()
    {
        if (!_initialized || _webView?.CoreWebView2 == null) return;

        // Convert ws:// to http:// for the web UI
        var httpUrl = _gatewayUrl
            .Replace("ws://", "http://")
            .Replace("wss://", "https://");

        // The gateway serves WebChat at the root with token auth
        var chatUrl = $"{httpUrl}?token={Uri.EscapeDataString(_token)}";
        _webView.CoreWebView2.Navigate(chatUrl);
        Logger.Info($"Navigating to WebChat: {httpUrl}");
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _webView?.Dispose();
        _instance = null;
        base.OnFormClosed(e);
    }
}

