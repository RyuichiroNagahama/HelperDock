using System;

namespace HelperDock
{
  public static class CPUUsage
  {
    public static bool Initialize() { return true; }
    public static float GetCPUUsage() { return ModCompat.CPUUsage; }
  }

  public static class GPUUsage
  {
    // オーバーロード（引数あり/なし両対応）
    public static bool Initialize() { return true; }
    public static bool Initialize(params object[] _) { return true; }
    public static float GetGPUUsage() { return ModCompat.GPUUsage; }
  }

  public static class MemoryUsage
  {
    public static bool Initialize() { return true; }
    public static float GetMemoryUsage() { return ModCompat.MemoryUsage; }
  }
}
