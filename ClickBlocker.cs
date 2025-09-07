// PM-style UI takeover
// return; // disabled for PM-style UI
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ModCommon {
  public static class ClickBlocker {

    const string OverlayName = "CS2_ModClickBlockerOverlay";
    const int TOP_ORDER = 1000000000;

    // デバッグ可視化（赤）用：見た目だけ。イベントは拾わない
    static UIDocument overlayDoc;
    static VisualElement overlayLayer;
    static bool debugOverlay = false;

    // すべてのウィンドウ矩形（スクリーン座標, IMGUI基準）
    static readonly Dictionary<string, Rect> zones = new Dictionary<string, Rect>();
    static bool dirty = true;

    // UI Toolkit パネル群（ゲーム側の UIDocument 含む）にグローバルでゲートを仕込む
    static readonly List<UIDocument> panels = new List<UIDocument>();
    static readonly HashSet<UIDocument> hooked = new HashSet<UIDocument>();

    class Runner : MonoBehaviour {
      float t;
      void Update(){
        t += Time.unscaledDeltaTime;
        if (t >= 2f){ t = 0f; RescanPanels(); }
        EnsureOverlay();
        if (dirty){ if (debugOverlay) RebuildDebugOverlay(); dirty = false; }
        if (Input.GetKeyDown(KeyCode.F10)) ToggleDebug();
      }
      // IMGUI 側：ウィンドウ“内”だけイベントを握り潰す（外側は素通り）
      void OnGUI(){
  // disabled for cohtml UI
  return;
        var e = Event.current; if (e == null) return;
        switch(e.type){
          case EventType.MouseDown:
          case EventType.MouseUp:
          case EventType.MouseDrag:
          case EventType.ScrollWheel:
          case EventType.MouseMove:
          case EventType.ContextClick: {
            Vector2 p = e.mousePosition;
            if (IsInsideAnyZone(p)){ e.Use(); }
            break;
          }
        }
      }
    }

    static bool IsInsideAnyZone(Vector2 p){
      foreach (var kv in zones){
        Rect r = kv.Value;
        if (p.x >= r.x && p.x <= r.x + r.width && p.y >= r.y && p.y <= r.y + r.height) return true;
      }
      return false;
    }

    static void EnsureRunner(){
      if (GameObject.Find("ClickBlockerRunner") != null) return;
      var go = new GameObject("ClickBlockerRunner");
      UnityEngine.Object.DontDestroyOnLoad(go);
      go.AddComponent<Runner>();
      Debug.Log("[CB] Runner installed");
    }

    // ゲーム内のすべての UIDocument に「ルートでイベントを止めるコールバック」を仕込む
    static void RescanPanels(){
      panels.RemoveAll(d => d == null || d.rootVisualElement == null);
      var found = UnityEngine.Object.FindObjectsOfType<UIDocument>(includeInactive: true);
      for (int i=0;i<found.Length;i++){
        var d = found[i];
        if (d == null || d.rootVisualElement == null) continue;
        if (!panels.Contains(d)) panels.Add(d);
        EnsureGate(d);
      }
    }

    // UITKゲート：パネルの最上位で、座標がウィンドウ“内”なら UI Toolkit 側のイベントを止める
    static void EnsureGate(UIDocument doc){
      if (doc == null || doc.rootVisualElement == null) return;
      if (hooked.Contains(doc)) return;  // 二重登録防止
      hooked.Add(doc);

      var root = doc.rootVisualElement;

      void Gate<T>(EventBase e) where T:EventBase {
        Vector2 pos;
        if (e is IPointerEvent pe) pos = pe.position;
        else if (e is WheelEvent we) pos = we.mousePosition;
        else return;
        if (IsInsideAnyZone(pos)){
          e.PreventDefault();
          e.StopImmediatePropagation();
        }
      }

      root.RegisterCallback<PointerDownEvent>(e => Gate<PointerDownEvent>(e), TrickleDown.TrickleDown);
      root.RegisterCallback<PointerMoveEvent>(e => Gate<PointerMoveEvent>(e), TrickleDown.TrickleDown);
      root.RegisterCallback<PointerUpEvent>(   e => Gate<PointerUpEvent>(e)   , TrickleDown.TrickleDown);
      root.RegisterCallback<ClickEvent>(       e => Gate<ClickEvent>(e)       , TrickleDown.TrickleDown);
      root.RegisterCallback<WheelEvent>(       e => Gate<WheelEvent>(e)       , TrickleDown.TrickleDown);
      root.RegisterCallback<PointerEnterEvent>(e => Gate<PointerEnterEvent>(e), TrickleDown.TrickleDown);
      root.RegisterCallback<PointerLeaveEvent>(e => Gate<PointerLeaveEvent>(e), TrickleDown.TrickleDown);
    }

    // デバッグ可視化（赤四角：ウィンドウ“内”だけ塗る）
    static void EnsureOverlay(){
      if (!debugOverlay) return; // デバッグ OFF のときは作らない
      if (overlayDoc != null && overlayDoc.rootVisualElement != null && overlayLayer != null) return;

      var go = GameObject.Find(OverlayName) ?? new GameObject(OverlayName);
      var ps = ScriptableObject.CreateInstance<PanelSettings>();
      ps.scaleMode = PanelScaleMode.ConstantPixelSize;
      ps.sortingOrder = TOP_ORDER;
      overlayDoc = go.GetComponent<UIDocument>() ?? go.AddComponent<UIDocument>();
      overlayDoc.panelSettings = ps; overlayDoc.sortingOrder = TOP_ORDER;
      UnityEngine.Object.DontDestroyOnLoad(go);

      overlayLayer = new VisualElement();
      overlayLayer.style.position = Position.Absolute;
      overlayLayer.style.left = 0; overlayLayer.style.top = 0; overlayLayer.style.right = 0; overlayLayer.style.bottom = 0;
      overlayLayer.pickingMode = PickingMode.Ignore; // 可視化のみ。イベントは拾わない
      overlayDoc.rootVisualElement.Add(overlayLayer);
      overlayLayer.BringToFront();
    }

    static void RebuildDebugOverlay(){
      if (overlayDoc == null || overlayDoc.rootVisualElement == null || overlayLayer == null) return;
      overlayLayer.Clear();
      foreach (var kv in zones){
        var r = kv.Value;
        var ve = new VisualElement();
        ve.style.position = Position.Absolute;
        ve.style.left = r.x; ve.style.top = r.y;
        ve.style.width = r.width; ve.style.height = r.height;
        ve.style.backgroundColor = new Color(1,0,0,0.15f); // 内側だけ赤
        ve.pickingMode = PickingMode.Ignore;
        overlayLayer.Add(ve);
      }
      Debug.Log($"[CB] Rebuilt tiles: zones={zones.Count} (debug=inner)");
    }

    public static void SetZone(string key, Rect rect, bool on){
      EnsureRunner();
      float W = Screen.width, H = Screen.height;
      if (on){
        float x = Mathf.Clamp(rect.x, 0f, W);
        float y = Mathf.Clamp(rect.y, 0f, H);
        float w = Mathf.Clamp(rect.width,  0f, W - x);
        float h = Mathf.Clamp(rect.height, 0f, H - y);
        zones[key] = new Rect(x,y,w,h);
      }else{
        if (zones.ContainsKey(key)) zones.Remove(key);
      }
      dirty = true;
    }

    public static void ToggleDebug(){
      debugOverlay = !debugOverlay;
      if (!debugOverlay){
        if (overlayLayer != null) overlayLayer.Clear();
      }else{
        EnsureOverlay();
      }
      dirty = true;
      Debug.Log($"[CB] DebugOverlay={(debugOverlay? "ON":"OFF")}, zones={zones.Count}");
    }
  }
}


