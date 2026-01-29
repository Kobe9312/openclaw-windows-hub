// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moltbot.Shared;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace Moltbot;

internal sealed partial class MoltbotPage : ListPage
{
    private static string _gatewayUrl = "ws://localhost:18789";
    private static string _token = "";
    
    public MoltbotPage()
    {
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = "Moltbot";
        Name = "Open";
        
        // Try to load settings from tray app
        LoadSettings();
    }

    private static void LoadSettings()
    {
        try
        {
            var settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MoltbotTray", "settings.json");
            
            if (File.Exists(settingsPath))
            {
                var json = File.ReadAllText(settingsPath);
                var settings = System.Text.Json.JsonDocument.Parse(json);
                
                if (settings.RootElement.TryGetProperty("GatewayUrl", out var url))
                    _gatewayUrl = url.GetString() ?? _gatewayUrl;
                if (settings.RootElement.TryGetProperty("Token", out var token))
                    _token = token.GetString() ?? "";
            }
        }
        catch { }
    }

    public override IListItem[] GetItems()
    {
        var items = new List<IListItem>
        {
            new ListItem(new OpenDashboardCommand(_gatewayUrl, _token))
            {
                Title = "🦞 Open Dashboard",
                Subtitle = "Open Moltbot web dashboard in browser"
            },
            new ListItem(new QuickSendCommand(_gatewayUrl, _token))
            {
                Title = "💬 Quick Send",
                Subtitle = "Send a message to Moltbot"
            },
            new ListItem(new StatusPage(_gatewayUrl, _token))
            {
                Title = "📊 Full Status",
                Subtitle = "View gateway, sessions, and channels"
            },
            new ListItem(new SessionsPage(_gatewayUrl, _token))
            {
                Title = "⚡ Sessions",
                Subtitle = "View active agent sessions"
            },
            new ListItem(new ChannelsPage(_gatewayUrl, _token))
            {
                Title = "📡 Channels",
                Subtitle = "View Telegram, WhatsApp status"
            },
            new ListItem(new HealthCheckCommand(_gatewayUrl, _token))
            {
                Title = "🔄 Health Check",
                Subtitle = "Run a gateway health check"
            }
        };

        return items.ToArray();
    }
}

/// <summary>
/// Command to open the Moltbot dashboard in the browser.
/// </summary>
internal sealed partial class OpenDashboardCommand : InvokableCommand
{
    private readonly string _gatewayUrl;
    private readonly string _token;

    public OpenDashboardCommand(string gatewayUrl, string token)
    {
        _gatewayUrl = gatewayUrl;
        _token = token;
    }

    public override ICommandResult Invoke()
    {
        try
        {
            var url = GetDashboardUrl(_gatewayUrl, _token);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) 
            { 
                UseShellExecute = true 
            });
        }
        catch { }

        return CommandResult.Hide();
    }

    internal static string GetDashboardUrl(string gatewayUrl, string token)
    {
        var url = gatewayUrl
            .Replace("ws://", "http://")
            .Replace("wss://", "https://");
        
        if (!string.IsNullOrEmpty(token))
        {
            var separator = url.Contains('?') ? "&" : "?";
            url = $"{url}{separator}token={Uri.EscapeDataString(token)}";
        }
        return url;
    }
}

/// <summary>
/// Command to send a quick message - prompts for input then sends.
/// </summary>
internal sealed partial class QuickSendCommand : InvokableCommand
{
    private readonly string _gatewayUrl;
    private readonly string _token;

    public QuickSendCommand(string gatewayUrl, string token)
    {
        _gatewayUrl = gatewayUrl;
        _token = token;
        Name = "Send message to Moltbot";
    }

    public override ICommandResult Invoke()
    {
        // Open a simple input dialog using Windows forms
        try
        {
            // Use the dashboard URL with a message prompt
            var url = OpenDashboardCommand.GetDashboardUrl(_gatewayUrl, _token);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) 
            { 
                UseShellExecute = true 
            });
        }
        catch { }

        return CommandResult.Hide();
    }
}

/// <summary>
/// Command to run a health check.
/// </summary>
internal sealed partial class HealthCheckCommand : InvokableCommand
{
    private readonly string _gatewayUrl;
    private readonly string _token;

    public HealthCheckCommand(string gatewayUrl, string token)
    {
        _gatewayUrl = gatewayUrl;
        _token = token;
    }

    public override ICommandResult Invoke()
    {
        // Just run the health check and show a toast/notification
        Task.Run(async () =>
        {
            try
            {
                using var client = new MoltbotGatewayClient(_gatewayUrl, _token);
                await client.ConnectAsync();
                await client.CheckHealthAsync();
                await Task.Delay(1000);
                await client.DisconnectAsync();
            }
            catch { }
        });

        // Keep palette open - user can check status page
        return CommandResult.KeepOpen();
    }
}

/// <summary>
/// Page showing active sessions.
/// </summary>
internal sealed partial class SessionsPage : ContentPage
{
    private readonly string _gatewayUrl;
    private readonly string _token;

    public SessionsPage(string gatewayUrl, string token)
    {
        _gatewayUrl = gatewayUrl;
        _token = token;
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = "Sessions";
        Name = "View sessions";
    }

    public override IContent[] GetContent()
    {
        var sb = new StringBuilder();
        sb.AppendLine("## ⚡ Active Sessions");
        sb.AppendLine();
        
        try
        {
            using var client = new MoltbotGatewayClient(_gatewayUrl, _token);
            
            var task = client.ConnectAsync();
            task.Wait(TimeSpan.FromSeconds(3));

            if (!task.IsCompletedSuccessfully)
            {
                sb.AppendLine("❌ Could not connect to gateway");
                return [new MarkdownContent { Body = sb.ToString() }];
            }

            client.RequestSessionsAsync().Wait(TimeSpan.FromSeconds(2));
            var sessions = client.GetSessionList();

            if (sessions.Length == 0)
            {
                sb.AppendLine("_No active sessions_");
            }
            else
            {
                // Group by main/sub
                var mainSessions = new List<SessionInfo>();
                var subSessions = new List<SessionInfo>();
                
                foreach (var s in sessions)
                {
                    if (s.IsMain) mainSessions.Add(s);
                    else subSessions.Add(s);
                }

                if (mainSessions.Count > 0)
                {
                    sb.AppendLine("### ⚡ Main Sessions");
                    foreach (var s in mainSessions)
                    {
                        sb.AppendLine($"- **{s.ShortKey}**");
                        if (!string.IsNullOrEmpty(s.Model))
                            sb.AppendLine($"  - Model: `{s.Model}`");
                        if (!string.IsNullOrEmpty(s.Channel))
                            sb.AppendLine($"  - Channel: {s.Channel}");
                        if (s.StartedAt.HasValue)
                            sb.AppendLine($"  - Started: {s.StartedAt:g}");
                    }
                    sb.AppendLine();
                }

                if (subSessions.Count > 0)
                {
                    sb.AppendLine($"### 🔹 Sub-Sessions ({subSessions.Count})");
                    foreach (var s in subSessions)
                    {
                        var activity = !string.IsNullOrEmpty(s.CurrentActivity) ? $" - {s.CurrentActivity}" : "";
                        sb.AppendLine($"- {s.ShortKey}{activity}");
                    }
                }
            }

            client.DisconnectAsync().Wait(TimeSpan.FromSeconds(1));
        }
        catch (Exception ex)
        {
            sb.AppendLine($"❌ Error: {ex.Message}");
        }

        return [new MarkdownContent { Body = sb.ToString() }];
    }
}

/// <summary>
/// Page showing channel health.
/// </summary>
internal sealed partial class ChannelsPage : ContentPage
{
    private readonly string _gatewayUrl;
    private readonly string _token;
    private ChannelHealth[]? _channels;

    public ChannelsPage(string gatewayUrl, string token)
    {
        _gatewayUrl = gatewayUrl;
        _token = token;
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = "Channels";
        Name = "View channels";
    }

    public override IContent[] GetContent()
    {
        var sb = new StringBuilder();
        sb.AppendLine("## 📡 Channel Status");
        sb.AppendLine();
        
        try
        {
            using var client = new MoltbotGatewayClient(_gatewayUrl, _token);
            
            client.ChannelHealthUpdated += (s, channels) => _channels = channels;
            
            var task = client.ConnectAsync();
            task.Wait(TimeSpan.FromSeconds(3));

            if (!task.IsCompletedSuccessfully)
            {
                sb.AppendLine("❌ Could not connect to gateway");
                return [new MarkdownContent { Body = sb.ToString() }];
            }

            // Health check fetches channel status
            client.CheckHealthAsync().Wait(TimeSpan.FromSeconds(2));

            if (_channels == null || _channels.Length == 0)
            {
                sb.AppendLine("_No channels configured_");
            }
            else
            {
                foreach (var ch in _channels)
                {
                    var statusIcon = ch.Status.ToLowerInvariant() switch
                    {
                        "running" or "ok" or "connected" => "🟢",
                        "ready" => "🟡",
                        "linked" => "🔵",
                        "stopped" or "configured" => "⚪",
                        "error" => "🔴",
                        _ => "⚫"
                    };
                    
                    var name = char.ToUpper(ch.Name[0]) + ch.Name[1..];
                    sb.AppendLine($"### {statusIcon} {name}");
                    sb.AppendLine();
                    sb.AppendLine($"- **Status:** {ch.Status}");
                    
                    if (ch.IsLinked)
                        sb.AppendLine("- **Linked:** ✅ Yes");
                    
                    if (!string.IsNullOrEmpty(ch.AuthAge))
                        sb.AppendLine($"- **Auth Age:** {ch.AuthAge}");
                    
                    if (!string.IsNullOrEmpty(ch.Error))
                        sb.AppendLine($"- **Error:** ⚠️ {ch.Error}");
                    
                    sb.AppendLine();
                }
            }

            client.DisconnectAsync().Wait(TimeSpan.FromSeconds(1));
        }
        catch (Exception ex)
        {
            sb.AppendLine($"❌ Error: {ex.Message}");
        }

        return [new MarkdownContent { Body = sb.ToString() }];
    }
}

/// <summary>
/// Page showing full Moltbot status information.
/// </summary>
internal sealed partial class StatusPage : ContentPage
{
    private readonly string _gatewayUrl;
    private readonly string _token;
    private ChannelHealth[]? _channels;

    public StatusPage(string gatewayUrl, string token)
    {
        _gatewayUrl = gatewayUrl;
        _token = token;
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = "Moltbot Status";
        Name = "View status";
    }

    public override IContent[] GetContent()
    {
        var markdown = new MarkdownContent
        {
            Body = GetStatusMarkdown()
        };
        return [markdown];
    }

    private string GetStatusMarkdown()
    {
        var sb = new StringBuilder();
        
        try
        {
            using var client = new MoltbotGatewayClient(_gatewayUrl, _token);
            
            client.ChannelHealthUpdated += (s, channels) => _channels = channels;
            
            var task = client.ConnectAsync();
            task.Wait(TimeSpan.FromSeconds(3));

            if (!task.IsCompletedSuccessfully)
            {
                return "## ❌ Disconnected\n\nCould not connect to gateway.\n\nMake sure Moltbot gateway is running.";
            }

            sb.AppendLine("## 🦞 Moltbot Status");
            sb.AppendLine();
            sb.AppendLine("### Connection");
            sb.AppendLine($"- **Gateway:** `{_gatewayUrl}`");
            sb.AppendLine("- **Status:** ✅ Connected");
            sb.AppendLine();

            // Get health and sessions
            client.CheckHealthAsync().Wait(TimeSpan.FromSeconds(2));
            client.RequestSessionsAsync().Wait(TimeSpan.FromSeconds(1));
            
            var sessions = client.GetSessionList();
            
            // Sessions
            sb.AppendLine("### ⚡ Sessions");
            if (sessions.Length == 0)
            {
                sb.AppendLine("_No active sessions_");
            }
            else
            {
                var mainCount = 0;
                var subCount = 0;
                foreach (var s in sessions)
                {
                    if (s.IsMain) mainCount++;
                    else subCount++;
                }
                sb.AppendLine($"- Main: **{mainCount}** | Sub: **{subCount}**");
                sb.AppendLine();
                
                foreach (var s in sessions.Take(5))
                {
                    var icon = s.IsMain ? "⚡" : "🔹";
                    sb.AppendLine($"- {icon} {s.DisplayText}");
                }
                
                if (sessions.Length > 5)
                    sb.AppendLine($"- _...and {sessions.Length - 5} more_");
            }
            sb.AppendLine();

            // Channels
            sb.AppendLine("### 📡 Channels");
            if (_channels == null || _channels.Length == 0)
            {
                sb.AppendLine("_No channels configured_");
            }
            else
            {
                foreach (var ch in _channels)
                {
                    var statusIcon = ch.Status.ToLowerInvariant() switch
                    {
                        "running" or "ok" => "🟢",
                        "ready" => "🟡", 
                        "linked" => "🔵",
                        "stopped" => "⚪",
                        "error" => "🔴",
                        _ => "⚫"
                    };
                    var name = char.ToUpper(ch.Name[0]) + ch.Name[1..];
                    sb.AppendLine($"- {statusIcon} **{name}:** {ch.Status}");
                }
            }

            client.DisconnectAsync().Wait(TimeSpan.FromSeconds(1));
            
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine("_Use 🦞 Open Dashboard for the full web interface_");
            
            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"## ❌ Error\n\n{ex.Message}\n\nMake sure the gateway is running and your settings are correct.";
        }
    }
}

