namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeFPSLimiter : NeoUIEntry
{
    public override string Path => "多角色模式/FPS限制器";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("FPS限制器")
        .TextWrapped("FPS限制器僅在多角色模式啟用時啟動")
        .Widget("空閒時的目標幀率", (x) =>
        {
            ImGui.SetNextItemWidth(100f);
            UIUtils.SliderIntFrameTimeAsFPS(x, ref C.TargetMSPTIdle, C.ExtraFPSLockRange ? 1 : 10);
        })
        .Widget("空閒時的目標幀率", (x) =>
        {
            ImGui.SetNextItemWidth(100f);
            UIUtils.SliderIntFrameTimeAsFPS("Target frame rate when operating", ref C.TargetMSPTRunning, C.ExtraFPSLockRange ? 1 : 20);
        })
        .Checkbox("當遊戲處於活動狀態時釋放FPS限制", () => ref C.NoFPSLockWhenActive)
        .Checkbox($"允許額外的低FPS限制值", () => ref C.ExtraFPSLockRange, "如果啟用此選項並在多角色模式下遇到任何錯誤，將不提供支持")
        .Checkbox($"僅在設定關閉計時器時啟動限制器", () => ref C.FpsLockOnlyShutdownTimer);
}
