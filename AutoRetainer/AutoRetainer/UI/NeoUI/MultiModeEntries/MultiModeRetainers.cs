namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeRetainers : NeoUIEntry
{
    public override string Path => "多角色模式/僱員設定";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("多角色模式 - 僱員設定")
        .Checkbox("等待探險完成", () => ref C.MultiModeRetainerConfiguration.MultiWaitForAll, "AutoRetainer will wait for all retainers to return before cycling to the next character in multi mode operation.")
        .DragInt(60f, "Advance Relog Threshold", () => ref C.MultiModeRetainerConfiguration.AdvanceTimer.ValidateRange(0, 300), 0.1f, 0, 300)
        .SliderInt(100f, "Minimum inventory slots to continue operation", () => ref C.MultiMinInventorySlots.ValidateRange(2, 9999), 2, 30)
        .Checkbox("同步僱員狀態（一次性）", () => ref MultiMode.Synchronize, "AutoRetainer will wait until all enabled retainers have completed their ventures. After that this setting will be disabled automatically and all characters will be processed.")
        .Checkbox($"強制執行完整角色輪換", () => ref C.CharEqualize, "Recommended for users with > 15 characters, forces multi mode to make sure ventures are processed on all characters in order before returning to the beginning of the cycle.")
        .Indent()
        .Checkbox("依探險完成時間排序角色", () => ref C.LongestVentureFirst, "Characters that have completed ventures longer time ago will be checked first")
        .Checkbox("依僱員等級與上限排序角色", () => ref C.CappedLevelsLast, "Characters with retainers that can be levelled up will be done first; then, characters with retainers at max level; and then characters with retainers less than max level and level capped.")
        .Unindent();
}
