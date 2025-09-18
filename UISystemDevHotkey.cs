using System;
using Unity.Entities;
using UnityEngine.InputSystem;

namespace HelperDock
{
  // ← クラス名を Mod.cs に合わせて "UISystemDevHotkey"
  public partial class UISystemDevHotkey : SystemBase
  {
    protected override void OnStartRunning()
    {
      ModCompat.log.Info("[HelperDock] DEVHOTKEY.OnStartRunning (USCAN14 UIS2 ACTIVE)");
    }

    protected override void OnUpdate()
    {
      try{
        var kb = Keyboard.current; if (kb==null) return;
        if (kb.escapeKey.wasPressedThisFrame) { ModCompat.log.Info("[HelperDock] HOTKEY ESC (revert SAFE14)"); HotkeySafety14.RevertUrl14(); } if (!kb.shiftKey.isPressed) return;

        if (kb.f1Key.wasPressedThisFrame){
          ModCompat.log.Info("[HelperDock] HOTKEY Shift+F1 (dump views DUMP14)");
          HotkeySafety14.DumpViews14();
        }
        if (kb.f2Key.wasPressedThisFrame){
          ModCompat.log.Info("[HelperDock] HOTKEY Shift+F2 (try open TRY14)");
          HotkeySafety14.TryOpenViaFound14_UIS2Toggle();
        }
        if (kb.f12Key.wasPressedThisFrame){
          ModCompat.log.Info("[HelperDock] HOTKEY Shift+F12 (revert SAFE14)");
          HotkeySafety14.RevertUrl14();
        }
      }catch(Exception ex){
        ModCompat.log.Info("[HelperDock] DEVHOTKEY EX: "+ex);
      }
    }
  }
}






