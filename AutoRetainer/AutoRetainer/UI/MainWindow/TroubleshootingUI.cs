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

        if((C.GlobalTeleportOptions.Enabled || C.OfflineData.Any(x => x.TeleportOptionsOverride.Enabled == true)) && !Svc.PluginInterface.InstalledPlugins.Any(x => x.InternalName == "Lifestream" && x.IsLoaded))
        {
            Error("\"Teleportation is enabled but Lifestream plugin is not installed/loaded. AutoRetainer can not function in this configuration. Either disable teleportation or install Lifestream plugin.");
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
                Error($"偵測到非正式版Dalamud分支。這可能導致問題。請透過輸入/xlbranch開啟分支切換器，切換到 "release" 分支並重新啟動遊戲");
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
            Info("Some characters have \"Wait For All Pending Deployables\" option enabled. This means that for these characters AutoRetainer will wait for all deployables to return before processing them. Hover to see complete list of characters with enabled option.", C.OfflineData.Where(x => x.MultiWaitForAllDeployables).Select(x => $"{x.Name}@{x.World}").Print("\n"));
        }

        if(C.MultiModeWorkshopConfiguration.MultiWaitForAll)
        {
            Info("Global option \"Wait For Venture Completion\" is enabled. This means that for all characters AutoRetainer will wait for all deployables to return before processing them, even for these whose per-character option is disabled.");
        }

        if(C.MultiModeWorkshopConfiguration.WaitForAllLoggedIn)
        {
            Info("Option \"Wait even when already logged in\" is enabled for deployables. This means that AutoRetainer will wait for all deployables on a character to be completed before processing them even when you are logged in.");
        }

        if(C.DisableRetainerVesselReturn > 0)
        {
            if(C.DisableRetainerVesselReturn > 10)
            {
                Warning("Option \"Retainer venture processing cutoff\" is set to abnormally high value. You may experience significant delays with resending retainers when deployables are soon to be available.");
            }
            else
            {
                Info("Option \"Retainer venture processing cutoff\" is enabled. You may experience delays with resending retainers when deployables are soon to be available.");
            }
        }

        if(C.MultiModeRetainerConfiguration.MultiWaitForAll)
        {
            Info("Option \"Wait For Venture Completion\" is enabled. This means that AutoRetainer will wait for all ventures from all retainers on a character to be completed before logging in to process them.");
        }

        if(C.MultiModeRetainerConfiguration.WaitForAllLoggedIn)
        {
            Info("Option \"Wait even when already logged in\" is enabled for retainers. This means that AutoRetainer will wait for all ventures from all retainers on a character to be completed before processing them even when you are logged in.");
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
                Info("Some of your retainers have manual entrust plans set. These plans won't be processed automatically after resending retainer for venture, but only manually upon clicking button in overlay. Hover to see the list.", manualList.Print("\n"));
            }
            if(deletedList.Count > 0)
            {
                Warning("Some of your retainers' entrust plans were deleted before. Retainers with deleted entrust plans will not entrust anything. Hover to see list.", deletedList.Print("\n"));
            }
        }

        if(C.No2ndInstanceNotify)
        {
            Info("You have \"Do not warn about second game instance running from same directory\" option enabled, which will skip AutoRetainer's loading on 2nd instance of the game running with the same Dalamud directory automatically.");
        }

        if(Svc.PluginInterface.InstalledPlugins.Any(x => x.InternalName == "SimpleTweaksPlugin" && x.IsLoaded))
        {
            Info("Simple Tweaks plugin detected. Any tweaks related to retainers or submarines may affect AutoRetainer functions negatively. Please ensure that tweaks are configured in a way to not interfere with AutoRetainer functions.");
        }

        if(Svc.PluginInterface.InstalledPlugins.Any(x => x.InternalName == "PandorasBox" && x.IsLoaded))
        {
            Info("Pandora's Box plugin detected. Automatic use of actions while AutoRetainer is enabled may affect AutoRetainer functions negatively. Please ensure that Pandora's Box is configured in a way to not automatically use actions while AutoRetainer is active.");
        }

        if(Svc.PluginInterface.InstalledPlugins.Any(x => x.InternalName == "Automaton" && x.IsLoaded))
        {
            Info("Automaton plugin detected. Automatic use of actions and automatic numeric inputs while AutoRetainer is enabled may affect AutoRetainer functions negatively. Please ensure that Automaton is configured in a way to not use automatically actions while AutoRetainer is active.");
        }

        if(Svc.PluginInterface.InstalledPlugins.Any(x => x.InternalName == "RotationSolver" && x.IsLoaded))
        {
            Info("RotationSolver plugin detected. Automatic use of actions while AutoRetainer is enabled may affect AutoRetainer functions negatively. Please ensure that RotationSolver is configured in a way to not automatically use actions while AutoRetainer is active.");
        }

        if(Svc.PluginInterface.InstalledPlugins.Any(x => x.InternalName.StartsWith("BossMod") && x.IsLoaded))
        {
            Info("BossMod plugin detected. Automatic use of actions while AutoRetainer is enabled may affect AutoRetainer functions negatively. Please ensure that BossMod is configured in a way to not automatically use actions while AutoRetainer is active.");
        }

        ImGui.Separator();
        ImGuiEx.TextWrapped("Expert settings alter behavior that was intended by developer. Please check that your issue is not related to incorrectly configured expert settings.");
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
        ImGuiEx.Text(EColor.RedBright, "\uf057");
        ImGui.PopFont();
        if(tooltip != null) ImGuiEx.Tooltip(tooltip);
        ImGui.SameLine();
        ImGuiEx.TextWrapped(EColor.RedBright, message);
        if(tooltip != null) ImGuiEx.Tooltip(tooltip);
    }

    private static void Warning(string message, string tooltip = null)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        ImGuiEx.Text(EColor.OrangeBright, "\uf071");
        ImGui.PopFont();
        if(tooltip != null) ImGuiEx.Tooltip(tooltip);
        ImGui.SameLine();
        ImGuiEx.TextWrapped(EColor.OrangeBright, message);
        if(tooltip != null) ImGuiEx.Tooltip(tooltip);
    }

    private static void Info(string message, string tooltip = null)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        ImGuiEx.Text(EColor.YellowBright, "\uf05a");
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
