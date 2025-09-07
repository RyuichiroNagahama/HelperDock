# HelperDock Playbook
- Project root: `C:\Users\ryuna\Documents\cs2_mods\HelperDock`
- Build: `dotnet build -c Release`
- Deploy DLL → `%LocalAppData%..\LocalLow\Colossal Order\Cities Skylines II\Mods\HelperDock\Code`
- UI assets duplicated under `Mods\HelperDock\UI\HelperDock` and `Mods\HelperDock\Code\UI\HelperDock`
- Hotkeys: Shift+F1 Toggle, Shift+F2 EnsureViewLoaded2 + ForceShow
- Tail logs:
  `Get-Content "$env:USERPROFILE\AppData\LocalLow\Colossal Order\Cities Skylines II\Player.log" -Wait -Tail 0 | ? { $_ -match 'HelperDock|EnsureViewLoaded2|candidate|VIEW OPEN' }`
- Last known good: VIEW OPEN OK via CreateInstance -> coui://HelperDock/index.html (window still not visible)
