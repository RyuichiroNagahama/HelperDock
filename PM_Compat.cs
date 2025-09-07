using System;
using UnityEngine;
using Colossal.UI.Binding;
using UnityEngine.InputSystem;

namespace HelperDock
{
  // PM 縺御ｽｿ縺・う繝吶Φ繝亥錐・・S 蛛ｴ縺ｨ蜷医ｏ縺帙∪縺吶よ圻螳壹〒OK・・
  public static class UIEventName
  {
    public const string GroupName          = "helperdock";
    public const string MainButtonClicked  = "helperdock.main_button";
    public const string MainPanelMoved     = "helperdock.panel_moved";
    public const string MainPanelVisible   = "helperdock.panel_visible";
    public const string MainPanelPositionX = "helperdock.panel_pos_x";
    public const string MainPanelPositionY = "helperdock.panel_pos_y";
    public const string CurrentGameMinute  = "helperdock.current_min";
    public const string PreviousGameMinute = "helperdock.prev_min";
    public const string FrameRate          = "helperdock.fps";
    public const string GPUUsage           = "helperdock.gpu";
    public const string CPUUsage           = "helperdock.cpu";
    public const string MemoryUsage        = "helperdock.mem";
    public const string ShowGPUUsage       = "helperdock.show_gpu";
    public const string ShowCPUUsage       = "helperdock.show_cpu";
    public const string ShowMemoryUsage    = "helperdock.show_mem";
  }

  public static class ModCompat
  {
    public static readonly LogProxy log = new LogProxy("[HelperDock]");

    public class LogProxy {
      readonly string tag;
      public LogProxy(string t){ tag=t; }
      public void Info(string m)   => Debug.Log($"{tag} {m}");
      public void Info(object m)   => Debug.Log($"{tag} {m}");
      public void Error(string m)  => Debug.LogError($"{tag} {m}");
      public void Error(Exception ex) => Debug.LogException(ex);
    }

    // PM 縺ｮ ModSettings 譛蟆丈ｺ呈鋤
    public class Settings
    {
      public bool  MainPanelVisible   = true;
      public int MainPanelPositionX = 120;
      public int MainPanelPositionY = 120;

      public bool ShowCPUUsage = false;
      public bool ShowGPUUsage = false;
      public bool ShowMemoryUsage = false;

      public const string ActivationKeyActionName = "HelperDock.Toggle";

      public ProxyAction GetAction(string name) => new ProxyAction(name);
      public void ApplyAndSave() { /* no-op */ }
    }

    public static Settings ModSettings = new Settings();

    // 謨ｰ蛟､・・I 縺ｸ繝舌う繝ｳ繝峨☆繧九ム繝溘・蛟､・・
    public static float CPUUsage = 0f;
    public static float GPUUsage = 0f;
    public static float MemoryUsage = 0f;

    // PM 蛛ｴ縺御ｻ｣蜈･縺励※縺上ｋ繧ｱ繝ｼ繧ｹ縺後≠繧九・縺ｧ setter 繧ら畑諢・
    public static bool ShowCPUUsage    { get => ModSettings.ShowCPUUsage;    set => ModSettings.ShowCPUUsage = value; }
    public static bool ShowGPUUsage    { get => ModSettings.ShowGPUUsage;    set => ModSettings.ShowGPUUsage = value; }
    public static bool ShowMemoryUsage { get => ModSettings.ShowMemoryUsage; set => ModSettings.ShowMemoryUsage = value; }
  }

  // 蜈･蜉帙い繧ｯ繧ｷ繝ｧ繝ｳ縺ｮ譛蟆上ム繝溘・
  public class ProxyAction
  {
    public string Id { get; }
    public Func<bool> shouldBeEnabled { get; set; }
    public Action<ProxyAction, InputActionPhase> onInteraction { get; set; }
    public event Action<ProxyAction, InputActionPhase> performed;

    public ProxyAction(string id){ Id = id; }
    public void Enable() {}
    public void Disable() {}
    public void Trigger(InputActionPhase phase = InputActionPhase.Performed)
    {
      onInteraction?.Invoke(this, phase);
      performed?.Invoke(this, phase);
    }
  }
}

