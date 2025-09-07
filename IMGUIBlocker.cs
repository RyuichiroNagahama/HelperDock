using System;
using UnityEngine;

namespace HelperDock {
  public static class IMGUIBlocker {
    static bool s_done;
    public static void RunOnce(){
      if (s_done) return; s_done = true;
      try {
        int removed = 0;
        var mbType = typeof(MonoBehaviour);

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()){
          Type target = null;
          try {
            foreach (var tp in asm.GetTypes()){
              if (tp.Name == "HelperDockWindow" && mbType.IsAssignableFrom(tp)) { target = tp; break; }
            }
          } catch { continue; }

          if (target == null) continue;

          var objs = Resources.FindObjectsOfTypeAll(target);
          foreach (var obj in objs){
            var mb = obj as MonoBehaviour;
            if (mb != null) mb.enabled = false;
            var comp = obj as Component;
            if (comp != null) UnityEngine.Object.Destroy(comp);
            removed++;
          }
        }

        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>()){
          if (go && go.name.IndexOf("HelperDockWindow", StringComparison.OrdinalIgnoreCase) >= 0){
            UnityEngine.Object.Destroy(go);
          }
        }

        ModCompat.log.Info($"IMGUIBlocker: removed {removed} components.");
      } catch (Exception e) {
        ModCompat.log.Error("IMGUIBlocker: " + e);
      }
    }
  }
}
