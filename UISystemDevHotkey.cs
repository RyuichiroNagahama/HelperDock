using Unity.Entities;
using UnityEngine.InputSystem;
using Game;

namespace HelperDock
{
    [UpdateInGroup(typeof(UpdateSystem))]
    public sealed partial class UISystemDevHotkey : GameSystemBase
    {
        protected override void OnStartRunning()
        {
            ModCompat.log.Info("[HelperDock] DEVHOTKEY.OnStartRunning");
        }

        protected override void OnUpdate()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.shiftKey.isPressed)
            {
                if (kb.f1Key.wasPressedThisFrame)
                {
                    var ui = World.GetExistingSystemManaged<UISystem>();
                    if (ui != null)
                    {
                        ModCompat.log.Info("[HelperDock] HOTKEY Shift+F1");
                        ui.ToggleMainPanel();
                    }
                }
                if (kb.f2Key.wasPressedThisFrame)
                {
                    var ui = World.GetExistingSystemManaged<UISystem>();
                    if (ui != null)
                    {
                        ModCompat.log.Info("[HelperDock] HOTKEY Shift+F2");
                        ui.EnsureViewLoaded2();  // まず開く
                        ui.ForceShowNow();       // 位置リセット+表示
                    }
                }
            }
        }
    }
}
