# Applet.Watch.at365 開発ガイド

機能の説明・操作方法・設定・既定値・動作環境は[機能・操作・設定](docs/usage.md)を参照してください。共通の文書構成は[ドキュメント方針](../AppDock.at365/docs/documentation.md)に従います。

## 発行・配置ファイルの保存先

標準発行先は `A:\40.ARTIFACT\Applet.Watch.at365` です。`publish.bat`はソースrepo、`deploy.bat`は発行先から実行します。配置用ファイル・設定の保持とテンプレートの更新は[共通発行手順](../AppDock.at365/docs/publishing.md)を参照してください。

実装時のテスト選択、コミット前の必要回帰、リリース前のコミット/プッシュ確認と検証証跡の再利用は、[本体・Applet共通手順](../AppDock.at365/docs/development-workflow.md)に従います。この文書の試験コマンドは、その段階に応じて実行します。

インストールとビルドの概要は[README.md](README.md)、実測結果と未確認事項は[VERIFICATION.md](VERIFICATION.md)を参照してください。

## 必要な環境

Windows x64、.NET 10 SDK、隣接するAppDock.at365のソースが必要です。

## ビルド・配置

```powershell
dotnet build Applet.Watch.at365.slnx -c Release
dotnet run --project Applet.Watch.RegressionTests -c Release
.\publish.bat
A:\40.ARTIFACT\Applet.Watch.at365\deploy.bat "C:\Tools\AppDock.at365"
```

`publish.bat`の第1引数には任意の発行先、`deploy.bat`の第1引数にはAppDock.at365.exeがあるフォルダーを指定できます。配置先は引数、`deploy.local.txt`の先頭行の順です。未指定ならエラーになります。実利用先への配置時はAppDockを完全終了してください。

発行物は`A:/40.ARTIFACT/Applet.Watch.at365/Applet.Watch.at365/`に生成します。配置に必要なファイルは次の2つです。

```text
extensions/Applet.Watch.at365/
├─ extension.json
└─ Applet.Watch.at365.exe
```

AppletはWPFで時計を描画するself-contained単一EXEです。AppDockはAppletの起動・停止・設定・コマンドを管理し、時計の描画、透過、クリック透過、モニター配置はApplet自身が担当します。配布先PCに.NETランタイムは不要です。

## 検証

```powershell
node scripts/test-ui.cjs
node scripts/test-ui.cjs ../../40.ARTIFACT/AppDock.at365/win-unpacked/AppDock.at365.exe
```

UIテストは隔離profileへApplet EXEを配置し、時計と日付の独立表示・サイズ・位置・書式、フォント選択と代替表示、設定反映・コマンド・終了を検証します。Nodeはホストのtoolchain.jsonに従う`.tools/node/<version>/node.exe`を使います。実利用のAppDock設定、他のプロセス、壁紙、マウス位置を変更しません。検証範囲と未確認項目は[VERIFICATION.md](VERIFICATION.md)を参照してください。

時計と日付は同じ`ClockWindow`を用途別に2つ生成し、表示・タイマー・DPI・モニター配置を個別に管理します。`ClockOptions`とmanifestで既定値を揃え、`visible`は時計、`showDate`は日付の独立した表示を保存します。フォントと不透明度は共有し、秒のサイズは時・分と独立しています。両WindowとSystemEvents購読はApplet停止時に解放します。日付書式は`ClockFormatting`で宣言済み候補を処理します。

Appletのnative実装とホストとの接続は、[AppDockのApplet実装ガイド](../AppDock.at365/docs/applet-development.md)を参照してください。

## 公開設定

外部公開項目はextension.jsonのsettings[].automationで宣言します。取得とrevision付き更新は[共通操作API](../AppDock.at365/docs/automation.md)を使い、Applet固有のAPIや本体側の許可一覧は追加しません。booleanのON/OFF/toggleはgenerateCommandsで明示生成します。設定反映は既存Settings.OnChangedを共用します。

GUI試験にscripts/settings-mcp-check.cjsによる隔離MCP確認を含みます。公開schema・読取り・更新、非公開項目/不正値の拒否、書込許可、dryRun、古いrevisionの拒否と、各Applet実プロセスへの反映を確認します。実利用Codex設定やモデルは使用しません。

## 設定画面の構成

`extension.json`の`settingSections`で用途別のカードを宣言します。描画・余白・入力・検索・保存は[本体の共通テンプレート](../AppDock.at365/docs/extensions.md#共通の設定ページ)へ委譲し、Applet固有の設定フォームやCSSを複製しません。フォント選択操作の`settingKey: fontFamily`で入力とボタンを同じFieldへ配置し、表示切り替えの説明には`[command:...]`を使います。設定キー・既定値・実行処理は変更しません。配置は`placement: below`、見た目は`appearance: filled`を宣言し、常に入力欄の下へ標準の塗りつぶしボタンを置きます。最低本体版は0.105.8です。

## 共通の運用

文書構成・生成物・版更新・配布は[共通手順](../AppDock.at365/docs/development-workflow.md)、[文書方針](../AppDock.at365/docs/documentation.md)、[発行と配置](../AppDock.at365/docs/publishing.md)、[更新パッケージ](../AppDock.at365/docs/updates.md)、[生成物の整理](../AppDock.at365/DEVELOPMENT.md#作業完了時のテストフォルダー整理)を参照する。Applet固有の実行条件と結果は[検証記録](VERIFICATION.md)へ記載する。

## フォント参照

フォントはWindowsのFonts.SystemFontFamiliesから確認し、保存したfontFamilyを名前で解決します。既定値は空文字で、空欄は正常なSegoe UIの既定表示として扱います。見つからない場合も通知せずSegoe UIで描画し、保存値は置換しません。選択は共通の文字列設定とWindowsのFontDialogから行い、128件の動的select候補上限に依存しません。通知capabilityと通知種類は宣言しません。

Resources/Hatten.ttfは.gitignore対象として手元に保持し、ビルドのNone/Resource/Contentへ含めません。新規checkoutやTTF不在でもビルド・発行できます。ソース・EXE・ZIPから再配布せず、過去Git履歴の扱いは検証記録を参照してください。
