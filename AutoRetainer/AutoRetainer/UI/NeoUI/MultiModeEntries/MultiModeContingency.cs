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
        .EnumComboFullWidth(null, "青磷水耗盡", () => ref C.FailureNoFuel, (x) => x != WorkshopFailAction.ExcludeVessel, WorkshopFailActionNames, "當青磷水不足以進行新航次時，執行所選的備用方案（如中止或切換角色）")
        .EnumComboFullWidth(null, "無法維修艦艇", () => ref C.FailureNoRepair, null, WorkshopFailActionNames, "魔導修理材料不足以修理潛艇時，執行所選的備用方案。")
        .EnumComboFullWidth(null, "背包空間不足", () => ref C.FailureNoInventory, (x) => x != WorkshopFailAction.ExcludeVessel, WorkshopFailActionNames, "當身上背包空間不足以接收航行獎勵時，執行所選的備用方案。")
        .EnumComboFullWidth(null, "關鍵操作失敗", () => ref C.FailureGeneric, (x) => x != WorkshopFailAction.ExcludeVessel, WorkshopFailActionNames, "發生任何未知或雜項錯誤時，執行所選的備用方案。")
        .Widget("被 GM 關監獄", (x) =>
        {
            ImGui.BeginDisabled();
            ImGuiEx.SetNextItemFullWidth();
            if(ImGui.BeginCombo("##jailsel", "強制結束遊戲")) { ImGui.EndCombo(); }
            ImGui.EndDisabled();
        }, "如果你在插件執行期間被 GM 關進小黑屋（監獄）時，執行所選的備用方案。祝你好運！");
}
