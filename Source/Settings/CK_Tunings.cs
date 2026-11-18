using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ColonySurnameZh
{
    /// <summary>单个关系的参数组：是否启用、生成权重、好感加成。</summary>
    public class CK_RelationTuning : IExposable
    {
        public bool enabled = true;
        public float weight = 10f;
        public int opinion = 0;

        public void ExposeData()
        {
            Scribe_Values.Look(ref enabled, "enabled", true);
            Scribe_Values.Look(ref weight, "weight", 10f);
            Scribe_Values.Look(ref opinion, "opinion", 0);
        }
    }

    /// <summary>关系目录条目：给设置页和播种器提供默认值与分组。</summary>
    public class CK_RelationEntry
    {
        public string defName;
        public string labelKey;
        public int group;
        public float defaultWeight;
        public int defaultOpinion;

        public CK_RelationEntry(string defName, string labelKey, int group, float defaultWeight, int defaultOpinion)
        {
            this.defName = defName;
            this.labelKey = labelKey;
            this.group = group;
            this.defaultWeight = defaultWeight;
            this.defaultOpinion = defaultOpinion;
        }
    }

    public static class CK_RelationCatalog
    {
        public const int GroupSpawn = 0;
        public const int GroupEvent = 1;
        public const int GroupFamily = 2;
        public const int GroupMarriage = 3;
        public const int GroupDerived = 4;

        public static readonly CK_RelationEntry[] Entries =
        {
            // 生成型（旧识）
            new CK_RelationEntry("CK_Friend", "CK.RelName.CK_Friend", GroupSpawn, 30f, 10),
            new CK_RelationEntry("CK_Classmate", "CK.RelName.CK_Classmate", GroupSpawn, 20f, 6),
            new CK_RelationEntry("CK_ChildhoodFriend", "CK.RelName.CK_ChildhoodFriend", GroupSpawn, 12f, 14),
            new CK_RelationEntry("CK_Coworker", "CK.RelName.CK_Coworker", GroupSpawn, 12f, 5),
            new CK_RelationEntry("CK_Neighbor", "CK.RelName.CK_Neighbor", GroupSpawn, 10f, 6),
            new CK_RelationEntry("CK_NetFriend", "CK.RelName.CK_NetFriend", GroupSpawn, 8f, 8),
            new CK_RelationEntry("CK_PenPal", "CK.RelName.CK_PenPal", GroupSpawn, 6f, 6),
            new CK_RelationEntry("CK_CardBuddy", "CK.RelName.CK_CardBuddy", GroupSpawn, 8f, 7),
            new CK_RelationEntry("CK_Comrade", "CK.RelName.CK_Comrade", GroupSpawn, 5f, 18),
            new CK_RelationEntry("CK_Master", "CK.RelName.CK_Master", GroupSpawn, 6f, 14),
            new CK_RelationEntry("CK_Apprentice", "CK.RelName.CK_Apprentice", GroupSpawn, 6f, 8),
            new CK_RelationEntry("CK_Rival", "CK.RelName.CK_Rival", GroupSpawn, 6f, -18),

            // 事件型
            new CK_RelationEntry("CK_LifeSaver", "CK.RelName.CK_LifeSaver", GroupEvent, 0f, 25),
            new CK_RelationEntry("CK_LifeSaved", "CK.RelName.CK_LifeSaved", GroupEvent, 0f, 20),
            new CK_RelationEntry("CK_SwornFriend", "CK.RelName.CK_SwornFriend", GroupEvent, 0f, 30),
            new CK_RelationEntry("CK_Doctor", "CK.RelName.CK_Doctor", GroupEvent, 0f, 8),
            new CK_RelationEntry("CK_Patient", "CK.RelName.CK_Patient", GroupEvent, 0f, 6),
            new CK_RelationEntry("CK_FormerPatient", "CK.RelName.CK_FormerPatient", GroupEvent, 0f, 6),
            new CK_RelationEntry("CK_Cellmate", "CK.RelName.CK_Cellmate", GroupEvent, 0f, 10),
            new CK_RelationEntry("CK_Conspirator", "CK.RelName.CK_Conspirator", GroupEvent, 0f, 15),
            new CK_RelationEntry("CK_Teacher", "CK.RelName.CK_Teacher", GroupEvent, 0f, 12),
            new CK_RelationEntry("CK_Student", "CK.RelName.CK_Student", GroupEvent, 0f, 6),
            new CK_RelationEntry("CK_BattleBrother", "CK.RelName.CK_BattleBrother", GroupEvent, 0f, 12),
            new CK_RelationEntry("CK_DrinkingBuddy", "CK.RelName.CK_DrinkingBuddy", GroupEvent, 0f, 8),
            new CK_RelationEntry("CK_Roommate", "CK.RelName.CK_Roommate", GroupEvent, 0f, 6),
            new CK_RelationEntry("CK_TravelCompanion", "CK.RelName.CK_TravelCompanion", GroupEvent, 0f, 10),
            new CK_RelationEntry("CK_Enemy", "CK.RelName.CK_Enemy", GroupEvent, 0f, -35),
            new CK_RelationEntry("CK_Sworn", "CK.RelName.CK_Sworn", GroupEvent, 0f, 20),
            new CK_RelationEntry("CK_SwornKin", "CK.RelName.CK_SwornKin", GroupEvent, 0f, 20),

            // 家庭
            new CK_RelationEntry("CK_Godparent", "CK.RelName.CK_Godparent", GroupFamily, 0f, 12),
            new CK_RelationEntry("CK_Godchild", "CK.RelName.CK_Godchild", GroupFamily, 0f, 10),
            new CK_RelationEntry("CK_Frenemy", "CK.RelName.CK_Frenemy", GroupFamily, 0f, 2),

            // 婚配
            new CK_RelationEntry("CK_SeniorSpouse", "CK.RelName.CK_SeniorSpouse", GroupMarriage, 0f, -10),
            new CK_RelationEntry("CK_JuniorSpouse", "CK.RelName.CK_JuniorSpouse", GroupMarriage, 0f, -6),
            new CK_RelationEntry("CK_CoSpouse", "CK.RelName.CK_CoSpouse", GroupMarriage, 0f, 0),

            // 派生
            new CK_RelationEntry("CK_LoveRival", "CK.RelName.CK_LoveRival", GroupDerived, 0f, -25),
            new CK_RelationEntry("CK_SameSchool", "CK.RelName.CK_SameSchool", GroupDerived, 0f, 6)
        };

        public static bool IsSpawn(string defName)
        {
            CK_RelationEntry entry = Find(defName);
            return entry != null && entry.group == GroupSpawn;
        }

        public static CK_RelationEntry Find(string defName)
        {
            for (int i = 0; i < Entries.Length; i++)
            {
                if (Entries[i].defName == defName)
                {
                    return Entries[i];
                }
            }

            return null;
        }
    }
}
