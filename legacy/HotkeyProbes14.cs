using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HelperDock {
  internal static class HotkeyProbes14 {

    static (object gm, Type gmType, object uiMgr, Type uiType) GetUiMgr14(){
      object gm=null; Type gmType=null, uiType=null; object uiMgr=null;
      try{
        gmType = AppDomain.CurrentDomain.GetAssemblies()
          .SelectMany(a=>{ try{ return a.GetTypes(); } catch{ return Array.Empty<Type>(); } })
          .FirstOrDefault(t => t.Name=="GameManager" || ((t.FullName??"").EndsWith(".GameManager", StringComparison.Ordinal)));
        if(gmType!=null){
          var ip = gmType.GetProperty("instance", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static);
          gm = ip?.GetValue(null) ?? gmType.GetField("instance", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static)?.GetValue(null);
        }
        if(gm!=null){
          var flags = BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.IgnoreCase;
          uiMgr =  gmType.GetProperty("uiManager",flags)?.GetValue(gm)
               ??  gmType.GetProperty("m_UIManager",flags)?.GetValue(gm)
               ??  gmType.GetField   ("uiManager",flags)?.GetValue(gm)
               ??  gmType.GetField   ("m_UIManager",flags)?.GetValue(gm);
          uiType = uiMgr?.GetType();
        }
      }catch{}
      return (gm,gmType,uiMgr,uiType);
    }

    static bool LooksLikeUIViewType(Type t){
      if(t==null) return false;
      var n  = t.Name;
      var fn = t.FullName ?? "";
      var asm= t.Assembly.GetName().Name ?? "";
      return  n.IndexOf("UIView", StringComparison.OrdinalIgnoreCase)>=0
           || fn.IndexOf(".UIView", StringComparison.OrdinalIgnoreCase)>=0
           || (n.IndexOf("View", StringComparison.OrdinalIgnoreCase)>=0
               && (asm.IndexOf("cohtml",StringComparison.OrdinalIgnoreCase)>=0
                   || asm.IndexOf("Colossal.UI",StringComparison.OrdinalIgnoreCase)>=0));
    }

    static bool LooksLikeLoaderName(string n){
      return n.IndexOf("Load",StringComparison.OrdinalIgnoreCase)>=0
          || n.IndexOf("Navigate",StringComparison.OrdinalIgnoreCase)>=0
          || n.IndexOf("Url",StringComparison.OrdinalIgnoreCase)>=0
          || n.IndexOf("URI",StringComparison.OrdinalIgnoreCase)>=0
          || n.StartsWith("Open",StringComparison.OrdinalIgnoreCase)
          || n.StartsWith("Show",StringComparison.OrdinalIgnoreCase)
          || n.IndexOf("View",StringComparison.OrdinalIgnoreCase)>=0;
    }

    static object[] BuildArgsFor14(MethodInfo m, string url){
      var ps = m.GetParameters();
      var args = new object[ps.Length];
      int totalStr = ps.Count(p=>p.ParameterType==typeof(string));
      int seenStr = 0;
      for(int i=0;i<ps.Length;i++){
        var pt = ps[i].ParameterType;
        if(pt==typeof(string)){
          if(totalStr==1) args[i]=url;
          else if(seenStr==totalStr-1) args[i]=url;
          else args[i]="HelperDock";
          seenStr++;
        } else if(pt==typeof(bool)) args[i]=true;
        else if(pt.IsEnum) args[i]=Activator.CreateInstance(pt);
        else if(pt.IsValueType) args[i]=Activator.CreateInstance(pt);
        else args[i]=null;
      }
      return args;
    }

    // UIManager 配下を浅く探索して UIView 候補を収集（IEnumerableは軽く展開）— 再帰は2段まで
    static List<object> CollectUIViews14(object root, int maxDepth=2){
      var list = new List<object>();
      var seen = new HashSet<int>();
      void Add(object o){
        if(o==null) return;
        int id = RuntimeHelpers.GetHashCode(o);
        if(!seen.Add(id)) return;
        if(LooksLikeUIViewType(o.GetType())) list.Add(o);
      }
      void Walk(object o, int d){
        if(o==null || d>maxDepth) return;
        try{
          // 直接 UIView なら登録
          if(LooksLikeUIViewType(o.GetType())){ Add(o); return; }

          // IEnumerable を展開
          if(o is IEnumerable en){
            foreach(var x in en){
              if(x==null) continue;
              if(LooksLikeUIViewType(x.GetType())) Add(x);
              else if(d<maxDepth) Walk(x, d+1);
            }
          }

          // Fields / Properties を覗く
          var t=o.GetType();
          var flags = BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
          foreach(var f in t.GetFields(flags)){
            object val=null; try{ val=f.GetValue(o);}catch{}
            if(val==null) continue;
            if(LooksLikeUIViewType(val.GetType())) Add(val);
            else if(d<maxDepth) Walk(val, d+1);
          }
          foreach(var p in t.GetProperties(flags)){
            if(p.GetIndexParameters().Length>0) continue;
            object val=null; try{ val=p.GetValue(o);}catch{}
            if(val==null) continue;
            if(LooksLikeUIViewType(val.GetType())) Add(val);
            else if(d<maxDepth) Walk(val, d+1);
          }
        }catch{}
      }
      Walk(root,0);
      // 重複除去済み
      return list;
    }

    internal static void DumpViews14(){
      try{
        ModCompat.log.Info("[HelperDock] DUMP14: ---- BEGIN ----");
        var tup = GetUiMgr14();
        if(tup.uiMgr==null){ ModCompat.log.Info("[HelperDock] DUMP14: uiMgr=<null>"); ModCompat.log.Info("[HelperDock] DUMP14: ---- END ----"); return; }
        ModCompat.log.Info("[HelperDock] DUMP14: uiMgr="+tup.uiType.FullName);

        var views = CollectUIViews14(tup.uiMgr, 2);
        ModCompat.log.Info("[HelperDock] DUMP14: found="+views.Count);

        int idx=0;
        foreach(var v in views.Take(50)){ // ログ爆発防止で50件まで
          var vt = v.GetType();
          ModCompat.log.Info("[HelperDock] DUMP14: VIEW["+idx+"]: "+vt.FullName+" (asm="+vt.Assembly.GetName().Name+")");
          foreach(var m in vt.GetMethods(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic)){
            if(!m.GetParameters().Any(p=>p.ParameterType==typeof(string))) continue;
            var looks = LooksLikeLoaderName(m.Name);
            var sig = string.Join(", ", m.GetParameters().Select(p=>p.ParameterType.Name+" "+p.Name));
            ModCompat.log.Info($"[HelperDock] DUMP14:   M {m.Name}({sig}) -> {m.ReturnType.Name} [looks={looks}]");
          }
          idx++;
        }
        ModCompat.log.Info("[HelperDock] DUMP14: ---- END ----");
      }catch(Exception ex){
        ModCompat.log.Info("[HelperDock] DUMP14 EX: "+ex);
      }
    }

    internal static void TryOpenViaFound14(){
      try{
        ModCompat.log.Info("[HelperDock] TRY14: ---- BEGIN ----");
        var tup = GetUiMgr14();
        if(tup.uiMgr==null){ ModCompat.log.Info("[HelperDock] TRY14: uiMgr=<null>"); ModCompat.log.Info("[HelperDock] TRY14: ---- END ----"); return; }

        var views = CollectUIViews14(tup.uiMgr, 2);
        ModCompat.log.Info("[HelperDock] TRY14: candidates="+views.Count);
        if(views.Count==0){ ModCompat.log.Info("[HelperDock] TRY14: no UIView"); ModCompat.log.Info("[HelperDock] TRY14: ---- END ----"); return; }

        string[] urls = {
          "coui://HelperDock/index.html",
          "UI/HelperDock/index.html",
          "coui://UI/HelperDock/index.html",
          "coui://helperdock/index.html",
          "UI/helperdock/index.html",
          "coui://UI/helperdock/index.html",
        };

        foreach(var v in views){
          var vt = v.GetType();
          foreach(var m in vt.GetMethods(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic)){
            if(!LooksLikeLoaderName(m.Name)) continue;
            if(!m.GetParameters().Any(p=>p.ParameterType==typeof(string))) continue;

            foreach(var url in urls){
              try{
                var a = BuildArgsFor14(m, url);
                m.Invoke(v, a);
                ModCompat.log.Info("[HelperDock] OPEN OK via "+vt.Name+"."+m.Name+" -> "+url);
                ModCompat.log.Info("[HelperDock] TRY14: ---- END (success) ----");
                return;
              }catch(Exception ex){
                ModCompat.log.Info("[HelperDock] TRY14: "+vt.Name+"."+m.Name+" fail: "+ex.Message);
              }
            }
          }
        }
        ModCompat.log.Info("[HelperDock] TRY14: no method worked");
        ModCompat.log.Info("[HelperDock] TRY14: ---- END ----");
      }catch(Exception ex){
        ModCompat.log.Info("[HelperDock] TRY14 EX: "+ex);
      }
    }
  }
}
