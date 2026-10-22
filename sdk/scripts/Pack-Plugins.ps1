param([string]$Configuration='Release')
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path -Parent $PSScriptRoot
$taskOutput = Join-Path $taskRoot 'artifacts\plugins'
New-Item -ItemType Directory -Path $taskOutput -Force | Out-Null
Add-Type -AssemblyName System.IO.Compression.FileSystem
$taskPlugins = @(
    @{Project='Shijian.StudyTools';Type='StudyToolsPlugin';File='StudyTools';Id='org.shijian.study-tools';Name='学习工具箱';Description='间隔复习任务与安全 CSV 导出';Permissions=@('tasks.read','projects.read','tasks.write','files.export','state.read','state.write')},
    @{Project='Shijian.WeeklyNotes';Type='WeeklyNotesPlugin';File='WeeklyNotes';Id='org.shijian.weekly-notes';Name='周计划手记';Description='将本周任务、成果进度、临近截止事项与复盘问题整理成 Markdown';Permissions=@('tasks.read','projects.read','files.export')}
)
Push-Location -LiteralPath $taskRoot
try {
    foreach ($taskPlugin in $taskPlugins) {
        dotnet build (Join-Path 'examples' ($taskPlugin.Project + '/' + $taskPlugin.Project + '.csproj')) -c $Configuration --nologo
        if ($LASTEXITCODE -ne 0) { throw 'Plugin build failed.' }
        $taskDll = Join-Path $taskRoot ('examples\' + $taskPlugin.Project + '\bin\' + $Configuration + '\net10.0\' + $taskPlugin.Project + '.dll')
        $taskStage = Join-Path $taskRoot ('.local\plugin-package-' + [Guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $taskStage -Force | Out-Null
        Copy-Item -LiteralPath $taskDll -Destination $taskStage
        $taskLicense = Join-Path $taskRoot 'LICENSE'
        if (-not (Test-Path -LiteralPath $taskLicense)) { $taskLicense = Join-Path $taskRoot 'marketplace/LICENSE' }
        Copy-Item -LiteralPath $taskLicense -Destination (Join-Path $taskStage 'LICENSE.txt')
        $taskManifest = [ordered]@{id=$taskPlugin.Id;name=$taskPlugin.Name;version='1.0.0';description=$taskPlugin.Description;apiVersion=1;entryAssembly=($taskPlugin.Project + '.dll');entryType=($taskPlugin.Project + '.' + $taskPlugin.Type);permissions=$taskPlugin.Permissions;files=@{}}
        $taskManifest.files[($taskPlugin.Project + '.dll')] = (Get-FileHash -LiteralPath $taskDll -Algorithm SHA256).Hash
        $taskManifest.files['LICENSE.txt'] = (Get-FileHash -LiteralPath (Join-Path $taskStage 'LICENSE.txt') -Algorithm SHA256).Hash
        [IO.File]::WriteAllText((Join-Path $taskStage 'plugin.json'),($taskManifest | ConvertTo-Json -Depth 5),(New-Object Text.UTF8Encoding($false)))
        $taskPackage = Join-Path $taskOutput ($taskPlugin.File + '-1.0.0.sjplugin')
        $taskTempZip = $taskStage + '.zip'
        [IO.Compression.ZipFile]::CreateFromDirectory($taskStage,$taskTempZip)
        Move-Item -LiteralPath $taskTempZip -Destination $taskPackage -Force
        Write-Output $taskPackage
    }
} finally { Pop-Location }
