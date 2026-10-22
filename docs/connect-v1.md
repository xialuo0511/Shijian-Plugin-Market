# 时笺 Connect v1

在时笺“课程表联动 → 手机插件直连”中手动开启。默认端口 19082；手机需启用局域网选项并使用电脑的局域网 IPv4 地址，例如 `http://192.168.1.20:19082`。服务随时笺关闭而停止，不自动修改防火墙。绑定局域网前请由用户在系统中允许应用连接。

除健康检查外，所有请求要求 `Authorization: Bearer <配对令牌>`。令牌默认仅本次有效，用户可选择 Windows 用户级加密保存，并可清除。服务不提供账号登录。带浏览器 `Origin` 请求头的请求会被拒绝，ClassSchedule 插件应通过其原生 HTTP Bridge 调用。

所有 POST 使用 `Content-Type: application/json`；课表 payload 最大 5 MB，总请求最大 6 MB。响应字段采用 camelCase。不记录请求内容或认证请求头。

## 健康检查

`GET /api/v1/health`

```json
{"app":"shijian","apiVersion":1,"version":"0.1.0-beta.2","pairingRequired":true}
```

## 预览课表同步

`POST /api/v1/course-sync/preview`

```json
{"payload":{"format":"classschedule-semester","formatVersion":1,"semester":{}}}
```

`payload` 为 `SemesterScheduleExporter` 完整原生导出 JSON；上面的空 semester 仅表示嵌入位置。也支持 `classschedule-all-semesters`。返回：

```json
{"previewId":"32位标识","expiresInSeconds":300,"contentSha256":"...","semesters":[{"key":"semester","name":"秋季学期","startDate":"2026-09-01","weeks":20,"courseCount":5,"occurrenceCount":90}],"warning":"..."}
```

预览只校验和暂存，不写学习数据。最多保留 20 份待确认预览。界面应明确说明：同名、同开始日学期的旧导入课次将被替换；当前 Android v1 JSON 不含单次调课/停课。

## 确认提交

`POST /api/v1/course-sync/commit`

```json
{"previewId":"预览返回的标识","commitId":"客户端生成的UUID","semesterKeys":["semester"]}
```

成功返回 `committed:true`、`commitId`、`semesterCount`、`occurrenceCount`。网络失败重试时沿用同一个 commitId；同一次服务运行内会返回已完成结果。课次标识还按学期、课程、安排和日期稳定生成，因此重新预览再导入也不会增加重复课次。

写入会在桌面规划线程中原子提交。若对应导入课表在预览后被修改，返回 409 `schedule_changed`，要求重新预览。只更新所选导入学期；个人任务、手工日程和其他课表保留。

## 今日计划（可选）

`GET /api/v1/today`

只有桌面用户勾选“允许已配对设备读取今日计划摘要”时可用，否则 403。返回日期与最多 100 条今日任务的 id、title、plannedDate、dueDate、state、progress、estimateMinutes；不包含备注与凭据。手机插件只读展示。

## 错误与保存

- 400：格式或学期选择错误；修正后重新预览。
- 401：配对令牌无效；提示用户重新配对。
- 403：浏览器 Origin 不被接受，或桌面未授权读取今日计划。
- 404：预览不存在或已过期；重新获取预览。
- 409：对应课表发生变化；重新预览并由用户确认。
- 413：请求过大；减少学期或课程范围。
- 429：待确认预览过多；稍后重试。

跨端配置可保留地址与用户选择；凭据仅按用户明确选择临时使用或在平台安全存储中保存。清除配置时同时清除凭据。禁止把学校登录凭据混入此协议。
