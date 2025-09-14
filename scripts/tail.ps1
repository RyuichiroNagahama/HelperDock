$log="$env:USERPROFILE\AppData\LocalLow\Colossal Order\Cities Skylines II\Player.log";
Get-Content "$log" -Wait -Tail 0 |
  ? { $_ -match "\[HelperDock\].*(UISVC READY|REGISTER OK|TOGGLE|REVERT|EX)" }
