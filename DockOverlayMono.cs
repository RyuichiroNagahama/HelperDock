#if false
using UnityEngine;
using UnityEngine.InputSystem;using ModCommon;

namespace HelperDock
{
    // 確認用の仮ウィンドウ（IMGUI）
    public sealed class DockOverlayMono : MonoBehaviour
    {
        private static DockOverlayMono _instance;
        private Rect _rect = new Rect(30, 120, 360, 140);
        private static bool _forceShow = false;   // F6 で切替
        private float _nextPulse;

        public static void Ensure()
        {
            if (_instance != null) return;
            var go = new GameObject("HelperDock.Overlay");
            Object.DontDestroyOnLoad(go);
            _instance = go.AddComponent<DockOverlayMono>();
            Debug.Log("[HelperDock] Overlay created");
        }

        void Awake()   { Debug.Log("[HelperDock] Overlay Awake"); }
        void OnEnable(){ Debug.Log("[HelperDock] Overlay OnEnable"); }
        void Start()   { Debug.Log("[HelperDock] Overlay Start"); }

                void Update(){

            // F6 = 強制表示トグル（他の設定に関係なく出す）
            var kb = Keyboard.current;
            if (kb != null && kb.f10Key.wasPressedThisFrame)
            {
                _forceShow = !_forceShow;
                Debug.Log("[HelperDock] Overlay FORCE = " + _forceShow);
            
            if (kb != null && kb.f9Key.wasPressedThisFrame) ModCommon.ClickBlocker.ToggleDebug();
        }

            // 生存パルス（2秒に1回）
            if (Time.unscaledTime >= _nextPulse)
            {
                _nextPulse = Time.unscaledTime + 2f;
                Debug.Log("[HelperDock] Overlay.Update alive (visible=" +
                    (HelperDock.ModCompat.ModSettings.MainPanelVisible || _forceShow) + ")");
            }
        }

        void OnGUI()
        {
            // OnGUI が回っているか軽く記録（Repaint 時のみ）
            if (Event.current != null && Event.current.type == EventType.Repaint)
            {
                // たくさん出ないように depth を毎回設定だけ
                GUI.depth = -9999; // できるだけ最前面
            }

            bool visible = HelperDock.ModCompat.ModSettings.MainPanelVisible || _forceShow;
            ModCommon.ClickBlocker.SetZone("HelperDock", _rect, visible);
            if (!visible) return;

            _rect = ClampToScreen(_rect);
            _rect = GUILayout.Window(0x48444F43, _rect, DoWindow, "HelperDock (IMGUI)");
        }

        private void DoWindow(int id)
        {
            GUILayout.Label("Provisional IMGUI window (cohtml 未実装)");
            GUILayout.Label(_forceShow ? "FORCE SHOW: ON (F10 で切替)" : "FORCE SHOW: OFF (F10 で切替)");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Hide (F4)")) { HelperDock.ModCompat.ModSettings.MainPanelVisible = false; }
            if (GUILayout.Button("Flash")) { StartCoroutine(Flash()); }
            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }

        System.Collections.IEnumerator Flash()
        {
            var old = HelperDock.ModCompat.ModSettings.MainPanelVisible;
            HelperDock.ModCompat.ModSettings.MainPanelVisible = true;
            yield return null;
            HelperDock.ModCompat.ModSettings.MainPanelVisible = old;
        }

        static Rect ClampToScreen(Rect r)
        {
            float w = Screen.width, h = Screen.height;
            if (r.x < 0) r.x = 0;
            if (r.y < 0) r.y = 0;
            if (r.xMax > w) r.x = w - r.width;
            if (r.yMax > h) r.y = h - r.height;
            return r;
        }
    }
}






#endif // DISABLE_IMGUI

