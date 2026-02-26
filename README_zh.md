# 🦞 OpenClaw Windows Hub (中文版)

[OpenClaw](https://openclaw.ai) 的 Windows 伴侣套件——一款 AI 驱动的个人助手。

*由 Scott Hanselman 和 Molty 用 🦞 爱心制作*

![Molty - Windows 托盘应用](docs/molty1.png)

![Molty - 命令面板](docs/molty2.png)

## 项目

此 monorepo 包含三个项目：

| 项目 | 描述 |
|---------|-------------|
| **OpenClaw.Tray** | 用于快速访问 OpenClaw 的系统托盘应用 |
| **OpenClaw.Shared** | 共享的网关客户端库 |
| **OpenClaw.CommandPalette** | PowerToys 命令面板扩展 |

## 🚀 快速开始

### 前置要求
- Windows 10 (20H2+) 或 Windows 11
- .NET 10.0 SDK - https://dotnet.microsoft.com/download/dotnet/10.0
- Windows 10 SDK（用于 WinUI 构建）- 通过 Visual Studio 或独立安装
- WebView2 Runtime - 现代 Windows 上已预装，或从 https://developer.microsoft.com/microsoft-edge/webview2 获取
- PowerToys（可选，用于命令面板扩展）

### 构建

使用构建脚本检查前置要求并构建：

```powershell
# 检查前置要求
.\build.ps1 -CheckOnly

# 构建所有项目
.\build.ps1

# 构建特定项目
.\build.ps1 -Project WinUI
.\build.ps1 -Project Tray -Configuration Release
```

或直接使用 dotnet 构建：

```powershell
# 构建全部（建议使用 build.ps1 以获得最佳效果）
dotnet build

# 构建 WinUI（需要运行时标识符以支持 WebView2）
dotnet build src/OpenClaw.Tray.WinUI/OpenClaw.Tray.WinUI.csproj -r win-arm64  # ARM64
dotnet build src/OpenClaw.Tray.WinUI/OpenClaw.Tray.WinUI.csproj -r win-x64    # x64

# 构建 MSIX 包（用于相机/麦克风权限提示）
dotnet build src/OpenClaw.Tray.WinUI -r win-arm64 -p:PackageMsix=true  # ARM64 MSIX
dotnet build src/OpenClaw.Tray.WinUI -r win-x64 -p:PackageMsix=true    # x64 MSIX
```

### 运行托盘应用

```powershell
# WinForms 版本
dotnet run --project src/OpenClaw.Tray/OpenClaw.Tray.csproj

# WinUI 版本 - 直接运行 exe（路径包含运行时标识符）
.\src\OpenClaw.Tray.WinUI\bin\Debug\net10.0-windows10.0.19041.0\win-arm64\OpenClaw.Tray.WinUI.exe  # ARM64
.\src\OpenClaw.Tray.WinUI\bin\Debug\net10.0-windows10.0.19041.0\win-x64\OpenClaw.Tray.WinUI.exe    # x64
```

## 📦 OpenClaw.Tray (Molty)

现代化的 Windows 11 风格系统托盘伴侣，连接到你的本地 OpenClaw 网关。

### 功能特性
- 🦞 **龙虾品牌** - 带状态颜色的像素风龙虾托盘图标
- 🎨 **现代 UI** - 支持深色/浅色模式的 Windows 11 飞出菜单
- 💬 **快速发送** - 通过全局热键（Ctrl+Alt+Shift+C）发送消息
- 🔄 **自动更新** - 从 GitHub Releases 自动更新
- 🌐 **Web 聊天** - 嵌入式 WebView2 聊天窗口
- 📊 **实时状态** - 实时显示会话、频道和使用情况
- ⚡ **活动流** - 专用于实时会话、使用情况、节点和通知事件的飞出面板
- 🔔 **Toast 通知** - 带有[智能分类](docs/NOTIFICATION_CATEGORIZATION.md)的可点击 Windows 通知
- 📡 **频道控制** - 从菜单启动/停止 Telegram 和 WhatsApp
- 🖥️ **节点可观测性** - 带在线/离线状态的可复制摘要的节点清单
- ⏱ **定时任务 (Cron Jobs)** - 快速访问计划任务
- 🚀 **开机自启动** - 随 Windows 启动
- ⚙️ **设置** - 完整配置对话框
- 🎯 **首次运行体验** - 欢迎对话框引导新用户

### 菜单分区
- **状态** - 网关连接状态，点击可查看详细信息
- **会话** - 带预览和每个会话控制的活跃 agent 会话
- **使用情况** - 提供商/成本摘要，可快速跳转到活动详细信息
- **频道** - Telegram/WhatsApp 状态和切换控制
- **节点** - 在线/离线节点清单和可复制摘要
- **最近活动** - 会话、使用情况、节点和通知的带时间戳事件流
- **操作** - 仪表板、Web 聊天、快速发送、活动流、历史
- **设置** - 配置、自启动、日志

### Mac 平行状态

与 [openclaw-menubar](https://github.com/magimetal/openclaw-menubar)（macOS Swift 菜单栏应用）对比：

| 功能 | Mac | Windows | 备注 |
|---------|-----|---------|-------|
| 菜单栏/托盘图标 | ✅ | ✅ | 颜色编码状态 |
| 网关状态显示 | ✅ | ✅ | 已连接/已断开 |
| PID 显示 | ✅ | ❌ | Mac 显示网关 PID |
| 频道状态 | ✅ | ✅ | Mac: Discord / Win: Telegram+WhatsApp |
| 会话计数 | ✅ | ✅ | |
| 最后检查时间戳 | ✅ | ✅ | 显示在托盘工具提示中 |
| 网关启动/停止/重启 | ✅ | ❌ | Mac 控制网关进程 |
| 查看日志 | ✅ | ✅ | |
| 打开 Web UI | ✅ | ✅ | |
| 刷新 | ✅ | ✅ | 打开菜单时自动刷新 |
| 登录时启动 | ✅ | ✅ | |
| 通知切换 | ✅ | ✅ | |

### Windows 独有功能

这些功能在 Windows 上可用，但 Mac 应用没有：

| 功能 | 描述 |
|---------|-------------|
| 快速发送热键 | Ctrl+Alt+Shift+C 全局热键 |
| 嵌入式 Web 聊天 | 基于 WebView2 的聊天窗口 |
| Toast 通知 | 可点击的 Windows 通知 |
| 频道控制 | 启动/停止 Telegram 和 WhatsApp |
| 现代飞出菜单 | Windows 11 风格，支持深色/浅色模式 |
| 深度链接 | `openclaw://` URL scheme with IPC |
| 首次运行欢迎 | 新用户引导式入门 |
| PowerToys 集成 | 命令面板扩展 |

### 🔌 节点模式 (Agent 控制)

在设置中启用节点模式后，你的 Windows PC 将成为一个 **节点**，OpenClaw agent 可以控制它——就像 Mac 应用一样！Agent 可以：

| 能力 | 命令 | 描述 |
|------------|----------|-------------|
| **系统** | `system.notify`, `system.run`, `system.execApprovals.get`, `system.execApprovals.set` | 显示 Windows toast 通知，执行带策略控制的命令 |
| **画布** | `canvas.present`, `canvas.hide`, `canvas.navigate`, `canvas.eval`, `canvas.snapshot`, `canvas.a2ui.push` (研究中), `canvas.a2ui.reset` (研究中) | 显示和控制 WebView2 窗口 |
| **屏幕** | `screen.capture`, `screen.list` | 捕获屏幕截图 |
| **相机** | `camera.list`, `camera.snap` | 枚举相机并捕获静态照片 |

#### 节点设置

1. 在**设置中启用节点模式**（默认启用）
2. **首次连接**会在网关上创建配对请求
3. **批准设备**在你的网关上：
   ```bash
   openclaw devices list          # 查找你的 Windows 设备
   openclaw devices approve <id>  # 批准它
   ```
4. **配置网关 allowCommands** - 在 `~/.openclaw/openclaw.json` 的 `gateway.nodes` 下添加你想要允许的命令：
   ```json
   {
     "gateway": {
       "nodes": {
         "allowCommands": [
           "system.notify",
           "system.run",
           "system.execApprovals.get",
           "system.execApprovals.set",
           "canvas.present",
           "canvas.hide",
           "canvas.navigate",
           "canvas.eval",
           "canvas.snapshot",
           "canvas.a2ui.push",
           "canvas.a2ui.reset",
           "screen.capture",
           "screen.list",
           "camera.list",
           "camera.snap"
         ]
       }
     }
   }
   ```
   > ⚠️ **重要**：网关有服务器端 allowlist。必须明确列出命令——像 `canvas.*` 这样的通配符不起作用！

5. **从你的 Mac/网关测试它**：
   ```bash
   # 显示通知
   openclaw nodes notify --node <id> --title "你好" --body "来自 Mac！"

   # 打开一个画布窗口
   openclaw nodes canvas present --node <id> --url "https://example.com"

   # 执行 JavaScript（注意：CLI 发送 "javaScript" 参数）
   openclaw nodes canvas eval --node <id> --javaScript "document.title"

   # 在画布中渲染 A2UI JSONL（将文件内容作为字符串传递）
   openclaw nodes canvas a2ui push --node <id> --jsonl "$(Get-Content -Raw .\ui.jsonl)"

   # 截取屏幕截图
   openclaw nodes invoke --node <id> --command screen.capture --params '{"screenIndex":0,"format":"png"}'

   # 列出相机
   openclaw nodes invoke --node <id> --command camera.list

   # 拍照 (NV12/MediaCapture 回退)
   openclaw nodes invoke --node <id> --command camera.snap --params '{"deviceId":"<device-id>","format":"jpeg","quality":80}'

   # 在 Windows 节点上执行命令
   openclaw nodes invoke --node <id> --command system.run --params '{"command":"Get-Process | Select -First 5","shell":"powershell","timeoutMs":10000}'

   # 查看 exec 批准策略
   openclaw nodes invoke --node <id> --command system.execApprovals.get

   # 更新 exec 批准策略（添加自定义规则）
   openclaw nodes invoke --node <id> --command system.execApprovals.set --params '{"rules":[{"pattern":"echo *","action":"allow"},{"pattern":"*","action":"deny"}],"defaultAction":"deny"}'
   ```
   > 📷 **相机权限**：桌面构建依赖于 Windows 隐私设置。打包的 MSIX 构建将显示系统同意提示。

   > 🔒 **Exec 策略**：`system.run` 在 Windows 节点上被批准策略限制，位于 `%LOCALAPPDATA%\OpenClawTray\exec-policy.json`（schema: `{ "defaultAction": "...", "rules": [...] }`）。这与网关端的 `~/.openclaw/exec-approvals.json` 是分开的。
   >
   > 规则与 `command` token（`argv[0]`）匹配。如果你的调用运行 `powershell.exe -File script.ps1`，请允许 `powershell.exe`/`pwsh.exe`（而不仅仅是脚本路径），否则你会收到 `No matching rule; default policy applied`。
   >
   > ```bash
   > openclaw nodes invoke --node <id> --command system.execApprovals.set --params '{"rules":[{"pattern":"powershell.exe","action":"allow"},{"pattern":"pwsh.exe","action":"allow"},{"pattern":"echo *","action":"allow"},{"pattern":"*","action":"deny"}],"defaultAction":"deny"}'
   > ```

   > 🔐 **Web 聊天安全上下文**：远程 Web 聊天需要 `https://`（或 localhost）。如果使用自签名证书，请在 Windows 中信任它（受信任的根证书颁发机构）或使用 SSH 隧道到 localhost。

#### 托盘菜单中的节点状态

托盘菜单显示节点连接状态：
- **🔌 节点模式**部分在启用时出现
- **⏳ 等待批准...** - 设备需要在网关上批准
- **✅ 已配对并连接** - 准备接收命令
- 点击设备 ID 可复制它用于批准命令

### 深度链接

OpenClaw 注册了 `openclaw://` URL scheme，用于自动化和集成：

| 链接 | 描述 |
|------|-------------|
| `openclaw://settings` | 打开设置对话框 |
| `openclaw://chat` | 打开 Web 聊天窗口 |
| `openclaw://dashboard` | 在浏览器中打开仪表板 |
| `openclaw://dashboard/sessions` | 打开特定的仪表板页面 |
| `openclaw://send?message=Hello` | 用预填充的文本打开快速发送 |
| `openclaw://agent?message=Hello` | 直接发送消息（带确认）深度链接即使在 Molty 已经运行时也有效——它们通过 IPC 转发。

## 📦 OpenClaw.CommandPalette

PowerToys 命令面板扩展，用于快速访问 OpenClaw。

### 命令
- **🦞 打开仪表板** - 启动 Web 仪表板
- **💬 快速发送** - 发送消息
- **📊 完整状态** - 查看网关状态
- **⚡ 会话** - 查看活跃会话
- **📡 频道** - 查看频道健康状况
- **🔄 健康检查** - 触发健康刷新

### 安装
1. 在 Release 模式下构建解决方案
2. 通过 Visual Studio 部署 MSIX 包
3. 打开命令面板 (Win+Alt+Space)
4. 输入 "OpenClaw" 查看命令

## 📦 OpenClaw.Shared

共享库包含：
- `OpenClawGatewayClient` - 网关协议的 WebSocket 客户端
- `IOpenClawLogger` - 日志接口
- 数据模型 (SessionInfo, ChannelHealth 等)
- 频道控制（通过网关启动/停止频道）

## 开发

### 项目结构
```
moltbot-windows-hub/
├── src/
│   ├── OpenClaw.Shared/           # 共享网关库
│   ├── OpenClaw.Tray/             # 系统托盘应用
│   └── OpenClaw.CommandPalette/   # PowerToys 扩展
├── docs/
│   └── molty1.png                # 截图
├── moltbot-windows-hub.sln
├── README.md
├── LICENSE
└── .gitignore
```

### 配置

设置存储在：
- 设置：`%APPDATA%\OpenClawTray\settings.json`
- 日志：`%LOCALAPPDATA%\OpenClawTray\openclaw-tray.log`

默认网关：`ws://localhost:18789`

### 首次运行

首次运行时如果没有 token，Molty 会显示一个欢迎对话框：
1. 解释开始使用需要什么
2. 链接到[文档](https://docs.molt.bot/web/dashboard)进行 token 设置
3. 打开设置以配置连接

## 许可证

MIT 许可证 - 详见 [LICENSE](LICENSE)

---

*以前称为 Moltbot，以前称为 Clawdbot*
