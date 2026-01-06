namespace AutoRetainer.UI.NeoUI;
public class MainSettings : NeoUIEntry
{
    public override string Path => "General";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("Delays")
        .Widget(100f, "時間不同步補償", (x) => ImGuiEx.SliderInt(x, ref C.UnsyncCompensation.ValidateRange(-60, 0), -10, 0), "Additional amount of seconds that will be subtracted from venture ending time to help mitigate possible issues of time desynchronization between the game and your PC.")
        .Widget(100f, "額外交互延遲（影格）", (x) => ImGuiEx.SliderInt(x, ref C.ExtraFrameDelay.ValidateRange(-10, 100), 0, 50), "The lower this value is the faster plugin will use actions. When dealing with low FPS or high latency you may want to increase this value. If you want the plugin to operate faster you may decrease it.")
        .Widget("額外日誌", (x) => ImGui.Checkbox(x, ref C.ExtraDebug), "This option enables excessive logging for debugging purposes. It will spam your log and cause performance issues while enabled. This option will disable itself upon plugin reload or game restart.")

            .Section("Operation")
        .Widget("指派 + 重新指派", (x) =>
        {
            if(ImGui.RadioButton(x, C.EnableAssigningQuickExploration && !C._dontReassign))
            {
                C.EnableAssigningQuickExploration = true;
                C.DontReassign = false;
            }
        }, "Automatically assigns enabled retainers to a Quick Venture if they have none already in progress and reassigns current venture.")
        .Widget("領取回報", (x) =>
        {
            if(ImGui.RadioButton(x, !C.EnableAssigningQuickExploration && C._dontReassign))
            {
                C.EnableAssigningQuickExploration = false;
                C.DontReassign = true;
            }
        }, "Only collect venture rewards from the retainer, and will not reassign them.\nHold CTRL when interacting with the Summoning Bell to apply this mode temporarily.")
        .Widget("重新指派", (x) =>
        {
            if(ImGui.RadioButton("重新指派", !C.EnableAssigningQuickExploration && !C._dontReassign))
            {
                C.EnableAssigningQuickExploration = false;
                C.DontReassign = false;
            }
        }, "Only reassign ventures that retainers are undertaking.")
        .Widget("雇員感官", (x) => ImGui.Checkbox(x, ref C.RetainerSense), "AutoRetainer will automatically enable itself when the player is within interaction range of a Summoning Bell. You must remain stationary or the activation will be cancelled.")
        .Widget(200f, "Activation Time", (x) => ImGuiEx.SliderIntAsFloat(x, ref C.RetainerSenseThreshold, 1000, 100000));


}
