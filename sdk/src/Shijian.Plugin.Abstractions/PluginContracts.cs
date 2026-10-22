using System.Text.Json;

namespace Shijian.Plugin.Abstractions;

public static class PluginApi
{
    public const int Version = 1;
    public const string ReadTasks = "tasks.read";
    public const string ReadProjects = "projects.read";
    public const string WriteTasks = "tasks.write";
    public const string ExportFiles = "files.export";
    public const string ReadState = "state.read";
    public const string WriteState = "state.write";
    public static readonly string[] Permissions = [ReadTasks, ReadProjects, WriteTasks, ExportFiles, ReadState, WriteState];
}

public interface IShijianPlugin
{
    IReadOnlyList<PluginCommand> GetCommands();
    Task<PluginResult> ExecuteAsync(string commandId, PluginContext context, JsonElement arguments, CancellationToken cancellationToken);
}

public sealed record PluginCommand(string Id, string Name, string Description, string ExampleArguments = "{}");
public sealed class PluginManifest
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Version { get; set; } = "1.0.0";
    public string Description { get; set; } = "";
    public int ApiVersion { get; set; } = PluginApi.Version;
    public string EntryAssembly { get; set; } = "";
    public string EntryType { get; set; } = "";
    public List<string> Permissions { get; set; } = [];
    public Dictionary<string, string> Files { get; set; } = [];
}
public sealed class PluginContext
{
    public DateOnly Today { get; set; }
    public List<PluginTaskSnapshot> Tasks { get; set; } = [];
    public List<PluginProjectSnapshot> Projects { get; set; } = [];
    public JsonElement? State { get; set; }
}
public sealed record PluginTaskSnapshot(string Id, string Title, string? ProjectId, string Stage, string Notes, DateOnly? PlannedDate, DateOnly? DueDate, string State, int Current, int Target, string Unit, double Progress);
public sealed record PluginProjectSnapshot(string Id, string Name, string Description);
public sealed class TaskDraft
{
    public string Title { get; set; } = "";
    public string? ProjectId { get; set; }
    public string Stage { get; set; } = "插件生成";
    public string Notes { get; set; } = "";
    public DateOnly? PlannedDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public int EstimateMinutes { get; set; } = 25;
    public string ProgressMode { get; set; } = "checklist";
    public int Target { get; set; } = 1;
    public string Unit { get; set; } = "次";
    public List<string> Subtasks { get; set; } = [];
}
public sealed record PluginExport(string FileName, string Content, string ContentType = "text/plain");
public sealed class PluginResult
{
    public string Summary { get; set; } = "";
    public List<TaskDraft> TasksToCreate { get; set; } = [];
    public PluginExport? Export { get; set; }
    public JsonElement? NewState { get; set; }
}
public sealed class PluginHostRequest
{
    public string ManifestPath { get; set; } = "";
    public string Operation { get; set; } = "describe";
    public string? CommandId { get; set; }
    public PluginContext Context { get; set; } = new();
    public JsonElement Arguments { get; set; } = JsonSerializer.SerializeToElement(new { });
}
public sealed class PluginHostResponse
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public List<PluginCommand> Commands { get; set; } = [];
    public PluginResult? Result { get; set; }
}
