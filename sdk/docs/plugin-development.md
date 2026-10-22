# 时笺插件 SDK v1

C# 插件以 .NET 10 类库实现 `IShijianPlugin`，市场包扩展名为 `.sjplugin`。SDK 的 `src/Shijian.Plugin.Abstractions` 是公共契约，`examples/` 提供学习工具箱、周计划手记两个可编译示例。运行 `scripts/Pack-Plugins.ps1` 生成示例包。

## 生命周期与结果

`GetCommands()` 提供命令名称、说明与参数示例；`ExecuteAsync()` 接收命令、权限过滤后的快照、JSON 参数和取消令牌，返回 `PluginResult`。时笺先预览结果，再由用户确认创建任务或选择文件导出。不直接提供数据库句柄或任意文件写入接口。

| 权限 | 提供的数据 / 接受的结果 |
| --- | --- |
| `tasks.read` | 有效任务的标题、备注、安排、截止、状态、成果进度 |
| `projects.read` | 项目摘要 |
| `tasks.write` | 最多 100 个新任务草稿，整批校验、整批写入 |
| `files.export` | 文本、CSV、Markdown 或 JSON，由用户选择保存位置 |
| `state.read` / `state.write` | 插件自身 JSON 状态，保存方式由用户选择 |

一个任务可有独立执行日期和截止日期；插件不能用专注时长冒充成果进度。结果不能直接修改已有任务或自动发送学习数据到其他服务。

## 打包

ZIP 根目录放置 `plugin.json`、入口 DLL 及依赖 DLL。清单包含 id、name、version、description、apiVersion=1、entryAssembly、entryType、permissions、files；`files` 必须逐一列出所有载荷文件及 SHA-256，不包含清单本身。禁止附带 `Shijian.Plugin.Abstractions.dll`，运行时共享宿主契约。仅接受根目录 DLL、JSON、MD、TXT，不接受子目录、EXE、链接或额外文件。包最大 64 MB，最多 64 个文件。

```json
{
  "id": "org.example.my-plugin",
  "name": "我的插件",
  "version": "1.0.0",
  "description": "功能说明",
  "apiVersion": 1,
  "entryAssembly": "MyPlugin.dll",
  "entryType": "MyPlugin.Entry",
  "permissions": ["tasks.read", "files.export"],
  "files": {"MyPlugin.dll": "64位十六进制SHA256"}
}
```

安装默认禁用。启用和命令执行由用户触发；每次运行前校验全部安装文件，默认执行超时 30 秒，崩溃或超时会结束独立宿主进程。独立进程用于隔离故障，**DLL 仍具有当前 Windows 用户的文件、网络与系统权限，并非安全沙箱**。权限声明约束时笺给出的快照及接受的结果。

## 本地状态与 MCP

插件状态默认仅本次使用；用户可开启 Windows DPAPI CurrentUser 加密保存并可删除。升级替换插件会重置旧插件状态。应用 JSON 备份包含安装记录和非秘密配置，不包含插件文件、状态和凭据；换机后需重新安装与配对。

MCP 直接配置 stdio / HTTP 服务，不需要封装成 DLL；使用官方 MCP SDK 发现并手动调用工具。密钥放在“凭据”中的环境变量或请求头值里，选择临时或本机加密保存。启动参数、地址和普通连接配置不是密钥存储位置。

社区投稿和官方签名说明见市场的 `CONTRIBUTING.md` 与 `docs/market.md`。ClassSchedule 联动接口见 `docs/connect-v1.md`。
