# HelperDock — UIManager-Registered UI (No Reflection)

**絶対方針（MVP以降ずっと維持）**
- 反射や set_url 乗っ取りで既存 UIView を探して差し替える実装は**使わない**。
- **UIサービス（UIManager / Gamefaceレイヤ）**の公式/半公式 API を介して
  **自前の UISystem / View を登録して開く**“生成型”の設計に統一する。
- HUD 切替やシーン遷移で UI が再生成されても **UIサービスのライフサイクルに追従**する。

**Hotkeys（デバッグ用）**
- Shift+F2 … HelperDock を **Toggle**（UIサービスでの Show/Hide もしくは Open/Close）
- Shift+F12 / ESC … **Revert**（安全弁：UIサービス経由で既定状態へ戻す）
- Shift+F1 … Debug Dump（UIサービスと登録ビューの状態確認）

**対応範囲**
- Cities: Skylines II（Gameface / cohtml ベース UI）
- .NET Framework 4.8（`TargetFramework=net48`）

**非方針（今後も採らない）**
- 反射での private フィールド探索
- `Resources.FindObjectsOfTypeAll(UIVIew)` など Unity 全走査の本番使用
- `UIView.url = "coui://..."` の“椅子差し替え”ハック

###この行以降は上記と併せてちゃんと再編集が必要！！！

# HelperDock — UIサービス登録型UI（反射なし）

## 絶対方針
- 反射や `set_url` 乗っ取りは使わない
- UIサービス（UIManager / Gameface）経由で自前ビューを登録・開閉
- HUD/シーン再生成に追従

## デバッグ用ホットキー
- Shift+F2: トグル（表示/非表示）
- Shift+F12 / ESC: 既定へ戻す（Revert）
- Shift+F1: 状態ダンプ

## ドキュメント
- 日本語: `docs/ja/`
- 英語: `docs/`

## 進行中のブランチ
- `feature/uimanager-registration`
