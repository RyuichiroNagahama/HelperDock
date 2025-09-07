using System;
using Game;
using Game.Modding;
using UnityEngine;
using Unity.Entities;

namespace HelperDock {
  public class Mod : IMod {
    public void OnLoad(UpdateSystem updateSystem){
      Debug.Log("[HelperDock] OnLoad");

      try {
        // UISystem を確実に作成
        var world = World.DefaultGameObjectInjectionWorld;
        world.GetOrCreateSystemManaged<HelperDock.UISystem>();
        // 補助: メインゲーム中に一度だけ強制表示（クラスがある場合のみ）
        try {
          world.GetOrCreateSystemManaged<HelperDock.UISystemEnsureOpen>();
          updateSystem.UpdateAt<HelperDock.UISystemEnsureOpen>(SystemUpdatePhase.UIUpdate);
          updateSystem.RequireForUpdate<HelperDock.UISystemEnsureOpen>();
        } catch {}

        // UIフレームで更新
        updateSystem.UpdateAt<HelperDock.UISystem>(SystemUpdatePhase.UIUpdate);
        updateSystem.RequireForUpdate<HelperDock.UISystem>();
            updateSystem.UpdateAt<HelperDock.UISystemDevHotkey>(SystemUpdatePhase.UIUpdate);
            updateSystem.RequireForUpdate<HelperDock.UISystemDevHotkey>();
      }
      catch (Exception ex) {
        Debug.LogError("[HelperDock] Mod.OnLoad error: " + ex);
      }
    }

    public void OnDispose(){
      Debug.Log("[HelperDock] OnDispose");
    }

    // 旧IMGUIオーバーレイは完全停止（貫通の原因）
    private void OnGUI(){
      return;
    }
  }
}

