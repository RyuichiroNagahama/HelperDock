# HANDOFF — HelperDock (UIManager Registration Path)

## Current Branch / Tip
- Branch: `feature/uimanager-registration`
- Tip: `<ここに git rev-parse --short HEAD の値>`  ← 例: `a566ac0`
- Base: `main`

## One-line Status
反射・乗っ取りは **廃止**。UIサービス登録型へ移行中（Phase 1–2）。

## What Works Now
- Hotkeys を UIサービス経由に配線（反射経路は `legacy/` へ隔離・非ビルド）
- ドキュメント類（README, ARCHITECTURE, IMPLEMENTATION_PLAN, DECISION_LOG, TEST-PLAN, TROUBLESHOOTING）追加

## What’s Next (MVP)
- `BootstrapSystem` で UIサービス検出 → `HelperDockUi.Register()` 実装
- `Toggle()` / `Revert()` をサービス API で実装
- ログ基準: `UISVC READY` → `REGISTER OK` → `TOGGLE …` / `REVERT OK`

## Acceptance Criteria
- 起動後に `UISVC READY` & `REGISTER OK`
- Shift+F2 で確実に開閉／ESC or F12 で既定復帰
- HUD/シーン切替後も自動再登録で継続動作

## Non-Goals / Never Again
- private 反射・Unity 全走査の本番使用・`UIView.url` 乗っ取り

## Useful Commands
```powershell
# ログテイル
$log="$env:USERPROFILE\AppData\LocalLow\Colossal Order\Cities Skylines II\Player.log";
Get-Content "$log" -Wait -Tail 0 | ? { $_ -match "\[HelperDock\].*(UISVC READY|REGISTER OK|TOGGLE|REVERT|EX)" }

# 作業ブランチへ移動
git checkout feature/uimanager-registration
git pull
