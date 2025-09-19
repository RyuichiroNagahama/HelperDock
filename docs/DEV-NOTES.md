# Dev Notes

## 名前の例（要置換）
- `IUiService` / `UiManager` / `ViewHandle` は **実プロジェクトの型名に差し替える**。
- 依存は **公式SDKの参照**越しに行う。`internal` をこじ開ける反射は使わない。

## API スケッチ（疑似コード）
var svc = UiServiceLocator.Get();                // UIサービス取得
var handle = svc.RegisterView("HelperDock",      // 一意名
  url: "coui://HelperDock/index.html",
  visible: false,
  onClosed: () => { /* state sync */ });

HelperDockUi.Toggle() { svc.SetVisible(handle, !svc.IsVisible(handle)); }
HelperDockUi.Revert() { svc.Close(handle); /* or SetDefault */ }

## DOTS System
- `OnUpdate()` で svc が null → return。非同期に後追いで Register。
- `OnDestroy()` で `Revert()` and `Unregister()` を保険実行。
