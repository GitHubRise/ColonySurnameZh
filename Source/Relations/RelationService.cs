using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace ColonySurnameZh
{
    /// <summary>
    /// 关系写入的统一入口：成对 Def 双向写、去重、成对删除。
    /// </summary>
    public static class RelationService
    {
        public static bool HasDirect(Pawn a, Pawn b, PawnRelationDef def)
        {
            return a != null && b != null && def != null && a.relations != null
                && a.relations.DirectRelations.Any(r => r.def == def && r.otherPawn == b);
        }

        public static bool HasAnyDirect(Pawn a, Pawn b)
        {
            if (a?.relations == null || b == null)
            {
                return false;
            }

            return a.relations.DirectRelations.Any(r => r.otherPawn == b)
                || (b.relations != null && b.relations.DirectRelations.Any(r => r.otherPawn == a));
        }

        public static bool IsBloodRelated(Pawn a, Pawn b)
        {
            if (a?.relations == null || b == null)
            {
                return false;
            }

            if (a.relations.FamilyByBlood.Contains(b))
            {
                return true;
            }

            return b.relations != null && b.relations.FamilyByBlood.Contains(a);
        }

        public static bool IsLovePartner(Pawn a, Pawn b)
        {
            if (a?.relations == null || b == null)
            {
                return false;
            }

            return a.relations.DirectRelations.Any(r =>
                r.otherPawn == b
                && (r.def == PawnRelationDefOf.Lover
                    || r.def == PawnRelationDefOf.Fiance
                    || r.def == PawnRelationDefOf.Spouse));
        }

        /// <summary>双向写入一对关系；已存在的一侧会跳过。返回是否有新增。</summary>
        public static bool AddPair(Pawn a, Pawn b, PawnRelationDef aToB, PawnRelationDef bToA)
        {
            if (a == null || b == null || a == b)
            {
                return false;
            }

            // 每种关系在设置里都有独立开关；关掉后不再写入新关系，已有关系保留。
            if (!CK_SettingsUtil.DefEnabled(aToB)
                || (bToA != null && !CK_SettingsUtil.DefEnabled(bToA)))
            {
                return false;
            }

            bool added = false;
            if (!HasDirect(a, b, aToB))
            {
                a.relations.AddDirectRelation(aToB, b);
                added = true;
            }

            if (bToA != null && aToB != bToA && !HasDirect(b, a, bToA))
            {
                b.relations.AddDirectRelation(bToA, a);
                added = true;
            }
            else if (bToA == aToB && !HasDirect(b, a, aToB))
            {
                b.relations.AddDirectRelation(aToB, a);
                added = true;
            }

            return added;
        }

        public static bool AddSymmetric(Pawn a, Pawn b, PawnRelationDef def)
        {
            return AddPair(a, b, def, def);
        }

        public static void RemovePair(Pawn a, Pawn b, PawnRelationDef aToB, PawnRelationDef bToA)
        {
            if (a?.relations != null && aToB != null)
            {
                a.relations.TryRemoveDirectRelation(aToB, b);
            }

            if (b?.relations != null && bToA != null)
            {
                b.relations.TryRemoveDirectRelation(bToA, a);
            }
        }

        public static List<Pawn> Spouses(Pawn pawn)
        {
            return CK_SettingsUtil.SpouseList(pawn);
        }

        /// <summary>
        /// 配偶名次：0 = 正妻/正夫，1 = 侧室/侧夫，-1 = 无法判定。
        /// 判定依据是“配偶的配偶列表里，startTicks 最早的那位”。
        /// </summary>
        public static int SpouseRank(Pawn pawn)
        {
            if (pawn?.relations == null || pawn.gender == Gender.None)
            {
                return -1;
            }

            int result = -1;
            foreach (Pawn spouse in Spouses(pawn))
            {
                if (spouse?.relations == null)
                {
                    continue;
                }

                List<Pawn> mates = Spouses(spouse)
                    .OrderBy(p => StartTicksOf(spouse, p))
                    .ThenBy(p => p.thingIDNumber)
                    .ToList();

                if (mates.Count < 2)
                {
                    continue;
                }

                int index = mates.IndexOf(pawn);
                if (index == 0)
                {
                    if (result < 0)
                    {
                        result = 0;
                    }
                }
                else if (index > 0)
                {
                    return 1;
                }
            }

            return result;
        }

        public static int StartTicksOf(Pawn pawn, Pawn other)
        {
            if (pawn?.relations == null || other == null)
            {
                return int.MaxValue;
            }

            DirectPawnRelation rel = pawn.relations.DirectRelations
                .FirstOrDefault(r => r.def == PawnRelationDefOf.Spouse && r.otherPawn == other);
            return rel?.startTicks ?? int.MaxValue;
        }

        public static int CountCustom(Pawn pawn)
        {
            if (pawn?.relations == null)
            {
                return 0;
            }

            return pawn.relations.DirectRelations.Count(r =>
                r.def != null && r.def.defName.StartsWith("CK_", StringComparison.Ordinal));
        }

        /// <summary>新关系是否可以加：双方人类、非同一人、无任何直接关系、非血亲。</summary>
        public static bool CanRelate(Pawn a, Pawn b)
        {
            return a != null && b != null && a != b
                && a.RaceProps != null && a.RaceProps.Humanlike
                && b.RaceProps != null && b.RaceProps.Humanlike
                && a.relations != null && b.relations != null
                && !HasAnyDirect(a, b)
                && !IsBloodRelated(a, b);
        }
    }
}
