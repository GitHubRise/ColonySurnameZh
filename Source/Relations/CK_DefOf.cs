using RimWorld;
using Verse;

namespace ColonySurnameZh
{
    /// <summary>
    /// 本 Mod 用到的 Def 引用。字段名必须与 defName 完全一致。
    /// </summary>
    [DefOf]
    public static class CK_DefOf
    {
        public static PawnRelationDef CK_Classmate;
        public static PawnRelationDef CK_ChildhoodFriend;
        public static PawnRelationDef CK_Friend;
        public static PawnRelationDef CK_NetFriend;
        public static PawnRelationDef CK_PenPal;
        public static PawnRelationDef CK_Coworker;
        public static PawnRelationDef CK_Neighbor;
        public static PawnRelationDef CK_Comrade;
        public static PawnRelationDef CK_Master;
        public static PawnRelationDef CK_Apprentice;
        public static PawnRelationDef CK_Rival;

        public static PawnRelationDef CK_LifeSaver;
        public static PawnRelationDef CK_LifeSaved;
        public static PawnRelationDef CK_SwornFriend;
        public static PawnRelationDef CK_Doctor;
        public static PawnRelationDef CK_Patient;
        public static PawnRelationDef CK_FormerPatient;
        public static PawnRelationDef CK_Cellmate;
        public static PawnRelationDef CK_Conspirator;
        public static PawnRelationDef CK_Teacher;
        public static PawnRelationDef CK_Student;
        public static PawnRelationDef CK_BattleBrother;
        public static PawnRelationDef CK_CardBuddy;
        public static PawnRelationDef CK_DrinkingBuddy;
        public static PawnRelationDef CK_Roommate;
        public static PawnRelationDef CK_TravelCompanion;
        public static PawnRelationDef CK_Enemy;
        public static PawnRelationDef CK_Sworn;
        public static PawnRelationDef CK_SwornKin;
        public static PawnRelationDef CK_Godparent;
        public static PawnRelationDef CK_Godchild;
        public static PawnRelationDef CK_Frenemy;

        public static PawnRelationDef CK_SeniorSpouse;
        public static PawnRelationDef CK_JuniorSpouse;
        public static PawnRelationDef CK_CoSpouse;

        public static PawnRelationDef CK_LoveRival;
        public static PawnRelationDef CK_SameSchool;

        public static ThoughtDef CK_Thought_Concubine;
        public static ThoughtDef CK_Thought_SpouseTookSpouse;
        public static ThoughtDef CK_Thought_Sworn;
        public static ThoughtDef CK_Thought_FrenemyBreak;
        public static ThoughtDef CK_Thought_Displaced;

        static CK_DefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(CK_DefOf));
        }
    }
}
