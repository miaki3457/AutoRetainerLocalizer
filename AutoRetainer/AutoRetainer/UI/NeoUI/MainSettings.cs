namespace AutoRetainer.UI.NeoUI;
public class MainSettings : NeoUIEntry
{
    public override string Path => "一般";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("延遲設定")
        .Widget(100f, "時間不同步補償", (x) => ImGuiEx.SliderInt(x, ref C.UnsyncCompensation.ValidateRange(-60, 0), -10, 0), "從探險結束時間額外扣除的秒數。這有助於減緩遊戲伺服器與你電腦之間時間不同步所產生的問題。")
        .Widget(100f, "額外交互延遲（影格）", (x) => ImGuiEx.SliderInt(x, ref C.ExtraFrameDelay.ValidateRange(-10, 100), 0, 50), "此數值越低，插件執行動作的速度越快。當幀率（FPS）較低或延遲較高時，建議增加此值；若希望插件運行更快，可以降低此值。")
        .Widget("額外日誌", (x) => ImGui.Checkbox(x, ref C.ExtraDebug), "此選項會啟用用於除錯的冗長日誌。開啟時會產生大量日誌並影響效能。此選項會在插件重載或遊戲重啟時自動關閉。")

            .Section("操作模式")
        .Widget("指派 + 重新指派", (x) =>
        {
            if(ImGui.RadioButton(x, C.EnableAssigningQuickExploration && !C._dontReassign))
            {
                C.EnableAssigningQuickExploration = true;
                C.DontReassign = false;
            }
        }, "若僱員當前沒有任務，將自動分派\"自由探索\"，並在完成後自動重新派遣相同的任務。")
        .Widget("領取回報", (x) =>
        {
            if(ImGui.RadioButton(x, !C.EnableAssigningQuickExploration && C._dontReassign))
            {
                C.EnableAssigningQuickExploration = false;
                C.DontReassign = true;
            }
        }, "僅領取僱員的探險獎勵，不會重新派遣。與僱員鈴互動時按住 CTRL 可暫時套用此模式。")
        .Widget("重新指派", (x) =>
        {
            if(ImGui.RadioButton("重新指派", !C.EnableAssigningQuickExploration && !C._dontReassign))
            {
                C.EnableAssigningQuickExploration = false;
                C.DontReassign = false;
            }
        }, "僅重新派遣僱員目前正在進行的相同任務")
        .Widget("僱員感官", (x) => ImGui.Checkbox(x, ref C.RetainerSense), "當玩家進入僱員鈴的互動範圍內時，AutoRetainer 將自動啟用。期間你必須保持靜止，否則會取消啟用。")
        .Widget(200f, "啟動時間", (x) => ImGuiEx.SliderIntAsFloat(x, ref C.RetainerSenseThreshold, 1000, 100000));


}
