# HANDOFF — HelperDock (UIManager Registration Path)

このドキュメントは「このコミットから開発を再開する人」向けの引き継ぎノートです。  
**絶対方針**・**入口ファイル**・**残課題**・**ロールバック**を1枚に集約しています。

---

## 0. TL;DR（最短ルート）
- ブランチ: `feature/uimanager-registration`
- 先に読む: `HANDOFF.md` → `README.md` → `docs/ARCHITECTURE.md`
- 動かし方: README の「Dev 環境」節 → ログ監視 `scripts/tail.ps1`
- 期待ログ: `UISVC READY` → `REGISTER OK`、F2 で `TOGGLE -> Visible=...`

---

## 1. 絶対方針（合意済）
- **反射・全走査・set_url 乗っ取りは使わない**  
- **UIサービス（UIManager/Gameface）で “自前 View を登録”** して開閉する  
- HUD/シーン再生成に **ライフサイクルで追従**（再登録を実装）

---

## 2. 最初に見るファイル
1. `UISystemDevHotkey.cs` … **F1/F2/F12/ESC の入口**  
2. `docs/ARCHITECTURE.md` … 設計の要点（UIサービス登録型）  
3. `docs/IMPLEMENTATION_PLAN.md` … 手順と受入基準  
4. `docs/TEST-PLAN.md` … ログでの合否判定  
5. `legacy/` … 反射系の実験（**ビルド対象外**）

---

## 3. 現状の実装状態（チェックボックス）
- [ ] Phase 1: `BootstrapSystem` で **UIサービス検出 → Register**  
- [ ] Phase 2: `HelperDockUi.Toggle()/Revert()` を **サービスAPI**で実装  
- [ ] Phase 3: **再登録（UI 再生成時）** と **起動直後の再試行**  
- [ ] Phase 4: legacy 完全無効化（`#if LEGACY` or 物理削除）

**ログ基準**  
- `UISVC READY` … サービス見つかった  
- `REGISTER OK handle=…` … 自前 View 登録完了  
- `TOGGLE -> Visible=True/False`  
- `REVERT OK` / `REVERT SKIP (no handle)`

---

## 4. 進め方（ToDo）
- [ ] `IUiService` 相当の**正規API参照**を解決（SDK/アセンブリ参照）  
- [ ] `BootstrapSystem` 追加、`TryGetUiService(...)` 実装  
- [ ] `HelperDockUi.Register/Toggle/Revert` 実装 & ログ追加  
- [ ] Hotkey → `HelperDockUi` を呼ぶよう統一（F1/F2/F12/ESC）  
- [ ] `docs/*` の受入基準に沿って動作確認→PR で記録

---

## 5. ロールバック/リスク
- UIサービス未検出時：**no-op**（ログだけ）  
- 緊急時: `legacy/` は参考用に残存（**既定非ビルド**）。方針違反なので恒久利用は不可。

---

## 6. 作業ログ（増分はここにも書く）
- 2025-09-14: UIサービス移行ドキュメント追加、 legacy を exclude。  
- 20XX-YY-ZZ: （あなたの更新を箇条書きで）

---

## 7. 連絡先 / PR / Issue
- Maintainer: @RyuichiroNagahama
- Roadmap Issue: #<番号を後で追記>
- PR: #<番号を後で追記>（Draft 推奨）

