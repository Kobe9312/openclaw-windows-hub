using Microsoft.Win32;
using Moltbot.Shared;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace MoltbotTray;

/// <summary>
/// Handles clawdbot:// URI scheme registration and processing.
/// Matches macOS deep link support (clawdbot://agent?message=...)
/// </summary>
public static class DeepLinkHandler
{
    private const string UriScheme = "Moltbot";
    private const string FriendlyName = "Clawdbot Agent Command";

    /// <summary>
    /// Registers the clawdbot:// URI scheme in the Windows registry.
    /// Requires elevation for HKCR, falls back to HKCU.
    /// </summary>
    public static void RegisterUriScheme()
    {
        try
        {
            var exePath = Environment.ProcessPath ?? Application.ExecutablePath;

            // Try HKCU\Software\Classes (no elevation needed)
            using var key = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{UriScheme}");
            if (key == null) return;

            key.SetValue("", $"URL:{FriendlyName}");
            key.SetValue("URL Protocol", "");

            using var iconKey = key.CreateSubKey("DefaultIcon");
            iconKey?.SetValue("", $"\"{exePath}\",1");

            using var commandKey = key.CreateSubKey(@"shell\open\command");
            commandKey?.SetValue("", $"\"{exePath}\" \"%1\"");

            Logger.Info($"Registered URI scheme: {UriScheme}://");
        }
        catch (Exception ex)
        {
            Logger.Error("Failed to register URI scheme", ex);
        }
    }

    /// <summary>
    /// Checks if the app was launched with a deep link argument.
    /// </summary>
    public static bool TryGetDeepLink(string[] args, out Uri? uri)
    {
        uri = null;
        if (args.Length == 0) return false;

        foreach (var arg in args)
        {
            if (arg.StartsWith($"{UriScheme}://", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    uri = new Uri(arg);
                    return true;
                }
                catch { }
            }
        }
        return false;
    }

    /// <summary>
    /// Processes a clawdbot:// deep link.
    /// Supports: clawdbot://agent?message=...&sessionKey=...&channel=...
    /// </summary>
    public static async Task ProcessDeepLinkAsync(Uri uri, MoltbotGatewayClient client)
    {
        Logger.Info($"Processing deep link: {uri}");

        var host = uri.Host.ToLowerInvariant();
        var query = HttpUtility.ParseQueryString(uri.Query);

        switch (host)
        {
            case "agent":
                await HandleAgentDeepLinkAsync(query, client);
                break;
            default:
                Logger.Warn($"Unknown deep link host: {host}");
                break;
        }
    }

    private static async Task HandleAgentDeepLinkAsync(NameValueCollection query, MoltbotGatewayClient client)
    {
        var message = query["message"];
        if (string.IsNullOrWhiteSpace(message))
        {
            Logger.Warn("Deep link: missing message parameter");
            return;
        }

        var key = query["key"];
        var hasKey = !string.IsNullOrEmpty(key);

        // Without a key, prompt for confirmation (safety)
        if (!hasKey)
        {
            var preview = message.Length > 100 ? message[..100] + "…" : message;
            var result = MessageBox.Show(
                $"A deep link wants to send this message to Clawdbot:\n\n\"{preview}\"\n\nAllow?",
                "Clawdbot Deep Link",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                Logger.Info("Deep link: user declined");
                return;
            }
        }

        try
        {
            await client.SendChatMessageAsync(message);
            Logger.Info($"Deep link: sent message ({message.Length} chars)");
        }
        catch (Exception ex)
        {
            Logger.Error("Deep link: failed to send", ex);
        }
    }
}

