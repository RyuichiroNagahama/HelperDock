# Test Plan

## ログ監視（PowerShell）
$log="$env:USERPROFILE\AppData\LocalLow\Colossal Order\Cities Skylines II\Player.log"; `
Get-Content "$log" -Wait -Tail 0 | ? { $_ -match "\[HelperDock\].*(UISVC READY|REGISTER OK|TOGGLE|REVERT|EX)" }

## シナリオ
1) 起動→マップ/HUD表示  
   - 期待: `UISVC READY` → `REGISTER OK`
2) Shift+F2  
   - 期待: `TOGGLE -> Visible=True`（以後押すたび True/False）
3) ESC / Shift+F12  
   - 期待: `REVERT OK`
4) HUD 切替 or ロードで UI 再生成  
   - 期待: 自動で `REGISTER OK` が再度出て、以後も F2 が効く
