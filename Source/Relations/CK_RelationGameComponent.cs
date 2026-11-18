using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace ColonySurnameZh
{
    /// <summary>
    /// 关系后台：新殖民者旧识播种 + 每日「自动结拜 / 塑料闺蜜反目 / 争宠」结算。
    /// </summary>
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
            CK_SettingsUtil.ApplyOverrides();
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

                if (SeedOne(pawn, other))
                {
                    added++;
                }
            }
        }

        private static bool SeedOne(Pawn pawn, Pawn other)
        {
            CK_RelationEntry picked = PickWeightedSpawn(pawn, other);
            if (picked == null)
            {
                return false;
            }

            if (picked.defName == "CK_Rival")
            {
                return RelationService.AddSymmetric(pawn, other, CK_DefOf.CK_Rival);
            }

            if (picked.defName == "CK_Master" || picked.defName == "CK_Apprentice")
            {
                bool pawnOlder = pawn.ageTracker.AgeBiologicalYearsFloat > other.ageTracker.AgeBiologicalYearsFloat;
                PawnRelationDef forward = pawnOlder ? CK_DefOf.CK_Master : CK_DefOf.CK_Apprentice;
                PawnRelationDef backward = pawnOlder ? CK_DefOf.CK_Apprentice : CK_DefOf.CK_Master;
                return RelationService.AddPair(pawn, other, forward, backward);
            }

            PawnRelationDef def = SpawnDef(picked.defName);
            return def != null && RelationService.AddPair(pawn, other, def, def);
        }

        private static PawnRelationDef SpawnDef(string defName)
        {
            switch (defName)
            {
                case "CK_Friend": return CK_DefOf.CK_Friend;
                case "CK_Classmate": return CK_DefOf.CK_Classmate;
                case "CK_ChildhoodFriend": return CK_DefOf.CK_ChildhoodFriend;
                case "CK_Coworker": return CK_DefOf.CK_Coworker;
                case "CK_Neighbor": return CK_DefOf.CK_Neighbor;
                case "CK_NetFriend": return CK_DefOf.CK_NetFriend;
                case "CK_PenPal": return CK_DefOf.CK_PenPal;
                case "CK_CardBuddy": return CK_DefOf.CK_CardBuddy;
                case "CK_Comrade": return CK_DefOf.CK_Comrade;
                default: return null;
            }
        }

        /// <summary>按设置里的权重在生成型关系里抽一条；年龄差不符合的会被跳过。</summary>
        private static CK_RelationEntry PickWeightedSpawn(Pawn a, Pawn b)
        {
            float gap = Math.Abs(
                a.ageTracker.AgeBiologicalYearsFloat - b.ageTracker.AgeBiologicalYearsFloat);

            List<CK_RelationEntry> pool = new List<CK_RelationEntry>();
            float total = 0f;
            foreach (CK_RelationEntry entry in CK_RelationCatalog.Entries)
            {
                if (entry.group != CK_RelationCatalog.GroupSpawn || !AgeEligible(entry.defName, gap))
                {
                    continue;
                }

                float weight = CK_SettingsUtil.Weight(entry.defName);
                if (weight <= 0f)
                {
                    continue;
                }

                pool.Add(entry);
                total += weight;
            }

            if (total <= 0f)
            {
                return null;
            }

            float roll = Rand.Value * total;
            foreach (CK_RelationEntry entry in pool)
            {
                roll -= CK_SettingsUtil.Weight(entry.defName);
                if (roll <= 0f)
                {
                    return entry;
                }
            }

            return pool[pool.Count - 1];
        }

        private static bool AgeEligible(string defName, float gap)
        {
            switch (defName)
            {
                case "CK_Classmate":
                    return gap <= 8f;
                case "CK_ChildhoodFriend":
                    return gap <= 12f;
                case "CK_Master":
                case "CK_Apprentice":
                    return gap >= 10f;
                default:
                    return true;
            }
        }

        private static bool TryAddSpecial(Pawn pawn, Pawn other)
        {
            if (!CK_SettingsUtil.FrenemyEnabled || !CK_SettingsUtil.DefEnabled(CK_DefOf.CK_Frenemy))
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

        private void DailyTick()
        {
            if (CK_SettingsUtil.AutoSworn && CK_SettingsUtil.DefEnabled(CK_DefOf.CK_Sworn))
            {
                UpdateSworn();
            }

            if (CK_SettingsUtil.FrenemyEnabled && CK_SettingsUtil.DefEnabled(CK_DefOf.CK_Frenemy))
            {
                UpdateFrenemy();
            }

            UpdateFavor();
        }

        /// <summary>争宠：一个多配偶者每天有概率让一名侧室吃醋。</summary>
        private static void UpdateFavor()
        {
            float chance = CK_SettingsUtil.FavorChance;
            if (chance <= 0f || CK_SettingsUtil.MarriageMode == CK_MarriageMode.Vanilla)
            {
                return;
            }

            foreach (Map colonyMap in Find.Maps)
            {
                if (colonyMap == null || !colonyMap.IsPlayerHome)
                {
                    continue;
                }

                foreach (Pawn pawn in colonyMap.mapPawns.FreeColonistsSpawned)
                {
                    List<Pawn> spouses = CK_SettingsUtil.SpouseList(pawn);
                    if (spouses.Count < 2)
                    {
                        continue;
                    }

                    if (spouses.Any(p => p == null || p.thingIDNumber < pawn.thingIDNumber))
                    {
                        continue;
                    }

                    if (Rand.Value >= chance)
                    {
                        continue;
                    }

                    Pawn junior = spouses[1];
                    Pawn senior = spouses[0];
                    if (junior?.needs?.mood?.thoughts?.memories != null)
                    {
                        junior.needs.mood.thoughts.memories.TryGainMemory(
                            CK_DefOf.CK_Thought_Displaced, senior);
                    }

                    Messages.Message(
                        "CK.Msg.Favor".Translate(junior.LabelShort, pawn.LabelShort, senior.LabelShort),
                        MessageTypeDefOf.NegativeEvent);
                }
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

                        if (a.relations.OpinionOf(b) >= CK_SettingsUtil.SwornOpinion
                            && b.relations.OpinionOf(a) >= CK_SettingsUtil.SwornOpinion)
                        {
                            streak.days++;
                        }
                        else
                        {
                            streak.days = 0;
                        }

                        map[key] = streak;

                        if (streak.days >= CK_SettingsUtil.SwornDays)
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


