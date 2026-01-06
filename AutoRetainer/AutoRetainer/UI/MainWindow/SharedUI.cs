using AutoRetainerAPI.Configuration;
using Dalamud.Interface.Components;
using PunishLib.ImGuiMethods;

namespace AutoRetainer.UI.MainWindow;

internal static class SharedUI
{
    internal static void DrawLockout(OfflineCharacterData data)
    {
        if(data.IsLockedOut())
        {
            FontAwesome.PrintV(EColor.RedBright, FontAwesomeIcon.Lock);
            ImGuiEx.Tooltip("此角色位於您已暫時停用的資料中心。請前往配置介面取消停用。");
            ImGui.SameLine();
        }
    }

    internal static void DrawMultiModeHeader(OfflineCharacterData data, string overrideTitle = null)
    {
        var b = true;
        ImGui.CollapsingHeader($"{Censor.Character(data.Name)} {overrideTitle ?? "Configuration"}##conf", ref b, ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.OpenOnArrow);
        if(b == false)
        {
            ImGui.CloseCurrentPopup();
        }
        ImGui.Dummy(new(500, 1));
    }

    internal static void DrawServiceAccSelector(OfflineCharacterData data)
    {
        ImGuiEx.Text($"服務帳戶選擇");
        ImGuiEx.SetNextItemWidthScaled(150);
        if(ImGui.BeginCombo("##Service Account Selection", $"服務帳戶 {data.ServiceAccount + 1}", ImGuiComboFlags.HeightLarge))
        {
            for(var i = 1; i <= 10; i++)
            {
                if(ImGui.Selectable($"Service Account {i}"))
                {
                    data.ServiceAccount = i - 1;
                }
            }
            ImGui.EndCombo();
        }
    }

    internal static void DrawPreferredCharacterUI(OfflineCharacterData data)
    {
        if(ImGui.Checkbox("首選角色", ref data.Preferred))
        {
            foreach(var z in C.OfflineData)
            {
                if(z.CID != data.CID)
                {
                    z.Preferred = false;
                }
            }
        }
        ImGuiComponents.HelpMarker("在多角色模式下，當沒有其他角色需要收取僱員時，插件會自動切換回您的首選角色。");
    }

    internal static void DrawExcludeReset(OfflineCharacterData data)
    {
        new NuiBuilder().Section("角色資料清除/重置", collapsible: true)
        .Widget(() =>
        {
            if(ImGuiEx.ButtonCtrl("排除角色"))
            {
                C.Blacklist.Add((data.CID, data.Name));
            }
            ImGuiComponents.HelpMarker("排除此角色將立即重置其設置，將其移出角色列表，並停止處理其所有僱員。您仍可手動操作此角色的僱員。可在設定中取消此操作。");
            if(ImGuiEx.ButtonCtrl("重置角色數據"))
            {
                new TickScheduler(() => C.OfflineData.RemoveAll(x => x.CID == data.CID));
            }
            ImGuiComponents.HelpMarker("Character's saved data will be removed without excluding it. Character data will be regenerated once you log back into this character.");

                if(ImGui.Button("清除部隊數據"))
            {
                data.ClearFCData();
            }
            ImGuiComponents.HelpMarker("角色保存的資料將被清除但不會排除該角色。當您再次登入此角色時，角色資料將重新產生。");
        }).Draw();
    }
}
