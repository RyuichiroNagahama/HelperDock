# Implementation Plan (UI Service Registration)

## Phase 0: 足場
- ✅ `TargetFramework=net48` を明記
- ✅ Modding Tool Chain（`Mod.props/targets`）配置と環境変数 `CSII_TOOLPATH` を維持
- ✅ 既存の反射/乗っ取りコードは **`legacy/`** へ隔離・無効化（ビルド対象から外す）

## Phase 1: UIサービス検出と登録
- [ ] `BootstrapSystem` を追加（SystemBase）
- [ ] `TryGetUiService(out svc)` を実装（正規API。無ければ公式に準拠したラッパを作る）
- [ ] `HelperDockUi.Register(svc)` で **自前View** を生成（URL= `coui://HelperDock/index.html`）
- [ ] ログ：`UISVC READY` / `REGISTER OK` を確認できること

**Acceptance Criteria**
- [ ] ゲーム起動→マップ/HUD表示後に、ログへ `UISVC READY`→`REGISTER OK` が確実に出る

## Phase 2: トグル＆リバート
- [ ] `HelperDockUi.Toggle()`：サービスの Show/Hide or Open/Close を呼ぶ
- [ ] `HelperDockUi.Revert()`：サービスの既定状態に戻す
- [ ] Hotkeys：Shift+F2→Toggle、Shift+F12/ESC→Revert
- [ ] ログ：`TOGGLE -> Visible=True/False` / `REVERT OK`

**Acceptance Criteria**
- [ ] HUD/シーン切替後でも Shift+F2 が**毎回**効く  
- [ ] ESC or F12 で確実に既定へ戻る

## Phase 3: 復元・再登録（タフネス）
- [ ] UI再生成イベント/失効検知で自動再登録
- [ ] 起動直後（未初期化）でも、数フレーム/数百ms の**自動再試行**で後追い登録

**Acceptance Criteria**
- [ ] 起動直後でも Shift+F2 一発で、**最終的に**ウィンドウが開く（手動の連打不要）
- [ ] UI破棄→再生成（HUD切替/ロード）後も動作継続

## Phase 4: レガシー削除
- [ ] 反射/乗っ取り系のソースをビルドから完全に外す（`#if LEGACY` でも可）
- [ ] ドキュメント更新（“非採用”を明記）

**Acceptance Criteria**
- [ ] ビルド成果物に `DUMP14`, `TRY14` 等の文字列が含まれない
