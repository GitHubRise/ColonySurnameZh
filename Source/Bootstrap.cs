using System;
using HarmonyLib;
using Verse;

namespace ColonySurnameZh
{
    /// <summary>
    /// 启动入口。Def 解析完成、DefOf 绑定之后执行一次。
    /// </summary>
    [StaticConstructorOnStartup]
    public static class Bootstrap
    {
        static Bootstrap()
        {
            try
            {
                new Harmony("local.colonysurnamezh").PatchAll(typeof(Bootstrap).Assembly);
                Log.Message("[ColonySurnameZh] Harmony patches applied.");
            }
            catch (Exception e)
            {
                Log.Error("[ColonySurnameZh] Harmony PatchAll failed: " + e);
            }
        }
    }
}
