using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace HelperDock
{
  internal static partial class HotkeySafety14
  {
    // --- ここは HotkeySafety14 本体の静的フィールド（s_lastView/s_lastUrl/s_propUrl）を再利用します ---

    static BindingFlags All => BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static|BindingFlags.IgnoreCase;

    static Type GetUIViewType_HYB()
    {
      try {
        return AppDomain.CurrentDomain.GetAssemblies()
          .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
          .FirstOrDefault(t => t.FullName == "Colossal.UI.UIView" ||
                               (t.Name == "UIView" && (t.Namespace ?? "").EndsWith("Colossal.UI", StringComparison.Ordinal)));
      } catch { return null; }
    }

    static (object[] views, int mgrViews, int unityViews) CollectViewsHybrid()
    {
      var views = new List<object>();
      int mgrCnt = 0, unityCnt = 0;

      // 経路A：GameManager.uiManager → m_UISystems → 各 UISystem の View を拾う
      try {
        var gmType = AppDomain.CurrentDomain.GetAssemblies()
          .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
          .FirstOrDefault(t => t.Name == "GameManager");
        object gm = null;
        if (gmType != null) {
          gm = gmType.GetProperty("instance", All)?.GetValue(null)
            ?? gmType.GetProperty("Instance", All)?.GetValue(null)
            ?? gmType.GetField("s_Instance", All)?.GetValue(null)
            ?? gmType.GetField("instance", All)?.GetValue(null);
        }
        object uiMgr = null;
        if (gm != null) {
          var gmt = gm.GetType();
          uiMgr = gmt.GetProperty("uiManager", All)?.GetValue(gm)
               ?? gmt.GetProperty("UIManager", All)?.GetValue(gm)
               ?? gmt.GetField("m_UIManager", All)?.GetValue(gm);
        }
        if (uiMgr != null) {
          mgrCnt = 1;
          var mt = uiMgr.GetType();
          var sysEnum = mt.GetField("m_UISystems", All)?.GetValue(uiMgr) as System.Collections.IEnumerable;
          if (sysEnum != null) {
            foreach (var sys in sysEnum) {
              if (sys == null) continue;
              var st = sys.GetType();
              var v = st.GetField("m_View", All)?.GetValue(sys)
                    ?? st.GetField("m_UIView", All)?.GetValue(sys)
                    ?? st.GetProperty("View", All)?.GetValue(sys)
                    ?? st.GetProperty("view", All)?.GetValue(sys);
              if (v != null && !views.Contains(v)) views.Add(v);
            }
          }
        }
      } catch { /* swallow */ }

      // 経路B：Unity 資源スキャンで UIView を拾う（重複は避ける）
      try {
        var vt = GetUIViewType_HYB();
        if (vt != null) {
          var arr = Resources.FindObjectsOfTypeAll(vt) ?? Array.Empty<object>();
          unityCnt = arr.Length;
          foreach (var v in arr) if (v != null && !views.Contains(v)) views.Add(v);
        }
      } catch { /* swallow */ }

      return (views.ToArray(), mgrCnt, unityCnt);
    }

    internal static void TryOpenViaFound14_Toggle()
    {
      try {
        ModCompat.log.Info("[HelperDock] TRY14: ---- BEGIN ---- (HYB-TOGGLE)");
        var (views, mgr, unity) = CollectViewsHybrid();
        ModCompat.log.Info($"[HelperDock] TRY14: HYB viewsFound={views.Length} (mgrViews={mgr}, unityViews={unity})");
        if (views.Length == 0) { ModCompat.log.Info("[HelperDock] TRY14: no UIView"); return; }

        var view = views[0];
        var vt = view.GetType();
        var p = vt.GetProperty("url", All);
        if (p == null || !p.CanWrite) { ModCompat.log.Info("[HelperDock] TRY14: url property not found/writable"); return; }

        // snapshot
        try {
          s_lastView = view;
          s_propUrl  = p;
          s_lastUrl  = p.CanRead ? (p.GetValue(view) as string) : null;
          ModCompat.log.Info("[HelperDock] SAFE14: snapshot url=" + (s_lastUrl ?? "<null>"));
          // 既に自分のページならトグルで戻す
          var cur = s_lastUrl;
          bool isOur = !string.IsNullOrEmpty(cur) && cur.IndexOf("coui://HelperDock/", StringComparison.OrdinalIgnoreCase) >= 0;
          if (isOur) {
            ModCompat.log.Info("[HelperDock] TRY14: toggle -> revert (already our page)");
            RevertUrl14();
            ModCompat.log.Info("[HelperDock] TRY14: ---- END (toggle revert) ----");
            return;
          }
        } catch (Exception exSnap) {
          ModCompat.log.Info("[HelperDock] SAFE14: snapshot ex=" + exSnap.Message);
        }

        string[] urls = {
          "coui://HelperDock/index.html",
          "UI/HelperDock/index.html",
          "coui://UI/HelperDock/index.html",
        };
        foreach (var u in urls)
        {
          try {
            p.SetValue(view, u);
            ModCompat.log.Info("[HelperDock] OPEN OK via UIView.url -> " + u);
            ModCompat.log.Info("[HelperDock] TRY14: ---- END (success) ----");
            return;
          } catch (Exception exSet) {
            ModCompat.log.Info("[HelperDock] TRY14 set_url fail: " + exSet.Message);
          }
        }
        ModCompat.log.Info("[HelperDock] TRY14: ---- END ----");
      }
      catch (Exception ex) {
        ModCompat.log.Info("[HelperDock] TRY14 EX: " + ex);
      }
    }

    // おまけ: F1用の軽いダンプ（数だけ）
    internal static void DumpViews14_HYB()
    {
      try {
        ModCompat.log.Info("[HelperDock] DUMP14: ---- BEGIN ---- (HYB-DUMP)");
        var (views, mgr, unity) = CollectViewsHybrid();
        ModCompat.log.Info($"[HelperDock] DUMP14: HYB viewsFound={views.Length} (mgrViews={mgr}, unityViews={unity})");
        int i=0;
        foreach (var v in views.Take(5)) {
          var p = v.GetType().GetProperty("url", All);
          bool hasUrl = p != null;
          ModCompat.log.Info($"[HelperDock] DUMP14: VIEW[{i++}]: {v.GetType().FullName} hasUrl={hasUrl}");
        }
        ModCompat.log.Info("[HelperDock] DUMP14: ---- END ----");
      } catch (Exception ex) {
        ModCompat.log.Info("[HelperDock] DUMP14 EX: " + ex);
      }
    }
  }
}
