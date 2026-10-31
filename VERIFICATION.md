# 検証記録

## 2026-10-31: 起動診断0.5.5のコミット・プッシュ前照合

- 診断ログ実装の12ファイルを対象に確認。mainとGitHub origin/mainはfetch後に一致し、別作業の変更なし。製品の追加修正は不要で、0.5.5と発行物を維持する。
- 実装時のソース/試験/ビルド定義/共有SDK・Runtime/固定ツール定義/ホスト試験入力302ファイルのSHA256が一致。Release build、標準回帰24件、発行済み実プロセス起動6条件、固定AppDock GUI15項目、ZIP/payload照合を成功ログと結果JSONから再利用した。現在のWatch EXE/ZIP/feed/manifestと固定ホストEXEのハッシュも一致。未実行の必要回帰や影響を受けた成功結果はなく、追加試験・版更新・再発行は不要。
- 証跡は`.artifacts/commit-validation-startup-diagnostics-20261031/`の`reuse.json`、`stage.json`、`commit.json`、`push.json`。今回の検証記録追記だけを製品入力の照合から除外し、stage/commit treeは一致を確認する。実Windows起動・ARSO・ロック中の再現は引き続き未確認。実利用先deploy・GitHub Releaseは対象外。

## 2026-10-31: 起動段階の診断ログ（0.5.5）

- Watchのmain・追跡先・clean状態を確認し、`pull --ff-only`で最新確認後に実装。起動要求受信、UI受付、時計/日付画面作成、フォント、Window/文字/配置/表示、モニター候補送信、イベント購読、状態パネル送信、完了/失敗を、共通Log APIへinfoで記録する。各行に起動単位のID、段階、単調増加時計の経過時間と直前段階の時間、Main開始からの時間、計測UTCを含む。
- 診断キューは64行で、固定の起動経路はこの範囲内。UIはログ応答を待たず、終了時の排出待ちは最大250ms。ログ拒否・無応答は再送せず、元の起動結果を保持する。設定値・フォント名・ユーザー名・パス・生の例外は新規ログへ出さない。設定変更や毎秒の描画では診断ログを出さない。
- 発行前に実装差分と影響先をレビューし、阻害指摘なし。時計/日付の独立表示、フォント代替と保存、既存ホスト通信、15秒期限と起動待ち設定を維持。設定・保存ファイル・公開API・依存の変更なしのためパッチ更新。AppDock本体・SDK/Runtimeの製品ソースは変更なし。
- Release buildは警告0/エラー0。標準回帰24/24成功（既存21＋診断ログの順序/時刻、送信失敗、無応答の排出期限3件）。開発EXEと発行済みEXEでそれぞれ起動試験6条件が成功：非表示、両表示、モニター候補送信の応答待ち中に段階ログ到達、起動失敗時の直前段階、ログ送信拒否、ログ無応答。設定反映でログが増えないことと、設定値をログに含めないことも確認。
- Watch 0.5.5を既定の40.ARTIFACTへpublish成功。固定AppDock 0.109.1との隔離GUI15項目成功。Watchのsource/infoで共通ログへ最初から完了まで届くこと、既存の表示/時計更新/クリック透過/非活性、設定確定・Escape取消・再起動保持、フォント選択の確定/取消、MCP公開15項目と非公開モニター2項目、停止/終了を確認。新しいログの確認以外は既存UI試験を利用した。
- manifest/EXE/feedの0.5.5とZIP全2ファイルのサイズ/SHA256が一致。EXE SHA256: `42d9c57c57249e3b0e740d793ea46d316132677faccb7ffd762e748f8effa1b5`、ZIP SHA256: `bed6027501e15fd694979fcb9ee7d4cd71eaae30eb2512f68019bf2743150290`。既存の配置スクリプト/ローカル配置先ファイルはハッシュ不変。
- 証跡は`.artifacts/startup-diagnostics-20261031/`の`review.json`、`build.log`、`regression.log`、`startup-development.log`、`publish.log`、`startup-published.log`、`ui-published.log`、`package-result.json`、`inputs.json`、`validation.json`。段階ログは開発`.artifacts/startup-protocol-1793424752628/`、発行済み`.artifacts/startup-protocol-1793424888090/`の各条件フォルダーへ保存。固定GUIは`.artifacts/independent-display-ui-1793424893426/`の`result.json`と`startup-log.json`。
- 整理は終了/成功/パス/プロセス/再解析ポイントを確認し、旧成功GUI `independent-display-ui-1793337532502`の本体管理対象692ファイルだけ削除。記録109ファイルのハッシュ一致と最新成功コピーの保持を確認。失敗試行・過去の参照中検証記録・起動試験2回を保持。`cleanup.json`参照。
- 実Windows起動・ARSO・ロック画面でのタイムアウト再現、物理複数画面/混在DPIは未確認。ロック状態自体は記録しない。Main以前のEXE展開/.NET起動と、最初の診断が未到達の場合はこのログだけで原因を特定できない。今回の変更は原因調査用で、タイムアウトの解消を確認したものではない。本体全回帰は本体無変更のため未実施。実利用先deploy、commit、push、GitHub Releaseは行わない。

## 2026-10-30: フォントボタン0.5.4のコミット・プッシュ照合

- 利用者の動作確認後、本体とWatchのmainをコミット・GitHubへプッシュする範囲で確認した。Watchのorigin/mainとHEADは一致し、今回の6ファイルだけを対象にする。
- 実装時のソース・試験・共有依存871入力、ネイティブ21件の対象29入力、固定GUI14群/MCPの結果・EXEと発行ZIP/payloadを照合し、成功を再利用した。本体の共通UIと宣言検証も再利用/追加回帰を合わせて必要範囲が成功。製品の追加修正は不要で、確認済み0.5.4を維持し再発行しない。
- 証跡は`../AppDock.at365/.artifacts/commit-validation-action-presentation-20261030/`のreuse.json、validation.json、stage.json、commits.json、push.json。発行時の物理複数モニター/混在DPIの未確認範囲は継続。既存証跡を保持し、GitHub Release・実利用先deployは行わない。

## 2026-10-30: フォント選択ボタンの下段配置・塗りつぶし（0.5.4）

- main `8ce78e6`から、fontFamilyに結合した設定操作へ`placement: below`と`appearance: filled`を指定。共通Field内で入力欄の下へ置き、ライトでは灰色にする。文字ボタンを維持し、Applet固有のCSSやC#処理は追加しない。最低本体版を0.105.8へ揃えた。設定項目・既定値・保存ファイルの増減がないためパッチ更新。
- 本体側と合わせて発行前レビュー済み。共通部品の余白と既存文字ボタンへの影響を修正後、型検査/build・関連50件・開発GUI7群成功。本体0.105.8とWatch0.5.4のpublish成功。ネイティブ標準21件はC#/SDK/runtime等29入力のSHA256一致を確認して既存成功を再利用した。
- 固定EXEの隔離試験14群成功。広幅/760幅で同じField内の入力欄の下にボタンが配置され、実画面の灰色面を確認。Windowsフォント選択のクリック/キーボード、確定/取消、入力の先行保存、再起動保持、未導入フォントの無通知代替、個別表示と両表示既定OFFを確認した。MCP15公開項目・モニター2項目の非公開・不正値拒否・書込み許可・dryRun・revisionと実プロセス反映も成功。MCP仕様の変更なし。
- 初回試験は以前の通知画面を前提に閉じた「読み上げ」の表示を待機して停止。現行画面の準備条件へ試験だけ修正し、同じ発行版で再検証成功。初回失敗記録を保持する。物理複数モニター/混在DPIは今回未検証、Pause/F13〜F24のOS登録・送信なし。
- 証跡は`../AppDock.at365/.artifacts/action-presentation-20261030/`のreview.json、native-reuse.json、inputs-final.json、publish-watch.log、watch-published-final.log、packages.json、validation.json。GUIは`.artifacts/independent-display-ui-1793337532502/result.json`。ZIP内2ファイルと発行payload、manifest/minimumHostVersion、feed/版/サイズ/SHA256を照合済み。
- 旧成功GUI3件の本体ランタイムコピー2,076ファイルだけを整理。結果・画像・設定・manifest等323ファイルはSHA256不変、最新成功コピーは保持。共通整理の詳細は本体側cleanup.json。実利用先deploy、commit、push、Releaseは行っていない。

## 2026-10-30: フォント項目内操作0.5.3のコミット前照合

- 利用者の確認後、mainの対象6ファイルと実装時22入力、本体845入力、両配布物695ファイル/ZIPをSHA256で照合。固定AppDock0.105.2とのGUI14群・MCP公開15項目の検証、C#/SDK依存29入力が同じ標準回帰21件を再利用した。本体の宣言読み込み/設定検索に依存する未実施65ファイル731件も成功。製品変更・版更新・再発行なし。
- 共通証跡は`../AppDock.at365/.artifacts/commit-validation-inline-settings-20261030/`。本repoの`stage.json`・`commits.json`・`push.json`でstage/commit treeとGitHub origin/mainを照合する。既存成功GUI3回と再利用記録を保持し、削除0。GitHub Release・実利用deployは対象外。

## 2026-10-30: フォント項目内の選択操作と表示コマンドリンク 0.5.3

- main clean/追跡先確認とpull --ff-only後に、フォント選択操作へ`settingKey: fontFamily`を追加。入力とボタンを同じFieldへ配置し、狭幅は折り返す。時計/日付の表示descriptionへ、それぞれの生成済みtoggleコマンドのリンクを追加した。リンクは対象を選んだパレットを開き、明示実行まで値を変えない。本体共通拡張を利用し、Watch固有フォーム/CSSやネイティブ処理は追加しない。
- 発行前レビューで本体0.105.2の契約、設定/コマンドID/既定値/保存/MCPの不変、フォント選択の実行経路を確認し阻害事項なし。表示metadataと説明のみの変更として0.5.2→0.5.3、minimumHostVersionも0.105.2へ更新した。C#・回帰コード・SDK/Runtimeの29入力が先行検証時のSHA256と一致し、標準回帰21件を再利用。模擬RPCの全再試験は行わず、発行EXEと実ホストのGUIで今回影響する操作を検証した。
- publish.bat終了コード0。ソース/配布/ZIPのmanifest、minimumHostVersion、EXE ProductVersion、feedが一致し、ZIPサイズ/SHA256と全2ファイルの一致、deploy.batの保持を確認。配布先は`A:\40.ARTIFACT\Applet.Watch.at365`。
- 固定AppDock0.105.2との隔離GUI14群成功。同じFieldに入力と選択ボタンが各1つ存在し、操作の検索識別子も維持。入力のblur確定→選択の取消、処理中無効化、キーボードで選択→Arial適用、Escape取消と再起動保持を確認。両descriptionのリンクは各toggle IDだけを選び、表示状態を変えず、Escapeでリンクへフォーカス復帰する。広幅/760pxの枠内配置と折り返しをPNGで目視確認した。
- 両表示の初期OFF、個別表示/サイズ/モニター/位置/余白/日付書式、未導入フォントの無通知代替と保存値保持、通知項目なし、再起動/停止も成功。MCP公開15項目のschema/読取/変更、モニター2項目の非公開、不正値/書込不許可の拒否、dryRun、古いrevision拒否、生成コマンドと実プロセスへの反映を再確認した。MCPの仕様変更なし。
- 証跡は`.artifacts/inline-font-actions-20261030/`の`reuse.json`、`inputs-build.json`、`inputs-after.json`、`publish.log`、`ui-published.log`、`validation.json`。GUIは`.artifacts/independent-display-ui-1793308040928/result.json`、本体レビュー/配布照合は`../AppDock.at365/.artifacts/inline-setting-actions-20261030/`。成功GUIの直近3回を保持し削除0。物理複数モニター/混在DPI等は今回の検証対象外。Pause/F13〜F24を登録/送信せず、実利用設定、commit/push/Release/deployは変更しない。

## 2026-10-30: 個別表示設定0.5.2のコミット前照合

- 利用者による動作確認後、Watchのmainだけをコミット・GitHubへプッシュする範囲で確認した。origin取得後のHEADとorigin/mainは同一で、別作業のstage内容はなかった。
- 実装時の31入力、共有依存、完全ログ14件、固定AppDock 0.105.0の隔離EXE、検証済みWatch EXE/manifest、発行ZIP/feedをSHA256・版・サイズへ照合して一致。Releaseビルド、標準回帰21/21、発行EXEのRPC/フォントダイアログ、GUI13群/MCP、配布物確認の成功を再利用した。製品の追加変更がないため、版更新・再発行・同じ試験の再実行は不要と判断した。
- 差分は今回のWatchの設定・描画・関連試験・文書に限定され、検証範囲の不足やコミットを妨げる指摘はない。物理複数モニター・混在DPI等の未確認範囲は実装時の記録を引き継ぐ。GitHub Releaseと実利用先deployは対象外。
- 再利用判定は`.artifacts/commit-validation-independent-display-20261030/reuse.json`、stage/commit treeとGitHub origin/mainの照合は同じ場所の`stage.json`・`result.json`へ記録する。実装時の検証資料を保持する。

## 2026-10-30: 時計・秒・日付の個別設定（0.5.2）

- Watchのmainがcleanでorigin/mainを追跡することを確認し、`pull --ff-only`で最新取得後に実装。本体では別作業の変更が進行しているため、その製品ソースは変更していない。通常環境でのpullはAlready up to date。制限環境のDNS失敗・診断なしの.NETビルド失敗は、同じ処理を通常のWindows環境で再実行して解消した。
- 時計と日付を別々のWindowへ分離。表示・モニター・上下左右の6配置・左右/上下の余白・文字サイズを個別化し、時計・日付の表示はともに既定OFF。既存の保存キー・表示コマンドIDと明示的な保存値は維持。秒の文字サイズも追加し、既定は時計420・秒210・日付210。フォントと不透明度は共通。日付書式は表示例付きの6候補を用意した。
- 共通設定フォームは5グループ。共通の見た目を「不透明度 → フォント → フォントを選ぶ」の順にし、フォント名の直下へ操作を配置した。同じ入力項目内へのボタン配置に必要な本体拡張は今回の範囲に含めない。由来を説明する表記は設定説明・状態パネル・現行ガイドから除去した。
- 発行前レビューで設定定義と17項目の配置、個別Windowの停止・DPI・モニター代替、設定/コマンド互換、フォント代替と取消、通知なし、共有依存への影響を確認。状態パネルの既存表示/非表示操作も保持した。設定追加のため0.4.1→0.5.0とし、発行後の描画記録補正と配置調整は0.5.1、検証PNGの右端を含む記録範囲補正は0.5.2として発行した。後者の画像処理はテスト用環境変数指定時だけ動作する。
- 最終Releaseビルドは警告0・エラー0。標準回帰21/21成功。最終差分で回帰対象のOptions/Layout/Fonts/Formatting・回帰コード・SDK入力のSHA256が不変であることを照合して成功を再利用した。空設定の両方OFF、明示ONの維持、各サイズと配置の独立、負座標/DPI換算、未接続モニター、書式と日付境界を含む。
- 発行EXEの模擬ホストRPCは成功。両方OFFでタイマー停止、日付だけ/時計だけ/両方の表示、サイズ変更、6方向それぞれの実コンテンツ座標、モニター候補の2項目公開、フォント代替・標準FontDialogのOK/取消、通知API呼出し0、正常終了を確認した。
- 固定AppDock 0.105.0の隔離コピーでGUI13群成功。両方の初期OFF、独立したON/OFFと時計/秒/日付サイズ、日付書式・位置・余白・モニター選択、保存と再起動復元、フォント入力のEscape取消、直下のボタンからの標準ダイアログ起動/取消、未導入フォントの保存値保持、760px幅で操作へ到達できること、15項目のMCP公開/型検証/権限/dryRun/revision競合、Applet無効化とホスト終了を確認。広幅・狭幅の画面と時計/日付の描画PNGを目視確認した。
- 最終publishの終了コード0。ソース/配置/ZIPのmanifest、EXE ProductVersion、feedが0.5.2で一致。ZIP全2ファイルのサイズ/SHA256、既存deploy.bat/配置スクリプトの保持を確認。検証前後の全記録入力に差分なし。配布先は`A:\40.ARTIFACT\Applet.Watch.at365`。
- 証跡は`.artifacts/independent-display-20261030/`の`inputs-final.json`・`review-final.json`・`build-052.log`・`regression.log`・`protocol-published-052.log`・`ui-published-052.log`・`package-result.json`・`validation.json`・`log-hashes.json`。最終GUIは`.artifacts/independent-display-ui-1793306710803/result.json`。先行試行のログも区別して保持。試験方式別の最新3回を残す整理結果は`cleanup.json`を参照する。GUI2回と既存再利用証跡は保持した。
- 実機は3840×2160・DPI144の1モニター。物理的な別モニター同時表示、混在DPI、モニター抜き差し、スリープ/RDPは未確認。Pause/F13〜F24の送信・OS登録は行っていない。実利用設定・他Applet・本体製品ソースの変更、commit/push、GitHub Release、実利用先deployは行っていない。

## 2026-10-29: 情報metadataのコミット前照合

- icon/taglineと版の変更、配置manifest/entry、更新ZIP/feedを先行発行時の入力とSHA256へ照合し一致。本体0.101.9の固定EXEでも既存8Appletの情報表示を確認した。実装入力は不変で、先行の発行・表示試験を再利用し、追加の版更新・再発行は行わない。
- 共通レビューと再利用判断は`../AppDock.at365/.artifacts/commit-validation-applet-information-20261029/{review,reuse,validation}.json`、stage/commitとGitHub origin/mainの照合結果は同じ場所の`stage.json`・`commits.json`・`push.json`を参照する。先行の検証資料は保持。実サービス・実利用のApplet起動・GitHub Release・実利用deployは今回の検証対象外。

上記照合時の対象版は`0.4.1`。以下は記録した条件での検証結果であり、外部サービスや実機条件の現在の動作保証とは区別する。

## 2026-10-29: Applet情報のアイコンとキャッチコピー 0.4.1

- main/追跡先とcleanを確認し、通常環境のpull --ff-only後に、manifestの任意表示情報 `icon: clock` と `tagline: いつもの画面に、見やすい時計を。` を設定。設定・コマンド・保存・権限・最低本体版は変更せず、0.4.0→0.4.1のパッチ更新として関連する版の正本を揃えた。発行前にmetadataと版の差分をレビューし、阻害事項なし。
- 標準publish.batで40.ARTIFACTへ発行。配置manifest/ZIP内manifest/feedのID・版・icon/tagline、ZIPのサイズ/SHA256を照合し成功。native/.NETはProductVersionも一致。本体0.101.6の固定EXEへ発行物をコピーし、停止状態で情報パネルの表示を確認した。通常のApplet起動や外部サービス・実入力は実行していない。
- 共通の入力・完全ログ・配布照合は`../AppDock.at365/.artifacts/applet-branding-20261029/`、本体の`VERIFICATION.md`、本repoの`.artifacts/applet-branding-20261029/validation.json`を参照。機能別の標準全回帰は今回実行せず、表示情報と配布の影響に必要な範囲を検証した。既存の再利用資料を保持し、今回の整理による削除なし。コミット/プッシュ/Release/実利用deployは行っていない。

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
