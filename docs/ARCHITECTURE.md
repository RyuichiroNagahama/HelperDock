# Architecture — Register-Your-Own-View via UI Service

## Why（背景）
- HUD/シーン切替で既存 UIView が再生成されるため、「見つけて差し替える」方式はタイミング依存で不安定。
- PerformanceMonitor 系は **UIサービス**に **自前ビューを登録** し、**登録先（サービス）が面倒を見る**ため堅牢。

## What（やること）
1. SystemBase / ISystem（DOTS）で **ゲームロード完了/UIServices 準備完了**を待つ
2. **UIサービスのファクトリ/登録API** で **自前 View / UISystem** を生成・登録  
   - URL：`coui://HelperDock/index.html`（固定）  
   - ハンドル保持：`_svc`, `_viewHandle`, `_isVisible`
3. **開閉はサービスAPIでトグル**（Shift+F2）
4. **Revert はサービスAPIで既定へ**（Shift+F12 / ESC）
5. **再生成検知**（OnDestroy/OnCreate or サービスイベント）で **自動再登録**（数フレーム遅延/再試行）

## Modules
- `HelperDock.BootstrapSystem`  
  - `OnUpdate()`で UIサービスAvailable? を監視 → 初回だけ `HelperDockUi.Register(svc)`
- `HelperDockUi`（登録・開閉・復帰のユーティリティ）  
  - `Register(svc)` / `Toggle()` / `Revert()` / `Dump()`
- `HotkeysSystem`（Shift+F2/F12/ESC ハンドラ）  
  - `Toggle`/`Revert` を **HelperDockUi** に委譲
- `web/`（cohtml UI）  
  - `index.html` と最小の bridge

## ログ・可観測性（必須）
- `[HelperDock] UISVC READY` … サービス検出
- `[HelperDock] REGISTER OK handle=…` … 自分の View 登録済み
- `[HelperDock] TOGGLE -> Visible=True/False`
- `[HelperDock] REVERT OK` / `REVERT SKIP (no handle)`
- 例外は `[HelperDock] EX:` で一本化
