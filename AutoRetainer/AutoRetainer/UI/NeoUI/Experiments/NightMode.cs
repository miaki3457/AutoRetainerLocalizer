namespace AutoRetainer.UI.NeoUI.Experiments;

internal class NightMode : ExperimentUIEntry
{
    public override string Name => "夜間模式";
    public override void Draw()
    {
        ImGuiEx.TextWrapped($"Night mode:
" +
                $"- Wait on login screen option is forcefully enabled
" +
                $"- Built-in FPS limiter restrictions forcefully applied
" +
                $"- While unfocused and awaiting, game is limited to 0.2 FPS
" +
                $"- It may look like game hung up, but let it up to 5 seconds to wake up after you reactivate game window.
" +
                $"- By default, only Deployables are enabled in Night mode
" +
                $"- After disabling Night mode, Bailout manager will activate to relog you back to the game.");
        if(ImGui.Checkbox("啟用夜間模式", ref C.NightMode)) MultiMode.BailoutNightMode();
        ImGui.Checkbox("顯示夜間模式勾選框", ref C.ShowNightMode);
        ImGui.Checkbox("在夜間模式下處理僱員", ref C.NightModeRetainers);
        ImGui.Checkbox("在夜間模式下處理派遣", ref C.NightModeDeployables);
        ImGui.Checkbox("使夜間模式狀態持久化", ref C.NightModePersistent);
        ImGui.Checkbox("使關機指令改為啟動夜間模式而非關閉遊戲", ref C.ShutdownMakesNightMode);
    }
}
