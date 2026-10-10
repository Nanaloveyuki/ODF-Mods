# ODF Mods

基于 Oxygen DSL Framework 的独立 RimWorld Mod monorepo。每个 `Mods/<Name>/` 是独立发布包，包名统一为 `odf.nanaloveyuki.<name>`。不把多个 Mod 打成一个必须全部启用的包，也不为 RHAH 增加依赖。

## 当前 Mod

| 目录 | 包名 | 功能 |
| --- | --- | --- |
| [PrisonerNames](Mods/PrisonerNames/README.md) | `odf.nanaloveyuki.prisonernames` | 保留原姓名；支持命名占位符、种族前缀、持久分数／等级、事件规则和单人覆盖 |

`PrisonerNames` 依赖 Harmony、`nanaloveyuki.oxygendsl` 和 `Nanaloveyuki.IrisMenus`，不依赖 RHAH、HAR 或旧囚犯代号。旧 Mod 仅参考公开功能说明；未读取或复用其实现源码。根目录已有 LICENSE 保持不变。ODF 的 IrisMenus 适配为依赖 Mod 自动提供本机网页编辑／退出操作，IrisMenus 不反向依赖 ODF。

## 构建和部署

需要 .NET SDK、RimWorld 1.6 本机程序集及相邻 `../Oxygen-Dsl-Framework` 仓库。游戏程序集与 Harmony 不随发布包分发；Oxygen 程序集只由框架 Mod 提供。

```sh
# 游戏运行时也可构建；发布包输出 artifacts/PrisonerNames
bash scripts/deploy.sh PrisonerNames --build-only

# 退出游戏后部署至 RimWorld/Mods/ODF-PrisonerNames
bash scripts/deploy.sh PrisonerNames

# 构建并部署当前所有 Mod
bash scripts/deploy.sh all

# 少量语言消费端行为检查
dotnet run --project Tests/PrisonerNames.Checks.csproj -c Release
```

默认游戏路径为 `/mnt/e/Apps/Steam/steamapps/common/RimWorld`；`RIMWORLD_DIR` 可覆盖。需要同时安装当前 Oxygen 框架；此仓库部署脚本不会自行修改玩家的启用列表或旧 Mod。部署前检查游戏进程，检查目标包名，并逐文件核对 SHA-256。

新增 Mod 在 `Mods/` 下独立放置 About、LoadFolders、Languages、Oxygen 和 Source；程序集名使用 `Odf.<DirectoryName>`，输出 `tmp/build/<DirectoryName>/`，在 `ODF-Mods.slnx` 添加项目。每个发布包只包含自己的程序集，不重复打包 Oxygen。

## 玩家配置与网页编辑验收（2026-10-10）

- Release 构建 0 警告、0 错误，20 项命名行为检查；30 项链接生产代码的状态 smoke 与事件总线 smoke 通过。引擎边界使用替身，不算游戏 UI 证明。
- 使用真实 RimWorld Scribe 的独立 XML 往返保留分数／等级、持久值及托管余量；没有完整游戏存读档或 Pawn 交叉引用验收。
- 真实 Chromium 与 ODF 生产网页后端验证文本／积木保存、事实试算、错误应用保留、导出及外部版本冲突。发布包不捆绑 ODF 或 IrisMenus 程序集。
- 新窗口与原生事件未进游戏实测，未部署本轮包到玩家游戏安装目录。用法和当前边界见 Mod README。

## 旧版代号游戏验收

- Release 构建 0 警告、0 错误，10 项命名规则行为检查通过。
- 隔离 RimWorld 1.6.4871／Windows Unity Mono 实测：实际 Pawn 标签随处置方式及 Flu 变化；错误应用保留有效规则；手动覆盖／清除、关闭／启用代号正常，原姓名未变。
- 暂停时无需推进 tick 自动刷新。实际窗口按钮载入中文草稿并应用，观察到 `囚002·维持生命·患病`；原生地图标签和代号窗口显示一致。
- 最终另存并读回后，编号 001/002、单人覆盖和中文规则结果保留。释放后立即显示原名。截图位于隔离配置 `RimCarbon14/artifacts/cfc454eeb76f481f95952eccaa00f910/d0b90fcd64804d1cad1905e68c990826.png`，回执保留在忽略的 `tmp/carbon-receipts.jsonl`。
- 初次 smoke 对 LabelNoCount 错误地要求不含原生头衔，修正检查而非删除头衔；旧存档已保存的手动代号优先于动态规则，后续用新囚犯执行动态场景。以上失败没有记为通过。
- 未验证大型 Mod 组合、第三方自绘姓名 UI、跨地图／商队运输和全部 DLC 组合。没有重跑框架旧性能矩阵。
