using System;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Conditions;
using GatherBuddy.Automation;
using FFXIVClientStructs.FFXIV.Client.Game;
using GatherBuddy.Plugin;
using static GatherBuddy.Automation.AddonMaster;

namespace GatherBuddy.AutoGather;

public partial class AutoGather
{

    unsafe int SpiritbondMax
    {
        get
        {
            if (!GatherBuddy.Config.AutoGatherConfig.DoMaterialize) return 0;

            var inventory = InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems);
            var result    = 0;
            for (var slot = 0; slot < inventory->Size; slot++)
            {
                var inventoryItem = inventory->GetInventorySlot(slot);
                if (inventoryItem == null || inventoryItem->ItemId <= 0)
                    continue;

                //GatherBuddy.Log.Debug("Slot " + slot + " has " + inventoryItem->Spiritbond + " Spiritbond");
                if (inventoryItem->SpiritbondOrCollectability == 10000)
                {
                    result++;
                }
            }

            return result;
        }
    }

    unsafe void DoMateriaExtraction()
    {
        if (!QuestManager.IsQuestComplete(66174))
        {
            GatherBuddy.Config.AutoGatherConfig.DoMaterialize = false;
            Communicator.PrintError("[GatherBuddy Reborn] Materia Extraction enabled but relevant quest not complete yet. Feature disabled.");
            return;
        }
        if (MaterializeAddon == null)
        {
            StopNavigation();
            EnqueueActionWithDelay(() => ActionManager.Instance()->UseAction(ActionType.GeneralAction, 14));
            TaskManager.Enqueue(() => MaterializeAddon != null, "MaterializeAddon != null");
            return;
        }

        EnqueueActionWithDelay(() => { if (MaterializeAddon is var addon and not null) Callback.Fire(&addon->AtkUnitBase, true, 2, 0); });
        // TC(api13 / game 7.20): the game still shows the MaterializeDialog confirm popup, and
        // YesAlready is locked for the whole AutoGather session (AutoGather.cs Enabled setter), so
        // nothing else dismisses it. Upstream d600f75c dropped this wait+click; without it the
        // sequence stalls on the item-selection window. Mirrors the plugin's own Crafting path
        // (Crafting/CraftingTasks.cs). The Occupied39 disjunct keeps this correct on a client that
        // does not show the popup: it falls through and the null-guarded click is a no-op.
        TaskManager.Enqueue(() => MaterializeDialogAddon != null || Dalamud.Conditions[ConditionFlag.Occupied39],
            1000, "MaterializeDialogAddon != null");
        EnqueueActionWithDelay(() => { if (MaterializeDialogAddon is var dialog and not null) new MaterializeDialog(dialog).Materialize(); });
        TaskManager.Enqueue(() => !Dalamud.Conditions[ConditionFlag.Occupied39], "!Dalamud.Conditions[ConditionFlag.Occupied39]");
        EnqueueActionWithDelay(() => { });

        if (SpiritbondMax == 1) 
        {
            EnqueueActionWithDelay(() => { if (MaterializeAddon is var addon and not null) Callback.Fire(&addon->AtkUnitBase, true, -1); });
        }
    }
}
