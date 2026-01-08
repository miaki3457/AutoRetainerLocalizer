using AutoRetainer.Modules.Voyage;
using AutoRetainer.UI.MainWindow.MultiModeTab;
using AutoRetainerAPI;
using AutoRetainerAPI.Configuration;
using Dalamud.Interface.Components;
using ECommons.Configuration;
using ECommons.Funding;
using NightmareUI;

namespace AutoRetainer.UI.MainWindow;

internal unsafe class AutoRetainerWindow : Window
{
    private TitleBarButton LockButton;

    public AutoRetainerWindow() : base($"")
    {
        PatreonBanner.IsOfficialPlugin = () => true;
        LockButton = new()
        {
            Click = OnLockButtonClick,
            Icon = C.PinWindow ? FontAwesomeIcon.Lock : FontAwesomeIcon.LockOpen,
            IconOffset = new(3, 2),
            ShowTooltip = () => ImGui.SetTooltip("鎖定視窗位置和大小"),
        };
        SizeConstraints = new()
        {
            MinimumSize = new(250, 100),
            MaximumSize = new(9999, 9999)
        };
        P.WindowSystem.AddWindow(this);
        AllowPinning = false;
        TitleBarButtons.Add(new()
        {
            Click = (m) => { if(m == ImGuiMouseButton.Left) S.NeoWindow.IsOpen = true; },
            Icon = FontAwesomeIcon.Cog,
            IconOffset = new(2, 2),
            ShowTooltip = () => ImGui.SetTooltip("開啟設定視窗"),
        });
        TitleBarButtons.Add(LockButton);
    }

    private Action<string> SomeAction;

    private void OnLockButtonClick(ImGuiMouseButton m)
    {
        SomeAction += (s) => { };
        SomeAction -= (s) => { };
        if(m == ImGuiMouseButton.Left)
        {
            C.PinWindow = !C.PinWindow;
            LockButton.Icon = C.PinWindow ? FontAwesomeIcon.Lock : FontAwesomeIcon.LockOpen;
        }
    }

    public override void PreDraw()
    {
        var prefix = SchedulerMain.PluginEnabled ? $" [{SchedulerMain.Reason}]" : "";
        var tokenRem = TimeSpan.FromMilliseconds(Utils.GetRemainingSessionMiliSeconds());
        WindowName = $"{P.Name} {P.GetType().Assembly.GetName().Version}{prefix} | {FormatToken(tokenRem)}###AutoRetainer";
        if(C.PinWindow)
        {
            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(C.WindowPos);
            ImGui.SetNextWindowSize(C.WindowSize);
        }
    }

    private string FormatToken(TimeSpan time)
    {
        if(time.TotalMilliseconds > 0)
        {
            if(time.Days > 0)
            {
                return $"會話將在 {time.Days} 天{(time.Days == 1 ? "" : "s")}" + (time.Hours > 0 ? $" {time.Hours} 小時過期" : "");
            }
            else
            {
                if(time.Hours > 0)
                {
                    return $"會話將在 {time.Hours} 小時後過期";
                }
                else
                {
                    return $"會話將在一個小時內過期";
                }
            }
        }
        else
        {
            return "會話已過期";
        }
    }
    public override void Draw()
    {
        //ImGuiEx.Text(GradientColor.Get(EColor.RedBright, EColor.YellowBright), "This version MUST NOT BE RUNNING UNATTENDED.");
        try
        {
            if(!C.AcceptedDisclamer)
            {
                new NuiBuilder()
                    .Section("免責聲明")
                    .TextWrapped(ImGuiColors.DalamudYellow, "請注意，嚴禁將 AutoRetainer 用於 RMT 目的")
                    .TextWrapped(ImGuiColors.DalamudRed, "為避免不必要的後果，使用AutoRetainer時請遵守下列規則 :")
                    .TextWrapped("1. 不要在遊戲聊天中提及您使用AutoRetainer;")
                    .TextWrapped("2. 請勿長時間在無人看管的情況下執行 AutoRetainer；")
                    .TextWrapped("3. 確保您的實際遊戲+AutoRetainer使用時間每天不超過16小時；確保在僱員/潛水艇檢查流程之間存在非活動間隔;")
                    .TextWrapped("4. 永遠不要回應那些試圖透過交易或聊天進行所謂'機器人檢測'的玩家；應立即將這些玩家加入黑名單;")
                    .TextWrapped("5. 如果GM詢問，始終聲稱所有操作都是手動完成的，絕不承認使用插件。")
                    .TextWrapped("違反這些規則可能導致您的帳號受到處罰。")
                    .TextWrapped(GradientColor.Get(ImGuiColors.DalamudYellow, ImGuiColors.DalamudRed), "您不得將 AutoRetainer 用於RMT行為或其他商業用途。如果您將其用於上述用途，我們將不提供任何支援。")
                    .Widget(() =>
                    {
                        if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Check, "接受並繼續"))
                        {
                            C.AcceptedDisclamer = true;
                            EzConfig.Save();
                        }
                    })
                    .Draw();
                return;
            }
            var e = SchedulerMain.PluginEnabledInternal;
            var disabled = MultiMode.Active && !ImGui.GetIO().KeyCtrl;

            if(disabled)
            {
                ImGui.BeginDisabled();
            }
            if(ImGui.Checkbox($"啟用 {P.Name}", ref e))
            {
                P.WasEnabled = false;
                if(e)
                {
                    SchedulerMain.EnablePlugin(PluginEnableReason.Auto);
                }
                else
                {
                    SchedulerMain.DisablePlugin();
                }
            }
            if(C.ShowDeployables && (VoyageUtils.Workshops.Contains(Svc.ClientState.TerritoryType) || VoyageScheduler.Enabled))
            {
                ImGui.SameLine();
                ImGui.Checkbox($"遠航探索", ref VoyageScheduler.Enabled);
            }
            if(disabled)
            {
                ImGui.EndDisabled();
                ImGuiComponents.HelpMarker($"多角色模式正控制此選項。按住 CTRL 可強制覆蓋。");
            }

            if(P.WasEnabled)
            {
                ImGui.SameLine();
                ImGuiEx.Text(GradientColor.Get(ImGuiColors.DalamudGrey, ImGuiColors.DalamudGrey3, 500), $"已暫停");
            }

            ImGui.SameLine();
            if(ImGui.Checkbox("多角色", ref MultiMode.Enabled))
            {
                MultiMode.OnMultiModeEnabled();
            }
            Utils.DrawLifestreamAvailabilityIndicator();
            if(C.ShowNightMode)
            {
                ImGui.SameLine();
                if(ImGui.Checkbox("夜間", ref C.NightMode))
                {
                    MultiMode.BailoutNightMode();
                }
            }
            if(C.DisplayMMType)
            {
                ImGui.SameLine();
                ImGuiEx.SetNextItemWidthScaled(100f);
                ImGuiEx.EnumCombo("##mode", ref C.MultiModeType);
            }
            if(C.CharEqualize && MultiMode.Enabled)
            {
                ImGui.SameLine();
                if(ImGui.Button("重設計數器"))
                {
                    MultiMode.CharaCnt.Clear();
                }
            }

            Svc.PluginInterface.GetIpcProvider<object>(ApiConsts.OnMainControlsDraw).SendMessage();

            if(IPC.Suppressed)
            {
                ImGuiEx.Text(ImGuiColors.DalamudRed, $"插件操作正被其他插件抑制中");
                ImGui.SameLine();
                if(ImGui.SmallButton("取消"))
                {
                    IPC.Suppressed = false;
                }
            }

            if(P.TaskManager.IsBusy)
            {
                ImGui.SameLine();
                if(ImGui.Button($"中止 {P.TaskManager.NumQueuedTasks} 個流程"))
                {
                    P.TaskManager.Abort();
                }
            }

            PatreonBanner.DrawRight();
            ImGuiEx.EzTabBar("tabbar", PatreonBanner.Text,
                            ("僱員管理", MultiModeUI.Draw, null, true),
                            ("遠航探索", WorkshopUI.Draw, null, true),
                            ("故障排除", TroubleshootingUI.Draw, null, true),
                            ("統計信息", DrawStats, null, true),
                            ("關於", CustomAboutTab.Draw, null, true)
                            );
            if(!C.PinWindow)
            {
                C.WindowPos = ImGui.GetWindowPos();
                C.WindowSize = ImGui.GetWindowSize();
            }
        }
        catch(Exception e)
        {
            ImGuiEx.TextWrapped(e.ToStringFull());
        }
    }

    private void DrawStats()
    {
        NuiTools.ButtonTabs([[C.RecordStats ? new("僱員派遣", S.VentureStats.DrawVentures) : null, new("Gil", S.GilDisplay.Draw), new("部隊信息", S.FCData.Draw)]]);
    }

    public override void OnClose()
    {
        EzConfig.Save();
        S.VentureStats.Data.Clear();
        MultiModeUI.JustRelogged = false;
    }

    public override void OnOpen()
    {
        MultiModeUI.JustRelogged = true;
    }
}
