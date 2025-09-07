# HelperDock Playbook
- Project root: `C:\Users\ryuna\Documents\cs2_mods\HelperDock`
- Build: `dotnet build -c Release`
- Deploy DLL → `%LocalAppData%..\LocalLow\Colossal Order\Cities Skylines II\Mods\HelperDock\Code`
- UI assets duplicated under `Mods\HelperDock\UI\HelperDock` and `Mods\HelperDock\Code\UI\HelperDock`
- Hotkeys: Shift+F1 Toggle, Shift+F2 EnsureViewLoaded2 + ForceShow
- Tail logs:
  `Get-Content "$env:USERPROFILE\AppData\LocalLow\Colossal Order\Cities Skylines II\Player.log" -Wait -Tail 0 | ? { $_ -match 'HelperDock|EnsureViewLoaded2|candidate|VIEW OPEN' }`
- Last known good: VIEW OPEN OK via CreateInstance -> coui://HelperDock/index.html (window still not visible)


## 2025-09-06

### 今日やった
- Shift+F2 → EnsureViewLoaded2 → **VIEW OPEN OK via CreateInstance** ログを確認
- UI フォルダを Mods\HelperDock\UI と Mods\HelperDock\Code\UI の両方に配置

### 次にやる
- 実際に cohtml パネルが画面に出るかの確認
- ToggleMainPanel/ForceShowNow が cohtml 側に連動するか検証

### ログ抜粋
```text
[HelperDock] HOTKEY Shift+F2
[HelperDock] EnsureViewLoaded2: type=HelperDock.UISystem
[HelperDock] candidate UISystem.CreateInstance (1 params)
[HelperDock] VIEW OPEN OK via CreateInstance -> coui://HelperDock/index.html
