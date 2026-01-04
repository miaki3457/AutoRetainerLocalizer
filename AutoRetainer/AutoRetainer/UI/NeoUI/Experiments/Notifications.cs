namespace AutoRetainer.UI.NeoUI.Experiments;
public class Notifications : ExperimentUIEntry
{
    public override void Draw()
    {
        ImGui.Checkbox($"當有僱員完成探險時顯示覆蓋層通知", ref C.NotifyEnableOverlay);
        ImGui.Checkbox($"在副本或戰鬥中不顯示覆蓋層", ref C.NotifyCombatDutyNoDisplay);
        ImGui.Checkbox($"包含其他角色", ref C.NotifyIncludeAllChara);
        ImGui.Checkbox($"忽略未在多重模式中啟用的其他角色", ref C.NotifyIgnoreNoMultiMode);
        ImGui.Checkbox($"在遊戲聊天欄顯示通知", ref C.NotifyDisplayInChatX);
        ImGuiEx.Text($"If game is inactive: (requires NotificationMaster to be installed and enabled)");
        ImGui.Checkbox($"當僱員可用時發送桌面通知", ref C.NotifyDeskopToast);
        ImGui.Checkbox($"閃爍工作列", ref C.NotifyFlashTaskbar);
        ImGui.Checkbox($"若 AutoRetainer 已啟用或多重模式運行中則不通知", ref C.NotifyNoToastWhenRunning);
    }
}
