# CHANGELOG

## [0.1.0] - 2026-11-19

首个可运行版本：中式姓名 + 关系系统 + 婚配制度设置。

### 新增

- **姓名**：`PawnBioAndNameGenerator.GeneratePawnName` postfix，按设置把人类殖民者换成「姓+名」；`NameTriple` 显示补丁（`ToStringFull` / `ToStringShort` / `Nick` / `ToString`）输出「姓+名」。
- **姓名数据**：`Data/Names/{Surnames,MaleGiven,FemaleGiven,UnisexGiven,Nick}.txt`，缺文件时回退内置池。
- **关系 Def（35 个）**：生成型（同学/发小/朋友/网友/笔友/工友/邻居/战友/师徒/冤家）、事件型（救命/被救/生死之交/医生/病人/前病人/病友/狱友/同谋/师生/袍泽/牌友/酒友/室友/旅伴/仇人/结拜/义兄妹）、义亲（义父义母/义子义女）、闺蜜、婚配（正/侧/共配偶）、派生（情敌/同门）。全部 `generationChanceFactor = 0`，由本 Mod 自己播种。
- **ThoughtDef**：屈居偏房 −5、配偶纳妾 −8、结为手足 +8、闺蜜反目 −8。
- **`CK_RelationGameComponent`**：旧识播种（每 2500 tick，按新殖民者 20% 概率）、自动结拜与闺蜜反目结算（每 60000 tick）、连续天数 Scribe。
- **`RelationService`**：成对写/删、去重、配偶名次、血亲/恋偶判断。
- **婚配**：`MarriageCeremonyUtility.Married` 前后缀、`ChangeSpouseRelationsToExSpouse` 前缀跳过旧配偶转换、`InteractionWorker_MarriageProposal` 权重/接受率放宽、`PawnRelationDef.GetGenderSpecificLabel` 改写正/侧室标签。
- **义亲**：`Pawn.GetGizmos` postfix 添加「认义亲」，`Find.Targeter` 选目标并校验。
- **设置**：`CK_Settings` + 四页签 `CK_SettingsWindow`（姓名/关系/婚配/关于），中英文 Keyed。
- **构建**：`build.ps1` 的 `-Deploy` 现在同时复制 `Data`（姓名池），否则运行时会退回内置小池。

### 修复

- `CK_RelationGameComponent` 构造：`Game.FillComponents` 用 `Activator.CreateInstance(type, game)`，组件必须提供 `(Game game)` 构造且不能调用 `base(game)`。
- `build.ps1` 保存为 UTF-8 BOM，避免 PowerShell 5.1 按 ANSI 读取导致中文乱码与语法错误。

### 验证

- `.\build.ps1` 0 错误 0 警告，DLL 37,888 bytes；`-Deploy` SHA256 校验通过。
- 实机（1.6.4871，Core + Biotech + Odyssey + Harmony 2.4.3）：中文姓名显示、邻居关系播种与社交页显示、义亲 gizmo、设置页渲染均正常，`Player.log` 无异常。
- 未实机验证：多人婚礼与正/侧室标签、自动结拜满 5 天、事件型关系钩子。
