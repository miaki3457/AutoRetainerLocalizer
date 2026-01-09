using AutoRetainer.Modules.Voyage;
using Dalamud.Game;
using ECommons.GameHelpers;
using ECommons.Reflection;

namespace AutoRetainer.UI.MainWindow;
public static unsafe class TroubleshootingUI
{
    private static readonly Config EmptyConfig = new();
    public static void Draw()
    {
        ImGuiEx.TextWrapped("本分頁將檢查您的配置是否有常見問題，您可以在聯絡技術支援前自行解決這些問題。");

        if(!Player.Available)
        {
            ImGuiEx.TextWrapped($"未登入時無法進行故障排除。");
            return;
        }

        if(Data == null)
        {
            ImGuiEx.TextWrapped($"找不到目前角色的資料。請開啟傳喚鈴、探險隊（派遣）面板或重新登入以產生資料。");
            return;
        }

        if(!Svc.ClientState.ClientLanguage.EqualsAny(ClientLanguage.Japanese, ClientLanguage.German, ClientLanguage.French, ClientLanguage.English))
        {
            Error($"偵測到非國際服客戶端。 AutoRetainer未在其它最終幻想14客戶端上進行測試。部分或全部功能可能無法正常運作。此外，請注意，ottercorp 的中國 Dalamud 分支會在未經您同意的情況下收集有關您的電腦、角色、所用插件和 Dalamud 配置的遙測數據，並且您無法選擇退出。");
        }

        if(C.DontLogout)
        {
            Error("已啟用DontLogout調試選項");
        }

        foreach(var x in C.OfflineData)
        {
            if(x.WorkshopEnabled)
            {
                var a = x.OfflineSubmarineData.Select(x => x.Name);
                if(a.Count() > a.Distinct().Count())
                {
                    Error($"角色 {Censor.Character(x.Name, x.World)} 的潛水艇名稱存在重複。潛水艇名稱必須是唯一的。");
                }
            }
        }

        if((C.GlobalTeleportOptions.Enabled || C.OfflineData.Any(x => x.TeleportOptionsOverride.Enabled == true)) && !Svc.PluginInterface.InstalledPlugins.Any(x => x.InternalName == "Lifestream"&& x.IsLoaded))
        {
            Error("已啟用傳送功能但未安裝或未加載 Lifestream 插件。在此配置下 AutoRetainer 無法運作。請停用傳送功能或安裝 Lifestream 插件。");
        }

        foreach(var x in C.SubmarineUnlockPlans)
        {
            if(x.EnforcePlan)
            {
                Info($"潛水艇解鎖計劃 {x.Name.NullWhenEmpty() ?? x.GUID} 設定為強制執行模式，如有需要解鎖的內容，將覆蓋所有潛水艇設定。");
            }
        }

        foreach(var x in C.SubmarineUnlockPlans)
        {
            if(x.EnforceDSSSinglePoint)
            {
                Info($"潛水艇解鎖計劃 {x.Name.NullWhenEmpty() ?? x.GUID} 設定為在深海站點單點部署，並將忽略手動設定的解鎖行為。");
            }
        }

        try
        {
            if(DalamudReflector.IsOnStaging())
            {
                Error($"偵測到非正式版Dalamud分支。這可能導致問題。請透過輸入/xlbranch開啟分支切換器，切換到 \"release\" 分支並重新啟動遊戲");
            }
        }
        catch(Exception e)
        {
        }

        if(Player.Available)
        {
            if(Player.CurrentWorld != Player.HomeWorld)
            {
                Error("您正在訪問其他伺服器。必須返回原始伺服器後，AutoRetainer才能繼續處理此角色。");
            }
            if(C.Blacklist.Any(x => x.CID == Player.CID))
            {
                Error("目前角色已完全排除在AutoRetainer處理之外。請前往設定→排除項進行變更。");
            }
            if(Data.ExcludeRetainer)
            {
                Error("目前角色已被排除在僱員清單外。請前往設定→排除項進行變更。");
            }
            if(Data.ExcludeWorkshop)
            {
                Error("當前角色已被排除在遠航探索清單外。請前往設定→排除項進行變更。");
            }
        }

        {
            var list = C.OfflineData.Where(x => x.GetAreTeleportSettingsOverriden());
            if(list.Any())
            {
                Info("部分角色的傳送選項已自訂。滑鼠懸停查看清單。", list.Select(x => $"{x.Name}@{x.World}").Print("\n"));
            }
        }

        if(C.NoTeleportHetWhenNextToBell)
        {
            Warning("當角色靠近傳喚鈴時，傳送或進入房屋/公寓的功能已被停用。請注意房屋拆除計時器。");
        }



        if(C.AllowSimpleTeleport)
        {
            Warning("已啟用簡單傳送選項。此選項不如在Lifestream中登記房屋可靠。如遇到傳送問題，請考慮停用此選項並在Lifestream中登記您的房屋。");
        }

        if(!C.EnableEntrustManager && C.AdditionalData.Any(x => x.Value.EntrustPlan != Guid.Empty))
        {
            Warning($"託管管理器已全域停用，但部分僱員已指派託管計劃。託管計劃將僅在手動操作時處理。");
        }

        if(C.ExtraDebug)
        {
            Info("已啟用額外日誌記錄選項。這將導致日誌大量輸出，請僅在收集偵錯資訊時使用。");
        }

        if(C.UnsyncCompensation > -5)
        {
            Warning("時間不同步補償值設定過高(>-5)，可能導致問題。");
        }

        if(UIUtils.GetFPSFromMSPT(C.TargetMSPTIdle) < 10)
        {
            Warning("空閒時幀率設定過低(<10)，可能導致問題。");
        }

        if(UIUtils.GetFPSFromMSPT(C.TargetMSPTRunning) < 20)
        {
            Warning("運行時的幀率設定過低(<20)，可能導致問題。");
        }

        if(Data?.GetIMSettings().AllowSellFromArmory == true)
        {
            Info("已啟用允許從裝備兵裝庫出售物品選項。請確保將您的零式裝備和絕境武器加入保護清單。");
        }

        {
            var list = C.OfflineData.Where(x => !x.ExcludeRetainer && !x.Enabled && x.RetainerData.Count > 0);
            if(list.Any())
            {
                Warning($"部分角色未啟用僱員多角色模式，但已登記僱員。滑鼠懸停查看清單。", list.Print("\n"));
            }
        }
        {
            var list = C.OfflineData.Where(x => !x.ExcludeRetainer && x.Enabled && x.RetainerData.Count > 0 && C.SelectedRetainers.TryGetValue(x.CID, out var rd) && !x.RetainerData.All(r => rd.Contains(r.Name)));
            if(list.Any())
            {
                Warning($"部分角色未啟用所有僱員進行處理。滑鼠懸停查看清單。", list.Print("\n"));
            }
        }
        {
            var list = C.OfflineData.Where(x => !x.ExcludeWorkshop && !x.WorkshopEnabled && (x.OfflineSubmarineData.Count + x.OfflineAirshipData.Count) > 0);
            if(list.Any())
            {
                Warning($"部分角色未啟用遠航探索多角色模式，但已登記遠航探索。滑鼠懸停查看清單。", list.Print("\n"));
            }
        }

        {
            var list = C.OfflineData.Where(x => !x.ExcludeWorkshop && x.WorkshopEnabled && x.GetEnabledVesselsData(Internal.VoyageType.Airship).Count + x.GetEnabledVesselsData(Internal.VoyageType.Submersible).Count < Math.Min(x.OfflineAirshipData.Count + x.OfflineSubmarineData.Count, 4));
            if(list.Any())
            {
                Warning($"部分角色未啟用所有遠航探索進行處理。滑鼠懸停查看清單。", list.Print("\n"));
            }
        }

        if(C.MultiModeType != AutoRetainerAPI.Configuration.MultiModeType.Everything)
        {
            Warning($"您的多角色模式類型設定為 {C.MultiModeType} ；這將限制AutoRetainer執行的功能。");
        }

        if(C.OfflineData.Any(x => x.MultiWaitForAllDeployables))
        {
            Info("部分角色已啟用了\"等待所有待處理潛艇\"選項。這代表對於這些角色，AutoRetainer 會等到所有潛艇回歸後才開始處理。將游標懸停在此處可查看啟用了此選項的角色清單。", C.OfflineData.Where(x => x.MultiWaitForAllDeployables).Select(x => $"{x.Name}@{x.World}").Print("\n"));
        }

        if(C.MultiModeWorkshopConfiguration.MultiWaitForAll)
        {
            Info("全局選項\"等待探險完成\"已啟用。這代表對於所有角色，AutoRetainer 都會等到所有僱員回歸後才處理，即使該角色的獨立選項已關閉也是如此。");
        }

        if(C.MultiModeWorkshopConfiguration.WaitForAllLoggedIn)
        {
            Info("潛艇已啟用「即使已登入也等待」選項。這代表即使你已在線上，AutoRetainer 仍會等到該角色的所有潛艇任務完成後才進行處理。");
        }

        if(C.DisableRetainerVesselReturn > 0)
        {
            if(C.DisableRetainerVesselReturn > 10)
            {
                Warning("\"僱員探險處理截止時間\"被設定為異常高值。當僱員即將可用時，你可能會在重新派遣僱員時遇到明顯延遲。");
            }
            else
            {
                Info("\"僱員探險處理截止時間\"已啟用。當僱員即將可用時，你可能會在重新派遣僱員時遇到明顯延遲。");
            }
        }

        if(C.MultiModeRetainerConfiguration.MultiWaitForAll)
        {
            Info("\"等待探險完成\"選項已啟用。這代表 AutoRetainer 會等到該角色的所有僱員探險都完成後，才會登入並處理。");
        }

        if(C.MultiModeRetainerConfiguration.WaitForAllLoggedIn)
        {
            Info("僱員已啟用\"即使已登入也等待\"選項。這代表即使你已在線上，AutoRetainer 仍會等到該角色的所有僱員探險完成後才進行處理。");
        }

        {
            var manualList = new List<string>();
            var deletedList = new List<string>();
            foreach(var x in C.OfflineData)
            {
                foreach(var ret in x.RetainerData)
                {
                    var planId = Utils.GetAdditionalData(x.CID, ret.Name).EntrustPlan;
                    var plan = C.EntrustPlans.FirstOrDefault(s => s.Guid == planId);
                    if(plan != null && plan.ManualPlan) manualList.Add($"{Censor.Character(x.Name)} - {Censor.Retainer(ret.Name)}");
                    if(plan == null && planId != Guid.Empty) deletedList.Add($"{Censor.Character(x.Name)} - {Censor.Retainer(ret.Name)}");
                }
            }
            if(manualList.Count > 0)
            {
                Info("你的一些僱員設定了手動存放計畫。這些計畫在重新派遣僱員後不會自動執行，只能透過點擊覆蓋介面上的按鈕來手動觸發。將游標懸停以查看名單。", manualList.Print("\n"));
            }
            if(deletedList.Count > 0)
            {
                Warning("你的一些僱員存放計畫先前已被刪除。這些僱員將不會存放任何物品。將游標懸停以查看名單。", deletedList.Print("\n"));
            }
        }

        if(C.No2ndInstanceNotify)
        {
            Info("你啟用了\"不針對從相同目錄執行的第二個遊戲實例進行警告\"，這會讓 AutoRetainer 在檢測到使用相同 Dalamud 目錄的第二個遊戲視窗時，自動跳過該視窗的加載。");
        }

        if(Svc.PluginInterface.InstalledPlugins.Any(x => x.InternalName == "SimpleTweaksPlugin"&& x.IsLoaded))
        {
            Info("偵測到 Simple Tweaks 插件。任何與僱員或潛水艇相關的微調都可能對 AutoRetainer 的功能造成負面影響。請確保微調設定不會干擾 AutoRetainer 的運作。");
        }

        if(Svc.PluginInterface.InstalledPlugins.Any(x => x.InternalName == "PandorasBox"&& x.IsLoaded))
        {
            Info("偵測到 Pandora's Box 插件。在 AutoRetainer 啟用時自動執行動作可能會造成負面影響。請確保當 AutoRetainer 處於活動狀態時，Pandora's Box 不會自動執行任何動作。");
        }

        if(Svc.PluginInterface.InstalledPlugins.Any(x => x.InternalName == "Automaton"&& x.IsLoaded))
        {
            Info("偵測到 Automaton 插件。在 AutoRetainer 啟用時自動執行動作或自動輸入數值可能會造成負面影響。請確保在 AutoRetainer 活動期間，Automaton 不會自動執行動作。");
        }

        if(Svc.PluginInterface.InstalledPlugins.Any(x => x.InternalName == "RotationSolver"&& x.IsLoaded))
        {
            Info("偵測到 RotationSolver 插件。在 AutoRetainer 啟用時自動執行技能可能會造成負面影響。請確保在 AutoRetainer 活動期間，RotationSolver 不會自動執行動作。");
        }

        if(Svc.PluginInterface.InstalledPlugins.Any(x => x.InternalName.StartsWith("BossMod") && x.IsLoaded))
        {
            Info("偵測到 BossMod 插件。在 AutoRetainer 啟用時自動執行動作可能會造成負面影響。請確保在 AutoRetainer 活動期間，BossMod 不會自動執行動作。");
        }

        ImGui.Separator();
        ImGuiEx.TextWrapped("專家設定會修改開發者預期的行為。請檢查你的問題是否與錯誤配置的專家設定有關。");
        CheckExpertSetting("無可用派遣任務時存取傳喚鈴的操作", nameof(C.OpenBellBehaviorNoVentures));
        CheckExpertSetting("有可用派遣任務時存取傳喚鈴的操作", nameof(C.OpenBellBehaviorWithVentures));
        CheckExpertSetting("訪問傳喚鈴後任務完成行為", nameof(C.TaskCompletedBehaviorAccess));
        CheckExpertSetting("手動啟用後任務完成行為", nameof(C.TaskCompletedBehaviorManual));
        CheckExpertSetting("如果 5 分鐘內有僱員將完成探險，則停留在僱員選單中", nameof(C.Stay5));
        CheckExpertSetting("關閉僱員列表時自動停用插件", nameof(C.AutoDisable));
        CheckExpertSetting("不顯示插件狀態圖標", nameof(C.HideOverlayIcons));
        CheckExpertSetting("顯示多角色模式類型選擇器", nameof(C.DisplayMMType));
        CheckExpertSetting("在部隊工房中顯示遠航探險", nameof(C.ShowDeployables));
        CheckExpertSetting("啟用應急復原模組", nameof(C.EnableBailout));
        CheckExpertSetting("AutoRetainer嘗試解除卡死前的超時時間(秒)", nameof(C.BailoutTimeout));
        CheckExpertSetting("禁用排序和折疊/展開功能", nameof(C.NoCurrentCharaOnTop));
        CheckExpertSetting("在插件UI欄顯示多角色模式複選框", nameof(C.MultiModeUIBar));
        CheckExpertSetting("僱員選單延遲(秒)", nameof(C.RetainerMenuDelay));
        CheckExpertSetting("不檢查派遣計劃錯誤", nameof(C.NoErrorCheckPlanner2));
        CheckExpertSetting("啟用多角色模式時，嘗試進入附近房屋", nameof(C.MultiHETOnEnable));
        CheckExpertSetting("Artisan 整合功能", nameof(C.ArtisanIntegration));
        CheckExpertSetting("使用伺服器時間而非本地時間", nameof(C.UseServerTime));
    }

    private static void Error(string message, string tooltip = null)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        ImGuiEx.Text(EColor.RedBright, "");
        ImGui.PopFont();
        if(tooltip != null) ImGuiEx.Tooltip(tooltip);
        ImGui.SameLine();
        ImGuiEx.TextWrapped(EColor.RedBright, message);
        if(tooltip != null) ImGuiEx.Tooltip(tooltip);
    }

    private static void Warning(string message, string tooltip = null)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        ImGuiEx.Text(EColor.OrangeBright, "");
        ImGui.PopFont();
        if(tooltip != null) ImGuiEx.Tooltip(tooltip);
        ImGui.SameLine();
        ImGuiEx.TextWrapped(EColor.OrangeBright, message);
        if(tooltip != null) ImGuiEx.Tooltip(tooltip);
    }

    private static void Info(string message, string tooltip = null)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        ImGuiEx.Text(EColor.YellowBright, "");
        ImGui.PopFont();
        if(tooltip != null) ImGuiEx.Tooltip(tooltip);
        ImGui.SameLine();
        ImGuiEx.TextWrapped(EColor.YellowBright, message);
        if(tooltip != null) ImGuiEx.Tooltip(tooltip);
    }

    private static void CheckExpertSetting(string setting, string nameOfSetting)
    {
        var original = EmptyConfig.GetFoP(nameOfSetting);
        var current = C.GetFoP(nameOfSetting);
        if(!original.Equals(current))
        {
            Info($"Expert setting \"{setting}\" differs from default", $"Default is \"{original}\", current is \"{current}\".");
        }
    }
}
