// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace Moltbot;

internal sealed partial class MoltbotPage : ListPage
{
    public MoltbotPage()
    {
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = "Moltbot";
        Name = "Open";
    }

    public override IListItem[] GetItems()
    {
        return [
            new ListItem(new OpenUrlCommand("http://localhost:18789"))
            {
                Title = "🦞 Open Dashboard",
                Subtitle = "Open Moltbot web dashboard"
            },
            new ListItem(new OpenUrlCommand("moltbot://chat"))
            {
                Title = "💬 Web Chat",
                Subtitle = "Open the Moltbot chat window"
            },
            new ListItem(new OpenUrlCommand("moltbot://send"))
            {
                Title = "📝 Quick Send", 
                Subtitle = "Send a message to Moltbot"
            },
            new ListItem(new OpenUrlCommand("moltbot://settings"))
            {
                Title = "⚙️ Settings",
                Subtitle = "Configure Moltbot Tray"
            }
        ];
    }
}

