using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ColonySurnameZh
{
    /// <summary>婚礼流程内的临时标记：只有本 Mod 触发的那次 Married 才跳过“旧配偶转前任”。</summary>
    internal static class CK_MarriageScope
    {
        public static bool Active;
    }

    [HarmonyPatch(typeof(MarriageCeremonyUtility), nameof(MarriageCeremonyUtility.Married))]
    internal static class Patch_MarriageCeremony
    {
        private static void Prefix()
        {
            CK_MarriageScope.Active = true;
        }

        private static void Postfix(Pawn firstPawn, Pawn secondPawn)
        {
            CK_MarriageScope.Active = false;
            CK_MarriageService.AfterMarriage(firstPawn, secondPawn);
        }
    }

    [HarmonyPatch(typeof(LovePartnerRelationUtility), nameof(LovePartnerRelationUtility.ChangeSpouseRelationsToExSpouse))]
    internal static class Patch_ChangeSpouseToEx
    {
        private static bool Prefix(Pawn pawn)
        {
            // 返回 false 跳过原方法：婚配制度允许时保留旧配偶。
            return !(CK_MarriageScope.Active && CK_SettingsUtil.AllowsAdditionalSpouse(pawn));
        }
    }

    [HarmonyPatch(typeof(InteractionWorker_MarriageProposal), "RandomSelectionWeight")]
    internal static class Patch_MarriageProposalWeight
    {
        private static void Postfix(Pawn initiator, Pawn recipient, ref float __result)
        {
            if (__result <= 0f && CK_MarriageService.CanPropose(initiator, recipient))
            {
                __result = 0.25f;
            }
        }
    }

    [HarmonyPatch(typeof(InteractionWorker_MarriageProposal), "AcceptanceChance")]
    internal static class Patch_MarriageProposalAcceptance
    {
        private static void Postfix(Pawn initiator, Pawn recipient, ref float __result)
        {
            if (CK_MarriageService.CanPropose(initiator, recipient))
            {
                __result = Mathf.Max(__result, 0.85f);
            }
        }
    }

    public static class CK_MarriageService
    {
        public static bool CanPropose(Pawn initiator, Pawn recipient)
        {
            if (CK_SettingsUtil.MarriageMode == CK_MarriageMode.Vanilla)
            {
                return false;
            }

            if (initiator == null || recipient == null || initiator == recipient)
            {
                return false;
            }

            if (!RelationService.IsLovePartner(initiator, recipient))
            {
                return false;
            }

            int existing = CK_SettingsUtil.SpouseList(initiator).Count + CK_SettingsUtil.SpouseList(recipient).Count;
            if (existing == 0)
            {
                return false;
            }

            return CK_SettingsUtil.AllowsAdditionalSpouse(initiator)
                || CK_SettingsUtil.AllowsAdditionalSpouse(recipient);
        }

        public static void AfterMarriage(Pawn first, Pawn second)
        {
            if (CK_SettingsUtil.MarriageMode == CK_MarriageMode.Vanilla)
            {
                return;
            }

            AddCoSpouseRelations(first);
            AddCoSpouseRelations(second);
        }

        private static void AddCoSpouseRelations(Pawn pawn)
        {
            List<Pawn> mates = RelationService.Spouses(pawn)
                .Where(p => p != null)
                .OrderBy(p => RelationService.StartTicksOf(pawn, p))
                .ThenBy(p => p.thingIDNumber)
                .ToList();

            if (mates.Count < 2)
            {
                return;
            }

            Pawn senior = mates[0];
            for (int i = 1; i < mates.Count; i++)
            {
                Pawn junior = mates[i];

                bool added = RelationService.AddPair(
                    senior, junior, CK_DefOf.CK_JuniorSpouse, CK_DefOf.CK_SeniorSpouse);

                if (added)
                {
                    junior.needs?.mood?.thoughts?.memories?.TryGainMemory(CK_DefOf.CK_Thought_Concubine, senior);
                    senior.needs?.mood?.thoughts?.memories?.TryGainMemory(CK_DefOf.CK_Thought_SpouseTookSpouse, junior);

                    Messages.Message(
                        "CK.Msg.Concubine".Translate(junior.LabelShort, pawn.LabelShort, senior.LabelShort),
                        MessageTypeDefOf.NeutralEvent);
                }

                for (int j = i + 1; j < mates.Count; j++)
                {
                    RelationService.AddSymmetric(mates[i], mates[j], CK_DefOf.CK_CoSpouse);
                }
            }
        }
    }

    /// <summary>
    /// 配偶标签：只改“配偶”这一条 Def 的显示文本，正/侧按最早 startTicks 排。
    /// </summary>
    [HarmonyPatch(typeof(PawnRelationDef), nameof(PawnRelationDef.GetGenderSpecificLabel))]
    internal static class Patch_SpouseLabel
    {
        private static void Postfix(PawnRelationDef __instance, Pawn pawn, ref string __result)
        {
            if (__instance != PawnRelationDefOf.Spouse || pawn == null)
            {
                return;
            }

            if (CK_SettingsUtil.MarriageMode == CK_MarriageMode.Vanilla)
            {
                return;
            }

            int rank = RelationService.SpouseRank(pawn);
            if (rank == 0)
            {
                __result = pawn.gender == Gender.Female
                    ? "CK.Rel.Label.SeniorWife".Translate()
                    : "CK.Rel.Label.SeniorHusband".Translate();
            }
            else if (rank == 1)
            {
                __result = pawn.gender == Gender.Female
                    ? "CK.Rel.Label.JuniorWife".Translate()
                    : "CK.Rel.Label.JuniorHusband".Translate();
            }
        }
    }
}
