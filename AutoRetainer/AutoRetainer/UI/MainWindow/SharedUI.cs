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
        if(ImGui.BeginCombo("##Service Account Selection", $"Service Account {data.ServiceAccount + 1}", ImGuiComboFlags.HeightLarge))
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
        ImGuiComponents.HelpMarker("When operating in multi mode, if there are no other characters with imminent ventures to collect, it will relog back to your preferred character.");
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
            ImGuiComponents.HelpMarker("Excluding this character will immediately reset it's settings, remove it from this list and exclude all retainers from being processed. You can still run manual tasks on it's retainers. You can cancel this action in settings.");
            if(ImGuiEx.ButtonCtrl("重置角色數據"))
            {
                new TickScheduler(() => C.OfflineData.RemoveAll(x => x.CID == data.CID));
            }
            ImGuiComponents.HelpMarker("Character's saved data will be removed without excluding it. Character data will be regenerated once you log back into this character.");

                if(ImGui.Button("清除部隊數據"))
            {
                data.ClearFCData();
            }
            ImGuiComponents.HelpMarker("Free company data, airships and submersibles will be removed from this character. Data will be regenerated once available.");
        }).Draw();
    }
}
