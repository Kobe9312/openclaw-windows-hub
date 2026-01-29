using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MoltbotTray;

/// <summary>
/// Shows recent notification history in a simple list view.
/// </summary>
public class NotificationHistoryForm : Form
{
    private ListView? _listView;
    private Button _clearButton = null!;
    private Button _closeButton = null!;
    private static NotificationHistoryForm? _instance;

    private static readonly List<NotificationEntry> _history = new();
    private const int MaxHistory = 200;

    public static void AddEntry(string title, string message, string type)
    {
        lock (_history)
        {
            _history.Add(new NotificationEntry
            {
                Timestamp = DateTime.Now,
                Title = title,
                Message = message,
                Type = type
            });

            // Trim old entries
            while (_history.Count > MaxHistory)
                _history.RemoveAt(0);
        }

        // If window is open, refresh it
        _instance?.RefreshList();
    }

    public static void ShowOrFocus()
    {
        if (_instance != null && !_instance.IsDisposed)
        {
            _instance.BringToFront();
            _instance.Focus();
            return;
        }

        _instance = new NotificationHistoryForm();
        _instance.Show();
    }

    private NotificationHistoryForm()
    {
        InitializeComponent();
        RefreshList();
    }

    private void InitializeComponent()
    {
        Text = "Notification History — Moltbot Tray";
        Size = new Size(600, 450);
        MinimumSize = new Size(400, 300);
        StartPosition = FormStartPosition.CenterScreen;
        Icon = IconHelper.GetLobsterIcon();

        _listView = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            Font = new Font("Segoe UI", 9F)
        };
        _listView.Columns.Add("Time", 130);
        _listView.Columns.Add("Type", 80);
        _listView.Columns.Add("Title", 150);
        _listView.Columns.Add("Message", 300);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 40,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(5)
        };

        _closeButton = new Button
        {
            Text = "&Close",
            Size = new Size(75, 26),
            Font = new Font("Segoe UI", 9F)
        };
        _closeButton.Click += (_, _) => Close();

        _clearButton = new Button
        {
            Text = "C&lear All",
            Size = new Size(85, 26),
            Font = new Font("Segoe UI", 9F)
        };
        _clearButton.Click += (_, _) =>
        {
            lock (_history) _history.Clear();
            RefreshList();
        };

        buttonPanel.Controls.Add(_closeButton);
        buttonPanel.Controls.Add(_clearButton);

        Controls.Add(_listView);
        Controls.Add(buttonPanel);
    }

    private void RefreshList()
    {
        if (_listView == null || _listView.IsDisposed) return;

        if (InvokeRequired)
        {
            Invoke(new Action(RefreshList));
            return;
        }

        _listView.BeginUpdate();
        _listView.Items.Clear();

        lock (_history)
        {
            // Show newest first
            for (int i = _history.Count - 1; i >= 0; i--)
            {
                var entry = _history[i];
                var item = new ListViewItem(entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
                item.SubItems.Add(entry.Type);
                item.SubItems.Add(entry.Title);
                item.SubItems.Add(entry.Message.Replace('\n', ' '));
                _listView.Items.Add(item);
            }
        }

        _listView.EndUpdate();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _instance = null;
        base.OnFormClosed(e);
    }

    private class NotificationEntry
    {
        public DateTime Timestamp { get; set; }
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string Type { get; set; } = "";
    }
}

