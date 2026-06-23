# InputVisualizer
Safe Input Overlay for Windows

Windows向けのゲーム配信用入力可視化ツールです。  
配信中に `WASD`、マウスクリック、ゲームパッド入力などを視聴者へ見せることを目的とします。

このプロジェクトの最重要要件は、**パスワード・個人情報・自由入力テキストを誤って可視化しないこと**です。  
そのため、本ツールは「入力をすべて取得して危険なものだけ隠す」のではなく、**安全だと確認できた状況で、許可されたゲーム操作だけ表示する**設計を採用します。

---

## 目的

- ゲーム配信・録画で、プレイヤーの操作を視覚的に表示する
- キーボード、マウス、ゲームパッドの操作状態をオーバーレイ表示する
- パスワード入力やチャット入力など、秘密情報・個人情報の露出を防ぐ
- OBSなどの配信環境で扱いやすい表示形式を提供する

---

## 非目的

このツールは以下を目的としません。

- キーロガーの作成
- 文字入力の記録
- パスワードや入力文字列の解析
- ブラウザ、チャットアプリ、メール、ターミナルなどでの自由入力の可視化
- 入力履歴の保存、再生、送信
- ユーザーの同意なしのバックグラウンド監視

---

## 基本方針

### 1. Fail Closed

安全確認に失敗した場合は、表示を継続しません。  
次のような状態では、即座に入力表示を停止します。

- 許可済みゲームウィンドウが前面にない
- 前面ウィンドウを判定できない
- テキスト入力中の可能性がある
- パスワード欄または認証画面の可能性がある
- Alt+Tab、Windowsキー、UAC、ロック画面などでフォーカスが変わった
- ユーザーが緊急停止ホットキーを押した
- OBSや設定画面など、ゲーム以外がアクティブになった

### 2. Whitelist Only

危険なアプリをブラックリストで除外するのではなく、**許可したゲームだけを表示対象**にします。

例:

```yaml
allowed_processes:
  - "ApexLegends.exe"
  - "VALORANT-Win64-Shipping.exe"
  - "eldenring.exe"
```

前面プロセスがこのリストに含まれない場合、入力は表示しません。

### 3. Show Actions, Not Text

初期設定では、文字そのものを表示しません。  
表示対象はゲーム操作として意味のあるキーに限定します。

表示してよい例:

- `W`, `A`, `S`, `D`
- `Space`
- `Shift`
- `Ctrl`
- `Mouse1`
- `Mouse2`
- `Wheel`
- `Q`, `E`, `R`, `F` など、ユーザーが明示的に許可したキー
- ゲームパッドのボタン、スティック、トリガー

既定で表示しない例:

- 英数字の連続入力
- 記号
- `Backspace`
- `Enter` 前後の入力内容
- IME変換中の入力
- クリップボード貼り付け内容
- 文字列として復元できる入力履歴

### 4. No History, No Logs

本ツールは、入力内容を履歴として保存しません。

禁止事項:

- キー入力履歴の保存
- 直近のキー列のデバッグログ出力
- クラッシュレポートへの入力内容混入
- ネットワーク送信
- 文字列復元可能な形式での記録

許可されるログの例:

```text
[INFO] Overlay enabled
[INFO] Foreground process: eldenring.exe
[WARN] Overlay hidden: focus lost
[WARN] Overlay hidden: text input mode
```

キー名や入力列はログに書きません。

---

## 想定ユーザー

- ゲーム配信者
- eスポーツ解説者
- 動画制作者
- 入力操作を見せたいゲーム開発者
- トレーニング・チュートリアル動画の制作者

---

## 想定リスク

| リスク | 対策 |
|---|---|
| ログイン画面でパスワードが表示される | 許可済みゲーム以外では表示しない |
| ブラウザへAlt+Tabした際に入力が残る | フォーカス喪失時に即時非表示 |
| ゲーム内チャットで個人情報が出る | テキスト入力モードでは全非表示 |
| パスワード欄検出に失敗する | 検出に依存せず、ホワイトリスト方式を採用 |
| デバッグログにキー入力が残る | 入力ログを禁止 |
| OBSに危険な状態が映る | ステータス表示と緊急停止キーを実装 |
| 自動復帰で事故る | 危険状態後の再開は手動にする |

---

## 推奨アーキテクチャ

```text
+-------------------------+
| Input Capture Layer     |
| - Keyboard              |
| - Mouse                 |
| - Gamepad               |
+-----------+-------------+
            |
            v
+-------------------------+
| Safety Gate             |
| - Foreground app check  |
| - Whitelist check       |
| - Text input detection  |
| - Password UI detection |
| - Panic mode            |
+-----------+-------------+
            |
            v
+-------------------------+
| Key Filter              |
| - Allowed key map       |
| - Hide text keys        |
| - Normalize actions     |
+-----------+-------------+
            |
            v
+-------------------------+
| Overlay Renderer        |
| - Transparent window    |
| - OBS browser source    |
| - Local-only display    |
+-------------------------+
```

入力を取得した後、すぐに表示するのではなく、必ず `Safety Gate` と `Key Filter` を通します。

---

## Windows向け実装方針

### 入力取得

用途に応じて、次の方式を検討します。

#### 推奨: Raw Input

- キーボード・マウス入力を比較的低レベルで取得できる
- 前面ウィンドウ判定と組み合わせやすい
- グローバルな文字列記録ではなく、ゲーム操作状態の取得に使う

#### ゲームパッド入力

- XInput
- Windows.Gaming.Input
- SDL GameController API

ゲームパッドはパスワード入力と分離しやすいため、比較的安全に扱えます。

#### 注意: Low-Level Keyboard Hook

`WH_KEYBOARD_LL` などの低レベルフックは強力ですが、キーロガー的な実装になりやすいため注意が必要です。  
使用する場合でも、次を必須にします。

- 許可済みゲームが前面のときだけ表示に使う
- 入力履歴を保存しない
- 文字列化しない
- ログにキー名を書かない
- フォーカス喪失時に即時無効化
- 緊急停止ホットキーを常時優先する

---

## 安全判定

### Foreground Window Check

前面ウィンドウを取得し、対象プロセスが許可済みか確認します。

判定例:

```text
foreground_window = GetForegroundWindow()
process_id = GetWindowThreadProcessId(foreground_window)
process_name = QueryProcessName(process_id)

if process_name not in allowed_processes:
    hide_overlay()
```

この判定は定期的に行うだけでなく、入力イベントごとにも確認します。

### Focus Loss Handling

以下を検出した場合、即座に表示を停止します。

- 前面ウィンドウが変わった
- 対象ゲームが最小化された
- Alt+Tabが押された
- Windowsキーが押された
- UACやシステム画面が出た可能性がある
- セッションがロックされた
- モニター構成やデスクトップ状態が変わった

### Text Input Mode

ゲーム内チャット、名前入力、検索欄などでは、パスワードでなくても個人情報が出る可能性があります。  
そのため、テキスト入力中は全キーを非表示にします。

検出方法の例:

- ゲーム側APIやModからチャット状態を受け取る
- `Enter`, `T`, `/` などチャット開始キーの後に非表示へ移行
- `Esc` または再度 `Enter` まで非表示を継続
- ユーザーが手動で「テキスト入力中」を切り替えられるようにする

### Password UI Detection

補助的な防御として、Windows UI Automationなどで現在のフォーカス要素がパスワード欄か確認します。

ただし、これは完全ではありません。

- 独自UIのゲームランチャー
- Electronアプリ
- ブラウザ内フォーム
- 古いアプリ
- アクセシビリティ情報を正しく公開しないアプリ

では検出できない場合があります。

そのため、パスワード欄検出は**最後の保険**として扱い、主要な防御はホワイトリスト方式とテキスト入力非表示にします。

---

## 表示仕様

### 通常時

```text
W A S D
Mouse1 Mouse2
Shift Space
```

### 非表示時

非表示状態では、キーを出さず、状態だけ表示します。

```text
Input hidden: focus lost
Input hidden: text input mode
Input hidden: unsafe app
Input hidden: panic mode
```

### 危険状態後の復帰

安全上の理由により、危険状態からの自動復帰は既定で無効にします。

例:

```yaml
manual_resume_required: true
```

ユーザーが明示的に再開操作をした場合のみ、表示を再開します。

---

## 初期設定例

```yaml
overlay:
  enabled_by_default: false
  manual_resume_required: true
  show_status_label: true
  panic_hotkey: "Ctrl+Alt+F12"

safety:
  whitelist_only: true
  hide_on_focus_lost: true
  hide_on_unknown_window: true
  hide_on_text_input_mode: true
  hide_on_password_field_detected: true
  hide_on_clipboard_paste: true
  disable_auto_resume_after_risk: true

input:
  store_history: false
  write_key_logs: false
  allow_text_keys: false
  allow_gamepad: true
  allow_mouse_buttons: true
  allow_mouse_movement: false

allowed_processes:
  - "example-game.exe"

allowed_keys:
  keyboard:
    - "W"
    - "A"
    - "S"
    - "D"
    - "Space"
    - "LeftShift"
    - "LeftCtrl"
    - "Q"
    - "E"
    - "R"
    - "F"
  mouse:
    - "Mouse1"
    - "Mouse2"
    - "Mouse3"
    - "WheelUp"
    - "WheelDown"
  gamepad:
    - "A"
    - "B"
    - "X"
    - "Y"
    - "LB"
    - "RB"
    - "LT"
    - "RT"
    - "LeftStick"
    - "RightStick"
```

---

## 推奨ディレクトリ構成

```text
safe-input-overlay/
├─ README.md
├─ LICENSE
├─ src/
│  ├─ app/
│  │  ├─ Program.*
│  │  └─ AppConfig.*
│  ├─ input/
│  │  ├─ KeyboardInputProvider.*
│  │  ├─ MouseInputProvider.*
│  │  └─ GamepadInputProvider.*
│  ├─ safety/
│  │  ├─ ForegroundWindowGuard.*
│  │  ├─ ProcessWhitelist.*
│  │  ├─ TextInputGuard.*
│  │  ├─ PasswordFieldGuard.*
│  │  └─ PanicMode.*
│  ├─ filter/
│  │  ├─ AllowedKeyFilter.*
│  │  └─ ActionNormalizer.*
│  ├─ overlay/
│  │  ├─ OverlayWindow.*
│  │  └─ OverlayRenderer.*
│  └─ logging/
│     └─ SafeLogger.*
├─ config/
│  └─ default.yaml
├─ docs/
│  ├─ SECURITY.md
│  ├─ PRIVACY.md
│  └─ THREAT_MODEL.md
└─ tests/
   ├─ safety/
   ├─ input/
   └─ filter/
```

---

## 開発ロードマップ

### Phase 1: Safety MVP

- 設定ファイル読み込み
- 許可済みプロセス判定
- 前面ウィンドウ監視
- オーバーレイON/OFF
- 緊急停止ホットキー
- ログにキー入力を書かない仕組み
- 危険状態後の手動復帰

### Phase 2: Input Overlay MVP

- キーボードの許可キー表示
- マウスボタン表示
- ゲームパッド表示
- OBS向け表示方式
- 非表示理由のステータス表示

### Phase 3: Text Safety

- テキスト入力モード
- チャット開始キーの推定
- UI Automationによるパスワード欄検出
- ブラウザ・ランチャー・チャットアプリでの強制非表示
- クリップボード貼り付け検知時の非表示

### Phase 4: Hardening

- 自動テスト
- セキュリティレビュー
- プライバシーレビュー
- クラッシュログのサニタイズ
- 設定UI
- 署名付きビルド

---

## テスト方針

### 必須テスト

| テスト | 期待結果 |
|---|---|
| 許可済みゲームが前面 | 許可キーのみ表示 |
| ブラウザへAlt+Tab | 即時非表示 |
| メモ帳が前面 | 非表示 |
| Discordが前面 | 非表示 |
| ログイン画面が前面 | 非表示 |
| Windowsキー押下 | 非表示 |
| Panic hotkey押下 | 即時非表示 |
| チャット入力モード | 全キー非表示 |
| 未知のプロセスが前面 | 非表示 |
| 前面ウィンドウ取得失敗 | 非表示 |
| クラッシュ発生 | 入力内容がログに残らない |

### 事故防止テスト

実際のパスワードを使わず、必ずダミー文字列でテストします。

```text
dummy-password-DO-NOT-USE
```

確認項目:

- ダミー文字列の各文字が画面に出ない
- ダミー文字列がログに残らない
- クラッシュログに残らない
- 設定ファイルに残らない
- OBS上にも表示されない

---

## セキュリティ要件

このプロジェクトでは、次をリリース条件にします。

- 入力履歴を保存しない
- デバッグログにキー入力を書かない
- 未許可アプリでは表示しない
- フォーカス喪失時に表示しない
- 文字入力モードでは表示しない
- 緊急停止キーが常に動作する
- 危険状態後は手動再開にする
- 設定の既定値は安全側に倒す
- ネットワーク送信機能を既定で持たない
- テレメトリを入れる場合は明示同意制にする

---

## プライバシー方針

本ツールは、入力内容を収集・保存・送信しません。

扱うデータ:

- 現在押されている許可済み操作キー
- 前面ウィンドウのプロセス名
- オーバーレイの表示状態
- 非表示理由

扱わないデータ:

- 入力文字列
- パスワード
- チャット内容
- クリップボード本文
- ブラウザ閲覧履歴
- 配信映像
- 音声
- 個人アカウント情報

---

## 実装時の注意

### 悪い例

```text
すべてのキー入力を記録する
あとから危険な入力だけ消す
パスワード欄検出だけに依存する
ブラックリストだけで危険アプリを除外する
デバッグログにキー名を書く
危険状態から自動復帰する
```

### 良い例

```text
許可済みゲームが前面のときだけ表示する
許可済みキーだけ表示する
テキスト入力中は全非表示にする
安全判定に失敗したら非表示にする
入力履歴を持たない
緊急停止を最優先する
危険状態後は手動で再開する
```

---

## 開発者向けチェックリスト

実装前に確認してください。

- [ ] 入力履歴を保存しない設計になっている
- [ ] 既定設定でオーバーレイがOFFになっている
- [ ] 許可済みプロセス以外では表示されない
- [ ] 未知の状態では表示されない
- [ ] テキスト入力中は表示されない
- [ ] Panic hotkeyが常に効く
- [ ] ログにキー入力が出ない
- [ ] クラッシュ時にも入力内容が残らない
- [ ] 危険状態から自動復帰しない
- [ ] パスワード欄検出に依存しすぎていない
- [ ] OBS上で非表示状態が確認できる
- [ ] セキュリティレビュー用ドキュメントがある

---

## ライセンス

未定。  
配布する場合は、利用者が安全性と制限を理解できるライセンスおよび免責文を用意してください。

---

## テックスタック
```
C# / .NET 10 LTS
WinUI 3
Raw Input
XInput
UI Automation
CsWin32
xUnit
YAML設定
MSIX配布
```

## 開発

初回はNuGet復元を行ってからビルドします。

```powershell
dotnet restore InputVisualizer.slnx
dotnet build InputVisualizer.slnx -m:1
dotnet test tests\InputVisualizer.Tests\InputVisualizer.Tests.csproj
```

WinUI/XAML生成を含むため、この環境ではソリューションビルドを `-m:1` で直列化します。

## 免責

このツールは、配信者本人の操作を視覚化するためのものです。  
他者の入力を記録・監視する目的で使用してはいけません。

開発・利用にあたっては、各ゲーム、配信プラットフォーム、OS、アンチチートシステムの規約を確認してください。  
特に入力フックやオーバーレイは、ゲームやアンチチートによって制限される場合があります。
