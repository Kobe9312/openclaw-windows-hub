using Moltbot.Shared;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MoltbotTray;

public partial class SettingsDialog : Form
{
    private readonly SettingsManager _settings;
    
    private TextBox _gatewayUrlTextBox = null!;
    private TextBox _tokenTextBox = null!;
    private CheckBox _autoStartCheckBox = null!;
    private CheckBox _showNotificationsCheckBox = null!;
    private CheckBox _globalHotkeyCheckBox = null!;
    private ComboBox _notificationSoundComboBox = null!;
    private Button _testConnectionButton = null!;
    private Button _okButton = null!;
    private Button _cancelButton = null!;
    private Label _statusLabel = null!;

    // Notification filter checkboxes
    private CheckBox _notifyHealthCb = null!;
    private CheckBox _notifyUrgentCb = null!;
    private CheckBox _notifyReminderCb = null!;
    private CheckBox _notifyEmailCb = null!;
    private CheckBox _notifyCalendarCb = null!;
    private CheckBox _notifyBuildCb = null!;
    private CheckBox _notifyStockCb = null!;
    private CheckBox _notifyInfoCb = null!;
    private Panel _notifyFilterPanel = null!;

    public SettingsDialog(SettingsManager settings)
    {
        _settings = settings;
        InitializeComponent();
        LoadSettings();
    }

    private void InitializeComponent()
    {
        Text = "Settings — Moltbot Tray";
        Size = new Size(480, 560);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        AutoScroll = true;
        Icon = IconHelper.GetLobsterIcon();

        var y = 12;
        var labelFont = new Font("Segoe UI", 9F);
        var headerFont = new Font("Segoe UI", 9F, FontStyle.Bold);

        // --- Connection Section ---
        var connHeader = new Label
        {
            Text = "CONNECTION",
            Location = new Point(12, y),
            Size = new Size(200, 20),
            Font = headerFont,
            ForeColor = Color.FromArgb(0, 120, 215)
        };
        y += 22;

        var gatewayUrlLabel = new Label
        {
            Text = "Gateway URL:",
            Location = new Point(12, y),
            Size = new Size(100, 20),
            Font = labelFont
        };
        y += 22;

        _gatewayUrlTextBox = new TextBox
        {
            Location = new Point(12, y),
            Size = new Size(310, 23),
            Font = labelFont
        };

        _testConnectionButton = new Button
        {
            Text = "Test",
            Location = new Point(330, y - 1),
            Size = new Size(65, 25),
            Font = labelFont
        };
        _testConnectionButton.Click += OnTestConnection;
        y += 30;

        var tokenLabel = new Label
        {
            Text = "Token:",
            Location = new Point(12, y),
            Size = new Size(100, 20),
            Font = labelFont
        };
        y += 22;

        _tokenTextBox = new TextBox
        {
            Location = new Point(12, y),
            Size = new Size(310, 23),
            Font = labelFont,
            UseSystemPasswordChar = true
        };

        _statusLabel = new Label
        {
            Text = "",
            Location = new Point(330, y + 2),
            Size = new Size(130, 20),
            Font = new Font("Segoe UI", 8F),
            ForeColor = Color.DarkGreen
        };
        y += 35;

        // --- Startup Section ---
        var startupHeader = new Label
        {
            Text = "STARTUP",
            Location = new Point(12, y),
            Size = new Size(200, 20),
            Font = headerFont,
            ForeColor = Color.FromArgb(0, 120, 215)
        };
        y += 22;

        _autoStartCheckBox = new CheckBox
        {
            Text = "Start automatically with Windows",
            Location = new Point(12, y),
            Size = new Size(280, 22),
            Font = labelFont
        };
        y += 26;

        _globalHotkeyCheckBox = new CheckBox
        {
            Text = "Global hotkey (Ctrl+Shift+Space → Quick Send)",
            Location = new Point(12, y),
            Size = new Size(340, 22),
            Font = labelFont
        };
        y += 35;

        // --- Notifications Section ---
        var notifyHeader = new Label
        {
            Text = "NOTIFICATIONS",
            Location = new Point(12, y),
            Size = new Size(200, 20),
            Font = headerFont,
            ForeColor = Color.FromArgb(0, 120, 215)
        };
        y += 22;

        _showNotificationsCheckBox = new CheckBox
        {
            Text = "Show desktop notifications",
            Location = new Point(12, y),
            Size = new Size(250, 22),
            Font = labelFont
        };
        _showNotificationsCheckBox.CheckedChanged += (_, _) =>
        {
            _notifyFilterPanel.Enabled = _showNotificationsCheckBox.Checked;
        };
        y += 26;

        var soundLabel = new Label
        {
            Text = "Sound:",
            Location = new Point(12, y),
            Size = new Size(50, 20),
            Font = labelFont
        };

        _notificationSoundComboBox = new ComboBox
        {
            Location = new Point(65, y - 2),
            Size = new Size(140, 23),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = labelFont
        };
        _notificationSoundComboBox.Items.AddRange(new[] { "Default", "None", "Critical", "Information" });
        y += 30;

        // Filter panel
        var filterLabel = new Label
        {
            Text = "Show toasts for:",
            Location = new Point(12, y),
            Size = new Size(120, 20),
            Font = labelFont,
            ForeColor = Color.Gray
        };
        y += 22;

        _notifyFilterPanel = new Panel
        {
            Location = new Point(12, y),
            Size = new Size(440, 72),
            BorderStyle = BorderStyle.None
        };

        // Two columns of filter checkboxes
        var cbFont = new Font("Segoe UI", 8.5F);
        _notifyHealthCb = MakeFilterCb("🩸 Health", 0, 0, cbFont);
        _notifyUrgentCb = MakeFilterCb("🚨 Urgent", 0, 24, cbFont);
        _notifyReminderCb = MakeFilterCb("⏰ Reminders", 0, 48, cbFont);
        _notifyEmailCb = MakeFilterCb("📧 Email", 150, 0, cbFont);
        _notifyCalendarCb = MakeFilterCb("📅 Calendar", 150, 24, cbFont);
        _notifyBuildCb = MakeFilterCb("🔨 Build/CI", 150, 48, cbFont);
        _notifyStockCb = MakeFilterCb("📦 Stock", 300, 0, cbFont);
        _notifyInfoCb = MakeFilterCb("🤖 General", 300, 24, cbFont);

        _notifyFilterPanel.Controls.AddRange(new Control[]
        {
            _notifyHealthCb, _notifyUrgentCb, _notifyReminderCb,
            _notifyEmailCb, _notifyCalendarCb, _notifyBuildCb,
            _notifyStockCb, _notifyInfoCb
        });

        y += 80;

        // --- Buttons ---
        y += 10;
        _okButton = new Button
        {
            Text = "&OK",
            Location = new Point(300, y),
            Size = new Size(75, 28),
            Font = labelFont
        };
        _okButton.Click += OnOkClick;

        _cancelButton = new Button
        {
            Text = "&Cancel",
            Location = new Point(382, y),
            Size = new Size(75, 28),
            Font = labelFont
        };
        _cancelButton.Click += OnCancelClick;

        AcceptButton = _okButton;
        CancelButton = _cancelButton;

        // Add all controls
        Controls.AddRange(new Control[]
        {
            connHeader, gatewayUrlLabel, _gatewayUrlTextBox, _testConnectionButton,
            tokenLabel, _tokenTextBox, _statusLabel,
            startupHeader, _autoStartCheckBox, _globalHotkeyCheckBox,
            notifyHeader, _showNotificationsCheckBox, soundLabel, _notificationSoundComboBox,
            filterLabel, _notifyFilterPanel,
            _okButton, _cancelButton
        });
    }

    private static CheckBox MakeFilterCb(string text, int x, int y, Font font)
    {
        return new CheckBox
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(140, 22),
            Font = font,
            Checked = true
        };
    }

    private void LoadSettings()
    {
        _gatewayUrlTextBox.Text = _settings.GatewayUrl;
        _tokenTextBox.Text = _settings.Token;
        _autoStartCheckBox.Checked = _settings.AutoStart;
        _globalHotkeyCheckBox.Checked = _settings.ShowGlobalHotkey;
        _showNotificationsCheckBox.Checked = _settings.ShowNotifications;
        _notifyFilterPanel.Enabled = _settings.ShowNotifications;
        
        var soundIndex = _notificationSoundComboBox.Items.IndexOf(_settings.NotificationSound);
        _notificationSoundComboBox.SelectedIndex = soundIndex >= 0 ? soundIndex : 0;

        _notifyHealthCb.Checked = _settings.NotifyHealth;
        _notifyUrgentCb.Checked = _settings.NotifyUrgent;
        _notifyReminderCb.Checked = _settings.NotifyReminder;
        _notifyEmailCb.Checked = _settings.NotifyEmail;
        _notifyCalendarCb.Checked = _settings.NotifyCalendar;
        _notifyBuildCb.Checked = _settings.NotifyBuild;
        _notifyStockCb.Checked = _settings.NotifyStock;
        _notifyInfoCb.Checked = _settings.NotifyInfo;
    }

    private void SaveSettings()
    {
        _settings.GatewayUrl = _gatewayUrlTextBox.Text.Trim();
        _settings.Token = _tokenTextBox.Text.Trim();
        _settings.AutoStart = _autoStartCheckBox.Checked;
        _settings.ShowGlobalHotkey = _globalHotkeyCheckBox.Checked;
        _settings.ShowNotifications = _showNotificationsCheckBox.Checked;
        _settings.NotificationSound = _notificationSoundComboBox.SelectedItem?.ToString() ?? "Default";
        _settings.NotifyHealth = _notifyHealthCb.Checked;
        _settings.NotifyUrgent = _notifyUrgentCb.Checked;
        _settings.NotifyReminder = _notifyReminderCb.Checked;
        _settings.NotifyEmail = _notifyEmailCb.Checked;
        _settings.NotifyCalendar = _notifyCalendarCb.Checked;
        _settings.NotifyBuild = _notifyBuildCb.Checked;
        _settings.NotifyStock = _notifyStockCb.Checked;
        _settings.NotifyInfo = _notifyInfoCb.Checked;
    }

    private async void OnTestConnection(object? sender, EventArgs e)
    {
        _testConnectionButton.Enabled = false;
        _statusLabel.Text = "Testing...";
        _statusLabel.ForeColor = Color.Blue;

        try
        {
            var testClient = new MoltbotGatewayClient(
                _gatewayUrlTextBox.Text.Trim(),
                _tokenTextBox.Text.Trim());

            await testClient.ConnectAsync();
            await testClient.DisconnectAsync();
            testClient.Dispose();

            _statusLabel.Text = "✅ Connected";
            _statusLabel.ForeColor = Color.DarkGreen;
        }
        catch (Exception ex)
        {
            _statusLabel.Text = $"❌ {ex.Message}";
            _statusLabel.ForeColor = Color.Red;
        }
        finally
        {
            _testConnectionButton.Enabled = true;
        }
    }

    private void OnOkClick(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_gatewayUrlTextBox.Text))
        {
            MessageBox.Show("Gateway URL is required.", "Settings", 
                          MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _gatewayUrlTextBox.Focus();
            return;
        }

        if (!Uri.TryCreate(_gatewayUrlTextBox.Text.Trim(), UriKind.Absolute, out var uri) ||
            (uri.Scheme != "ws" && uri.Scheme != "wss"))
        {
            MessageBox.Show("Gateway URL must be a valid WebSocket URL (ws:// or wss://).", "Settings",
                          MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _gatewayUrlTextBox.Focus();
            return;
        }

        SaveSettings();
        DialogResult = DialogResult.OK;
        Close();
    }

    private void OnCancelClick(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}

