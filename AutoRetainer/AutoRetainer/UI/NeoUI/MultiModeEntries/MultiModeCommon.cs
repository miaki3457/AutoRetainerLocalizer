namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeCommon : NeoUIEntry
{
    public override string Path => "多角色模式/通用設定";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("通用設定")
        .Checkbox($"在登入介面等待", () => ref C.MultiWaitOnLoginScreen, "如果沒有角色可進行探險任務，你將保持登出狀態直到有角色可用。啟用此選項和多角色模式時，標題畫面動畫將會停用。")
        .Checkbox($"手動登入時停用多角色模式", () => ref C.MultiDisableOnRelog, "使用 AutoRetainer 介面或指令重新登入後，多角色模式將被關閉。")
        .Checkbox($"手動登入時不重置首選角色", () => ref C.MultiNoPreferredReset, "使用 AutoRetainer 介面或指令重新登入後，首選角色保持不變。")
        .Checkbox("允許進入共享房屋", () => ref C.SharedHET)
        .Checkbox("即使多角色模式停用也嘗試在登入時進入房屋", () => ref C.HETWhenDisabled)
        .Checkbox("當已在傳喚鈴旁時禁止傳送或進入房屋", () => ref C.NoTeleportHetWhenNextToBell)

        .Section("遊戲啟動")
        .Checkbox($"遊戲啟動時啟用多角色模式", () => ref C.MultiAutoStart)
        .Checkbox($"外掛啟動時啟用多角色模式", () => ref C.MultiOnPluginLoad)
        .Indent()
        .SliderInt(150f, "Delay, seconds", () => ref C.MultiModeOnPluginLoadDelay, 0, 20)
        .Unindent()
        .Widget("遊戲啟動時自動登入", (x) =>
        {
            ImGui.SetNextItemWidth(150f);
            var names = C.OfflineData.Where(s => !s.Name.IsNullOrEmpty()).Select(s => $"{s.Name}@{s.World}");
            var dict = names.ToDictionary(s => s, s => Censor.Character(s));
            dict.Add("", "Disabled");
            dict.Add("~", "Last logged in character");
            ImGuiEx.Combo(x, ref C.AutoLogin, ["", "~", .. names], names: dict);
        })
        .SliderInt(150f, "Delay", () => ref C.AutoLoginDelay.ValidateRange(0, 60), 0, 20, "Set appropriate delay to let plugins fully load before logging in and to allow yourself some time to cancel login if needed")

        .Section("背包空間警告")
        .InputInt(100f, $"僱員清單：剩餘背包空格警告", () => ref C.UIWarningRetSlotNum.ValidateRange(2, 1000))
        .InputInt(100f, $"僱員清單：剩餘探險幣警告", () => ref C.UIWarningRetVentureNum.ValidateRange(2, 1000))
        .InputInt(100f, $"潛艇清單：剩餘背包空格警告", () => ref C.UIWarningDepSlotNum.ValidateRange(2, 1000))
        .InputInt(100f, $"潛艇清單：剩餘青磷水警告", () => ref C.UIWarningDepTanksNum.ValidateRange(20, 1000))
        .InputInt(100f, $"潛艇清單：剩餘修理材料警告", () => ref C.UIWarningDepRepairNum.ValidateRange(5, 1000))

        .Section("傳送設定")
        .Widget(() => ImGuiEx.Text("需要安裝 Lifestream 插件"))
        .Widget(() => ImGuiEx.PluginAvailabilityIndicator([new("Lifestream", new Version("2.2.1.1"))]))
        .TextWrapped("你必須在 Lifestream 插件中為每個角色註冊房屋，此選項才會生效，或者啟用簡易傳送")
        .TextWrapped("你可以在角色配置選單中為每個角色自定義這些設定。")
        .Widget(() =>
        {
            if(Data != null && Data.GetAreTeleportSettingsOverriden())
            {
                ImGuiEx.TextWrapped(ImGuiColors.DalamudRed, "目前角色的傳送選項已自定義。");
            }
        })
        .Checkbox("啟用", () => ref C.GlobalTeleportOptions.Enabled)
        .Indent()
        .Checkbox("為傳喚鈴傳送...", () => ref C.GlobalTeleportOptions.Retainers)
        .Indent()
        .Checkbox("...到私人房屋", () => ref C.GlobalTeleportOptions.RetainersPrivate)
        .Checkbox("...到共享房屋", () => ref C.GlobalTeleportOptions.RetainersShared)
        .Checkbox("...到部隊房屋", () => ref C.GlobalTeleportOptions.RetainersFC)
        .Checkbox("...到公寓", () => ref C.GlobalTeleportOptions.RetainersApartment)
        .TextWrapped("如果以上所有選項都停用或失敗，將會傳送到旅館")
        .Unindent()
        .Checkbox("為潛水艇/飛艇傳送至部隊房屋", () => ref C.GlobalTeleportOptions.Deployables)
        .Checkbox("啟用簡易傳送", () => ref C.AllowSimpleTeleport)
        .Unindent()
        .Widget(() => ImGuiEx.HelpMarker("允許在未向Lifestream註冊房屋的情況下傳送。傳送功能仍需安裝Lifestream外掛才能運作。\n\n警告：此選項比在Lifestream中註冊房屋更不可靠。請僅在必要時使用。", EColor.RedBright, FontAwesomeIcon.ExclamationTriangle.ToIconString()))

        .Section("緊急逃生模組")
        .Checkbox("發生連線錯誤時自動關閉並重試登入", () => ref C.ResolveConnectionErrors, "斷線時 AutoRetainer 將嘗試重新登入。若會話（Session）已過期，則不會嘗試登入。")
        .Widget(() => ImGuiEx.PluginAvailabilityIndicator([new("NoKillPlugin")]));
}
