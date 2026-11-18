using System;
using Verse;

namespace ColonySurnameZh
{
    /// <summary>
    /// 中文姓名生成：姓 + 名（单/双字）+ 可选小名，写进 NameTriple(First=名, Nick=小名, Last=姓)。
    /// </summary>
    public static class ChineseNameGenerator
    {
        public static NameTriple Generate(Gender gender, string forcedLastName, bool forceNoNick)
        {
            string surname = forcedLastName;
            if (!NameData.IsSurname(surname))
            {
                surname = NameData.RandomSurname();
            }

            string given = NameData.RandomGiven(gender);
            if (given.Length == 1 && Rand.Value < 0.65f)
            {
                string extra = NameData.RandomSingleGiven(gender);
                if (!extra.NullOrEmpty())
                {
                    given += extra;
                }
            }

            string nick;
            if (!forceNoNick && Rand.Value < CK_SettingsUtil.NickChance)
            {
                nick = NameData.RandomNick();
                if (nick.NullOrEmpty())
                {
                    nick = "小" + given.Substring(0, 1);
                }
            }
            else
            {
                nick = given;
            }

            return new NameTriple(given, nick, surname);
        }

        public static bool IsChineseName(NameTriple name)
        {
            return name != null
                && !name.First.NullOrEmpty()
                && NameData.IsSurname(name.Last);
        }
    }
}
