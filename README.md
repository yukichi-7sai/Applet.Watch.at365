# Applet.Watch.at365

## インストール方法

[AppDock](../AppDock.at365/README.md)を完全終了し、配布物の `extension.json` を含むフォルダー一式を、EXE隣の `extensions/Applet.Watch.at365/` へ配置してください。AppDockを起動し、Applet管理で有効にすると利用できます。

## ビルド方法

Windows x64と.NET 10 SDK、隣の `../AppDock.at365` ソースが必要です。

本リポジトリのルートで実行します。

```powershell
dotnet build Applet.Watch.at365.slnx -c Release
.\publish.bat
```

出力先は `A:\40.ARTIFACT\Applet.Watch.at365` です。詳しい手順と機能・操作・設定の文書は[開発ガイド](DEVELOPMENT.md)を参照してください。
