using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace ColonySurnameZh
{
    /// <summary>
    /// 鍏崇郴鍚庡彴锛氭柊娈栨皯鑰呮棫璇嗘挱绉?+ 姣忔棩銆岃嚜鍔ㄧ粨鎷?/ 濉戞枡闂鸿湝鍙嶇洰銆嶇粨绠椼€?    /// </summary>
    public class CK_RelationGameComponent : GameComponent
    {
        private const int SeedIntervalTicks = 2500;
        private const int DailyIntervalTicks = 60000;
        private const int MaxSeededPerPawn = 2;

        private List<int> seededPawnIds = new List<int>();
        private List<CK_PairStreak> streaks = new List<CK_PairStreak>();
        private HashSet<int> seeded = new HashSet<int>();
        private int lastSeedTick;
        private int lastDailyTick;

        public CK_RelationGameComponent(Game game)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref seededPawnIds, "ck_seededPawnIds", LookMode.Value);
            Scribe_Collections.Look(ref streaks, "ck_streaks", LookMode.Deep);
            Scribe_Values.Look(ref lastDailyTick, "ck_lastDailyTick", 0);

            if (seededPawnIds == null)
            {
                seededPawnIds = new List<int>();
            }

            if (streaks == null)
            {
                streaks = new List<CK_PairStreak>();
            }

            seeded = new HashSet<int>(seededPawnIds);
        }

        public override void GameComponentTick()
        {
            if (!CK_SettingsUtil.RelationsEnabled)
            {
                return;
            }

            int now = Find.TickManager.TicksGame;
            if (now - lastSeedTick >= SeedIntervalTicks)
            {
                lastSeedTick = now;
                SeedNewPawns();
            }

            if (now - lastDailyTick >= DailyIntervalTicks)
            {
                lastDailyTick = now;
                DailyTick();
            }
        }

        private void SeedNewPawns()
        {
            float chance = CK_SettingsUtil.OldAcquaintanceChance;
            if (chance <= 0f)
            {
                return;
            }

            foreach (Map map in Find.Maps)
            {
                if (map == null || !map.IsPlayerHome)
                {
                    continue;
                }

                List<Pawn> colonists = map.mapPawns.FreeColonistsSpawned;
                for (int i = 0; i < colonists.Count; i++)
                {
                    Pawn pawn = colonists[i];
                    if (pawn == null || seeded.Contains(pawn.thingIDNumber))
                    {
                        continue;
                    }

                    seeded.Add(pawn.thingIDNumber);
                    seededPawnIds.Add(pawn.thingIDNumber);
                    TrySeed(pawn, colonists, chance);
                }
            }
        }

        private void TrySeed(Pawn pawn, List<Pawn> colonists, float chance)
        {
            if (RelationService.CountCustom(pawn) >= 5)
            {
                return;
            }

            List<Pawn> candidates = colonists
                .Where(p => p != null && p != pawn)
                .OrderBy(_ => Rand.Value)
                .ToList();

            int added = 0;
            foreach (Pawn other in candidates)
            {
                if (added >= MaxSeededPerPawn)
                {
                    break;
                }

                if (!RelationService.CanRelate(pawn, other))
                {
                    continue;
                }

                if (Rand.Value > chance)
                {
                    continue;
                }

                if (TryAddSpecial(pawn, other))
                {
                    added++;
                    continue;
                }

                if (Rand.Value < 0.10f)
                {
                    if (RelationService.AddSymmetric(pawn, other, CK_DefOf.CK_Rival))
                    {
                        added++;
                    }
                    continue;
                }

                PawnRelationDef forward;
                PawnRelationDef backward;
                PickPositive(pawn, other, out forward, out backward);
                if (RelationService.AddPair(pawn, other, forward, backward))
                {
                    added++;
                }
            }
        }

        private static bool TryAddSpecial(Pawn pawn, Pawn other)
        {
            if (!CK_SettingsUtil.FrenemyEnabled)
            {
                return false;
            }

            if (pawn.gender != Gender.Female || other.gender != Gender.Female)
            {
                return false;
            }

            float ageGap = Math.Abs(pawn.ageTracker.AgeBiologicalYearsFloat - other.ageTracker.AgeBiologicalYearsFloat);
            if (ageGap > 8f || Rand.Value >= 0.10f)
            {
                return false;
            }

            return RelationService.AddSymmetric(pawn, other, CK_DefOf.CK_Frenemy);
        }

        private static void PickPositive(Pawn a, Pawn b, out PawnRelationDef forward, out PawnRelationDef backward)
        {
            forward = CK_DefOf.CK_Friend;
            backward = CK_DefOf.CK_Friend;

            float ageA = a.ageTracker.AgeBiologicalYearsFloat;
            float ageB = b.ageTracker.AgeBiologicalYearsFloat;
            float ageGap = Math.Abs(ageA - ageB);

            if (ageGap >= 10f && Rand.Value < 0.08f)
            {
                if (ageA > ageB)
                {
                    forward = CK_DefOf.CK_Master;
                    backward = CK_DefOf.CK_Apprentice;
                }
                else
                {
                    forward = CK_DefOf.CK_Apprentice;
                    backward = CK_DefOf.CK_Master;
                }
                return;
            }

            float roll = Rand.Value * 100f;
            if (roll < 24f)
            {
                forward = CK_DefOf.CK_Friend;
                backward = CK_DefOf.CK_Friend;
            }
            else if (roll < 40f && ageGap <= 8f)
            {
                forward = CK_DefOf.CK_Classmate;
                backward = CK_DefOf.CK_Classmate;
            }
            else if (roll < 51f && ageGap <= 12f)
            {
                forward = CK_DefOf.CK_ChildhoodFriend;
                backward = CK_DefOf.CK_ChildhoodFriend;
            }
            else if (roll < 63f)
            {
                forward = CK_DefOf.CK_Coworker;
                backward = CK_DefOf.CK_Coworker;
            }
            else if (roll < 73f)
            {
                forward = CK_DefOf.CK_NetFriend;
                backward = CK_DefOf.CK_NetFriend;
            }
            else if (roll < 82f)
            {
                forward = CK_DefOf.CK_Neighbor;
                backward = CK_DefOf.CK_Neighbor;
            }
            else if (roll < 89f)
            {
                forward = CK_DefOf.CK_CardBuddy;
                backward = CK_DefOf.CK_CardBuddy;
            }
            else if (roll < 95f)
            {
                forward = CK_DefOf.CK_Comrade;
                backward = CK_DefOf.CK_Comrade;
            }
            else
            {
                forward = CK_DefOf.CK_PenPal;
                backward = CK_DefOf.CK_PenPal;
            }
        }

        private void DailyTick()
        {
            if (CK_SettingsUtil.AutoSworn)
            {
                UpdateSworn();
            }

            if (CK_SettingsUtil.FrenemyEnabled)
            {
                UpdateFrenemy();
            }
        }

        private void UpdateSworn()
        {
            Dictionary<long, CK_PairStreak> map = new Dictionary<long, CK_PairStreak>();
            foreach (CK_PairStreak s in streaks)
            {
                map[PairKey(s.a, s.b)] = s;
            }

            HashSet<long> present = new HashSet<long>();
            foreach (Map colonyMap in Find.Maps)
            {
                if (colonyMap == null || !colonyMap.IsPlayerHome)
                {
                    continue;
                }

                List<Pawn> colonists = colonyMap.mapPawns.FreeColonistsSpawned;
                for (int i = 0; i < colonists.Count; i++)
                {
                    for (int j = i + 1; j < colonists.Count; j++)
                    {
                        Pawn a = colonists[i];
                        Pawn b = colonists[j];
                        if (!EligibleSworn(a, b))
                        {
                            continue;
                        }

                        long key = PairKey(a.thingIDNumber, b.thingIDNumber);
                        present.Add(key);

                        CK_PairStreak streak;
                        if (!map.TryGetValue(key, out streak))
                        {
                            streak = new CK_PairStreak { a = a.thingIDNumber, b = b.thingIDNumber };
                        }

                        if (a.relations.OpinionOf(b) >= 60 && b.relations.OpinionOf(a) >= 60)
                        {
                            streak.days++;
                        }
                        else
                        {
                            streak.days = 0;
                        }

                        map[key] = streak;

                        if (streak.days >= 5)
                        {
                            streak.days = 0;
                            if (RelationService.AddSymmetric(a, b, CK_DefOf.CK_Sworn))
                            {
                                GainSwornThoughts(a, b);
                                Messages.Message(
                                    "CK.Msg.Sworn".Translate(a.LabelShort, b.LabelShort),
                                    MessageTypeDefOf.PositiveEvent);
                            }
                        }
                    }
                }
            }

            streaks = map.Values.Where(s => present.Contains(PairKey(s.a, s.b))).ToList();
        }

        private static bool EligibleSworn(Pawn a, Pawn b)
        {
            return RelationService.CanRelate(a, b)
                && a.ageTracker.AgeBiologicalYearsFloat >= 16f
                && b.ageTracker.AgeBiologicalYearsFloat >= 16f;
        }

        private static void GainSwornThoughts(Pawn a, Pawn b)
        {
            a.needs?.mood?.thoughts?.memories?.TryGainMemory(CK_DefOf.CK_Thought_Sworn, b);
            b.needs?.mood?.thoughts?.memories?.TryGainMemory(CK_DefOf.CK_Thought_Sworn, a);
        }

        private void UpdateFrenemy()
        {
            foreach (Map colonyMap in Find.Maps)
            {
                if (colonyMap == null || !colonyMap.IsPlayerHome)
                {
                    continue;
                }

                foreach (Pawn pawn in colonyMap.mapPawns.FreeColonistsSpawned)
                {
                    if (pawn?.relations == null)
                    {
                        continue;
                    }

                    List<DirectPawnRelation> frenemies = pawn.relations.DirectRelations
                        .Where(r => r.def == CK_DefOf.CK_Frenemy && r.otherPawn != null)
                        .ToList();

                    foreach (DirectPawnRelation rel in frenemies)
                    {
                        Pawn other = rel.otherPawn;
                        if (pawn.thingIDNumber > other.thingIDNumber)
                        {
                            continue;
                        }

                        float chance = pawn.relations.OpinionOf(other) < 0 ? 0.05f : 0.01f;
                        if (Rand.Value >= chance)
                        {
                            continue;
                        }

                        RelationService.RemovePair(pawn, other, CK_DefOf.CK_Frenemy, CK_DefOf.CK_Frenemy);
                        if (RelationService.AddSymmetric(pawn, other, CK_DefOf.CK_Enemy))
                        {
                            pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(CK_DefOf.CK_Thought_FrenemyBreak, other);
                            other.needs?.mood?.thoughts?.memories?.TryGainMemory(CK_DefOf.CK_Thought_FrenemyBreak, pawn);
                            Messages.Message(
                                "CK.Msg.FrenemyBreak".Translate(pawn.LabelShort, other.LabelShort),
                                MessageTypeDefOf.NegativeEvent);
                        }
                    }
                }
            }
        }

        private static long PairKey(int a, int b)
        {
            int low = Math.Min(a, b);
            int high = Math.Max(a, b);
            return ((long)low << 32) | (uint)high;
        }

        public class CK_PairStreak : IExposable
        {
            public int a;
            public int b;
            public int days;

            public void ExposeData()
            {
                Scribe_Values.Look(ref a, "ck_a", 0);
                Scribe_Values.Look(ref b, "ck_b", 0);
                Scribe_Values.Look(ref days, "ck_days", 0);
            }
        }
    }
}


