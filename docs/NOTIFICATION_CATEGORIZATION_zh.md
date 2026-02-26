# 通知分类

托盘应用对传入的通知进行分类，以应用每类别的过滤器、显示适当的图标，并让用户控制他们看到哪些通知。

## 工作原理

通知通过**分层管道**流动——匹配的第一层获胜：

```
结构化元数据  →  用户规则  →  关键词匹配  →  默认 (信息)
```

### 1. 结构化元数据（最高优先级）

如果网关在通知上发送了元数据，则直接使用它：

- **意图** (例如 `reminder`, `build`, `alert`) — 映射到一个类别
- **频道** (例如 `email`, `calendar`, `ci`) — 映射到一个类别

这消除了误分类的可能性。提到"邮件"的聊天响应不会被分类为邮件——网关知道实际的来源。

> **注意：** 网关尚未发送结构化元数据。当它发送时，分类将在无需客户端更改的情况下自动改进。

### 2. 用户自定义规则

自定义正则或关键词规则，按顺序评估。在 `%APPDATA%\OpenClawTray\settings.json` 中配置这些：

```json
{
  "UserRules": [
    {
      "Pattern": "invoice|receipt",
      "IsRegex": true,
      "Category": "email",
      "Enabled": true
    },
    {
      "Pattern": "deploy to prod",
      "IsRegex": false,
      "Category": "urgent",
      "Enabled": true
    }
  ]
}
```

规则与通知标题和消息匹配（不区分大小写）。无效的正则模式将被静默跳过。

### 3. 关键词匹配（传统回退）

原始的基于关键词的系统，为向后兼容性而保留：

| 类别 | 关键词 | 图标 |
|----------|----------|------|
| `health` | blood sugar, glucose, cgm, mg/dl | 🩸 |
| `urgent` | urgent, critical, emergency | 🚨 |
| `reminder` | reminder | ⏰ |
| `stock` | stock, in stock, available now | 📦 |
| `email` | email, inbox, gmail | 📧 |
| `calendar` | calendar, meeting, event | 📅 |
| `error` | error, failed, exception | ⚠️ |
| `build` | build, ci, deploy | 🔨 |
| `info` | *(其他所有)* | 🤖 |

### 4. 默认

如果没有任何匹配，通知将被分类为 `info`。

## 聊天响应切换

通知要么是**聊天响应**（来自 AI agent 的回复），要么是**系统通知**（警报、提醒、构建状态等）。`NotifyChatResponses` 设置控制聊天响应是否生成 Windows toasts：

| 设置 | 聊天响应 | 系统通知 |
|---------|----------------|----------------------|
| `true` (默认) | ✅ 显示 | ✅ 显示 |
| `false` | ❌ 抑制 | ✅ 显示 |

当你通过其他设备进行对话，并且不希望每个回复都作为 toast 在你的桌面上弹出时，这很有用。

## 设置

所有通知设置都在 `%APPDATA%\OpenClawTray\settings.json` 中：

```json
{
  "ShowNotifications": true,
  "NotificationSound": "Default",

  "NotifyHealth": true,
  "NotifyUrgent": true,
  "NotifyReminder": true,
  "NotifyEmail": true,
  "NotifyCalendar": true,
  "NotifyBuild": true,
  "NotifyStock": true,
  "NotifyInfo": true,

  "NotifyChatResponses": true,
  "PreferStructuredCategories": true,
  "UserRules": []
}
```

| 设置 | 类型 | 默认值 | 描述 |
|---------|------|---------|-------------|
| `ShowNotifications` | bool | `true` | 所有通知的主开关 |
| `NotifyHealth` | bool | `true` | 显示健康/葡萄糖警报 |
| `NotifyUrgent` | bool | `true` | 显示紧急警报（也覆盖 `error` 类型）|
| `NotifyReminder` | bool | `true` | 显示提醒 |
| `NotifyEmail` | bool | `true` | 显示邮件通知 |
| `NotifyCalendar` | bool | `true` | 显示日历事件 |
| `NotifyBuild` | bool | `true` | 显示构建/CI/部署通知 |
| `NotifyStock` | bool | `true` | 显示库存警报 |
| `NotifyInfo` | bool | `true` | 显示一般信息通知 |
| `NotifyChatResponses` | bool | `true` | 显示聊天响应 toasts |
| `PreferStructuredCategories` | bool | `true` | 使用网关元数据而非关键词 |
| `UserRules` | array | `[]` | 自定义分类规则（见上文）|

## 频道和 Agent 映射

当结构化元数据可用时，频道和 agent 映射到类别：

**频道 → 类别：**
| 频道 | 类别 |
|---------|----------|
| `calendar` | calendar |
| `email` | email |
| `ci`, `build` | build |
| `stock`, `inventory` | stock |
| `health` | health |
| `alerts` | urgent |

**Agent 映射**也被支持——每个 agent 的类别默认值可以添加到 `NotificationCategorizer.cs` 中的频道映射中。

## 架构

分类逻辑位于 `OpenClaw.Shared.NotificationCategorizer` 中，使其可供 WinUI 托盘应用和共享库的任何其他使用者使用。网关客户端（`OpenClawGatewayClient`）在发出通知时调用分类器，而托盘应用的 `ShouldShowNotification` 方法在显示 toast 之前应用每类别和聊天切换过滤器。
