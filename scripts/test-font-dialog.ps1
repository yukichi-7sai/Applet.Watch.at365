param([Parameter(Mandatory)][int]$WatchProcessId, [ValidateSet('cancel', 'choose')][string]$Action = 'cancel')
$ErrorActionPreference = 'Stop'
Add-Type @'
using System;
using System.Runtime.InteropServices;
using System.Text;
public static class WatchFontDialogProbe {
    public delegate bool EnumProc(IntPtr window, IntPtr parameter);
    [DllImport("user32.dll")] public static extern bool EnumWindows(EnumProc callback, IntPtr parameter);
    [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr window, out uint process);
    [DllImport("user32.dll", CharSet=CharSet.Unicode)] public static extern int GetClassName(IntPtr window, StringBuilder text, int size);
    [DllImport("user32.dll")] public static extern IntPtr GetDlgItem(IntPtr window, int id);
    [DllImport("user32.dll", CharSet=CharSet.Unicode)] public static extern IntPtr SendMessage(IntPtr window, int message, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll", CharSet=CharSet.Unicode)] public static extern IntPtr SendMessage(IntPtr window, int message, IntPtr wParam, string text);
    public static IntPtr Find(uint process) {
        IntPtr result = IntPtr.Zero;
        EnumWindows((window, parameter) => {
            uint owner; GetWindowThreadProcessId(window, out owner);
            if (owner != process) return true;
            var name = new StringBuilder(128); GetClassName(window, name, name.Capacity);
            if (name.ToString() != "#32770") return true;
            result = window; return false;
        }, IntPtr.Zero);
        return result;
    }
}
'@
$taskDeadline = [DateTime]::UtcNow.AddSeconds(15)
$taskWindow = [IntPtr]::Zero
while ($taskWindow -eq [IntPtr]::Zero -and [DateTime]::UtcNow -lt $taskDeadline) {
    $taskWindow = [WatchFontDialogProbe]::Find($WatchProcessId)
    if ($taskWindow -eq [IntPtr]::Zero) { Start-Sleep -Milliseconds 80 }
}
if ($taskWindow -eq [IntPtr]::Zero) { throw 'The test process font dialog was not found.' }
if ($Action -eq 'choose') {
    $taskFamily = [WatchFontDialogProbe]::GetDlgItem($taskWindow, 0x470)
    if ($taskFamily -eq [IntPtr]::Zero) { throw 'Font family control was not found.' }
    $taskChoice = [WatchFontDialogProbe]::SendMessage($taskFamily, 0x014D, [IntPtr](-1), 'Arial')
    if ($taskChoice.ToInt64() -lt 0) { throw 'Arial is not available in the test font dialog.' }
    [WatchFontDialogProbe]::SendMessage($taskWindow, 0x0111, [IntPtr](0x470 + (1 -shl 16)), $taskFamily) | Out-Null
    [WatchFontDialogProbe]::SendMessage($taskWindow, 0x0111, [IntPtr]1, [IntPtr]::Zero) | Out-Null
} else {
    [WatchFontDialogProbe]::SendMessage($taskWindow, 0x0111, [IntPtr]2, [IntPtr]::Zero) | Out-Null
}
Write-Output "Font dialog $Action verified for test process $WatchProcessId."
