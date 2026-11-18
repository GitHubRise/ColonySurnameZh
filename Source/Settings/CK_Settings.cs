using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace ColonySurnameZh
{
    public enum CK_NameScope
    {
        AllHumanlike = 0,
        PlayerFaction = 1,
        HumanRace = 2
    }

    public enum CK_MarriageMode
    {
        Vanilla = 0,
        Polygyny = 1,
        Polyandry = 2,
        Both = 3
    }

    /// <summary>
    /// 全部设置项。字段用 int 存枚举，避免 Scribe 对枚举的兼容问题。
    /// </summary>
    public class CK_Settings : ModSettings
    {
        public bool nameEnabled = true;
        public float chineseChance = 1f;
        public int nameScope = 0;
        public float nickChance = 0.3f;

        public bool relationsEnabled = true;
        public float oldAcquaintanceChance = 0.2f;
        public bool autoSworn = true;
        public int swornOpinion = 60;
        public int swornDays = 5;
        public bool frenemyEnabled = true;

        public bool kinshipEnabled = true;
        public int kinshipMinAge = 16;
        public int kinshipMinOpinion = 20;
        public int kinshipMax = 3;

        public int marriageMode = 0;
        public int maxSpouses = 3;
        public bool jealousyEnabled = true;
        public bool coSpouseEnabled = true;
        public bool spouseLabelsEnabled = true;
        public float favorChance = 0.03f;

        public List<string> tuningKeys = new List<string>();
        public List<CK_RelationTuning> tuningValues = new List<CK_RelationTuning>();

        public bool verboseLogging = false;

        [Unsaved] private Dictionary<string, CK_RelationTuning> tunings;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref nameEnabled, "ck_nameEnabled", true);
            Scribe_Values.Look(ref chineseChance, "ck_chineseChance", 1f);
            Scribe_Values.Look(ref nameScope, "ck_nameScope", 0);
            Scribe_Values.Look(ref nickChance, "ck_nickChance", 0.3f);

            Scribe_Values.Look(ref relationsEnabled, "ck_relationsEnabled", true);
            Scribe_Values.Look(ref oldAcquaintanceChance, "ck_oldAcquaintanceChance", 0.2f);
            Scribe_Values.Look(ref autoSworn, "ck_autoSworn", true);
            Scribe_Values.Look(ref swornOpinion, "ck_swornOpinion", 60);
            Scribe_Values.Look(ref swornDays, "ck_swornDays", 5);
            Scribe_Values.Look(ref frenemyEnabled, "ck_frenemyEnabled", true);

            Scribe_Values.Look(ref kinshipEnabled, "ck_kinshipEnabled", true);
            Scribe_Values.Look(ref kinshipMinAge, "ck_kinshipMinAge", 16);
            Scribe_Values.Look(ref kinshipMinOpinion, "ck_kinshipMinOpinion", 20);
            Scribe_Values.Look(ref kinshipMax, "ck_kinshipMax", 3);

            Scribe_Values.Look(ref marriageMode, "ck_marriageMode", 0);
            Scribe_Values.Look(ref maxSpouses, "ck_maxSpouses", 3);
            Scribe_Values.Look(ref jealousyEnabled, "ck_jealousyEnabled", true);
            Scribe_Values.Look(ref coSpouseEnabled, "ck_coSpouseEnabled", true);
            Scribe_Values.Look(ref spouseLabelsEnabled, "ck_spouseLabelsEnabled", true);
            Scribe_Values.Look(ref favorChance, "ck_favorChance", 0.03f);

            Scribe_Collections.Look(ref tuningKeys, "ck_tuningKeys", LookMode.Value);
            Scribe_Collections.Look(ref tuningValues, "ck_tuningValues", LookMode.Deep);

            Scribe_Values.Look(ref verboseLogging, "ck_verboseLogging", false);

            BuildTuningMap();
            EnsureTunings();
            SyncTuningLists();
            Validate();
        }

        private void BuildTuningMap()
        {
            tunings = new Dictionary<string, CK_RelationTuning>();
            if (tuningKeys == null || tuningValues == null)
            {
                return;
            }

            int count = Math.Min(tuningKeys.Count, tuningValues.Count);
            for (int i = 0; i < count; i++)
            {
                if (!string.IsNullOrEmpty(tuningKeys[i]) && tuningValues[i] != null)
                {
                    tunings[tuningKeys[i]] = tuningValues[i];
                }
            }
        }

        private void EnsureTunings()
        {
            if (tunings == null)
            {
                tunings = new Dictionary<string, CK_RelationTuning>();
            }

            foreach (CK_RelationEntry entry in CK_RelationCatalog.Entries)
            {
                if (!tunings.ContainsKey(entry.defName))
                {
                    tunings[entry.defName] = new CK_RelationTuning
                    {
                        enabled = true,
                        weight = entry.defaultWeight,
                        opinion = entry.defaultOpinion
                    };
                }
            }
        }

        private void SyncTuningLists()
        {
            tuningKeys = new List<string>();
            tuningValues = new List<CK_RelationTuning>();
            foreach (CK_RelationEntry entry in CK_RelationCatalog.Entries)
            {
                CK_RelationTuning tuning;
                if (tunings.TryGetValue(entry.defName, out tuning))
                {
                    tuningKeys.Add(entry.defName);
                    tuningValues.Add(tuning);
                }
            }
        }

        public CK_RelationTuning Tuning(string defName)
        {
            if (string.IsNullOrEmpty(defName))
            {
                return null;
            }

            EnsureTunings();
            CK_RelationTuning tuning;
            tunings.TryGetValue(defName, out tuning);
            return tuning;
        }

        public void Validate()
        {
            chineseChance = Clamp01(chineseChance);
            nickChance = Clamp01(nickChance);
            oldAcquaintanceChance = Clamp01(oldAcquaintanceChance);
            favorChance = Clamp01(favorChance);
            nameScope = Math.Max(0, Math.Min(2, nameScope));
            marriageMode = Math.Max(0, Math.Min(3, marriageMode));
            maxSpouses = Math.Max(1, Math.Min(10, maxSpouses));
            swornOpinion = Math.Max(0, Math.Min(100, swornOpinion));
            swornDays = Math.Max(1, Math.Min(30, swornDays));
            kinshipMinAge = Math.Max(1, Math.Min(60, kinshipMinAge));
            kinshipMinOpinion = Math.Max(-100, Math.Min(100, kinshipMinOpinion));
            kinshipMax = Math.Max(1, Math.Min(10, kinshipMax));

            if (tunings != null)
            {
                foreach (CK_RelationTuning tuning in tunings.Values)
                {
                    tuning.weight = Math.Max(0f, Math.Min(100f, tuning.weight));
                    tuning.opinion = Math.Max(-100, Math.Min(100, tuning.opinion));
                }
            }
        }

        private static float Clamp01(float v)
        {
            if (float.IsNaN(v))
            {
                return 0f;
            }
            return Math.Max(0f, Math.Min(1f, v));
        }
    }

    /// <summary>
    /// 静态访问器。设置未就绪时回退到默认值，避免早期调用空引用。
    /// </summary>
    public static class CK_SettingsUtil
    {
        public static CK_Settings Raw => ColonySurnameZhMod.Settings;

        public static bool NameEnabled => Raw == null || Raw.nameEnabled;

        public static float ChineseChance => Raw?.chineseChance ?? 1f;

        public static CK_NameScope Scope =>
            (CK_NameScope)(Raw?.nameScope ?? 0);

        public static float NickChance => Raw?.nickChance ?? 0.3f;

        public static bool RelationsEnabled => Raw == null || Raw.relationsEnabled;

        public static float OldAcquaintanceChance =>
            Raw?.oldAcquaintanceChance ?? 0.2f;

        public static bool AutoSworn => Raw == null || Raw.autoSworn;

        public static int SwornOpinion => Raw?.swornOpinion ?? 60;

        public static int SwornDays => Raw?.swornDays ?? 5;

        public static bool FrenemyEnabled => Raw == null || Raw.frenemyEnabled;

        public static bool KinshipEnabled => Raw == null || Raw.kinshipEnabled;

        public static int KinshipMinAge => Raw?.kinshipMinAge ?? 16;

        public static int KinshipMinOpinion => Raw?.kinshipMinOpinion ?? 20;

        public static int KinshipMax => Raw?.kinshipMax ?? 3;

        public static CK_MarriageMode MarriageMode =>
            (CK_MarriageMode)(Raw?.marriageMode ?? 0);

        public static int MaxSpouses => Raw?.maxSpouses ?? 3;

        public static bool JealousyEnabled => Raw == null || Raw.jealousyEnabled;

        public static bool CoSpouseEnabled => Raw == null || Raw.coSpouseEnabled;

        public static bool SpouseLabelsEnabled => Raw == null || Raw.spouseLabelsEnabled;

        public static float FavorChance => Raw?.favorChance ?? 0.03f;

        public static bool DefEnabled(string defName)
        {
            CK_RelationTuning tuning = Raw?.Tuning(defName);
            return tuning == null || tuning.enabled;
        }

        public static bool DefEnabled(PawnRelationDef def)
        {
            return def == null || DefEnabled(def.defName);
        }

        public static float Weight(string defName)
        {
            CK_RelationTuning tuning = Raw?.Tuning(defName);
            if (tuning == null || !tuning.enabled)
            {
                return 0f;
            }

            return tuning.weight;
        }

        /// <summary>把每个关系的「好感加成」写回 Def，并把权重与开关留在设置里。</summary>
        public static void ApplyOverrides()
        {
            CK_Settings settings = Raw;
            if (settings == null)
            {
                return;
            }

            foreach (CK_RelationEntry entry in CK_RelationCatalog.Entries)
            {
                ApplyOpinion(entry.defName);
            }
        }

        public static void ApplyOpinion(string defName)
        {
            CK_Settings settings = Raw;
            if (settings == null)
            {
                return;
            }

            CK_RelationTuning tuning = settings.Tuning(defName);
            PawnRelationDef def = DefDatabase<PawnRelationDef>.GetNamedSilentFail(defName);
            if (tuning != null && def != null)
            {
                def.opinionOffset = tuning.opinion;
            }
        }

        /// <summary>该 pawn 是否还能再添一名配偶（按婚配制度与上限判断）。</summary>
        public static bool AllowsAdditionalSpouse(Pawn pawn)
        {
            if (pawn == null)
            {
                return false;
            }

            CK_MarriageMode mode = MarriageMode;
            if (mode == CK_MarriageMode.Vanilla)
            {
                return false;
            }

            bool male = pawn.gender == Gender.Male;
            bool modeOk =
                mode == CK_MarriageMode.Both ||
                (mode == CK_MarriageMode.Polygyny && male) ||
                (mode == CK_MarriageMode.Polyandry && !male);

            if (!modeOk)
            {
                return false;
            }

            return SpouseList(pawn).Count < Math.Max(1, MaxSpouses);
        }

        public static List<Pawn> SpouseList(Pawn pawn)
        {
            List<Pawn> result = new List<Pawn>();
            if (pawn?.relations == null)
            {
                return result;
            }

            foreach (DirectPawnRelation rel in pawn.relations.DirectRelations)
            {
                if (rel.def == PawnRelationDefOf.Spouse && rel.otherPawn != null && !result.Contains(rel.otherPawn))
                {
                    result.Add(rel.otherPawn);
                }
            }
            return result;
        }
    }
}
