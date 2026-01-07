namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeRetainers : NeoUIEntry
{
    public override string Path => "多角色模式/僱員設定";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("多角色模式 - 僱員設定")
        .Checkbox("等待探險完成", () => ref C.MultiModeRetainerConfiguration.MultiWaitForAll, "在多角色模式下，AutoRetainer 會等到所有僱員都回歸後才切換至下一個角色。")
        .DragInt(60f, "Advance Relog Threshold", () => ref C.MultiModeRetainerConfiguration.AdvanceTimer.ValidateRange(0, 300), 0.1f, 0, 300)
        .SliderInt(100f, "Minimum inventory slots to continue operation", () => ref C.MultiMinInventorySlots.ValidateRange(2, 9999), 2, 30)
        .Checkbox("同步僱員狀態（一次性）", () => ref MultiMode.Synchronize, "AutoRetainer 會等待直到所有啟用的僱員都完成探險。之後此設定將自動停用，並開始處理所有角色。")
        .Checkbox($"強制執行完整角色輪換", () => ref C.CharEqualize, "推薦給擁有超過 15 個角色的用戶。強制多角色模式按順序處理所有角色的探險，然後才回到循環起點。")
        .Indent()
        .Checkbox("依探險完成時間排序角色", () => ref C.LongestVentureFirst, "優先檢查那些很久以前就已完成探險的角色")
        .Checkbox("依僱員等級與上限排序角色", () => ref C.CappedLevelsLast, "優先處理有僱員可升級的角色；其次是僱員滿級的角色；最後是僱員未滿級且達到當前等級上限的角色。")
        .Unindent();
}
