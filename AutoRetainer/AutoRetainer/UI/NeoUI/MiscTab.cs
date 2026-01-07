namespace AutoRetainer.UI.NeoUI;
public class MiscTab : NeoUIEntry
{
    public override string Path => "雜項";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("統計信息")
        .Checkbox($"記錄僱員探險統計", () => ref C.RecordStats)

        .Section("自動籌備稀有品")
        .Checkbox("籌備稀有品完成時發送托盤通知（需要NotificationMaster插件）", () => ref C.GCHandinNotify)

        .Section("效能")

        .If(() => Utils.IsBusy)
        .Widget("", (x) => ImGui.BeginDisabled())
        .EndIf()

        .Checkbox($"插件運轉時解除最小化時的FPS限制", () => ref C.UnlockFPS)
        .Checkbox($"- 同時解除常規FPS限制", () => ref C.UnlockFPSUnlimited)
        .Checkbox($"- 同時暫停ChillFrames插件", () => ref C.UnlockFPSChillFrames)
        .Checkbox($"插件運行時提高FFXIV進程優先權", () => ref C.ManipulatePriority, "可能導致其他程式變慢")

        .If(() => Utils.IsBusy)
        .Widget("", (x) => ImGui.EndDisabled())
        .EndIf();
}
