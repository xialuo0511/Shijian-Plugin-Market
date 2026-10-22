# 时笺插件市场

为 Windows 学生日程与进度应用「时笺」提供可下载扩展、SDK 和 ClassSchedule 联动协议。无需账号登录即可浏览与下载。

## 下载

| 插件 | 功能 | 来源 | 下载 |
| --- | --- | --- | --- |
| 学习工具箱 1.0.0 | 按 1 / 3 / 7 / 14 天生成复习任务；安全 CSV 导出 | 🛡️ **时笺官方认证** | [StudyTools.sjplugin](https://github.com/xialuo0511/Shijian-Plugin-Market/releases/download/plugins-v1.0.0/StudyTools-1.0.0.sjplugin) |
| 周计划手记 1.0.0 | 本周计划、成果进度、临近截止和复盘提纲导出为 Markdown | 🛡️ **时笺官方认证** | [WeeklyNotes.sjplugin](https://github.com/xialuo0511/Shijian-Plugin-Market/releases/download/plugins-v1.0.0/WeeklyNotes-1.0.0.sjplugin) |

[Windows 时笺 0.1 Beta](https://github.com/xialuo0511/Shijian-Plugin-Market/releases/tag/app-v0.1.0-beta.1) · [插件 SDK 1.0.0](https://github.com/xialuo0511/Shijian-Plugin-Market/releases/download/plugins-v1.0.0/Shijian-Plugin-SDK-1.0.0.zip) · [全部发行版本](https://github.com/xialuo0511/Shijian-Plugin-Market/releases)

应用定位为 0.1 Beta 预览版；插件和 SDK 的 API v1 是独立版本。Beta 客户端可获取后续 Beta 或正式版，更新检查无需登录。

推荐在时笺“插件与 MCP → 加载插件市场”中安装，以验证官方签名并显示认证标识。也可下载 `.sjplugin` 后本地安装；手动安装不附认证证明。安装默认禁用，启用及运行均由用户选择。

## 官方认证

官方插件由我们编写、维护，并使用本机维护的 ECDSA P-256 私钥签发。应用内置公钥，核对每个版本的身份、权限、包哈希与清单哈希，校验通过才显示官方标识。社区插件不能通过填写 `official: true` 获得认证。此标识代表时笺团队的来源认证。

DLL 插件拥有当前 Windows 用户的系统权限；独立进程用于隔离崩溃和超时，不是安全沙箱。应用提供的任务快照与接受的变更按声明权限处理，结果先预览再确认。请阅读权限和源码后启用。

## 开发与联动

- [SDK 开发指南](docs/plugin-development.md)，可编译源码位于 [sdk](sdk/)。
- [ClassSchedule Connect v1 协议](docs/connect-v1.md)：配对、课表预览、确认提交与今日摘要。手机插件由 ClassSchedule 侧实现。
- [市场签名和发行规范](docs/market.md)。目录源码位于 [market/index.json](market/index.json)，应用读取 [目录 Release](https://github.com/xialuo0511/Shijian-Plugin-Market/releases/download/market-index/index.json)。
- [社区投稿](CONTRIBUTING.md)：源码、许可、数据访问说明、包与校验值，经审核入库；社区插件使用单独来源标识。

时笺不提供账号登录信息存储。MCP / 插件状态和配对凭据由用户选择临时使用或 Windows 用户级加密保存，支持删除，默认备份不包含这些秘密数据。

本仓库的插件示例、SDK 与文档采用 MIT 许可。Windows 应用二进制及随附第三方组件以应用包内说明和第三方许可为准。
