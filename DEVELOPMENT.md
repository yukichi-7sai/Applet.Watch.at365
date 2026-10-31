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

`extension.json`の`settingSections`で用途別のカードを宣言します。描画・余白・入力・検索・保存は[本体の共通テンプレート](../AppDock.at365/docs/extensions.md#共通の設定ページ)へ委譲し、Applet固有の設定フォームやCSSを複製しません。フォント選択操作の`settingKey: fontFamily`で入力とボタンを同じFieldへ配置し、表示切り替えの説明には`[command:...]`を使います。設定キー・既定値・実行処理は変更しません。配置は`placement: below`、見た目は`appearance: filled`を宣言し、常に入力欄の下へ標準の塗りつぶしボタンを置きます。最低本体版はdebugログ対応に合わせて0.109.2です。

## 共通の運用

文書構成・生成物・版更新・配布は[共通手順](../AppDock.at365/docs/development-workflow.md)、[文書方針](../AppDock.at365/docs/documentation.md)、[発行と配置](../AppDock.at365/docs/publishing.md)、[更新パッケージ](../AppDock.at365/docs/updates.md)、[生成物の整理](../AppDock.at365/DEVELOPMENT.md#作業完了時のテストフォルダー整理)を参照する。Applet固有の実行条件と結果は[検証記録](VERIFICATION.md)へ記載する。

## フォント参照

フォントはWindowsのFonts.SystemFontFamiliesから確認し、保存したfontFamilyを名前で解決します。既定値は空文字で、空欄は正常なSegoe UIの既定表示として扱います。見つからない場合も通知せずSegoe UIで描画し、保存値は置換しません。選択は共通の文字列設定とWindowsのFontDialogから行い、128件の動的select候補上限に依存しません。通知capabilityと通知種類は宣言しません。

Resources/Hatten.ttfは.gitignore対象として手元に保持し、ビルドのNone/Resource/Contentへ含めません。新規checkoutやTTF不在でもビルド・発行できます。ソース・EXE・ZIPから再配布せず、過去Git履歴の扱いは検証記録を参照してください。

## 起動タイムアウトの診断

起動ごとに共通Log APIへ`[Watch起動診断]`をdebugで記録します。AppDockのログで提供元をWatch、レベルをDebugにして、失敗した起動の`attempt`が同じ行を確認してください。通常のInfo以上の表示では隠れますが、ファイルへの記録は継続します。分類と保存は[本体のログ仕様](../AppDock.at365/docs/logging.md)を参照してください。debug対応の最低本体版は0.109.2です。新しい設定や独自の保存ファイルは追加しません。設定変更や毎秒の描画では出力しません。

`stage`はこれから実行する処理、`elapsedMs`はActivateAsync開始からの経過時間、`previousStage`と`previousMs`は直前の段階とそこに費やした時間です。`managedMainMs`は管理コードのMain開始からの経過時間で、EXEの展開や.NETランタイムの起動前処理は含みません。`utc`は計測時刻で、送信待ちがあるとホストの受信時刻とは異なります。計測は単調増加時計を使います。

| stage | 処理 |
| --- | --- |
| `activate.received` | 起動要求を受信して初期化を開始 |
| `ui.dispatch` / `settings.read` | UIスレッドの受付待ち / 保存済み設定の読み取り |
| `clock.create` / `date.create` | 時計 / 日付のWPF画面作成 |
| `clock.fonts` / `date.fonts` | インストール済みフォントの確認と選択 |
| `*.handle` / `*.text` | ネイティブWindowの準備 / 時刻テキストの設定 |
| `*.layout` | モニター選択・文字計測・画面配置 |
| `*.show` / `*.hide` / `*.visible-layout` / `*.applied` | 表示 / 非表示 / 表示後の再配置 / その画面の適用完了 |
| `monitors.dispatch` / `monitors.enumerate` | UIスレッドの受付待ち / モニター一覧の取得 |
| `monitor.options` / `dateMonitor.options` | モニター候補をホストへ送信し、応答待ち |
| `events.subscribe` | 設定変更・画面構成変更の購読 |
| `state.dispatch` / `state.build` / `state.publish` | UIスレッドの受付待ち / 状態パネルの作成 / ホストへの送信・応答待ち |
| `activate.ready` / `activate.failed` | 初期化完了（起動応答送信前） / 例外発生（previousStageを確認） |

最後の段階の後にホストのタイムアウトがあれば、その段階を調査対象にします。ただしログ自体の通信遅延・欠落もあり、最後の行だけで停止場所を断定しません。最初の`activate.received`もない場合は、ActivateAsyncに到達する前の起動、またはログ通信の問題を含み、この計測だけでは区別できません。Windowsのロック状態はこのログから断定できません。

ログはUIスレッドから応答待ちをせず、最大64行の専用キューで順に送ります（現行の固定経路は64行未満）。ログ送信失敗は起動の失敗にせず、再送しません。起動処理の終了時は最大250msだけ排出を待ち、無応答なら送信待ちを取り消します。フォント名・設定値・ユーザー名・パスなどは出力しません。ホストの15秒期限、自動再試行の有無、起動遅延やWindows設定は変更しません。

`scripts/test-startup.cjs [Watch EXE]`で、非表示/表示起動、ホスト応答待ち中の段階到達、起動失敗の直前段階、ログ送信の拒否/無応答、通常設定変更でログが増えないことを隔離検証します。共通ログへの実際の到達は既存の固定EXE UI試験でも確認します。実サインイン・ロック状態での再現は別の実機確認です。
