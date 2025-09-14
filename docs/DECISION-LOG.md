# Decision Log (ADR-style)

## ADR-0001: 採用 — UIサービス登録型（2025-09-14）
- **決定**: UI は UIManager / Gameface の API で「自前ビューを登録/開閉」する。
- **理由**: ライフサイクル追従・安定性・再生成耐性。
- **却下**: 反射・全走査・set_url 乗っ取りは非採用（検証ブランチのみ）。

## ADR-0002: Hotkeys とログ規約（2025-09-14）
- Shift+F2＝Toggle、Shift+F12/ESC＝Revert、Shift+F1＝Dump
- ログ接頭辞は `[HelperDock]` に統一、例外は `EX:` で一列管理。

## ADR-0003: 再試行の標準化（2025-09-14）
- UI 未初期化時は N フレーム/最大 M ms で再試行。ユーザーの連打に依存しない。

## ADR-0004: レガシー隔離（2025-09-14）
- `legacy/` に隔離し、既定ビルドからは外す。緊急時も `#define LEGACY` でしか使えない。
