# 検証記録

対象の現行版は`0.4.0`。以下は記録した条件での検証結果であり、外部サービスや実機条件の現在の動作保証とは区別する。

## 2026-10-28: フォント案内通知の削除（0.4.0）

- Watchだけを変更し、`font-unavailable`の通知種類、`notifications` capability、起動・設定変更時の送信処理を削除した。capabilityも外し、本体による旧方式の通知種類の自動生成を防ぐ。本体・共通SDK/Runtimeのソース変更はない。
- フォント選択、空欄のSegoe UI表示、未導入時の代替表示と保存済みフォント名、状態パネルは維持した。通知設定項目の削除にあたるため0.3.2から0.4.0へマイナー更新。時計の設定キー・保存ファイルの追加や削除はない。
- 発行前レビューで通知宣言・実行経路の除去と互換通知への置換防止、設定・表示の維持を確認し、阻害指摘なし。Releaseビルドは警告・エラー0、標準回帰12/12成功。廃止した通知の重複抑止・解消等の5検査とfixtureを削除し、通知APIを一切呼ばないことを発行EXEのRPC検証で確認した。
- `publish.bat`は終了コード0。ソース/配布manifest・EXE・feedの0.4.0一致、ZIP全2ファイルのサイズ/SHA256、既存配置スクリプトの保持を確認。発行EXEのRPCでは空欄・未導入フォントの反映、通知API呼出し0、実FontDialogのOK/取消、正常終了が成功した。
- 固定AppDock 0.99.21の隔離コピーでGUI9群成功。通知設定にWatchのフォント案内/互換通知がないこと、フォント入力・保存・Escape取消、未導入名の保持と再起動後の代替表示、既存9項目のMCP公開設定、表示切替・無効化・終了を確認した。GUI fixtureの本体コピーは既存`copyHost`を使い、フォルダー型配布の同梱物も保持する。
- 初回GUIは旧テストの「fontFamilyは非公開」という期待値で失敗した。現行本体は`automation:false`等のない対応文字列設定を公開するため、製品の公開定義を変えず、テストを既存9項目・フォント値の型検証へ更新して再実行し成功。失敗ログと成功ログを分けて保持する。
- 証跡は`.artifacts/remove-font-notice-20261028/`のreview.json、build.log、regression.log、publish.log、published-protocol.log、published-ui-attempt1.log、published-ui.log、package-result.json、validation.json。最終GUIは`.artifacts/installed-fonts-ui-1793198154841/result.json`。コミット時は製品入力・共有依存・発行物を照合して今回の成功を再利用し、同じ検証を重複実行しない。
- 当該GUI方式の確認済み成功は1件のため削除0件。失敗・参照中の証跡を保持。Pause/F13〜F24の送信・OS登録、実利用先deploy、GitHub Releaseは行っていない。実利用環境への反映は発行物を配置した後に確認する。

## 2026-10-28: 取り消し版のコミット照合

- 取り消し版0.3.2の全31入力、完全ログのSHA256、発行物全2ファイル・ZIP/feedを実装時の証跡と照合し、一致を確認。Releaseビルド、標準回帰17/17、発行EXEのRPC、固定本体での接続・表示切替・正常終了の成功を再利用する。製品の追加修正・版更新・再発行はない。
- コミット対象は版番号と取り消し記録だけで、起動コードと開発文書は試行前のまま。本体の並行作業は対象外。必要範囲に不足はなく、製品テストの再実行は行わない。
- 証跡: `.artifacts/commit-validation-app-id-rollback-20261028/`のreuse.json、stage.json、result.json。stage treeとcommit treeを照合し、GitHub origin/mainへ通常pushした結果とリモート先端の一致を記録する。Release・実利用deployは行わない。

## 2026-10-28: AppUserModelIDの試行を取り消し（0.3.2）

- 0.3.1で本体と共通のAppUserModelIDを設定したが、利用者の実機ではタスクマネージャーの統合表示に効果がなかったため、依頼に従って取り消した。Program.csとDEVELOPMENT.mdは試行前のmain/5145b46とGit blob一致。設定・保存形式・公開API・終了管理は変更していない。
- 発行済み試行版より新しい取り消し版として0.3.2へパッチ更新。発行前レビューで識別子設定・P/Invoke・追加説明の除去と既存処理の復元を確認し、阻害指摘なし。
- Releaseビルドは警告・エラー0、既存回帰17/17成功。発行EXEの既存RPC試験（フォント・通知・実FontDialogのOK/取消・終了）、固定AppDock 0.99.16の隔離コピーで接続・表示ON/OFF・正常終了が成功。Pause/F13〜F24の送信・登録なし。
- publish.batは終了コード0。EXE/manifest/feedの版一致、ZIP全2ファイルのサイズ/SHA256、既存配置スクリプト保持を確認。取り消し版の実利用deployは行っていない。
- 証跡: `.artifacts/process-app-id-rollback-20261028/`のreview.json、build.log、regression.log、publish.log、published-protocol.log、published-host-result.json、package-result.json、validation.json。試行時の登録API検証と未達結果は`.artifacts/process-app-id-20261028/`に保持する。今回の方式の成功は2回で削除0件。
- Appletの検証前後の入力は文書追記以外一致。本体では並行する画像関連9ファイルと新規試験の変更を検出して保持した。SDK/Runtime・ビルド依存・固定配布物に変更はなく、今回の固定EXE試験には影響しない。本作業による本体ソース変更・commit・push・Releaseなし。

## 2026-10-26: 全変更Appletのローカルコミット照合

- 空欄既定／Segoe UIと公式情報操作削除を含む0.3.0。今回の17回帰・発行EXEの実FontDialog／RPC・固定GUI7群／MCP成功を入力hashと配布物で照合して再利用。利用者追加のLICENSEを含む現ソースを、新しいmainの初回commit対象とする。TTFは追跡せずローカル保持。
- 各repoの実装時入力一覧と現在のソースを照合。host toolchain／lockfile／SDK／Runtimeは一致し、package.jsonのdependencies／devDependencies／scripts／packageManager／enginesも一致。UI側は実装時から更新されているため、最新の固定EXEで4Appletの表示名／版・起動停止を追加検証し成功。.artifacts/commit-validation-applets-20261026/reuse.jsonへ試験単位の判断を記録。
- 全4件のfeed／ZIP hashを確認。新たな製品修正はなく、現発行版を維持する。新GUI証跡は../Applet.WebBrowserTools.at365/.artifacts/commit-validation-applets-20261026/のpublished-smoke.log、smoke-result.json。設定は隔離profileのみで、既定ショートカット初期化をfixtureで抑止し、Pause／F13～F24の送信・OS登録なし。
- commit前後の入力・stage tree／commit treeとcommit SHAは各.artifacts/commit-validation-applets-20261026/へ保存する。今回の差分を1repoずつローカルcommitし、push・Release・実利用deployは行わない。旧検証資料は参照中として保持、削除0件。
## 2026-10-26: フォント既定値を空欄へ変更・公式情報操作の削除（0.3.0）

- フォントの既定値を空文字へ変更し、共通設定で空欄を保存できるようminLengthを0に変更。空欄は正常なSegoe UIの既定表示で、未導入通知を発行しない。未導入の明示指定から空欄へ戻した場合は元の案内を解消する。保存済みの明示的なフォント名は維持。
- 公式情報の設定アクション・宣言済み操作コマンド・runtime登録・通知内アクションと、不要になったbrowser capability／URLを削除。割り当て可能な宣言済みコマンドの削除としてマイナー更新0.3.0を選択。フォント選択・サイズ等の既存設定は維持。
- レビューで阻害指摘なし。標準回帰17/17、発行EXEの模擬RPC（空欄時の通知なし／明示指定の未導入・重複抑止／空欄での解消／削除コマンド拒否／実FontDialogのOK・キャンセル）、固定AppDockでの隔離GUI7群と既存MCP公開設定8項目が成功。空欄の入力・保存／既定値省略・実描画・再起動を確認。
- publish.batは終了コード0。EXE／sourceと配布manifest／feedは0.3.0で一致、ZIPの全ファイル・サイズ・SHA256を照合。TTFは埋め込まれず、ローカルの元TTFはignore対象として保持。発行物を変更する追加実装はない。
- 最初のRPC起動はpnpmのworkspace照合で停止。AppDock側の変更後の状態をinstall --frozen-lockfileで同期し、固定lockfile・依存版・供給網ポリシーを維持してRPC／GUIを成功させた。失敗記録と成功記録を区別する。
- 証跡: .artifacts/font-default-empty-20261026/のreview.json、inputs-before.json／inputs-after.json、regression.log、publish.log、protocol-final.log、ui.log、package-result.json。GUI: .artifacts/installed-fonts-ui-1793021549776/result.json。終了済みの当該方式の成功3件を保持し、既存・診断資料の削除0件。キーの送信／Pause・F13～F24のOS登録なし。
- 利用者がGitを再初期化した状態を尊重し、今回の実装で再初期化・commitは行わなかった。実装検証時の既存stage内容は一致を確認。後続の全Appletコミット依頼開始時はmain初回commit前・全ソース未追跡であり、別途その初回commitを作成する。
## 2026-10-26: インストール済みフォント参照・選択設定とGit再作成（0.2.1）

- Hatten.ttfのWPF Resource指定とpack URIを除去。ローカルTTFは元のSHA256のまま保持し、.gitignoreへ追加、Git追跡から除外。フォント名はWindowsのインストール済みファミリーと照合し、未導入時はSegoe UIで表示する。保存済みの選択は保持する。
- fontFamily設定を追加し、共通入力のEnter／blur確定・Escape取消とWindows FontDialogのOK／キャンセルに対応。サイズ等の既存設定は維持。既定のHaettenschweiler、代替表示、他PCでの選択保持、再起動後復元を確認。新しい設定と通知設定の追加により0.1.6→0.2.0とし、発行後に見つかった即時反映の修正を0.2.1として再発行。
- 未導入時の案内は共通の種類指定通知で発行。同じ未導入フォントについて設定変更のたびに再発行せず、利用可能なフォントへ変えると終了。Haettenschweilerの公式情報を開くコマンドと設定アクションを追加。現在利用できるMicrosoft公式の単体無料ダウンロード先は確認できず、リンクは公式情報ページとして表示する。自動取得やフォントのインストールは行わない。
- 発行前レビューで、動的select候補が本体APIで128件までの制約を確認。全インストール済みフォントを扱えるよう、共通の文字列設定と標準FontDialogに修正し、本体／SDKは変更しない。発行EXEの実ダイアログ試験で自己設定の保存後にローカルキャッシュが先に更新されるため描画が旧値のままになることを検出。成功した保存の直後にApplyAsyncを実行する修正を行い、0.2.1のOK／キャンセル・即時反映・正常終了を再検証した。
- 標準回帰15/15成功。フォント解決・未導入時の保存値保持・入力正規化・案内の重複抑止／解消／抑止を含む。追加修正はClockAppletの自己保存後の反映と版更新だけであり、試験対象のClockOptions／ClockFonts／ClockLayoutと回帰本体・SDKは不変のため15件を再利用。発行ビルド、発行EXEの模擬RPC（通知・公式URL・標準FontDialogの実OK／キャンセル）、固定AppDock 0.94.4での隔離GUI6群と既存MCP公開設定8項目の検査が成功。fontFamilyは外部公開しない。
- 0.2.1のEXE／配布manifest／feedの版一致、ZIP全2ファイルとサイズ・SHA256を確認。managed resourcesはclockwindow.bamlだけでTTF／OTFなし、ZIPにもフォントなし。TTFはローカルに保持し、新しいGitオブジェクトへ含めない。記録: .artifacts/installed-fonts-20261026/のreview.json、regression.log、publish-021.log、protocol-021.log、ui-021.log、package-021-result.json、host-inputs.json。GUI証跡: .artifacts/installed-fonts-ui-1793019286140/result.jsonとfont-settings.png。
- 初期のダイアログfixtureは文字入力だけでは選択確定しないため失敗。ComboBoxの選択と選択変更イベントへ修正し、その後の製品側の反映漏れとは区別して記録。旧ホーム／全体保存UI・フォント内蔵検査を使っていたtest-ui.cjsは、現行フォーム・固定EXE・フォント切替・MCP・寿命確認の入口へ更新。Pause／F13～F24は送信・登録せず、fixtureで既定ショートカットの初期化済み状態を指定した。
- ローカルGitは利用者指定で旧履歴を引き継がず新規mainへ再作成。初回commitとfont blob不存在、リモートURL保持、作業ツリーcleanの結果は.artifacts/installed-fonts-20261026/git-reinitialization.jsonへ記録する。GitHub／AQUA側の削除・作成・pushは行わない。旧Gitは新規初回commitと作業ファイル保持の確認後に除去する。
- .artifacts直下を列挙し、今回の方式のGUI成功2件と照合・失敗調査ログを保持。古い実行の成功・参照・所有条件が確定しないため既存テスト資料は削除0件。実ブラウザーでの外部ページ表示、新規フォントのOSインストール、別PC／混在DPIは未確認。実利用先deploy／GitHub Releaseなし。
## 2026-10-26: 同梱フォントの再配布条件調査（製品変更なし）

- 対象は`Applet.Watch/Resources/Hatten.ttf`。nameテーブルからHaettenschweiler／Version 2.30／Eraman・Monotype・Microsoftの1995年著作権表記を確認。108,664 bytes、SHA256 `40E898E471FA4DE3CA09A6DFED961D00D6395AF20FE6CF1C6B83C795BEA04543`。ライセンス本文・URL用nameレコード13／14はなし。Microsoftの[フォント情報](https://learn.microsoft.com/en-us/typography/font-list/haettenschweiler)とファイル名・著作権表記が一致。
- csprojのResource指定とClockWindow.xamlのpack URIから、TTFをEXE内で利用する構成を確認。[WPF公式資料](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/packaging-fonts-with-applications)は、MicrosoftがアプリやWindowsに付属させるフォントを含め通常のライセンスではアプリ同梱・再配布できないこと、fsTypeは文書埋め込みの指定でありアプリ埋め込みの許諾ではないことを明記。このファイルのfsTypeは8であるが、アプリの再配布許可とは判断しない。
- [Microsoft FAQ](https://learn.microsoft.com/en-us/typography/fonts/font-faq)は文書埋め込みとアプリへの同梱を区別する。FAQの直接の適用範囲はWindows付属フォントのため、取得元不明の本ファイルにそのままEULAを特定する根拠にはしない。[Haettenschweiler販売元の条件](https://www.myfonts.com/collections/haettenschweiler-font-microsoft-corporation?tab=licensing)でもDesktopライセンスはアプリ同梱・フォント共有を認めていない。Appの販売区分はモバイル向けと案内されており、Windowsデスクトップアプリの同梱許諾を得られるかは別途確認が必要。
- 結論: 適用可能な追加の再配布許諾を保有していることを確認できない限り、TTFを含むソースやEXEをそのまま公開配布する根拠はない。取得元・適用EULA・追加契約の有無は未確認。対応候補は、同梱をやめて利用端末に適法に導入済みのフォントを参照する、再配布可能なフォントへ置換する、必要なソフトウェア再配布許諾を取得すること。
- 調査のみ。製品コード／フォント／ライセンス／版を変更せず、再発行・commit・push・公開もしない。根拠情報は`.artifacts/font-license-audit-20261026/findings.json`。AppDockのライセンス対応は別作業、NuGet非公開の.NETライセンスメタデータは変更不要との指定を受領。

## 2026-10-26: 表示名と発行（0.1.6）

- 既存のdisplayName変更を確認。表示名はWatch、内部name／id／entry・設定・公開APIは保持。開始時はmainで未コミット変更があったため、自動pull／stashは行わず既存差分を保持した。通常修正としてパッチ番号を1つ更新。
- 発行前にmanifest／版／発行スクリプトと影響先をレビューし、阻害指摘なし。レビュー記録は.artifacts/displayname-publish-20261026/review.json。
- Release構成の既存回帰8/8成功。発行EXEの版一致と、時計を非表示にした隔離ホストで起動／停止成功。
- publish.batは終了コード0。40.ARTIFACTの配布manifest／実行ファイル／update.jsonの版、ZIPの全エントリー・サイズ・SHA256と配布フォルダーの一致を確認。package-result.jsonに記録。既存deploy関連ファイルを保持。
- 固定の発行済みAppDock 0.94.4へ4つの発行物をそのままコピーし、コピーhash一致、snapshotの内部名／表示名／版と管理画面の4行を確認。Watch／WebBrowserTools／WindowsToolsの起動・停止、ホストの終了コード0を確認。共通証跡は../Applet.WebBrowserTools.at365/.artifacts/displayname-publish-20261026/のpublished-smoke.cjs、smoke-result.json、displaynames.png、host-inputs.json。参照ソースはAppDock main 0.94.3で、固定EXEは0.94.4。
- 各repoの同名.artifactsフォルダーへ入力hash、コマンド／終了コード、完全ログとそのhashを保存。検証後の製品入力は一致。使用方法と設計の版表記のみの文書修正は製品試験へ影響しない。ホスト全回帰／旧GUI全回帰／実ブラウザーや物理入力は変更範囲外として未実施。
- 整理は各.artifacts直下を列挙してcleanup.jsonへ記録。今回の試験は同じ方式の成功1回として保持。他の古い資料は参照・実行方式・所有が確定しないため保持、削除0件。隔離ホストとAppletは終了済み。commit／push／Release／実利用deployなし。
## 2026-10-19: GitHub更新元の新アカウント対応（0.1.5）

- 開始時main／clean、現在のorigin/mainからpull --ff-only成功（Already up to date）。更新元を`yukichi-7sai/Applet.Watch.at365`へ統一。既存updateRepositoryの旧アカウント名を置換。.NET発行ビルドとruntime/manifestの版一致を確認。

- 発行前レビューは指摘なし。設定項目・保存ファイル・公開APIの追加削除なしのためパッチ更新。既存publish.batで自身の40.ARTIFACT配下へ発行成功。ZIP/feed/同梱manifestのID・版・size・SHA256・runtimeの存在が一致。

- 最終AppDock 0.62.4 EXE（SHA256 043d72acd4f4176d0417bbc4166a111cecc5aaa7e8740baf21b8d6647b0e85ab）と最終Applet一式を隔離コピー。全6Appletを停止した状態で、保存済み旧指定の保持、新しい既定値の説明、版表示、「既定に戻す」後の画面／メモリ／settings.jsonの一致、停止状態維持を各Appletで確認。正常終了0。

- 証跡: `.artifacts/github-account-20261019/validation.json`。共通GUI/画像: `../AppDock.at365/.artifacts/applet-github-account-20261019/profile-1792345747763/`。入力hashは前後一致、hostはb27bd046b704e1a27ad31fdc2f481f80ea086b31。変更と無関係な全回帰・実アカウント・実Google受信・実GitHub Release取得・各機能の実Windows効果は未実施。今回と過去の成功/失敗・再利用資料を保持し削除0件。

- 本体のGitHub参照修正は先にb27bd04へコミット済み。今回のApplet変更は未コミット。push・Release・実利用先deployなし。
