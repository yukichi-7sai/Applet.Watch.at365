param([string]$AppDockDirectory)
$ErrorActionPreference = 'Stop'
$taskAppletRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if (-not $AppDockDirectory) {
    $taskConfig = Join-Path $taskAppletRoot 'deploy.local.txt'
    if (Test-Path -LiteralPath $taskConfig -PathType Leaf) { $AppDockDirectory = Get-Content -LiteralPath $taskConfig -Encoding UTF8 -TotalCount 1 }
}
if ([string]::IsNullOrWhiteSpace($AppDockDirectory)) { throw 'Pass an AppDock directory or create deploy.local.txt.' }
$taskHostDirectory = [System.IO.Path]::GetFullPath($AppDockDirectory)
if (-not (Test-Path -LiteralPath (Join-Path $taskHostDirectory 'AppDock.at365.exe') -PathType Leaf)) { throw 'AppDock.at365.exeがあるフォルダーを指定してください。' }
$taskDestination = Join-Path $taskHostDirectory 'extensions\Applet.Watch.at365'
$taskSource = Join-Path $taskAppletRoot 'Applet.Watch.at365'
if (-not (Test-Path -LiteralPath (Join-Path $taskSource 'Applet.Watch.at365.exe') -PathType Leaf)) { throw '先にscripts\publish.ps1でビルドしてください。' }
New-Item -ItemType Directory -Path $taskDestination -Force | Out-Null
foreach ($taskFile in @('Applet.Watch.at365.exe', 'extension.json')) { Copy-Item -LiteralPath (Join-Path $taskSource $taskFile) -Destination (Join-Path $taskDestination $taskFile) -Force }
Write-Output "Appletを配置しました: $taskDestination"
Write-Output 'AppDockを起動し直し、Applet.Watch.at365を有効にしてください。'
