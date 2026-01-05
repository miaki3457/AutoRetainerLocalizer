using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.InventoryCleanupEntries;
public unsafe sealed class DiscardList : InventoryManagementBase
{
    public override string Name => "背包清理/丟棄清單";
    private InventoryManagementCommon InventoryManagementCommon = new();

    public override int DisplayPriority => -1;

    private DiscardList()
    {
        Builder = InventoryCleanupCommon.CreateCleanupHeaderBuilder()
            .Section(Name)
            .TextWrapped("這些物品將始終被丟棄，不論其來源為何，只要其堆疊數量不超過下方可設定的數量。丟棄動作會非常頻繁地發生，會在每次可能改變背包的操作前後進行。丟棄優先級最高，即使同一物品也存在於販售或分解清單中，也會被丟棄。已設定為保護的物品不會被丟棄。 ")
            .InputInt(150f, $"Maximum stack size to be discarded", () => ref InventoryCleanupCommon.SelectedPlan.IMDiscardStackLimit)
            .Widget(() => InventoryManagementCommon.DrawListNew(
                itemId => InventoryCleanupCommon.SelectedPlan.AddItemToList(IMListKind.Discard, itemId, out _),
                itemId => InventoryCleanupCommon.SelectedPlan.IMDiscardList.Remove(itemId),
                InventoryCleanupCommon.SelectedPlan.IMDiscardList,
                (x) =>
                {
                    ImGui.SameLine();
                    ImGui.PushFont(UiBuilder.IconFont);
                    ImGuiEx.CollectionButtonCheckbox(FontAwesomeIcon.Database.ToIconString(), x, InventoryCleanupCommon.SelectedPlan.IMDiscardIgnoreStack);
                    ImGui.PopFont();
                    ImGuiEx.Tooltip($"忽略此物品的堆疊設定");
                }))
            .Separator()
            .Widget(() =>
            {
                InventoryManagementCommon.ImportFromArDiscard(InventoryCleanupCommon.SelectedPlan.IMDiscardList);
            });
    }
}