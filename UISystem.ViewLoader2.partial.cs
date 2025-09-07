using System;
using System.Reflection;

namespace HelperDock
{
    public sealed partial class UISystem
    {
        private bool _viewTried2;

        internal void EnsureViewLoaded2()
        {
            if (_viewTried2) return;
            _viewTried2 = true;
            try
            {
                // ★ 自分自身はスキップして基底型から探索（CreateInstance 等の罠回避）
                Type t = this.GetType().BaseType;
                while (t != null)
                {
                    ModCompat.log.Info("[HelperDock] EnsureViewLoaded2: type=" + t.FullName);
                    var methods = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    foreach (var m in methods)
                    {
                        string name = m.Name;

                        // “View” を含む、または Open/Show で始まるものだけに限定
                        bool nameOK =
                            name.IndexOf("View", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            name.StartsWith("Open",  StringComparison.OrdinalIgnoreCase) ||
                            name.StartsWith("Show",  StringComparison.OrdinalIgnoreCase);
                        if (!nameOK) continue;

                        // 典型的な除外ワード
                        if (name.IndexOf("Binding",          StringComparison.OrdinalIgnoreCase) >= 0) continue;
                        if (name.IndexOf("Preview",          StringComparison.OrdinalIgnoreCase) >= 0) continue;
                        if (name.IndexOf("EnsureViewLoaded", StringComparison.OrdinalIgnoreCase) >= 0) continue;
                        if (name.IndexOf("Instance",         StringComparison.OrdinalIgnoreCase) >= 0) continue;
                        if (name.IndexOf("System",           StringComparison.OrdinalIgnoreCase) >= 0) continue;

                        var ps = m.GetParameters();

                        // 少なくとも1つは string パラメータを要求（URL/ID想定）
                        bool hasStr = false; foreach (var p in ps) if (p.ParameterType == typeof(string)) { hasStr = true; break; }
                        if (!hasStr) continue;

                        ModCompat.log.Info("[HelperDock] candidate " + t.Name + "." + name + " (" + ps.Length + " params)");

                        string[] urls = new string[]{
                            "coui://HelperDock/index.html",
                            "UI/HelperDock/index.html"
                        };

                        foreach (var url in urls)
                        {
                            object[] args = BuildArgsV2(ps, url, true);
                            if (args != null)
                            {
                                try { m.Invoke(this, args); ModCompat.log.Info("[HelperDock] VIEW OPEN OK via " + name + " -> " + url); return; }
                                catch (Exception ex) { ModCompat.log.Info("[HelperDock] call " + name + " failed: " + ex.Message); }
                            }
                            args = BuildArgsV2(ps, url, false);
                            if (args != null)
                            {
                                try { m.Invoke(this, args); ModCompat.log.Info("[HelperDock] VIEW OPEN OK via " + name + " -> " + url); return; }
                                catch (Exception ex) { ModCompat.log.Info("[HelperDock] call " + name + " failed: " + ex.Message); }
                            }
                        }
                    }
                    t = t.BaseType;
                }
                ModCompat.log.Info("[HelperDock] EnsureViewLoaded2: done scanning (no success)");
            }
            catch (Exception ex)
            {
                ModCompat.log.Info("[HelperDock] EnsureViewLoaded2 EX: " + ex.ToString());
            }
        }

        private object[] BuildArgsV2(ParameterInfo[] ps, string url, bool firstStringIsId)
        {
            int totalString = 0; foreach (var p in ps) if (p.ParameterType == typeof(string)) totalString++;
            if (totalString == 0) return null;

            object[] args = new object[ps.Length];
            int seenString = 0;
            for (int i = 0; i < ps.Length; i++)
            {
                var pt = ps[i].ParameterType;
                if (pt == typeof(string))
                {
                    if (firstStringIsId && seenString == 0 && totalString >= 2) args[i] = "HelperDock";
                    else if (seenString == totalString - 1) args[i] = url;
                    else args[i] = "HelperDock";
                    seenString++;
                }
                else if (pt == typeof(bool)) args[i] = true;
                else if (pt.IsEnum)          args[i] = Activator.CreateInstance(pt);
                else if (pt.IsValueType)     args[i] = Activator.CreateInstance(pt);
                else                         args[i] = null;
            }
            return args;
        }
    }
}
