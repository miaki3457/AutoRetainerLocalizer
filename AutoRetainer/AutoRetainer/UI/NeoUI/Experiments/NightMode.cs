namespace AutoRetainer.UI.NeoUI.Experiments;

internal class NightMode : ExperimentUIEntry
{
    public override string Name => "夜間模式";
    public override void Draw()
    {
        ImGuiEx.TextWrapped($"夜間模式:\n" +
                $"- 在登入畫面等待選項將被強制啟用\n" +
                $"- 將強制執行內建的 FPS 限制器規範\n" +
                $"- 當視窗未聚焦且在等待時，遊戲將限制在 0.2 FPS\n" +
                $"- 遊戲看起來可能會像當機，但在你重新激活遊戲視窗後，請給它最多 5 秒的時間恢復運作。\n" +
                $"- 預設情況下，夜間模式僅啟用潛艇自動化\n" +
                $"- 停用夜間模式後，救援管理器 (Bailout manager) 會啟動並帶領你重新登入遊戲。");
        if(ImGui.Checkbox("啟用夜間模式", ref C.NightMode)) MultiMode.BailoutNightMode();
        ImGui.Checkbox("顯示夜間模式勾選框", ref C.ShowNightMode);
        ImGui.Checkbox("在夜間模式下處理僱員", ref C.NightModeRetainers);
        ImGui.Checkbox("在夜間模式下處理派遣", ref C.NightModeDeployables);
        ImGui.Checkbox("使夜間模式狀態持久化", ref C.NightModePersistent);
        ImGui.Checkbox("使關機指令改為啟動夜間模式而非關閉遊戲", ref C.ShutdownMakesNightMode);
    }
}
