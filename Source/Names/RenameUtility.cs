using System;
using Verse;

namespace ColonySurnameZh
{
    /// <summary>
    /// 一键为玩家派系现有人口换中文名。只改 Name，不动关系。
    /// </summary>
    public static class RenameUtility
    {
        public static int RenameAllColonists()
        {
            int count = 0;
            foreach (Map map in Find.Maps)
            {
                if (map == null || !map.IsPlayerHome)
                {
                    continue;
                }

                foreach (Pawn pawn in map.mapPawns.FreeColonistsSpawned)
                {
                    if (pawn == null || !pawn.RaceProps.Humanlike)
                    {
                        continue;
                    }

                    pawn.Name = ChineseNameGenerator.Generate(pawn.gender, null, false);
                    count++;
                }
            }

            return count;
        }
    }
}
