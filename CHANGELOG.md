# CHANGELOG

## [Unreleased]

## [0.2.0] - 2026-11-19

关系改成「每条关系一个参数组」，并让姓名页与关系页彼此独立。

### 新增

- **每关系参数组**：`CK_RelationTuning`（enabled / weight / opinion）与 `CK_RelationCatalog`（35 条关系、五段分组：旧识 / 事件 / 家庭 / 婚配 / 派生）。
- 关系页每行：启用复选框 + 权重步进器（仅生成型）+ 好感步进器（−40…+40），好感即时写回 Def。
- 规则段：结拜阈值（默认 60）与连续天数（默认 5）、塑料闺蜜开关、义亲开关 / 年龄差 / 好感 / 上限、重置默认。
- 婚配页新增：正/侧室标签、共夫/共妻、嫉妒念头、争宠概率（默认 3%）。
- `CK_Thought_Displaced`（争宠落败 −4，5 天）。
- 设置持久化用 `tuningKeys` / `tuningValues` 两个并行列表，避免字典键值在 Scribe 下的兼容问题。

### 变更

- 旧识播种改为按设置权重挑选（不再硬编码权重）；冤家为负向候选；师徒按年龄决定方向。
- `RelationService.AddPair` 统一按关系开关拦截；关闭开关不会删除已存在的关系。
- 婚配的共夫/共妻、嫉妒念头、正侧标签分别受各自开关控制。
- 版本号 0.1.0 → 0.2.0。
- 修复 `About.xml` 被误写成乱码的问题（重写为 UTF-8）。

### 验证

- `.\build.ps1` 0 错误 0 警告，DLL 50,176 bytes；`-Deploy` SHA256 校验通过。
- 未实机验证：关系页 35 行参数组的实际渲染与点击（本次会话游戏窗口被前台窗口遮挡，未能可靠截图），需人工打开「选项 → Mod 选项 → Chinese Names & Relations → Relations」确认。

### 新增

- **姓名池扩充**：`MaleGiven.txt` 132 → 227、`FemaleGiven.txt` 142 → 222、`UnisexGiven.txt` 40 → 79，全部为单字，仍由原逻辑随机拼成 1–2 字名；三个池内部及与旧条目均无重复。

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
