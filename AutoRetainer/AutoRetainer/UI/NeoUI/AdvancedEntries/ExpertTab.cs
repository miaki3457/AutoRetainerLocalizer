using ECommons.Configuration;
using ECommons.Reflection;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries;
public class ExpertTab : NeoUIEntry
{
    public override string Path => "Advanced/Expert Settings";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("Behavior")
        .EnumComboFullWidth(null, "存取僱員鈴鐺時若無可用探險任務的動作：", () => ref C.OpenBellBehaviorNoVentures)
        .EnumComboFullWidth(null, "存取僱員鈴鐺時若有可用探險任務的動作：", () => ref C.OpenBellBehaviorWithVentures)
        .EnumComboFullWidth(null, "存取鈴鐺後任務完成的行為：", () => ref C.TaskCompletedBehaviorAccess)
        .EnumComboFullWidth(null, "手動啟用後任務完成的行為：", () => ref C.TaskCompletedBehaviorManual)
        .EnumComboFullWidth(null, "插件運作期間任務完成的行為：", () => ref C.TaskCompletedBehaviorAuto)
        .TextWrapped(ImGuiColors.DalamudGrey, "\"Close retainer list and disable plugin\" option for 3 previous settings is enforced during MultiMode operation.")
        .Checkbox("如果 5 分鐘內有僱員將完成探險，則停留在僱員選單中", () => ref C.Stay5, "This option is enforced during MultiMode operation.")
        .Checkbox($"關閉僱員列表時自動停用插件", () => ref C.AutoDisable, "Only applies when you exit menu by yourself. Otherwise, settings above apply.")
        .Checkbox($"Do not show plugin status icons", () => ref C.HideOverlayIcons)
        .Checkbox($"Display multi mode type selector", () => ref C.DisplayMMType)
        .Checkbox($"Display deployables checkbox in workshop", () => ref C.ShowDeployables)
        .Checkbox("Enable bailout module", () => ref C.EnableBailout)
        .InputInt(150f, "Timeout before AutoRetainer will attempt to unstuck, seconds", () => ref C.BailoutTimeout)

        .Section("設定")
        .Checkbox($"Disable sorting and collapsing/expanding", () => ref C.NoCurrentCharaOnTop)
        .Checkbox($"Show MultiMode checkbox on plugin UI bar", () => ref C.MultiModeUIBar)
        .SliderIntAsFloat(100f, "Retainer menu delay, seconds", () => ref C.RetainerMenuDelay.ValidateRange(0, 2000), 0, 2000)
        .Checkbox($"Allow venture timer to display negative values", () => ref C.TimerAllowNegative)
        .Checkbox($"Do not error check venture planner", () => ref C.NoErrorCheckPlanner2)
        .Checkbox("Enable Manual relogs character postprocess", () => ref C.AllowManualPostprocess, "Allow manual command invocation while AutoRetainer locked in postprocess. ")
        .Widget("Market Cooldown Overlay", (x) =>
        {
            if(ImGui.Checkbox(x, ref C.MarketCooldownOverlay))
            {
                if(C.MarketCooldownOverlay)
                {
                    P.Memory.OnReceiveMarketPricePacketHook?.Enable();
                }
                else
                {
                    P.Memory.OnReceiveMarketPricePacketHook?.Disable();
                }
            }
        })

        .Section("Integrations")
        .Checkbox($"Artisan integration", () => ref C.ArtisanIntegration, "Automatically enables AutoRetainer while Artisan is Pauses Artisan operation when ventures are ready to be collected and a retainer bell is within range. Once ventures have been dealt with Artisan will be enabled and resume whatever it was doing.")

        .Section("Server Time")
        .Checkbox("Use server time instead of PC time", () => ref C.UseServerTime)

        .Section("Utility")
        .Widget("Cleanup ghost retainers", (x) =>
        {
            if(ImGui.Button(x))
            {
                var i = 0;
                foreach(var d in C.OfflineData)
                {
                    i += d.RetainerData.RemoveAll(x => x.Name == "");
                }
                DuoLog.Information($"Cleaned {i} entries");
            }
        })

        .Section("Import/Export")
        .Widget(() =>
        {
            if(ImGui.Button("匯出（不含角色資料）"))
            {
                var clone = C.JSONClone();
                clone.OfflineData = null;
                clone.AdditionalData = null;
                clone.FCData = null;
                clone.SelectedRetainers = null;
                clone.Blacklist = null;
                clone.AutoLogin = "";
                Copy(EzConfig.DefaultSerializationFactory.Serialize(clone, false));
            }
            if(ImGui.Button("匯入並合併角色資料"))
            {
                try
                {
                    var c = EzConfig.DefaultSerializationFactory.Deserialize<Config>(Paste());
                    c.OfflineData = C.OfflineData;
                    c.AdditionalData = C.AdditionalData;
                    c.FCData = C.FCData;
                    c.SelectedRetainers = C.SelectedRetainers;
                    c.Blacklist = C.Blacklist;
                    c.AutoLogin = C.AutoLogin;
                    if(c.GetType().GetFieldPropertyUnions().Any(x => x.GetValue(c) == null)) throw new NullReferenceException();
                    EzConfig.SaveConfiguration(C, $"Backup_{DateTimeOffset.Now.ToUnixTimeMilliseconds()}.json");
                    P.SetConfig(c);
                }
                catch(Exception e)
                {
                    e.LogDuo();
                }
            }
        });
}
