using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ColonySurnameZh
{
    /// <summary>
    /// 生成阶段：在 PawnBioAndNameGenerator 产出 NameTriple 之后替换为中文姓名。
    /// </summary>
    [HarmonyPatch(typeof(PawnBioAndNameGenerator), nameof(PawnBioAndNameGenerator.GeneratePawnName))]
    internal static class Patch_GeneratePawnName
    {
        private static void Postfix(Pawn pawn, NameStyle style, string forcedLastName, bool forceNoNick, ref Name __result)
        {
            if (!CK_SettingsUtil.NameEnabled)
            {
                return;
            }

            if (style != NameStyle.Full)
            {
                return;
            }

            NameTriple triple = __result as NameTriple;
            if (triple == null)
            {
                return;
            }

            if (pawn == null || pawn.RaceProps == null || !pawn.RaceProps.Humanlike || pawn.IsCreepJoiner)
            {
                return;
            }

            if (!ScopeAllows(pawn))
            {
                return;
            }

            if (Rand.Value >= CK_SettingsUtil.ChineseChance)
            {
                return;
            }

            __result = ChineseNameGenerator.Generate(pawn.gender, forcedLastName, forceNoNick);
        }

        private static bool ScopeAllows(Pawn pawn)
        {
            switch (CK_SettingsUtil.Scope)
            {
                case CK_NameScope.PlayerFaction:
                    return pawn.Faction == Faction.OfPlayer;
                case CK_NameScope.HumanRace:
                    return pawn.def == ThingDefOf.Human;
                default:
                    return true;
            }
        }
    }

    /// <summary>
    /// 显示层：中文名统一输出「姓 + 名」，短名用小名/名。
    /// </summary>
    [HarmonyPatch(typeof(NameTriple), "get_ToStringFull")]
    internal static class Patch_ToStringFull
    {
        private static void Postfix(NameTriple __instance, ref string __result)
        {
            if (ChineseNameGenerator.IsChineseName(__instance))
            {
                __result = __instance.Last + __instance.First;
            }
        }
    }

    [HarmonyPatch(typeof(NameTriple), "get_ToStringShort")]
    internal static class Patch_ToStringShort
    {
        private static void Postfix(NameTriple __instance, ref string __result)
        {
            if (ChineseNameGenerator.IsChineseName(__instance))
            {
                // 没有独立小名时（nick 与 first 相同）显示全名，避免地图上只剩「名」。
                __result = __instance.Nick == __instance.First
                    ? __instance.Last + __instance.First
                    : __instance.Nick;
            }
        }
    }

    [HarmonyPatch(typeof(NameTriple), "get_Nick")]
    internal static class Patch_Nick
    {
        private static void Postfix(NameTriple __instance, ref string __result)
        {
            if (!__instance.NickSet && ChineseNameGenerator.IsChineseName(__instance))
            {
                __result = __instance.First;
            }
        }
    }

    [HarmonyPatch(typeof(NameTriple), nameof(NameTriple.ToString))]
    internal static class Patch_NameToString
    {
        private static void Postfix(NameTriple __instance, ref string __result)
        {
            if (ChineseNameGenerator.IsChineseName(__instance))
            {
                __result = __instance.Last + __instance.First;
            }
        }
    }
}
