using System.Globalization;
using System.Text;
using System.Text.Json;
using Shijian.Plugin.Abstractions;

namespace Shijian.StudyTools;

public sealed class StudyToolsPlugin : IShijianPlugin
{
    public IReadOnlyList<PluginCommand> GetCommands() =>
    [
        new("spaced-review", "生成间隔复习计划", "按 1、3、7、14 天安排四轮复习，保留截止日期独立设置。", "{\"topic\":\"极限与连续\",\"minutes\":25}"),
        new("export-tasks", "导出学习任务 CSV", "把有效任务、截止日期和成果进度导出，兼容电子表格。")
    ];
    public Task<PluginResult> ExecuteAsync(string commandId, PluginContext context, JsonElement arguments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var runs = context.State is { ValueKind: JsonValueKind.Object } state && state.TryGetProperty("runs", out var count) && count.TryGetInt32(out var n) ? n : 0;
        var result = new PluginResult { NewState = JsonSerializer.SerializeToElement(new { runs = runs + 1, lastCommand = commandId }) };
        if (commandId == "spaced-review")
        {
            var topic = arguments.TryGetProperty("topic", out var t) ? t.GetString()?.Trim() ?? "" : "复习主题";
            var minutes = arguments.TryGetProperty("minutes", out var m) ? m.GetInt32() : 25;
            if (string.IsNullOrWhiteSpace(topic) || topic.Length > 200 || minutes is < 1 or > 180) throw new ArgumentException("主题应为 1 至 200 字，每轮专注为 1 至 180 分钟。");
            var projectId = arguments.TryGetProperty("projectId", out var p) ? p.GetString() : null;
            if (string.IsNullOrWhiteSpace(projectId)) projectId = null;
            var offsets = new[] { 1, 3, 7, 14 };
            for (var i = 0; i < offsets.Length; i++) result.TasksToCreate.Add(new()
            {
                Title = $"{topic} · 第 {i + 1} 轮复习", ProjectId = projectId, Stage = "间隔复习", PlannedDate = context.Today.AddDays(offsets[i]), EstimateMinutes = minutes,
                Notes = "先尝试主动回忆，再对照笔记，最后记录仍不熟悉的地方。", Subtasks = ["主动回忆知识点", "检查并补全遗漏", "记录本轮薄弱点"]
            });
            result.Summary = $"为「{topic}」生成 4 轮复习，计划执行日期已安排；截止日期留空，由你独立设置。";
        }
        else if (commandId == "export-tasks")
        {
            var csv = new StringBuilder("任务,项目,阶段,状态,计划执行,截止日期,成果进度\r\n");
            foreach (var task in context.Tasks)
            {
                var project = context.Projects.FirstOrDefault(p => p.Id == task.ProjectId)?.Name ?? "个人待办";
                csv.AppendLine(string.Join(',', new[] { task.Title, project, task.Stage, task.State, task.PlannedDate?.ToString("yyyy-MM-dd") ?? "", task.DueDate?.ToString("yyyy-MM-dd") ?? "", task.Progress.ToString("P0", CultureInfo.InvariantCulture) }.Select(Cell)));
            }
            result.Export = new($"时笺学习任务-{context.Today:yyyyMMdd}.csv", csv.ToString(), "text/csv");
            result.Summary = $"已准备 {context.Tasks.Count} 条任务的 CSV，选择文件位置后保存。";
        }
        else throw new ArgumentException("未知命令。");
        return Task.FromResult(result);
    }
    private static string Cell(string value)
    {
        if (value.TrimStart().StartsWith('=') || value.TrimStart().StartsWith('+') || value.TrimStart().StartsWith('-') || value.TrimStart().StartsWith('@')) value = "'" + value;
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
