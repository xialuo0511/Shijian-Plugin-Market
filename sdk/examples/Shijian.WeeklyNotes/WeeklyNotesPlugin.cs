using System.Text;
using System.Text.Json;
using Shijian.Plugin.Abstractions;

namespace Shijian.WeeklyNotes;

public sealed class WeeklyNotesPlugin : IShijianPlugin
{
    public IReadOnlyList<PluginCommand> GetCommands() => [new("weekly-notes", "导出本周计划", "把本周安排、成果进度和未完成的临近截止事项整理成 Markdown，附复盘问题。")];
    public Task<PluginResult> ExecuteAsync(string commandId, PluginContext context, JsonElement arguments, CancellationToken cancellationToken)
    {
        if (commandId != "weekly-notes") throw new ArgumentException("未知命令。");
        cancellationToken.ThrowIfCancellationRequested();
        var monday = context.Today.AddDays(-((int)context.Today.DayOfWeek + 6) % 7);
        var end = monday.AddDays(6);
        var selected = context.Tasks.Where(t => t.PlannedDate >= monday && t.PlannedDate <= end).OrderBy(t => t.PlannedDate).ThenBy(t => t.Title).ToArray();
        var text = new StringBuilder($"# 本周学习计划\n\n{monday:yyyy-MM-dd} — {end:yyyy-MM-dd}\n\n");
        foreach (var day in selected.GroupBy(t => t.PlannedDate))
        {
            text.AppendLine($"## {day.Key:MM-dd}\n");
            foreach (var task in day)
            {
                var project = context.Projects.FirstOrDefault(p => p.Id == task.ProjectId)?.Name ?? "个人待办";
                text.AppendLine($"- {Clean(task.Title)} · {Clean(project)} · 成果 {task.Progress:P0}" + (task.DueDate is { } due ? $" · 截止 {due:MM-dd}" : ""));
            }
            text.AppendLine();
        }
        if (selected.Length == 0) text.AppendLine("本周尚未安排任务。\n");
        text.AppendLine("## 未完成的临近截止事项\n");
        foreach (var task in context.Tasks.Where(t => t.Progress < 1 && t.DueDate <= end).OrderBy(t => t.DueDate)) text.AppendLine($"- {Clean(task.Title)} · {task.DueDate:MM-dd} · {task.Progress:P0}");
        text.AppendLine("\n## 我的复盘\n\n- 本周推进了什么？\n- 哪一步比预期困难？\n- 下周可以缩小成哪一个行动？\n");
        return Task.FromResult(new PluginResult { Summary = $"已整理本周安排的 {selected.Length} 个任务；不修改任务或成果进度。", Export = new($"时笺周计划-{monday:yyyyMMdd}.md", text.ToString(), "text/markdown") });
    }
    private static string Clean(string value) => value.Replace("\r", " ").Replace("\n", " ").Replace("<", "&lt;").Replace(">", "&gt;")
        .Replace("\\", "\\\\").Replace("[", "\\[").Replace("]", "\\]").Replace("*", "\\*").Replace("`", "\\`").Replace("_", "\\_");
}
