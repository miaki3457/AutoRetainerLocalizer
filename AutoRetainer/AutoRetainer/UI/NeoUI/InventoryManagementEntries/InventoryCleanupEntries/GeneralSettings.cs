using AutoRetainer.Internal.InventoryManagement;
using ECommons.GameHelpers;
using TerraFX.Interop.Windows;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.InventoryCleanupEntries;
public class GeneralSettings : InventoryManagementBase
{
    public override string Name { get; } = "Inventory Cleanup/General Settings";

    private GeneralSettings()
    {
        Builder = InventoryCleanupCommon.CreateCleanupHeaderBuilder()
            .Section(Name)
            .Checkbox($"自動打開僱員寶箱", () => ref InventoryCleanupCommon.SelectedPlan.IMEnableCofferAutoOpen, "僅多角色模式。登出前會自動打開所有寶箱，除非背包空間不足。")
            .Indent()
            .InputInt(100f, "單次開啟最大數量", () => ref InventoryCleanupCommon.SelectedPlan.MaxCoffersAtOnce)
            .Unindent()
            .Checkbox($"啟用將物品出售給僱員", () => ref InventoryCleanupCommon.SelectedPlan.IMEnableAutoVendor, "當 AutoRetainer 將僱員派往任務時，物品將依照背包清理方案自動出售。")
            .Checkbox($"啟用將物品出售給房屋NPC", () => ref InventoryCleanupCommon.SelectedPlan.IMEnableNpcSell, "當 AutoRetainer 進入住宅時，物品將依照背包清理方案出售。住宅 NPC 必須放置在住宅入口附近（非工作台入口），進入後可立即互動。")
            .Indent()
            .Checkbox($"若僱員可用則忽略 NPC", () => ref InventoryCleanupCommon.SelectedPlan.IMSkipVendorIfRetainer)
            .Widget("立即出售", (x) =>
            {
                if(ImGuiEx.Button(x, Player.Interactable && InventoryCleanupCommon.SelectedPlan.IMEnableNpcSell && NpcSaleManager.GetValidNPC() != null && !IsOccupied() && !P.TaskManager.IsBusy))
                {
                    NpcSaleManager.EnqueueIfItemsPresent(true);
                }
            })
            .Unindent()
            .Checkbox($"自動分解物品", () => ref InventoryCleanupCommon.SelectedPlan.IMEnableItemDesynthesis)
            .Indent()
            .Widget("兵裝庫: ", t =>
            {
                ImGuiEx.TextV(t);
                ImGui.SameLine();
                ImGuiEx.RadioButtonBool("分解", "跳過", ref InventoryCleanupCommon.SelectedPlan.IMEnableItemDesynthesisFromArmory, true);
            })
            .Unindent()
            .Checkbox($"啟用右鍵選單整合", () => ref InventoryCleanupCommon.SelectedPlan.IMEnableContextMenu)
            .Checkbox($"允許從兵裝庫出售/丟棄物品", () => ref InventoryCleanupCommon.SelectedPlan.AllowSellFromArmory)
            .Checkbox($"演示模式", () => ref InventoryCleanupCommon.SelectedPlan.IMDry, "不實際出售/丟棄物品，僅在聊天視窗顯示哪些物品將被處理")
            ;
    }
}
