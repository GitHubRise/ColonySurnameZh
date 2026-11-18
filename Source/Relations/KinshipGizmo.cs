using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ColonySurnameZh
{
    /// <summary>
    /// 「认义亲」：选中一名成年殖民者，右键另一名年龄小至少 16 岁的殖民者，结为义父/义母与义子/义女。
    /// </summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.GetGizmos))]
    internal static class Patch_GetGizmos
    {
        private static void Postfix(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            if (!CanShow(__instance))
            {
                return;
            }

            Command_Action command = new Command_Action
            {
                defaultLabel = "CK.Gizmo.AdoptGodparent".Translate(),
                defaultDesc = "CK.Gizmo.AdoptGodparent.Desc".Translate(),
                action = delegate { BeginTargeting(__instance); }
            };

            __result = __result.Concat(new Gizmo[] { command });
        }

        private static bool CanShow(Pawn pawn)
        {
            return pawn != null
                && pawn.Spawned
                && pawn.IsFreeColonist
                && pawn.Faction == Faction.OfPlayer
                && pawn.RaceProps != null
                && pawn.RaceProps.Humanlike
                && pawn.ageTracker != null
                && pawn.ageTracker.AgeBiologicalYearsFloat >= 16f;
        }

        private static void BeginTargeting(Pawn adopter)
        {
            TargetingParameters parameters = new TargetingParameters
            {
                canTargetPawns = true,
                canTargetHumans = true,
                canTargetAnimals = false,
                canTargetBuildings = false,
                canTargetLocations = false,
                validator = target => target.HasThing && IsValidTarget(adopter, target.Thing as Pawn)
            };

            Find.Targeter.BeginTargeting(parameters, delegate(LocalTargetInfo target)
            {
                TryAdopt(adopter, target.Thing as Pawn);
            });
        }

        private static bool IsValidTarget(Pawn adopter, Pawn target)
        {
            if (adopter == null || target == null || adopter == target)
            {
                return false;
            }

            if (target.Faction != Faction.OfPlayer || !target.IsFreeColonist)
            {
                return false;
            }

            if (!target.RaceProps.Humanlike)
            {
                return false;
            }

            float gap = adopter.ageTracker.AgeBiologicalYearsFloat - target.ageTracker.AgeBiologicalYearsFloat;
            if (gap < 16f)
            {
                return false;
            }

            if (RelationService.HasAnyDirect(adopter, target)
                || RelationService.IsBloodRelated(adopter, target)
                || RelationService.IsLovePartner(adopter, target))
            {
                return false;
            }

            if (adopter.relations.OpinionOf(target) < 20 || target.relations.OpinionOf(adopter) < 20)
            {
                return false;
            }

            return CountGodchildren(adopter) < 3 && CountGodparents(target) < 3;
        }

        private static void TryAdopt(Pawn adopter, Pawn target)
        {
            if (!IsValidTarget(adopter, target))
            {
                Messages.Message("CK.Gizmo.AdoptGodparent.Invalid".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }

            // 义子/义女 → 义父/义母：Godparent 挂在孩子一侧，标签才是「义父/义母」。
            bool added = RelationService.AddPair(
                target, adopter, CK_DefOf.CK_Godparent, CK_DefOf.CK_Godchild);

            if (added)
            {
                Messages.Message(
                    "CK.Msg.Godparent".Translate(target.LabelShort, adopter.LabelShort),
                    MessageTypeDefOf.PositiveEvent);
            }
        }

        private static int CountGodchildren(Pawn pawn)
        {
            return pawn.relations.DirectRelations.Count(r => r.def == CK_DefOf.CK_Godchild);
        }

        private static int CountGodparents(Pawn pawn)
        {
            return pawn.relations.DirectRelations.Count(r => r.def == CK_DefOf.CK_Godparent);
        }
    }
}
