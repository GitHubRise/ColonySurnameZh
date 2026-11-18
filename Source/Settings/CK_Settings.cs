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
        public bool frenemyEnabled = true;

        public int marriageMode = 0;
        public int maxSpouses = 3;

        public bool verboseLogging = false;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref nameEnabled, "ck_nameEnabled", true);
            Scribe_Values.Look(ref chineseChance, "ck_chineseChance", 1f);
            Scribe_Values.Look(ref nameScope, "ck_nameScope", 0);
            Scribe_Values.Look(ref nickChance, "ck_nickChance", 0.3f);

            Scribe_Values.Look(ref relationsEnabled, "ck_relationsEnabled", true);
            Scribe_Values.Look(ref oldAcquaintanceChance, "ck_oldAcquaintanceChance", 0.2f);
            Scribe_Values.Look(ref autoSworn, "ck_autoSworn", true);
            Scribe_Values.Look(ref frenemyEnabled, "ck_frenemyEnabled", true);

            Scribe_Values.Look(ref marriageMode, "ck_marriageMode", 0);
            Scribe_Values.Look(ref maxSpouses, "ck_maxSpouses", 3);

            Scribe_Values.Look(ref verboseLogging, "ck_verboseLogging", false);

            Validate();
        }

        public void Validate()
        {
            chineseChance = Clamp01(chineseChance);
            nickChance = Clamp01(nickChance);
            oldAcquaintanceChance = Clamp01(oldAcquaintanceChance);
            nameScope = Math.Max(0, Math.Min(2, nameScope));
            marriageMode = Math.Max(0, Math.Min(3, marriageMode));
            maxSpouses = Math.Max(1, Math.Min(10, maxSpouses));
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

        public static bool FrenemyEnabled => Raw == null || Raw.frenemyEnabled;

        public static CK_MarriageMode MarriageMode =>
            (CK_MarriageMode)(Raw?.marriageMode ?? 0);

        public static int MaxSpouses => Raw?.maxSpouses ?? 3;

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
