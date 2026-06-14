# GatherBuddy Reborn（繁中移植版 · TC12） / Traditional-Chinese Port

> 簡化採集與釣魚。<br>
> Simplify gathering and fishing.

**繁體中文**：這是 **[GatherBuddy Reborn](https://github.com/FFXIV-CombatReborn/GatherBuddyReborn)** 的繁體中文客戶端移植版，對應 **FFXIV 7.1 / yanmucorp Dalamud API12（.NET 9）**。本專案僅做相容性移植，**非官方、非原作維護**；所有原始功能與設計著作權歸原作者 **The Combat Reborn Team**。

**English**: A Traditional-Chinese-client port of **[GatherBuddy Reborn](https://github.com/FFXIV-CombatReborn/GatherBuddyReborn)** targeting **FFXIV 7.1 / yanmucorp Dalamud API12 (.NET 9)**. Compatibility port only — **unofficial and not maintained by the original author**. All original work © **The Combat Reborn Team**.

---

## 這是什麼 / About

用物品名稱即可查詢採集點、魚與其座標，並提供追蹤特殊出現時間與天氣條件的介面，讓採集與釣魚更輕鬆。

Look up nodes, fish and their locations by item name, with a UI for tracking special uptime windows and weather conditions.

## 安裝 / Installation

**繁體中文**
1. 使用 **XIVTCLauncher** 啟動繁體中文客戶端。
2. 遊戲內輸入 `/xlsettings` → 切到 **Experimental** 分頁 → **Custom Plugin Repositories（自訂插件庫）**。
3. 貼上下列網址並按 **+** 儲存：
   ```
   https://raw.githubusercontent.com/lilasrepo/DalamudPlugins/main/pluginmaster.json
   ```
4. 輸入 `/xlplugins`，搜尋 **GatherBuddy Reborn (TC12)** → 安裝 → 啟用。

**English**
1. Launch the Traditional-Chinese client with **XIVTCLauncher**.
2. In-game, type `/xlsettings` → **Experimental** tab → **Custom Plugin Repositories**.
3. Add this URL and save with **+**:
   ```
   https://raw.githubusercontent.com/lilasrepo/DalamudPlugins/main/pluginmaster.json
   ```
4. Type `/xlplugins`, search **GatherBuddy Reborn (TC12)** → Install → Enable.

## 對應版本 / Compatibility

| 項目 / Item | 版本 / Version |
|---|---|
| 遊戲 / Game | FFXIV 7.1（繁中客戶端 / TC client） |
| Dalamud | yanmucorp API12（.NET 9） |
| 移植自上游 / Ported from upstream | v7.2.4.0 |

## 原作與授權 / Credits & License

本專案 fork 自 **[FFXIV-CombatReborn/GatherBuddyReborn](https://github.com/FFXIV-CombatReborn/GatherBuddyReborn)**，授權沿用上游；所有原始功能著作權歸 **The Combat Reborn Team**。<br>
Forked from **[FFXIV-CombatReborn/GatherBuddyReborn](https://github.com/FFXIV-CombatReborn/GatherBuddyReborn)**. License follows upstream; all original work © **The Combat Reborn Team**.

## 免責聲明 / Disclaimer

第三方插件，使用風險自負。**移植相關問題請回報到本 repo 的 Issues，請勿打擾上游原作者。**<br>
Third-party plugin — use at your own risk. **For port-specific issues please open an Issue here; do not contact the upstream author.**
