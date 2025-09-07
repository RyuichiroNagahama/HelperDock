using UnityEngine;
using UnityEngine.UIElements;

namespace ModCommon {
  public static class UiTkBlockerShim {
    class Binder {
      public string key;
      public VisualElement ve;
      public bool attached;

      public Binder(VisualElement v, string k){
        ve = v; key = k;
        if (ve == null) return;

        ve.RegisterCallback<AttachToPanelEvent>(OnAttach);
        ve.RegisterCallback<DetachFromPanelEvent>(OnDetach);
        // 既にパネル上なら即アタッチ扱い
        if (ve.panel != null) TryArm();
      }

      void OnAttach(AttachToPanelEvent e){ TryArm(); }
      void OnDetach(DetachFromPanelEvent e){ Disarm(); }

      void TryArm(){
        if (attached || ve == null) return;
        attached = true;
        ve.RegisterCallback<GeometryChangedEvent>(OnGeom);
        // 初回反映
        OnGeom(default(GeometryChangedEvent));
      }

      void Disarm(){
        if (!attached) return;
        attached = false;
        if (ve != null) ve.UnregisterCallback<GeometryChangedEvent>(OnGeom);
        ModCommon.ClickBlocker.SetZone(key, default(Rect), false);
      }

      void OnGeom(GeometryChangedEvent e){
        if (ve == null) return;
        bool visible = ve.resolvedStyle.display != DisplayStyle.None &&
                       ve.resolvedStyle.opacity > 0f &&
                       ve.worldBound.width  > 0f &&
                       ve.worldBound.height > 0f;
        var r = ve.worldBound; // 画面左上原点(px)
        ModCommon.ClickBlocker.SetZone(key, r, visible);
      }
    }

    // 呼び出し口：好きな VisualElement を渡す
    public static void Attach(VisualElement ve, string key){
      if (ve == null) return;
      // 1要素に複数アタッチされても害はないが、一応ガード
      if (ve.userData is Binder) return;
      ve.userData = new Binder(ve, key);
    }
  }
}
