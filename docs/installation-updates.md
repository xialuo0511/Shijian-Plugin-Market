# 安装与增量更新

应用版本 0.1.0-beta.3，更新通道 `win-beta`，安装标识 `Shijian.Desktop`。使用固定版本 Velopack 1.2.161；不使用托管 Flow 服务或账号凭据。

## 使用

下载 `Shijian.Desktop-win-beta-Setup.exe`，安装到当前用户目录，提供桌面、开始菜单及系统卸载入口。程序目录 `%LOCALAPPDATA%/Shijian.Desktop` 与数据 `%LOCALAPPDATA%/Shijian` 分开。升级和卸载保留数据库、插件及用户选择保存的加密凭据。

便携版首次迁移需要完整 Setup。先退出旧版再安装；默认数据位置相同，无需手动搬数据库。安装版在设置中手动检查更新，确认下载后校验差分包、重建完整载荷；不可用时回退完整包。再次确认后保存专注、断开扩展，正常关闭并由更新器替换文件和重新启动。取消安装确认会保留下载缓存，下次可继续。

## 更新目录

默认公开目录：`https://github.com/xialuo0511/Shijian-Plugin-Market/releases/download/app-updates/`。客户端读取 `releases.win-beta.json`，从同目录下载版本命名的 `.nupkg`。不调用 GitHub 匿名 API，不需要登录信息。

设置支持替换为公开 HTTPS 目录，或本机/私有局域网 HTTP。目录必须直接返回文件；网盘分享落地页不能直接作为更新源，需要具有固定直链的目录服务。地址不接受用户名密码、查询参数和片段；应用没有更新登录凭据存储。

## 维护者构建

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Build.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Pack-Installer.ps1
```

`dotnet-tools.json` 固定 vpk 版本。每次构建先递增 `Directory.Build.props` 中的 `ShijianVersion`。保留上次发布的完整 `.nupkg` 在 `artifacts/installer/`；本次打包自动生成差分包。若换机/CI，从上次 Release 下载已发布的完整包作为基线再打包，禁止重新构造既有版本的基线。首次安装版没有上一安装基线，因此 Beta.3 只发布完整包；之后才生成生产差分。

发布步骤：

1. 在版本 Release 发布 Setup、完整包、差分包（若生成）、`releases.win-beta.json` 及校验文件，保留便携 ZIP。
2. 把相同版本命名的完整/差分包上传到固定 `app-updates` Release，核对 GitHub 返回的 SHA-256 后，最后更新其 `releases.win-beta.json`。版本包不可覆盖重建；更新目录只允许在新资产齐备后切换索引。
3. 固定目录保留索引引用的历史差分和完整包，支持跨多个版本升级。清理历史前先按仍支持的基线验证；不要删除当前索引引用的资产。
4. 用真实客户端匿名读取公开 feed，确认当前版本无更新、旧版获得正确版本和包标识。预览通道的正式包仍须发布到同一通道；不要把其他应用包混入目录。

安装程序检测 VC++ x64 运行库，缺少时下载微软官方运行库；应用和 .NET/WinUI 已自包含。当前没有 Windows 代码签名证书。安装测试在本机当前用户下执行，不代表全新系统和所有系统策略均已验证。

## 验证

`tests/Shijian.InstallerChecks` 在隔离目录中移走目标完整包，验证只能通过差分重建，并逐文件核对目标包内全部文件的 SHA-256；ZIP 重压缩元数据无需相同。另损坏差分包，验证自动回退完整包且归档 SHA-256 相同。测试基线是专用 QA 标识的合成版本，不能当作已发布便携版支持直接增量升级的证据。

`scripts/Verify-Installer.ps1` 仅接受 `Shijian.QA.*` 标识，实际安装、启动、升级、再次启动及卸载，复查外部 SQLite 任务与 Windows 用户级加密凭据。所有测试进程使用独立 `SHIJIAN_DATA_DIR`，只正常关闭安装目录中的进程。

参考：[Velopack 应用集成](https://docs.velopack.io/integrating/overview)、[差分包构建](https://docs.velopack.io/packaging/deltas)。
