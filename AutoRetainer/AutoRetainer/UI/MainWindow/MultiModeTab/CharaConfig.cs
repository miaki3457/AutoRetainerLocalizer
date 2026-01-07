using AutoRetainerAPI.Configuration;
using Dalamud.Interface.Components;
using PunishLib.ImGuiMethods;

namespace AutoRetainer.UI.MainWindow.MultiModeTab;
public class CharaConfig
{
    public static void Draw(OfflineCharacterData data, bool isRetainer)
    {
        ImGui.PushID(data.CID.ToString());
        SharedUI.DrawMultiModeHeader(data);
        var b = new NuiBuilder()

        .Section("通用角色特定設定")
        .Widget(() =>
        {
            SharedUI.DrawServiceAccSelector(data);
            SharedUI.DrawPreferredCharacterUI(data);
        });
        if(isRetainer)
        {
            b = b.Section("僱員管理").Widget(() =>
            {
                ImGuiEx.Text($"自動籌備稀有品:");
                if(!AutoGCHandin.Operation)
                {
                    ImGuiEx.SetNextItemWidthScaled(200f);
                    ImGuiEx.EnumCombo("##gcHandin", ref data.GCDeliveryType);
                }
                else
                {
                    ImGuiEx.Text($"目前無法更改此設定");
                }
            });
        }
        else
        {
            b = b.Section("遠航探索").Widget(() =>
            {
                ImGui.Checkbox($"等待航程完成", ref data.MultiWaitForAllDeployables);
                ImGuiComponents.HelpMarker("此設定類似於全域選項，但應用於單一角色。啟用後，AutoRetainer 將在登入角色之前等待所有遠航探索返回。如果您因其他原因已經登錄，它仍然會重新派遣已完成的潛水艇/飛艇，除非全域設定「即使已登錄也等待」也同時開啟。");
            });
        }
        b = b.Section("傳送覆蓋設定", data.GetAreTeleportSettingsOverriden() ? ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBg] with { X = 1f } : null, true)
        .Widget(() =>
        {
            ImGuiEx.Text($"您可以為每個角色覆蓋傳送設置");
            bool? demo = null;
            ImGuiEx.Checkbox("標記此圖示的選項將使用全域配置中的值", ref demo);
            ImGuiEx.Checkbox("啟用", ref data.TeleportOptionsOverride.Enabled);
            ImGui.Indent();
            ImGuiEx.Checkbox("為傳喚鈴傳送...", ref data.TeleportOptionsOverride.Retainers);
            ImGui.Indent();
            ImGuiEx.Checkbox("...到私人房屋", ref data.TeleportOptionsOverride.RetainersPrivate);
            ImGuiEx.Checkbox("...到共享房屋", ref data.TeleportOptionsOverride.RetainersShared);
            ImGuiEx.Checkbox("...到部隊房屋", ref data.TeleportOptionsOverride.RetainersFC);
            ImGuiEx.Checkbox("...到公寓", ref data.TeleportOptionsOverride.RetainersApartment);
            ImGui.Text("如果以上所有選項都停用或失敗，將會傳送到旅館");
            ImGui.Unindent();
            ImGuiEx.Checkbox("為潛水艇/飛艇傳送至部隊房屋", ref data.TeleportOptionsOverride.Deployables);
            ImGui.Unindent(); 
        }).Draw();
        SharedUI.DrawExcludeReset(data);
        ImGui.PopID();
    }
}
