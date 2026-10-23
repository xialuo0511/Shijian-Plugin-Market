# GitHub 插件市场与官方认证

市场仓库：https://github.com/xialuo0511/Shijian-Plugin-Market

时笺“插件与 MCP → 插件市场”手动读取 `market-index` Release 的 `index.json`，下载该仓库 Release 中的 `.sjplugin`。目录源码保存在 `main/market/index.json`，维护时同步上传到目录 Release，避免依赖原始文件域名。目录最多 200 条，当前契约 API v1。首批官方插件为学习工具箱、周计划手记；安装后仍需用户启用。

## 认证含义

蓝色认证符号和“时笺官方认证 · 签名已验证”仅在 ECDSA P-256 / SHA-256 签名通过时显示。签名绑定插件身份、版本、名称、描述、API、权限、归档 SHA-256 和清单 SHA-256。安装后同时核对载荷哈希；修改 DLL、清单、权限或安装证明都不能沿用官方标识。`official: true` 等自报字段不起作用。

这是由时笺团队签发的来源认证，不是 GitHub、微软或第三方安全认证。社区条目没有官方签名，显示社区标识。本地手动安装的包也显示未验证来源；需从市场安装，才能验证市场中的签名证明。

公钥 SPKI Base64：

```text
MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEqzGkzB6BcdRVfNWpZtI3ajzNGtSxY9wBxrY5J/R2qF9wpZ5aS/UDxQ0BP3x0ujBFhDnhXn0Xwz73gs54txTvOg==
```

规范签名载荷为以下字符串数组，使用 .NET `System.Text.Json` 默认编码规则序列化为无 BOM、无空白的 UTF-8 JSON；哈希转大写，权限按 Ordinal 排序后用英文逗号连接：

```text
["shijian-plugin-v1",id,version,name,description,apiVersion十进制,permissions逗号串,archiveSha256,manifestSha256]
```

签名采用 `IeeeP1363FixedFieldConcatenation` 64 字节格式，再做 Base64。完整实现见 `OfficialPlugins.SignaturePayload`。私钥仅保留在维护者本机 `.local/market-signing-key.pem`，不进入市场、SDK 或应用包。更换信任公钥须发布新版应用。目录和下载失败时保留已安装插件，不会自动执行下载内容。

## 维护与投稿

维护者在本地打包后用 `tools/Shijian.MarketTool index <私钥路径> <index.json> <release-tag> <插件包...>` 签发；将包上传对应 Release，再合并目录。已发布版本保持不可变，改动包文件应递增插件版本。不要重新生成生产密钥来处理普通更新。

第三方提交源码、许可、功能与数据访问说明、权限清单和 `.sjplugin`；维护者审核后托管到市场 Release，并加入空 `officialSignature` 的条目。社区入库不授予官方认证。仅我们编写并维护的插件使用官方私钥签发。

## 应用更新

当前应用为 0.1 Beta，标签为 `app-v0.1.0-beta.3`，GitHub 标记为 Pre-release；推荐可选择目录的 `Shijian-0.1.0-beta.3-win-x64-Install-r2.exe`，便携包为 `Shijian-0.1.0-beta.3-win-x64.zip`，均附 `.sha256`。插件和 SDK 的 v1 版本独立于应用版本。

Beta 客户端接受后续 `-beta.N` 及正式版；正式版客户端忽略 Beta。相同基础版本的正式版高于 Beta，Beta 编号按数字比较。主路径检查最近 100 条 Release，跳过插件和草稿。匿名 API 限流或服务不可用时，读取公开 Atom feed；该路径按命名约定识别通道，因此预览版必须使用 `-beta.N`，其他预览使用 `-alpha`、`-rc` 等后缀，插件使用 `plugins-` 前缀。

便携版只下载和核对 ZIP 哈希，由用户退出旧版后解压。Beta.3 起提供安装版，使用固定 `app-updates` Release 分发差分/完整包及 `releases.win-beta.json`；更新需用户确认重启，程序与学习数据分目录，升级和卸载保留数据。详见 [安装与增量更新](installation-updates.md)。
