using ECommons.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.InventoryCleanupEntries;
public static unsafe class InventoryCleanupCommon
{
    public static Guid SelectedPlanGuid = Guid.Empty;

    public static InventoryManagementSettings SelectedPlan
    {
        get
        {
            if(SelectedPlanGuid == Guid.Empty)
            {
                return C.DefaultIMSettings;
            }
            else
            {
                var planIndex = C.AdditionalIMSettings.IndexOf(x => x.GUID == SelectedPlanGuid);
                if(planIndex == -1)
                {
                    SelectedPlanGuid = Guid.Empty;
                    return C.DefaultIMSettings;
                }
                else
                {
                    return C.AdditionalIMSettings[planIndex];
                }
            }
        }
    }

    public static NuiBuilder CreateCleanupHeaderBuilder()
    {
        return new NuiBuilder().Section("背包清理計畫選擇器").Widget(DrawPlanSelector);
    }

    public static void DrawPlanSelector()
    {
        var selectedPlan = C.AdditionalIMSettings.FirstOrDefault(x => x.GUID == SelectedPlanGuid);
        ImGuiEx.InputWithRightButtonsArea(() =>
        {
            if(ImGui.BeginCombo("##selimplan", selectedPlan?.DisplayName ?? "預設計畫"))
            {
                if(ImGui.Selectable("Default Plan", selectedPlan == null)) SelectedPlanGuid = Guid.Empty;
                ImGui.Separator();
                foreach(var x in C.AdditionalIMSettings)
                {
                    ImGui.PushID(x.ID);
                    if(ImGui.Selectable(x.DisplayName)) SelectedPlanGuid = x.GUID;
                    ImGui.PopID();
                }
                ImGui.EndCombo();
            }
        }, () =>
        {
            if(ImGuiEx.IconButton(FontAwesomeIcon.Plus))
            {
                var newPlan = new InventoryManagementSettings()
                {
                    AllowSellFromArmory = C.DefaultIMSettings.AllowSellFromArmory,
                    IMEnableContextMenu = C.DefaultIMSettings.IMEnableContextMenu,
                    IMEnableCofferAutoOpen = C.DefaultIMSettings.IMEnableCofferAutoOpen,
                    IMSkipVendorIfRetainer = C.DefaultIMSettings.IMSkipVendorIfRetainer,
                    IMEnableAutoVendor = C.DefaultIMSettings.IMEnableAutoVendor,
                    IMEnableNpcSell = C.DefaultIMSettings.IMEnableNpcSell,
                };
                C.AdditionalIMSettings.Add(newPlan);
                SelectedPlanGuid = newPlan.GUID;
            }
            ImGuiEx.Tooltip("添加新的計畫");
            ImGui.SameLine(0, 1);
            if(ImGuiEx.IconButton(FontAwesomeIcon.Copy))
            {
                var clone = (selectedPlan ?? C.DefaultIMSettings).DSFClone();
                clone.GUID = Guid.Empty;
                Copy(EzConfig.DefaultSerializationFactory.Serialize(clone));
            }
            ImGuiEx.Tooltip("複製");
            ImGui.SameLine(0, 1);
            if(ImGuiEx.IconButton(FontAwesomeIcon.Paste))
            {
                try
                {
                    var newPlan = EzConfig.DefaultSerializationFactory.Deserialize<InventoryManagementSettings>(Paste()) ?? throw new NullReferenceException();
                    newPlan.GUID.Regenerate();
                    C.AdditionalIMSettings.Add(newPlan);
                    SelectedPlanGuid = newPlan.GUID;
                }
                catch(Exception e)
                {
                    e.Log();
                    Notify.Error(e.Message);
                }
            }
            ImGuiEx.Tooltip("貼上");
            if(selectedPlan != null)
            {
                ImGui.SameLine(0, 1);
                if(ImGuiEx.IconButton(FontAwesomeIcon.ArrowsUpToLine, enabled: ImGuiEx.Ctrl && selectedPlan != null))
                {
                    C.DefaultIMSettings = selectedPlan.DSFClone();
                    C.DefaultIMSettings.GUID.Regenerate();
                    C.DefaultIMSettings.Name = "";
                    new TickScheduler(() => C.AdditionalIMSettings.Remove(selectedPlan));
                }
                ImGuiEx.Tooltip("將此計畫設為預設計畫，當前預設計畫將會被覆蓋。按住CTRL + 左鍵");
                ImGui.SameLine(0, 1);
                if(ImGuiEx.IconButton(FontAwesomeIcon.Trash, enabled: ImGuiEx.Ctrl && selectedPlan != null))
                {
                    new TickScheduler(() => C.AdditionalIMSettings.Remove(selectedPlan));
                }
                ImGuiEx.Tooltip("刪除此計畫。按住CTRL + 左鍵");
            }
        });
        if(selectedPlan != null)
        {
            ImGuiEx.SetNextItemFullWidth();
            ImGui.InputTextWithHint("##name", "Enter plan name", ref selectedPlan.Name, 100);

            if(Data != null)
            {
                if(Data.InventoryCleanupPlan == SelectedPlanGuid)
                {
                    ImGuiEx.Text(ImGuiColors.ParsedGreen, UiBuilder.IconFont, FontAwesomeIcon.Check.ToIconString());
                    ImGui.SameLine();
                    ImGuiEx.Text(ImGuiColors.ParsedGreen, $"當前角色使用");
                    ImGui.SameLine();
                    if(ImGui.SmallButton("取消分配"))
                    {
                        Data.InventoryCleanupPlan = Guid.Empty;
                    }
                }
                else
                {
                    ImGuiEx.Text(ImGuiColors.DalamudOrange, UiBuilder.IconFont, FontAwesomeIcon.ExclamationTriangle.ToIconString());
                    ImGui.SameLine();
                    ImGuiEx.Text(ImGuiColors.DalamudOrange, $"非當前角色使用");
                    ImGui.SameLine();
                    if(ImGui.SmallButton("分配"))
                    {
                        Data.InventoryCleanupPlan = selectedPlan.GUID;
                    }
                }
                ImGui.SameLine();
            }

            var charas = C.OfflineData.Where(x => x.ExchangePlan == selectedPlan.GUID).ToArray();
            if(charas.Length > 0)
            {
                ImGuiEx.Text($"共有 {charas.Length} 個角色使用");
                ImGuiEx.Tooltip($"{charas.Select(x => x.NameWithWorldCensored)}");
            }
            else
            {
                ImGuiEx.Text($"沒有任何角色使用");
            }

            ImGuiEx.Text("將此方案的清單與預設方案合併:");
            ImGui.Indent();
            ImGui.Checkbox("合併僱員快速出售清單", ref selectedPlan.AdditionModeSoftSellList);
            ImGuiEx.HelpMarker("從快速探索中獲得的物品，若同時包含在此方案與預設方案中，將會被出售。");
            ImGui.Checkbox("合併無條件出售清單", ref selectedPlan.AdditionModeHardSellList);
            ImGuiEx.HelpMarker("包含在此方案與預設方案中的物品都將被出售。如果同時包含在兩者中，將優先採用此方案的「忽略堆疊」設定與「最大堆疊數量」限制。");
            ImGui.Checkbox("合併丟棄清單", ref selectedPlan.AdditionModeDiscardList);
            ImGuiEx.HelpMarker("包含在此方案與預設方案中的物品都將被丟棄。如果同時包含在兩者中，將優先採用此方案的「忽略堆疊」設定與「最大堆疊數量」限制。");
            ImGui.Checkbox("合併保護清單", ref selectedPlan.AdditionModeProtectList);
            ImGuiEx.HelpMarker("包含在此方案與預設方案中的物品將受到保護，不會被自動出售、丟棄或籌備給軍隊。");
            ImGui.Unindent();
        }
    }
}