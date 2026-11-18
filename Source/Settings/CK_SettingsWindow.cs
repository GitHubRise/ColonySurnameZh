using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace ColonySurnameZh
{
    /// <summary>
    /// 简易四页签设置面板：姓名 / 关系 / 婚配 / 关于。
    /// </summary>
    public class CK_SettingsWindow
    {
        private readonly CK_Settings settings;
        private int tab;
        private Vector2 scroll;

        private static readonly string[] TabKeys =
        {
            "CK.Tab.Names", "CK.Tab.Relations", "CK.Tab.Marriage", "CK.Tab.About"
        };

        public CK_SettingsWindow(CK_Settings settings)
        {
            this.settings = settings;
        }

        public void Draw(Rect rect)
        {
            if (settings == null)
            {
                return;
            }

            Widgets.DrawMenuSection(rect);
            Rect inner = rect.ContractedBy(8f);

            Rect tabBar = new Rect(inner.x, inner.y, inner.width, 30f);
            float x = tabBar.x;
            for (int i = 0; i < TabKeys.Length; i++)
            {
                Rect tabRect = new Rect(x, tabBar.y, 120f, 30f);
                Color old = GUI.color;
                if (tab == i)
                {
                    GUI.color = new Color(0.7f, 0.9f, 1f);
                }
                if (Widgets.ButtonText(tabRect, TabKeys[i].Translate()))
                {
                    tab = i;
                    scroll = Vector2.zero;
                }
                GUI.color = old;
                x += 126f;
            }

            Rect body = new Rect(inner.x, tabBar.yMax + 6f, inner.width, inner.height - 38f);
            Rect view = new Rect(0f, 0f, body.width - 20f, 1000f);
            Widgets.BeginScrollView(body, ref scroll, view);
            Listing_Standard list = new Listing_Standard();
            list.Begin(view);
            switch (tab)
            {
                case 0:
                    DrawNames(list);
                    break;
                case 1:
                    DrawRelations(list);
                    break;
                case 2:
                    DrawMarriage(list);
                    break;
                default:
                    DrawAbout(list);
                    break;
            }
            list.End();
            Widgets.EndScrollView();
        }

        private void DrawNames(Listing_Standard list)
        {
            list.CheckboxLabeled("CK.Names.Enabled".Translate(), ref settings.nameEnabled);
            SliderPercent(list, "CK.Names.Chance".Translate(), ref settings.chineseChance);
            CycleRow(list, "CK.Names.Scope".Translate(), ScopeLabel((CK_NameScope)settings.nameScope), delegate
            {
                settings.nameScope = (settings.nameScope + 1) % 3;
            });
            SliderPercent(list, "CK.Names.NickChance".Translate(), ref settings.nickChance);

            list.Gap(6f);
            if (list.ButtonText("CK.Names.RenameAll".Translate()))
            {
                int count = RenameUtility.RenameAllColonists();
                Messages.Message("CK.Names.RenameDone".Translate(count), MessageTypeDefOf.TaskCompletion);
            }
            list.Label("CK.Names.Hint".Translate());
        }

        private void DrawRelations(Listing_Standard list)
        {
            list.CheckboxLabeled("CK.Relations.Enabled".Translate(), ref settings.relationsEnabled);
            SliderPercent(list, "CK.Relations.OldChance".Translate(), ref settings.oldAcquaintanceChance);
            list.Label("CK.Relations.OldChance.Hint".Translate());
            list.Gap(6f);
            list.CheckboxLabeled("CK.Relations.AutoSworn".Translate(), ref settings.autoSworn);
            list.Label("CK.Relations.AutoSworn.Hint".Translate());
            list.Gap(6f);
            list.CheckboxLabeled("CK.Relations.Frenemy".Translate(), ref settings.frenemyEnabled);
            list.Gap(6f);
            list.Label("CK.Relations.Hint".Translate());
        }

        private void DrawMarriage(Listing_Standard list)
        {
            CycleRow(list, "CK.Marriage.Mode".Translate(), MarriageLabel((CK_MarriageMode)settings.marriageMode), delegate
            {
                settings.marriageMode = (settings.marriageMode + 1) % 4;
            });
            CycleRow(list, "CK.Marriage.Max".Translate(), settings.maxSpouses.ToString(), delegate
            {
                settings.maxSpouses = settings.maxSpouses >= 10 ? 1 : settings.maxSpouses + 1;
            });
            list.Gap(6f);
            list.Label("CK.Marriage.Hint".Translate());
        }

        private void DrawAbout(Listing_Standard list)
        {
            string text = "CK.About.Text".Translate();
            float height = Text.CalcHeight(text, list.ColumnWidth);
            Widgets.Label(list.GetRect(height), text);
        }

        private static void SliderPercent(Listing_Standard list, string label, ref float value)
        {
            list.Label(label + "：" + (value * 100f).ToString("F0") + "%");
            value = Widgets.HorizontalSlider(list.GetRect(28f), value, 0f, 1f);
        }

        private static void CycleRow(Listing_Standard list, string label, string value, Action next)
        {
            Rect row = list.GetRect(30f);
            Widgets.Label(row.LeftPart(0.6f), label);
            if (Widgets.ButtonText(row.RightPart(0.38f), value))
            {
                next();
            }
        }

        private static string ScopeLabel(CK_NameScope scope)
        {
            switch (scope)
            {
                case CK_NameScope.PlayerFaction:
                    return "CK.Names.Scope.Player".Translate();
                case CK_NameScope.HumanRace:
                    return "CK.Names.Scope.Human".Translate();
                default:
                    return "CK.Names.Scope.All".Translate();
            }
        }

        private static string MarriageLabel(CK_MarriageMode mode)
        {
            switch (mode)
            {
                case CK_MarriageMode.Polygyny:
                    return "CK.Marriage.Mode.Polygyny".Translate();
                case CK_MarriageMode.Polyandry:
                    return "CK.Marriage.Mode.Polyandry".Translate();
                case CK_MarriageMode.Both:
                    return "CK.Marriage.Mode.Both".Translate();
                default:
                    return "CK.Marriage.Mode.Vanilla".Translate();
            }
        }
    }
}
