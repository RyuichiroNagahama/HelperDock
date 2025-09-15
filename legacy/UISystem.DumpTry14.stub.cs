using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace HelperDock {
  public sealed partial class UISystem {

    static object _lastView14;
    static Type   _lastType14;
    static string _lastUrl14;

    Type GetUIViewType14(){
      return AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
        .FirstOrDefault(t =>
          t.Name == "UIView" &&
          (t.Namespace ?? "").IndexOf("Colossal.UI", StringComparison.OrdinalIgnoreCase) >= 0);
    }

    object[] FindUnityViews14(Type viewT){
      try{
        var objT = typeof(UnityEngine.Object);
        // Unity 2022+: (Type,bool) があれば優先（非アクティブも含める）
        var m2 = objT.GetMethod("FindObjectsOfType", new[]{ typeof(Type), typeof(bool) });
        if(m2!=null){
          var arr = (Array)m2.Invoke(null, new object[]{ viewT, true });
          return arr == null ? Array.Empty<object>() : arr.Cast<object>().ToArray();
        }
        // フォールバック: (Type)
        var m1 = objT.GetMethod("FindObjectsOfType", new[]{ typeof(Type) });
        if(m1!=null){
          var arr = (Array)m1.Invoke(null, new object[]{ viewT });
          return arr == null ? Array.Empty<object>() : arr.Cast<object>().ToArray();
        }
      }catch(Exception ex){
        ModCompat.log.Info("[HelperDock] DUMP14: unity find ex=" + ex.Message);
      }
      return Array.Empty<object>();
    }

    string GetUrl14(object v, Type vt){
      var flags = BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.IgnoreCase;
      try { var p = vt.GetProperty("url", flags);   if(p!=null) return p.GetValue(v) as string; } catch {}
      try { var f = vt.GetField   ("url", flags);   if(f!=null) return f.GetValue(v) as string; } catch {}
      try { var p = vt.GetProperty("m_Url", flags); if(p!=null) return p.GetValue(v) as string; } catch {}
      try { var f = vt.GetField   ("m_Url", flags); if(f!=null) return f.GetValue(v) as string; } catch {}
      return null;
    }

    bool TrySetUrl14(object v, Type vt, string url){
      var flags = BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.IgnoreCase;
      try { var p = vt.GetProperty("url", flags); if(p!=null && p.CanWrite){ p.SetValue(v, url); return true; } } catch (Exception ex) { ModCompat.log.Info("[HelperDock] TRY14: set url(prop) ex="+ex.Message); }
      try { var m = vt.GetMethod  ("set_url", flags); if(m!=null){ m.Invoke(v, new object[]{ url }); return true; } } catch (Exception ex) { ModCompat.log.Info("[HelperDock] TRY14: set_url(m) ex="+ex.Message); }
      try { var f = vt.GetField   ("url", flags); if(f!=null){ f.SetValue(v, url); return true; } } catch {}
      try { var f = vt.GetField   ("m_Url", flags); if(f!=null){ f.SetValue(v, url); return true; } } catch {}
      return false;
    }

    internal void DumpViews14(){
      try{
        ModCompat.log.Info("[HelperDock] DUMP14: ---- BEGIN ----");
        var viewT = GetUIViewType14();
        if(viewT == null){
          ModCompat.log.Info("[HelperDock] DUMP14: UIView type not found");
          ModCompat.log.Info("[HelperDock] DUMP14: ---- END ----");
          return;
        }
        var views = FindUnityViews14(viewT);
        ModCompat.log.Info("[HelperDock] DUMP14: unityFound="+views.Length);
        for(int i=0;i<views.Length;i++){
          var v  = views[i];
          var vt = v.GetType();
          var url = GetUrl14(v, vt) ?? "<null>";
          ModCompat.log.Info($"[HelperDock] DUMP14: VIEW[{i}]: {vt.FullName} url='{url}'");
          foreach(var m in vt.GetMethods(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic)){
            if(!m.GetParameters().Any(p=>p.ParameterType==typeof(string))) continue;
            bool looks = m.Name.IndexOf("url", StringComparison.OrdinalIgnoreCase)>=0
                      || m.Name.IndexOf("load", StringComparison.OrdinalIgnoreCase)>=0
                      || m.Name.StartsWith("set_", StringComparison.OrdinalIgnoreCase);
            if(looks){
              var sig = string.Join(", ", m.GetParameters().Select(p=>p.ParameterType.Name+" "+p.Name));
              ModCompat.log.Info($"[HelperDock] DUMP14:   M {m.Name}({sig}) -> {m.ReturnType.Name} [looks={looks}]");
            }
          }
        }
        ModCompat.log.Info("[HelperDock] DUMP14: ---- END ----");
      }catch(Exception ex){
        ModCompat.log.Info("[HelperDock] DUMP14 EX: "+ex);
      }
    }

    internal void TryOpenViaFound14(){
      try{
        ModCompat.log.Info("[HelperDock] TRY14: ---- BEGIN ----");
        var viewT = GetUIViewType14();
        if(viewT == null){ ModCompat.log.Info("[HelperDock] TRY14: UIView type not found"); return; }
        var views = FindUnityViews14(viewT);
        if(views.Length == 0){ ModCompat.log.Info("[HelperDock] TRY14: no UIView"); return; }

        // できれば「空っぽ/プレースホルダー」っぽいURLのビューを優先
        object target = null; string prev = null;
        foreach(var v in views){
          var vt  = v.GetType();
          var cur = GetUrl14(v, vt);
          if(string.IsNullOrEmpty(cur) || cur.IndexOf("empty", StringComparison.OrdinalIgnoreCase)>=0 || cur.IndexOf("blank", StringComparison.OrdinalIgnoreCase)>=0){
            target = v; prev = cur; break;
          }
        }
        if(target == null){ target = views[0]; prev = GetUrl14(target, target.GetType()); }

        _lastView14 = target;
        _lastType14 = target.GetType();
        _lastUrl14  = prev;

        var ok = TrySetUrl14(target, _lastType14, "coui://HelperDock/index.html");
        if(ok){
          ModCompat.log.Info("[HelperDock] OPEN OK via "+_lastType14.Name+".set_url -> coui://HelperDock/index.html");
          ModCompat.log.Info("[HelperDock] TRY14: ---- END (success) ----");
        } else {
          ModCompat.log.Info("[HelperDock] TRY14: failed to set url");
        }
      }catch(Exception ex){
        ModCompat.log.Info("[HelperDock] TRY14 EX: "+ex);
      }
    }
  }
}
