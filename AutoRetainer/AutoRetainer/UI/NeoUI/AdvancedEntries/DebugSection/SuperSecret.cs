using Dalamud.Interface.Components;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;

internal class SuperSecret : DebugSectionBase
{
    public override void Draw()
    {
        ImGuiEx.TextWrapped(ImGuiColors.ParsedOrange, "這裡可能會發生任何狀況");
        ImGui.Checkbox("舊版傳喚鈴感應", ref C.OldRetainerSense);
        ImGuiComponents.HelpMarker("偵測並使用玩家有效距離內最近的傳喚鈴");
        ImGuiEx.TextWrapped(ImGuiColors.DalamudGrey, "在多角色模式執行期間，強制啟用傳喚鈴感應");
        ImGui.Separator();
        ImGui.Checkbox($"不安全選項保護", ref C.UnsafeProtection);
        ImGui.SameLine();
        if(ImGui.Button($"寫入登錄檔"))
        {
            Safety.Set(C.UnsafeProtection);
        }
        var g = Safety.Get();
        ImGuiEx.Text(g ? ImGuiColors.ParsedGreen : ImGuiColors.DalamudRed, $"安全標記: {(g ? "Present" : "Absent")}");
    }
}
