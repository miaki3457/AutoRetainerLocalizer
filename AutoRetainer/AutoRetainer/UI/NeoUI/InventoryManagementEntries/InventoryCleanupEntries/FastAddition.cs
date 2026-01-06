using ECommons.Automation;
using ECommons.ExcelServices;
using ECommons.Throttlers;
using ECommons.WindowsFormsReflector;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.InventoryCleanupEntries;
public unsafe class FastAddition : InventoryManagementBase
{
    public override string Name { get; } = "Inventory Cleanup/Fast Addition and Removal";

    private FastAddition()
    {
        Builder = InventoryCleanupCommon.CreateCleanupHeaderBuilder()
        .Section(Name)
        .Widget(() =>
        {
            var selectedSettings = InventoryCleanupCommon.SelectedPlan;
            ImGuiEx.TextWrapped(GradientColor.Get(EColor.RedBright, EColor.YellowBright), $"當此文字可見時，將滑鼠懸停在物品上並按住按鍵:");
            ImGuiEx.Text(!ImGui.GetIO().KeyShift ? ImGuiColors.DalamudGrey : ImGuiColors.DalamudRed, $"Shift - 添加至快速僱員販售清單");
            ImGuiEx.Text($"* 已在「無條件出售」及「丟棄」清單內的物品不會添加至快速僱員販售清單");
            ImGuiEx.Text(!ImGui.GetIO().KeyCtrl ? ImGuiColors.DalamudGrey : ImGuiColors.DalamudRed, $"Ctrl - 添加至無條件出售清單");
            ImGuiEx.Text($"* 物品將會從其他清單「移至」無條件出售清單");
            ImGuiEx.Text(!IsKeyPressed(Keys.Tab) ? ImGuiColors.DalamudGrey : ImGuiColors.DalamudRed, $"Tab - 添加至丟棄清單");
            ImGuiEx.Text($"* 物品將會從其他清單「移至」丟棄清單");
            //ImGuiEx.Text(IsKeyPressed(Keys.Space) ? ImGuiColors.DalamudGrey : ImGuiColors.DalamudRed, $"Space - add to Desynthesis List");
            //ImGuiEx.Text($"* Items that already in other lists WILL BE MOVED to Desynthesis List");
            ImGuiEx.Text(!ImGui.GetIO().KeyAlt ? ImGuiColors.DalamudGrey : ImGuiColors.DalamudRed, $"Alt - 從任何清單中刪除");
            ImGuiEx.Text("受保護的物品不受此操作影響");
            if(Svc.GameGui.HoveredItem > 0)
            {
                var id = (uint)(Svc.GameGui.HoveredItem % 1000000);
                if(ImGui.GetIO().KeyShift)
                {
                    if(!selectedSettings.IMProtectList.Contains(id) 
                    && !selectedSettings.IMAutoVendorSoft.Contains(id)
                    && !selectedSettings.IMAutoVendorHard.Contains(id)
                    && !selectedSettings.IMDiscardList.Contains(id)
                    && !selectedSettings.IMDesynth.Contains(id)
                    )
                    {
                        if(selectedSettings.AddItemToList(IMListKind.SoftSell, id, out var error))
                        {
                            Notify.Success($"Added {ExcelItemHelper.GetName(id)} to Quick Venture Sell List");
                        }
                        else
                        {
                            if(EzThrottler.Throttle($"Error_{error}", 1000)) Notify.Error(error);
                        }
                    }
                }
                if(ImGui.GetIO().KeyCtrl)
                {
                    if(!selectedSettings.IMProtectList.Contains(id) && !selectedSettings.IMAutoVendorHard.Contains(id) && !selectedSettings.IMAutoVendorSoft.Contains(id))
                    {
                        if(selectedSettings.AddItemToList(IMListKind.HardSell, id, out var error))
                        {
                            Notify.Success($"Added {ExcelItemHelper.GetName(id)} to Unconditional Sell List");
                        }
                        else
                        {
                            if(EzThrottler.Throttle($"Error_{error}", 1000)) Notify.Error(error);
                        }
                    }
                }
                if(!CSFramework.Instance()->WindowInactive && IsKeyPressed(Keys.Tab))
                {
                    if(!selectedSettings.IMProtectList.Contains(id) && !selectedSettings.IMDiscardList.Contains(id))
                    {
                        if(selectedSettings.AddItemToList(IMListKind.Discard, id, out var error))
                        {
                            Notify.Success($"Added {ExcelItemHelper.GetName(id)} to Discard List");
                        }
                        else
                        {
                            if(EzThrottler.Throttle($"Error_{error}", 1000)) Notify.Error(error);
                        }
                    }
                }
                /*if(!CSFramework.Instance()->WindowInactive && IsKeyPressed(Keys.Space))
                {
                    if(!selectedSettings.IMProtectList.Contains(id) && !selectedSettings.IMDesynth.Contains(id))
                    {
                        if(selectedSettings.AddItemToList(IMListKind.Desynth, id, out var error))
                        {
                            Notify.Success($"Added {ExcelItemHelper.GetName(id)} to Desynthesis List");
                        }
                        else
                        {
                            if(EzThrottler.Throttle($"Error_{error}", 1000)) Notify.Error(error);
                        }
                    }
                }*/
                if(ImGui.GetIO().KeyAlt)
                {
                    if(selectedSettings.IMAutoVendorSoft.Remove(id)) Notify.Info($"移除 {ExcelItemHelper.GetName(id)} 從快速僱員販售清單");
                    if(selectedSettings.IMAutoVendorHard.Remove(id)) Notify.Info($"移除 {ExcelItemHelper.GetName(id)} 從無條件出售清單");
                    if(selectedSettings.IMDiscardList.Remove(id)) Notify.Info($"移除 {ExcelItemHelper.GetName(id)} 從丟棄清單");
                    if(selectedSettings.IMDesynth.Remove(id)) Notify.Info($"移除 {ExcelItemHelper.GetName(id)} 從分解清單");
                }
            }
        });
        DisplayPriority = -10;
    }
}
