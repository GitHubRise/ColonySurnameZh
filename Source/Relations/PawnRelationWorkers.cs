using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ColonySurnameZh
{
    /// <summary>
    /// 派生「情敌」：两人共享同一名恋人/未婚夫（不含配偶——共妻共夫是妻妾关系，不是情敌）。
    /// </summary>
    public class PawnRelationWorker_LoveRival : PawnRelationWorker
    {
        public override bool InRelation(Pawn me, Pawn other)
        {
            if (me?.relations == null || other?.relations == null || me == other)
            {
                return false;
            }

            foreach (Pawn mine in LoversOf(me))
            {
                foreach (Pawn theirs in LoversOf(other))
                {
                    if (mine == theirs)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static IEnumerable<Pawn> LoversOf(Pawn pawn)
        {
            foreach (DirectPawnRelation rel in pawn.relations.DirectRelations)
            {
                if (rel.otherPawn == null)
                {
                    continue;
                }

                if (rel.def == PawnRelationDefOf.Lover || rel.def == PawnRelationDefOf.Fiance)
                {
                    yield return rel.otherPawn;
                }
            }
        }
    }

    /// <summary>
    /// 派生「同门」：两人拜在同一名老师门下（共享 CK_Teacher 关系）。
    /// </summary>
    public class PawnRelationWorker_SameSchool : PawnRelationWorker
    {
        public override bool InRelation(Pawn me, Pawn other)
        {
            if (me?.relations == null || other?.relations == null || me == other)
            {
                return false;
            }

            foreach (Pawn mine in TeachersOf(me))
            {
                foreach (Pawn theirs in TeachersOf(other))
                {
                    if (mine == theirs)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static IEnumerable<Pawn> TeachersOf(Pawn pawn)
        {
            foreach (DirectPawnRelation rel in pawn.relations.DirectRelations)
            {
                if (rel.otherPawn != null && rel.def == CK_DefOf.CK_Teacher)
                {
                    yield return rel.otherPawn;
                }
            }
        }
    }
}
