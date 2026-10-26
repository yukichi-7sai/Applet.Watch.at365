param([string]$OutputDirectory)
$ErrorActionPreference = 'Stop'
$taskAppletRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
. ([IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\AppDock.at365\scripts\publish-paths.ps1')))
$taskArtifact = Get-PublishDirectory $taskAppletRoot
$taskPublishDirectory = if ($OutputDirectory) { [System.IO.Path]::GetFullPath($OutputDirectory) } else { Join-Path $taskArtifact 'Applet.Watch.at365' }
dotnet publish (Join-Path $taskAppletRoot 'Applet.Watch\Applet.Watch.csproj') -c Release -r win-x64 --self-contained true -o $taskPublishDirectory
if ($LASTEXITCODE -ne 0) { throw "Clock Applet publish failed ($LASTEXITCODE)" }
Copy-Item -LiteralPath (Join-Path $taskAppletRoot 'extension.json') -Destination (Join-Path $taskPublishDirectory 'extension.json') -Force
foreach ($taskSymbols in @('AppDock.Runtime.pdb', 'AppDock.SDK.pdb')) {
    $taskSymbolFile = Join-Path $taskPublishDirectory $taskSymbols
    if (Test-Path -LiteralPath $taskSymbolFile -PathType Leaf) { Remove-Item -LiteralPath $taskSymbolFile }
}
Write-Output "Applet output: $taskPublishDirectory"

# Match the existing deploy payload; remove obsolete build outputs before creating the ZIP.
foreach ($taskOldName in @('AppDock.SDK.dll', 'AppDock.SDK.pdb', 'AppDock.Runtime.dll', 'Applet.Watch.at365.dll', 'Applet.Watch.at365.deps.json', 'Applet.Watch.at365.runtimeconfig.json', 'Applet.Watch.at365.pdb', 'Resources\Hatten.ttf')) {
    $taskOldFile = Join-Path $taskPublishDirectory $taskOldName
    if (Test-Path -LiteralPath $taskOldFile -PathType Leaf) { Remove-Item -LiteralPath $taskOldFile }
}

$taskUpdateHostRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\AppDock.at365'))
& (Join-Path $taskUpdateHostRoot 'scripts\pack-applet-update.ps1') -SourceDirectory $taskPublishDirectory -OutputDirectory $taskArtifact

Install-PublishedDeployment $taskAppletRoot $taskArtifact
