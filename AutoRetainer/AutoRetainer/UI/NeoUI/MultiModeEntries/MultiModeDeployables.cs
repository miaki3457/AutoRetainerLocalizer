using ECommons.Throttlers;

namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeDeployables : NeoUIEntry
{
    public override string Path => "多角色模式/遠航探索";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("Multi Mode - Deployables")
        .Checkbox("等待航程完成", () => ref C.MultiModeWorkshopConfiguration.MultiWaitForAll, "When enabled, AutoRetainer will wait for all deployables to return before logging into the character. If you're already logged in for another reason, it will still resend completed submarines—unless the global setting \"Wait even when already logged in\" is also turned on.")
        .Indent()
        .Checkbox("即使已登錄也等待", () => ref C.MultiModeWorkshopConfiguration.WaitForAllLoggedIn, "Changes the behavior of \"Wait for Voyage Completion\" (both global and per-character) so that AutoRetainer no longer resends individual submarines while already logged in. Instead, it will wait until all submarines have returned before taking action.")
        .InputInt(120f, "最大等待時間（分鐘）", () => ref C.MultiModeWorkshopConfiguration.MaxMinutesOfWaiting.ValidateRange(0, 9999), 10, 60, "If waiting for other deployables to return would exceed this number of minutes, AutoRetainer will ignore both the \"Wait for Voyage Completion\" and \"Wait even when already logged in\" settings.")
        .Unindent()
        .DragInt(60f, "Advance Relog Threshold, seconds", () => ref C.MultiModeWorkshopConfiguration.AdvanceTimer.ValidateRange(0, 300), 0.1f, 0, 300, "The number of seconds AutoRetainer should log in early before submarines on this character are ready to be resent.")
        .DragInt(120f, "Retainer venture processing cutoff, minutes", () => ref C.DisableRetainerVesselReturn.ValidateRange(0, 60), "If set to a value greater than 0, AutoRetainer will stop processing any retainers this number of minutes before any character is scheduled to redeploy submarines, taking all previous settings into account.")
        .Checkbox("Sell items from Unconditional sell list right after deployment (requires retainers)", () => ref C.VendorItemAfterVoyage)
        .Checkbox("Periodically check FC chest for gil upon entering workshop", () => ref C.FCChestGilCheck, "Periodically checks the Free Company chest when entering the Workshop to keep the gil counter up to date.")
        .Indent()
        .SliderInt(150f, "Check frequency, hours", () => ref C.FCChestGilCheckCd, 0, 24 * 5)
        .Widget("Reset cooldowns", (x) =>
        {
            if(ImGuiEx.Button(x, C.FCChestGilCheckTimes.Count > 0)) C.FCChestGilCheckTimes.Clear();
        })
        .Unindent()
        .Checkbox("處理完所有遠航探索後關閉遊戲", () => ref C.ShutdownOnSubExhaustion)
        .Indent()
        .SliderFloat(150f, "Don't shutdown if there are deployables that return within this amount of hours", () => ref C.HoursForShutdown, 0f, 10f)
        .Widget(() =>
        {
            ImGuiEx.HelpMarker($"""
            Currently: {(Utils.CanShutdownForSubs() ? "Can shutdown" : "Can NOT shutdown")}
            Remaining for force shutdown: {EzThrottler.GetRemainingTime("ForceShutdownForSubs")}
                """);
        })
        .Unindent()
        .TextWrapped("進入工房後自動購買青磷水：")
        .Indent()
        .Widget(() =>
        {
            if(Data != null)
            {
                ImGui.Checkbox($"在 {Data.NameWithWorldCensored} 上啟用", ref Data.AutoFuelPurchase);
            }
            ImGuiEx.TextWrapped($"若要啟用/停用其他角色的燃料購買，請前往「功能、排除與排序」區塊。");
        })
        .InputInt(150f, "觸發購買的剩餘青磷水數量", () => ref C.AutoFuelPurchaseLow.ValidateRange(100, 99999))
        .InputInt(150f, "購買至背包內達到此數量", () => ref C.AutoFuelPurchaseMax)
        .Checkbox("Only buy when workstation is unlocked", () => ref C.AutoFuelPurchaseOnlyWsUnlocked)
        .Unindent()
        ;
}
