using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace ColonySurnameZh
{
    /// <summary>
    /// 四页签设置面板：姓名 / 关系 / 婚配 / 关于。
    /// 关系页里每条关系都是一个独立参数组：启用、权重、好感。
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

            CK_SettingsUtil.ApplyOverrides();

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
            float viewHeight = TabViewHeight();
            Rect view = new Rect(0f, 0f, body.width - 20f, viewHeight);
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

        private float TabViewHeight()
        {
            switch (tab)
            {
                case 1:
                    return 1900f;
                case 2:
                    return 520f;
                case 3:
                    return 420f;
                default:
                    return 460f;
            }
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

            SectionHeader(list, "CK.Rel.Group.Spawn");
            foreach (CK_RelationEntry entry in CK_RelationCatalog.Entries)
            {
                if (entry.group == CK_RelationCatalog.GroupSpawn)
                {
                    RelationRow(list, entry, true);
                }
            }

            SectionHeader(list, "CK.Rel.Group.Event");
            foreach (CK_RelationEntry entry in CK_RelationCatalog.Entries)
            {
                if (entry.group == CK_RelationCatalog.GroupEvent)
                {
                    RelationRow(list, entry, false);
                }
            }

            SectionHeader(list, "CK.Rel.Group.Family");
            foreach (CK_RelationEntry entry in CK_RelationCatalog.Entries)
            {
                if (entry.group == CK_RelationCatalog.GroupFamily)
                {
                    RelationRow(list, entry, false);
                }
            }

            SectionHeader(list, "CK.Rel.Group.Marriage");
            foreach (CK_RelationEntry entry in CK_RelationCatalog.Entries)
            {
                if (entry.group == CK_RelationCatalog.GroupMarriage)
                {
                    RelationRow(list, entry, false);
                }
            }

            SectionHeader(list, "CK.Rel.Group.Derived");
            foreach (CK_RelationEntry entry in CK_RelationCatalog.Entries)
            {
                if (entry.group == CK_RelationCatalog.GroupDerived)
                {
                    RelationRow(list, entry, false);
                }
            }

            SectionHeader(list, "CK.Rel.Group.Rules");
            list.CheckboxLabeled("CK.Relations.AutoSworn".Translate(), ref settings.autoSworn);
            SliderInt(list, "CK.Relations.SwornOpinion".Translate(), ref settings.swornOpinion, 0, 100);
            SliderInt(list, "CK.Relations.SwornDays".Translate(), ref settings.swornDays, 1, 30);
            list.CheckboxLabeled("CK.Relations.Frenemy".Translate(), ref settings.frenemyEnabled);

            list.CheckboxLabeled("CK.Relations.Kinship".Translate(), ref settings.kinshipEnabled);
            SliderInt(list, "CK.Relations.KinshipAge".Translate(), ref settings.kinshipMinAge, 1, 40);
            SliderInt(list, "CK.Relations.KinshipOpinion".Translate(), ref settings.kinshipMinOpinion, -20, 60);
            SliderInt(list, "CK.Relations.KinshipMax".Translate(), ref settings.kinshipMax, 1, 10);

            list.Gap(8f);
            if (list.ButtonText("CK.Relations.Reset".Translate()))
            {
                ResetRelationTunings();
            }
            list.Label("CK.Relations.Hint".Translate());
        }

        private void RelationRow(Listing_Standard list, CK_RelationEntry entry, bool showWeight)
        {
            CK_RelationTuning tuning = settings.Tuning(entry.defName);
            if (tuning == null)
            {
                return;
            }

            Rect row = list.GetRect(30f);
            Rect check = new Rect(row.x, row.y, 200f, 28f);
            Widgets.CheckboxLabeled(check, entry.labelKey.Translate(), ref tuning.enabled);

            float x = row.x + 206f;
            if (showWeight)
            {
                Stepper(new Rect(x, row.y, 150f, 28f), ref tuning.weight, 0f, 100f, 5f, "CK.Rel.Weight".Translate());
                x += 156f;
            }

            StepperInt(new Rect(x, row.y, 150f, 28f), ref tuning.opinion, -40, 40, 1, "CK.Rel.Opinion".Translate());
            CK_SettingsUtil.ApplyOpinion(entry.defName);
        }

        private static void SectionHeader(Listing_Standard list, string key)
        {
            list.Gap(8f);
            list.Label(key.Translate().Colorize(new Color(0.75f, 0.9f, 1f)));
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
            list.CheckboxLabeled("CK.Marriage.Labels".Translate(), ref settings.spouseLabelsEnabled);
            list.CheckboxLabeled("CK.Marriage.CoSpouse".Translate(), ref settings.coSpouseEnabled);
            list.CheckboxLabeled("CK.Marriage.Jealousy".Translate(), ref settings.jealousyEnabled);
            SliderPercent(list, "CK.Marriage.Favor".Translate(), ref settings.favorChance);
            list.Gap(6f);
            list.Label("CK.Marriage.Hint".Translate());
        }

        private void DrawAbout(Listing_Standard list)
        {
            string text = "CK.About.Text".Translate();
            float height = Text.CalcHeight(text, list.ColumnWidth);
            Widgets.Label(list.GetRect(height), text);
        }

        private void ResetRelationTunings()
        {
            foreach (CK_RelationEntry entry in CK_RelationCatalog.Entries)
            {
                CK_RelationTuning tuning = settings.Tuning(entry.defName);
                if (tuning == null)
                {
                    continue;
                }

                tuning.enabled = true;
                tuning.weight = entry.defaultWeight;
                tuning.opinion = entry.defaultOpinion;
                CK_SettingsUtil.ApplyOpinion(entry.defName);
            }
        }

        private static void SliderPercent(Listing_Standard list, string label, ref float value)
        {
            list.Label(label + "：" + (value * 100f).ToString("F0") + "%");
            value = Widgets.HorizontalSlider(list.GetRect(28f), value, 0f, 1f);
        }

        private static void SliderInt(Listing_Standard list, string label, ref int value, int min, int max)
        {
            list.Label(label + "：" + value);
            value = Mathf.RoundToInt(Widgets.HorizontalSlider(list.GetRect(28f), value, min, max));
        }

        private static void Stepper(Rect rect, ref float value, float min, float max, float step, string label)
        {
            Widgets.Label(rect.LeftPart(0.34f), label);
            Rect minus = new Rect(rect.x + rect.width * 0.34f, rect.y, 24f, rect.height);
            Rect plus = new Rect(rect.xMax - 24f, rect.y, 24f, rect.height);
            Rect valueRect = new Rect(minus.xMax, rect.y, plus.x - minus.xMax, rect.height);

            if (Widgets.ButtonText(minus, "-"))
            {
                value = Mathf.Clamp(value - step, min, max);
            }
            if (Widgets.ButtonText(plus, "+"))
            {
                value = Mathf.Clamp(value + step, min, max);
            }
            Widgets.Label(valueRect, value.ToString("F0"));
        }

        private static void StepperInt(Rect rect, ref int value, int min, int max, int step, string label)
        {
            Widgets.Label(rect.LeftPart(0.34f), label);
            Rect minus = new Rect(rect.x + rect.width * 0.34f, rect.y, 24f, rect.height);
            Rect plus = new Rect(rect.xMax - 24f, rect.y, 24f, rect.height);
            Rect valueRect = new Rect(minus.xMax, rect.y, plus.x - minus.xMax, rect.height);

            if (Widgets.ButtonText(minus, "-"))
            {
                value = Mathf.Clamp(value - step, min, max);
            }
            if (Widgets.ButtonText(plus, "+"))
            {
                value = Mathf.Clamp(value + step, min, max);
            }
            Widgets.Label(valueRect, value >= 0 ? "+" + value : value.ToString());
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
