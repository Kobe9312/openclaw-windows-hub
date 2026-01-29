using System;
using System.Drawing;
using System.Windows.Forms;

namespace MoltbotTray;

public partial class QuickSendDialog : Form
{
    private TextBox _messageTextBox = null!;
    private Button _sendButton = null!;
    private Button _cancelButton = null!;
    private Label _hintLabel = null!;

    public string Message => _messageTextBox.Text;

    public QuickSendDialog()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        // Form properties
        Text = "Quick Send — Clawdbot";
        Size = new Size(500, 220);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = true;
        TopMost = true; // Always on top when opened via hotkey
        Icon = IconHelper.GetLobsterIcon();

        // Label
        var label = new Label
        {
            Text = "Send a message to Clawdbot:",
            Location = new Point(12, 12),
            Size = new Size(460, 20),
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular)
        };

        // Message text box
        _messageTextBox = new TextBox
        {
            Location = new Point(12, 36),
            Size = new Size(460, 90),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Segoe UI", 10F, FontStyle.Regular),
            AcceptsReturn = false // Enter sends, Shift+Enter for newline
        };

        // Hint label
        _hintLabel = new Label
        {
            Text = "Enter to send · Esc to cancel · Shift+Enter for new line",
            Location = new Point(12, 132),
            Size = new Size(300, 18),
            Font = new Font("Segoe UI", 8F, FontStyle.Regular),
            ForeColor = Color.Gray
        };

        // Send button
        _sendButton = new Button
        {
            Text = "&Send",
            Location = new Point(316, 148),
            Size = new Size(75, 28),
            UseVisualStyleBackColor = true,
            Font = new Font("Segoe UI", 9F, FontStyle.Regular)
        };
        _sendButton.Click += OnSendClick;

        // Cancel button
        _cancelButton = new Button
        {
            Text = "&Cancel",
            Location = new Point(397, 148),
            Size = new Size(75, 28),
            UseVisualStyleBackColor = true,
            Font = new Font("Segoe UI", 9F, FontStyle.Regular)
        };
        _cancelButton.Click += OnCancelClick;

        // Set dialog buttons
        AcceptButton = _sendButton;
        CancelButton = _cancelButton;

        // Add controls
        Controls.Add(label);
        Controls.Add(_messageTextBox);
        Controls.Add(_hintLabel);
        Controls.Add(_sendButton);
        Controls.Add(_cancelButton);

        // Focus the text box on show
        Shown += (_, _) =>
        {
            _messageTextBox.Focus();
            Activate(); // Ensure window is focused when opened via hotkey
        };
    }

    private void OnSendClick(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_messageTextBox.Text))
        {
            _messageTextBox.Focus();
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private void OnCancelClick(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        // Ctrl+Enter or Enter (without Shift) as send
        if (keyData == (Keys.Control | Keys.Enter) || keyData == Keys.Enter)
        {
            OnSendClick(null, EventArgs.Empty);
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }
}

