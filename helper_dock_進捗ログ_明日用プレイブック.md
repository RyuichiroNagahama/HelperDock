# 目的
CS2 上に cohtml (index.html + index.js + style.css) を読み込む「HelperDock」パネルを出し入れ（Shift+F1/表示切替、Shift+F2/位置リセット+表示）できる基盤を作る。

---

# ここまでで分かったこと（要約）
- **Mod 本体は起動**している：`OnLoad` → `UISystem.OnCreate` → `DEVHOTKEY.OnStartRunning` のログが出る。
- **ホットキー入力は取れている**：ゲーム画面フォーカスで `HOTKEY Shift+F1/F2` が Player.log に出る。
- **Binding は存在**：`Init check: visBinding=True, posX=True, posY=True, settings.Visible=True`。
- **ビューは未表示**：`EnsureViewLoaded*` の走査結果が **`CreateInstance` 系に当たってしまい、実ビューを開く API には未到達**。
- **UI 資材は配備済み**：
  - `Mods\HelperDock\UI\HelperDock\(index.html, index.js, style.css)`
  - `Mods\HelperDock\Code\UI\HelperDock\(同上)`

---

# 現在の実装（コア部）
- `UISystemDevHotkey`：Shift+F1/Shift+F2 で `ToggleMainPanel()` / `EnsureViewLoaded2() → ForceShowNow()` を呼ぶ。
- `EnsureViewLoaded2()`：
  - `this` と UI っぽいフィールド/プロパティ、`GameManager.uiManager`（反射）を巡回。
  - **メソッド名フィルタ**：`View / Open* / Show* / Create* / Add* / Push* / *Overlay*` を含む／始まるもの。
  - **除外**：`*Binding* / *Preview* / *Save* / *LoadGame* / *Instance* / （自分の UISystem）`。
  - 1–3個の string/bool/int を含むありがちなシグネチャに対し、`id/url` を当て込んで総当たり。
  - **成功ログ**：`VIEW OPEN OK via <Method> -> <url>`。

> 直近ログでは `candidate UISystem.CreateInstance (1 params)` → `VIEW OPEN OK via CreateInstance -> coui://HelperDock/index.html` が出たが、これは **C# インスタンス生成**であり **cohtml 表示ではない**（画面に出ない）。

---

# 明日の最短チェックリスト
1. **ゲーム起動 → セーブをロード → 都市内で** Shift+F2 を1回押す。
2. Tail コマンド（下記）で **次の並び**が出るか確認：
   - `[HelperDock] HOTKEY Shift+F2`
   - `EnsureViewLoaded2: type=...` が複数（`...UISystemBase` や `...UIManager` が含まれると良い）
   - `candidate ...` が **`CreateInstance` 以外**で何行か出る
   - 成功時：`VIEW OPEN OK via <Open*/Add*/Show*/...> -> coui://...`
3. もし **`candidate` が 1 行だけ（CreateInstance）**なら、ログ全文（`candidate` 行すべて）を貼る。

### Tail（コピペ用）
```powershell
$log = "$env:USERPROFILE\AppData\LocalLow\Colossal Order\Cities Skylines II\Player.log"
Get-Content $log -Wait -Tail 0 | ? { $_ -match 'HelperDock|EnsureViewLoaded2|candidate|VIEW OPEN' }
```

---

# 一撃スクリプト（最新）
> partial 再生成（正しいフィルタ & UI 管理まで探索）→ ビルド → DLL 配備 → UI 同期 → tail コマンド出力

```powershell
$ErrorActionPreference='Stop'
function Ok($m){ Write-Host "✅ $m" -ForegroundColor Green }
function Warn($m){ Write-Host "⚠ $m" -ForegroundColor Yellow }
function Err($m){ Write-Host "❌ $m" -ForegroundColor Red }

$proj   = "$env:USERPROFILE\Documents\cs2_mods\HelperDock"
$csproj = (Get-ChildItem $proj -Recurse -Filter *.csproj | Select-Object -First 1).FullName
if(!(Test-Path $proj) -or !(Test-Path $csproj)){ throw "project/csproj not found" }

$partialPath = Join-Path $proj 'UISystem.ViewLoader2.partial.cs'
# （中略：EnsureViewLoaded2 の本体はこのプレイブック上部の実装と同一）
# ※ 実運用は直近の私が貼ったスクリプトをそのまま使用してください。

& dotnet build "`"$csproj`"" -c Release
if($LASTEXITCODE -ne 0){ throw "build failed" }
Ok "Build OK"

$outDir  = Join-Path $proj 'bin\Release\net48'
$dll     = Join-Path $outDir 'HelperDock.dll'
if(!(Test-Path $dll)){ throw "出力DLLが見つかりません: $dll" }

$modsRoot = "$env:USERPROFILE\AppData\LocalLow\Colossal Order\Cities Skylines II\Mods"
$modDir   = Join-Path $modsRoot 'HelperDock'
$codeDir  = Join-Path $modDir  'Code'
New-Item -ItemType Directory -Force -Path $codeDir | Out-Null
Copy-Item $dll $codeDir -Force
Ok "Deployed DLL: $dll → $codeDir"

$srcUI = Join-Path $modDir 'UI\HelperDock'
$dstUI = Join-Path $codeDir 'UI\HelperDock'
if(Test-Path $srcUI){
  New-Item -ItemType Directory -Force -Path (Split-Path $dstUI) | Out-Null
  Copy-Item $srcUI $dstUI -Recurse -Force
  Ok "Synced UI: $srcUI → $dstUI"
}

$log = "$env:USERPROFILE\AppData\LocalLow\Colossal Order\Cities Skylines II\Player.log"
"`nTail (press Shift+F2 once):"
"Get-Content `"$log`" -Wait -Tail 0 | ? {`$_.Contains('HelperDock') -or `$_.Contains('candidate') -or `$_.Contains('VIEW OPEN OK') -or `$_.Contains('EnsureViewLoaded2') }"
```

---

# トラブル時の即対応メモ
- `CS0111/CS0102`（二重定義）：
  - `UISystem.ViewLoader*.cs` が複数になっていないか。古いものは `*.off` にリネームして隔離。
- DLL 未配備なのに「Deployed」と出る：
  - 一撃スクリプト中の `Copy-Item` は失敗時に止まるよう `$ErrorActionPreference='Stop'` を維持。
- `candidate` が `CreateInstance` しか出ない：
  - 走査対象に UI 管理オブジェクトが足りない可能性。ログを貼ってください（候補名をさらに拡張します）。

---

# 資産を“次スレ”へ持ち越す方法
- **このプレイブックをローカルに保存**：テキスト全選択→`PLAYBOOK_HelperDock.md` としてプロジェクト直下に保存。
- **Git 管理**：`HelperDock` のソースに `notes/PLAYBOOK.md` を同梱し、スクリプトは `scripts/` へ。
- **小さなメモリ**（任意）：
  - 「私は *一撃 PowerShell* を好む」「ログ監視は Player.log の tail」という**方針だけ**を覚えさせておくと、次スレでも同じ作法で提案しやすくなります（機密でない範囲の短い情報のみ推奨）。

---

# 参考：明日合流時に貼ってほしいログ
- `EnsureViewLoaded2: type=...` の列挙
- 直後に続く `candidate ...` の**全行**（`CreateInstance` 以外があるか確認）
- もし `VIEW OPEN OK via ...` が出たら、その 1 行

---

おつかれさまでした。明日は **「候補名の特定 → 正式 API 1 本に固定」**の順で、一気にパネルを表示まで持っていきます。

