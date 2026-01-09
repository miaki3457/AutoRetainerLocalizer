using ECommons.Throttlers;

namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeDeployables : NeoUIEntry
{
    public override string Path => "多角色模式/遠航探索";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("多角色模式 - 潛艇/飛空艇")
        .Checkbox("等待航程完成", () => ref C.MultiModeWorkshopConfiguration.MultiWaitForAll, "啟用時，AutoRetainer 會等到所有探險潛艇回歸後才登入該角色。若你因其他原因已在線上，它仍會重新派遣已完成的潛艇——除非\"即使已登入也等待\"的全局設定也被開啟。")
        .Indent()
        .Checkbox("即使已登錄也等待", () => ref C.MultiModeWorkshopConfiguration.WaitForAllLoggedIn, "更改\"等待航行完成\"的行為（包括全局與單一角色設定），使 AutoRetainer 在已登入時不再單獨派遣個別回歸的潛艇，而是等到\"全部\"潛艇都回歸後才一併處理。")
        .InputInt(120f, "最大等待時間（分鐘）", () => ref C.MultiModeWorkshopConfiguration.MaxMinutesOfWaiting.ValidateRange(0, 9999), 10, 60, "如果等待其餘潛艇回歸的時間超過此分鐘數，AutoRetainer 將忽略\"等待航行完成\"與\"即使已登入也等待\"的設定。")
        .Unindent()
        .DragInt(60f, "Advance Relog Threshold, seconds", () => ref C.MultiModeWorkshopConfiguration.AdvanceTimer.ValidateRange(0, 300), 0.1f, 0, 300, "The number of seconds AutoRetainer should log in early before submarines on this character are ready to be resent.")
        .DragInt(120f, "Retainer venture processing cutoff, minutes", () => ref C.DisableRetainerVesselReturn.ValidateRange(0, 60), "If set to a value greater than 0, AutoRetainer will stop processing any retainers this number of minutes before any character is scheduled to redeploy submarines, taking all previous settings into account.")
        .Checkbox("派遣後立即出售\"無條件出售清單\"中的物品（需要僱員）", () => ref C.VendorItemAfterVoyage)
        .Checkbox("進入部隊工作坊時，定期檢查部隊箱中的金幣", () => ref C.FCChestGilCheck, "在進入工作坊時定期檢查部隊箱，以保持金幣計數為最新狀態。")
        .Indent()
        .SliderInt(150f, "Check frequency, hours", () => ref C.FCChestGilCheckCd, 0, 24 * 5)
        .Widget("重設冷卻時間", (x) =>
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
                當前狀態：{(Utils.CanShutdownForSubs() ? "Can shutdown" : "Can NOT shutdown")}\n距離強制關機剩餘：{EzThrottler.GetRemainingTime("ForceShutdownForSubs")}
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
        .Checkbox("僅在工作站解鎖時進行購買", () => ref C.AutoFuelPurchaseOnlyWsUnlocked)
        .Unindent()
        ;
}
