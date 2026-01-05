using AutoRetainerAPI.Configuration;
using System.Collections.Frozen;

namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeContingency : NeoUIEntry
{
    private static readonly FrozenDictionary<WorkshopFailAction, string> WorkshopFailActionNames = new Dictionary<WorkshopFailAction, string>()
    {
        [WorkshopFailAction.StopPlugin] = "Halt all plugin operation",
        [WorkshopFailAction.ExcludeVessel] = "Exclude deployable from operation",
        [WorkshopFailAction.ExcludeChar] = "Exclude captain from multi mode rotation",
    }.ToFrozenDictionary();

    public override string Path => "多角色模式/應急設定";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("應急設定")
        .TextWrapped("在此配置各種常見故障狀態或潛在操作錯誤時的緊急方案")
        .EnumComboFullWidth(null, "青磷水耗盡", () => ref C.FailureNoFuel, (x) => x != WorkshopFailAction.ExcludeVessel, WorkshopFailActionNames, "Applies selected fallback action in the case of insufficient Ceruleum Tanks to deploy vessel on a new voyage.")
        .EnumComboFullWidth(null, "無法維修艦艇", () => ref C.FailureNoRepair, null, WorkshopFailActionNames, "Applies selected fallback action in the case of insufficient Magitek Repair Materials to repair a vessel.")
        .EnumComboFullWidth(null, "背包空間不足", () => ref C.FailureNoInventory, (x) => x != WorkshopFailAction.ExcludeVessel, WorkshopFailActionNames, "Applies selected fallback action in the case of the captain's inventory having insufficient space to receive voyage rewards.")
        .EnumComboFullWidth(null, "關鍵操作失敗", () => ref C.FailureGeneric, (x) => x != WorkshopFailAction.ExcludeVessel, WorkshopFailActionNames, "Applies selected fallback action in the case of any unknown or miscellaneous error.")
        .Widget("被 GM 關監獄", (x) =>
        {
            ImGui.BeginDisabled();
            ImGuiEx.SetNextItemFullWidth();
            if(ImGui.BeginCombo("##jailsel", "強制結束遊戲")) { ImGui.EndCombo(); }
            ImGui.EndDisabled();
        }, "Applies selected fallback action in the case if you got jailed by the GM while plugin is running. Good luck!");
}
