using UnityEngine;
using Verse;

namespace ColonySurnameZh
{
    /// <summary>
    /// Mod 入口：持有设置，并把设置页交给 <see cref="CK_SettingsWindow"/>。
    /// </summary>
    public class ColonySurnameZhMod : Mod
    {
        public static ColonySurnameZhMod Instance { get; private set; }

        public static CK_Settings Settings =>
            Instance?.GetSettings<CK_Settings>();

        public ColonySurnameZhMod(ModContentPack content) : base(content)
        {
            Instance = this;
            GetSettings<CK_Settings>();
        }

        public override string SettingsCategory()
        {
            return "CK.SettingsCategory".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            new CK_SettingsWindow(Settings).Draw(inRect);
        }

        public override void WriteSettings()
        {
            Settings?.Validate();
            base.WriteSettings();
        }
    }
}
