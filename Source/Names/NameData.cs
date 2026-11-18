using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Verse;

namespace ColonySurnameZh
{
    /// <summary>
    /// 姓名数据池。优先读 Mod 目录 Data/Names/*.txt，读不到用内置小池兜底。
    /// </summary>
    public static class NameData
    {
        private static bool loaded;
        private static List<string> surnames;
        private static List<string> maleGiven;
        private static List<string> femaleGiven;
        private static List<string> unisexGiven;
        private static List<string> maleGivenSingles;
        private static List<string> femaleGivenSingles;
        private static List<string> unisexGivenSingles;
        private static List<string> nicks;
        private static HashSet<string> surnameSet;

        private static readonly string[] FallbackSurnames =
        {
            "赵", "钱", "孙", "李", "周", "吴", "郑", "王", "冯", "陈", "张", "刘",
            "杨", "黄", "何", "林", "高", "罗", "郑", "梁", "宋", "谢", "唐", "许",
            "欧阳", "司马", "上官", "诸葛"
        };

        private static readonly string[] FallbackMale =
        {
            "伟", "强", "磊", "军", "洋", "勇", "杰", "涛", "明", "超", "刚", "平",
            "辉", "鹏", "华", "飞", "鑫", "波", "斌", "宇", "浩", "凯", "健", "俊"
        };

        private static readonly string[] FallbackFemale =
        {
            "芳", "娜", "敏", "静", "丽", "秀", "娟", "英", "慧", "巧", "美", "玲",
            "桂", "兰", "凤", "洁", "梅", "琳", "素", "云", "莲", "真", "环", "雪"
        };

        private static readonly string[] FallbackNick =
        {
            "二狗", "狗蛋", "铁柱", "石头", "大壮", "小强", "小芳", "阿强", "阿珍", "老李"
        };

        public static void EnsureLoaded()
        {
            if (loaded)
            {
                return;
            }
            loaded = true;

            surnames = Load("Surnames.txt", FallbackSurnames);
            maleGiven = Load("MaleGiven.txt", FallbackMale);
            femaleGiven = Load("FemaleGiven.txt", FallbackFemale);
            unisexGiven = Load("UnisexGiven.txt", FallbackFemale);
            nicks = Load("Nick.txt", FallbackNick);
            maleGivenSingles = maleGiven.Where(n => n.Length == 1).ToList();
            femaleGivenSingles = femaleGiven.Where(n => n.Length == 1).ToList();
            unisexGivenSingles = unisexGiven.Where(n => n.Length == 1).ToList();
            surnameSet = new HashSet<string>(surnames);
        }

        private static List<string> Load(string fileName, string[] fallback)
        {
            try
            {
                ModContentPack mod = LoadedModManager.RunningModsListForReading
                    .FirstOrDefault(m => m.PackageId == "local.colonysurnamezh");
                if (mod != null)
                {
                    string path = Path.Combine(mod.RootDir, "Data", "Names", fileName);
                    if (File.Exists(path))
                    {
                        List<string> lines = File.ReadAllLines(path)
                            .Select(l => l.Trim())
                            .Where(l => l.Length > 0)
                            .Distinct()
                            .ToList();
                        if (lines.Count > 0)
                        {
                            return lines;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Warning("[ColonySurnameZh] failed to read " + fileName + ": " + e.Message);
            }

            return fallback.Distinct().ToList();
        }

        public static bool IsSurname(string value)
        {
            EnsureLoaded();
            if (value.NullOrEmpty())
            {
                return false;
            }
            return surnameSet.Contains(value);
        }

        public static string RandomSurname()
        {
            EnsureLoaded();
            return surnames.RandomElement();
        }

        public static string RandomGiven(Gender gender)
        {
            EnsureLoaded();
            if (gender == Gender.Male)
            {
                return maleGiven.RandomElement();
            }
            if (gender == Gender.Female)
            {
                return femaleGiven.RandomElement();
            }
            return unisexGiven.RandomElement();
        }

        /// <summary>
        /// 随机取一个单字名，供生成器在单字基础上再拼一个字时使用；池中没有单字时返回 null。
        /// </summary>
        public static string RandomSingleGiven(Gender gender)
        {
            EnsureLoaded();
            List<string> pool;
            if (gender == Gender.Male)
            {
                pool = maleGivenSingles;
            }
            else if (gender == Gender.Female)
            {
                pool = femaleGivenSingles;
            }
            else
            {
                pool = unisexGivenSingles;
            }
            return pool.Count > 0 ? pool.RandomElement() : null;
        }

        public static string RandomNick()
        {
            EnsureLoaded();
            return nicks.Count > 0 ? nicks.RandomElement() : null;
        }
    }
}
