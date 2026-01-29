using MoltbotTray;
using System;
using System.Threading;
using System.Windows.Forms;

namespace MoltbotTray;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        // Single instance check
        using var mutex = new Mutex(true, "MoltbotTray", out bool createdNew);
        if (!createdNew)
        {
            // TODO: Forward deep link args to running instance via named pipe
            MessageBox.Show("Moltbot Tray is already running.", "Moltbot Tray",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Register URI scheme on first run
        DeepLinkHandler.RegisterUriScheme();

        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        var trayApp = new TrayApplication(args);
        Application.Run(trayApp);
    }
}

