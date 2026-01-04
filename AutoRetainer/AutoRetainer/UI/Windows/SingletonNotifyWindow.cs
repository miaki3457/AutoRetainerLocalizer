namespace AutoRetainer.UI.Windows;
public class SingletonNotifyWindow : NotifyWindow
{
    private bool IAmIdiot = false;
    private WindowSystem ws;
    public SingletonNotifyWindow() : base("AutoRetainer - warning!")
    {
        IsOpen = true;
        ws = new();
        Svc.PluginInterface.UiBuilder.Draw += ws.Draw;
        ws.AddWindow(this);
    }

    public override void OnClose()
    {
        Svc.PluginInterface.UiBuilder.Draw -= ws.Draw;
    }

    public override void DrawContent()
    {
        ImGuiEx.Text($"AutoRetainer has detected that another instance of the plugin is running \nwith the same data path configuration.");
        ImGuiEx.Text($"為防止資料遺失，插件載入已中止。");
        if(ImGui.Button("關閉此視窗且不載入 AutoRetainer"))
        {
            IsOpen = false;
        }
        if(ImGui.Button("了解如何正確運行2個或更多遊戲"))
        {
            ShellStart("https://github.com/PunishXIV/AutoRetainer/issues/62");
        }
        ImGui.Separator();
        ImGui.Checkbox($"勾選代表您同意可能會遺失所有 AutoRetainer 資料", ref IAmIdiot);
        if(!IAmIdiot) ImGui.BeginDisabled();
        if(ImGui.Button("載入 AutoRetainer"))
        {
            IsOpen = false;
            new TickScheduler(P.Load);
        }
        if(!IAmIdiot) ImGui.EndDisabled();
    }
}
