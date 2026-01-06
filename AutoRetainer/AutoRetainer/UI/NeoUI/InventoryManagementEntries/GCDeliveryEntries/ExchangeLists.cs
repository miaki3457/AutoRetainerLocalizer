using AutoRetainerAPI.Configuration;
using ECommons.Configuration;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using Lumina.Excel.Sheets;
using System.Numerics;
using GrandCompany = ECommons.ExcelServices.GrandCompany;
using GrandCompanyRank = Lumina.Excel.Sheets.GrandCompanyRank;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.GCDeliveryEntries;
public sealed unsafe class ExchangeLists : InventoryManagementBase
{
    private ImGuiEx.RealtimeDragDrop<GCExchangeItem> DragDrop = new("GCELDD", x => x.ID);
    public override string Name { get; } = "大國防聯軍 - 軍票交換清單";
    private GCExchangeCategoryTab? SelectedCategory = null;
    private GCExchangeCategoryTab? SelectedCategory2 = null;
    private GCExchangeRankTab? SelectedRank = null;
    private Guid SelectedPlanGuid = Guid.Empty;

    public override int DisplayPriority => -5;

    public override void Draw()
    {
        C.AdditionalGCExchangePlans.Where(x => x.GUID == Guid.Empty).Each(x => x.GUID = Guid.NewGuid());
        ImGuiEx.TextWrapped($"""
選擇在大國防聯軍專家籌備期間要自動購買的物品。購買邏輯：
                                                               - 系統將嘗試購買清單中第一個可用的物品。
                                                               - 購買將持續到該物品在背包中的數量達到指定目標為止。
                                                               如果清單中沒有物品可供購買，或背包空間不足：
                                                               - 系統將轉為購買探險幣。
                                                               - 購買探險幣將持續到您的探險幣數量達到 65,000 個。
                                                               一旦達到探險幣上限且無法再進行其他購買：
                                                               - 任何多餘的軍票都將被丟棄。
            """);

        var selectedPlan = C.AdditionalGCExchangePlans.FirstOrDefault(x => x.GUID == SelectedPlanGuid);
        ImGuiEx.InputWithRightButtonsArea(() =>
        {
            if(ImGui.BeginCombo("##selplan", selectedPlan?.DisplayName ?? "預設計畫"))
            {
                if(ImGui.Selectable("預設計畫", selectedPlan == null)) SelectedPlanGuid = Guid.Empty;
                ImGui.Separator();
                foreach(var x in C.AdditionalGCExchangePlans)
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
                var newPlan = new GCExchangePlan();
                C.AdditionalGCExchangePlans.Add(newPlan);
                SelectedPlanGuid = newPlan.GUID;
            }
            ImGuiEx.Tooltip("添加新的計畫");
            ImGui.SameLine(0, 1);
            if(ImGuiEx.IconButton(FontAwesomeIcon.Copy))
            {
                var clone = (selectedPlan ?? C.DefaultGCExchangePlan).DSFClone();
                clone.GUID = Guid.Empty;
                Copy(EzConfig.DefaultSerializationFactory.Serialize(clone));
            }
            ImGuiEx.Tooltip("複製");
            ImGui.SameLine(0, 1);
            if(ImGuiEx.IconButton(FontAwesomeIcon.Paste))
            {
                try
                {
                    var newPlan = EzConfig.DefaultSerializationFactory.Deserialize<GCExchangePlan>(Paste()) ?? throw new NullReferenceException();
                    newPlan.GUID.Regenerate();  
                    C.AdditionalGCExchangePlans.Add(newPlan);
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
                    C.DefaultGCExchangePlan = selectedPlan.DSFClone();
                    C.DefaultGCExchangePlan.Name = "";
                    C.DefaultGCExchangePlan.GUID.Regenerate();
                    new TickScheduler(() => C.AdditionalGCExchangePlans.Remove(selectedPlan));
                }
                ImGuiEx.Tooltip("將此計畫設為預設計畫，當前預設計畫將會被覆蓋。按住CTRL + 左鍵");
                ImGui.SameLine(0, 1);
                if(ImGuiEx.IconButton(FontAwesomeIcon.Trash, enabled: ImGuiEx.Ctrl && selectedPlan != null))
                {
                    new TickScheduler(() => C.AdditionalGCExchangePlans.Remove(selectedPlan));
                }
                ImGuiEx.Tooltip("刪除此計畫。按住CTRL + 左鍵");
            }
        });

        if(SelectedPlanGuid == Guid.Empty)
        {
            DrawGCEchangeList(C.DefaultGCExchangePlan);
        }
        else
        {
            if(Data != null)
            {
                if(Data.ExchangePlan == SelectedPlanGuid)
                {
                    ImGuiEx.Text(ImGuiColors.ParsedGreen, UiBuilder.IconFont, FontAwesomeIcon.Check.ToIconString());
                    ImGui.SameLine();
                    ImGuiEx.Text(ImGuiColors.ParsedGreen, $"當前角色使用");
                    ImGui.SameLine();
                    if(ImGui.SmallButton("取消分配"))
                    {
                        Data.ExchangePlan = Guid.Empty;
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
                        Data.ExchangePlan = selectedPlan.GUID;
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

                var planIndex = C.AdditionalGCExchangePlans.IndexOf(x => x.GUID == SelectedPlanGuid);
            if(planIndex == -1)
            {
                SelectedPlanGuid = Guid.Empty;
            }
            else
            {
                DrawGCEchangeList(C.AdditionalGCExchangePlans[planIndex]);
            }
        }
    }

    public void DrawGCEchangeList(GCExchangePlan plan)
    {
        ref string getFilter() => ref Ref<string>.Get($"{plan.ID}filter");
        ref bool onlySelected() => ref Ref<bool>.Get($"{plan.ID}onlySel");
        ref string getFilter2() => ref Ref<string>.Get($"{plan.ID}filter2");

        ImGui.PushID(plan.ID);
        plan.Validate();

        ImGuiEx.InputWithRightButtonsArea("GCPlanSettings", () =>
        {
            if(ReferenceEquals(plan, C.DefaultGCExchangePlan))
            {
                ImGui.BeginDisabled();
                var s = "預設的交換計劃無法被重新命名";
                ImGui.InputText("##name", ref s, 1);
                ImGui.EndDisabled();
            }
            else
            {
                ImGui.InputTextWithHint($"##name", "名稱", ref plan.Name, 100);
                ImGuiEx.Tooltip("交換計畫名稱");
            }
        }, () =>
        {
            ImGui.SetNextItemWidth(100f);
            ImGui.InputInt("保留軍票數量", ref plan.RemainingSeals.ValidateRange(0, 70000), 0, 0);
            ImGuiEx.HelpMarker($"執行交換清單後保留的軍票數量(最大不超過角色軍階可持有上限減 20,000)");
            ImGui.SameLine();
            ImGui.Checkbox("籌備後交換", ref plan.FinalizeByPurchasing);
            ImGuiEx.HelpMarker("勾選後將會在籌備稀有品完成後交換物品；未勾選則在軍票到達上限後才交換");
        });

        ImGuiEx.SetNextItemFullWidth();
        if(ImGui.BeginCombo("##Add Items", "添加物品", ImGuiComboFlags.HeightLarge))
        {
            ImGuiEx.InputWithRightButtonsArea(() =>
            {
                ImGui.InputTextWithHint("##filter2", "搜索...", ref getFilter2(), 100);
            }, () =>
            {
                ImGui.SetNextItemWidth(100f);
                ImGuiEx.EnumCombo("##cat2", ref SelectedCategory2, nullName: "所有類別");
                ImGuiEx.Tooltip("類別");
            });
            foreach(var x in Utils.SharedGCExchangeListings)
            {
                if(getFilter2().Length > 0
                    && !x.Value.Data.Name.GetText().Contains(getFilter2(), StringComparison.OrdinalIgnoreCase)
                    && !x.Value.Category.ToString().Equals(getFilter2(), StringComparison.OrdinalIgnoreCase)
                    && !Utils.GCRanks[x.Value.MinPurchaseRank].Equals(getFilter2(), StringComparison.OrdinalIgnoreCase)
                    ) continue;
                if(SelectedCategory2 != null && x.Value.Category != SelectedCategory2.Value) continue;
                var cont = plan.Items.Select(s => s.ItemID).ToArray();
                if(ThreadLoadImageHandler.TryGetIconTextureWrap(x.Value.Data.Icon, false, out var t))
                {
                    ImGui.Image(t.Handle, new(ImGui.GetTextLineHeight()));
                    ImGui.SameLine();
                }
                if(ImGui.Selectable(x.Value.Data.GetName() + $"##{x.Key}", cont.Contains(x.Key), ImGuiSelectableFlags.DontClosePopups))
                {
                    plan.Items.Add(new(x.Key, 0));
                }
            }
            ImGui.EndCombo();
        }
        if(ImGui.BeginPopup("Ex"))
        {
            if(ImGui.Selectable("自動交換軍票武器與裝備，最佳化以獲取額外部隊點數。(FC Point)"))
            {
                List<GCExchangeItem> items = [];
                var qualifyingItems = Utils.SharedGCExchangeListings.Where(x => (x.Value.Category == GCExchangeCategoryTab.Weapons || x.Value.Category == GCExchangeCategoryTab.Armor) && x.Value.Data.GetRarity() == ItemRarity.Green).ToDictionary();
                plan.Items.RemoveAll(x => qualifyingItems.ContainsKey(x.ItemID));
                foreach(var item in qualifyingItems)
                {
                    items.Add(new(item.Key, 0));
                }
                items = items.OrderByDescending(x => (double)Svc.Data.GetExcelSheet<GCSupplyDutyReward>().GetRow(x.Data.Value.LevelItem.RowId).SealsExpertDelivery / (double)Utils.SharedGCExchangeListings[x.ItemID].Seals).ToList();
                foreach(var x in items)
                {
                    plan.Items.Add(x);
                    x.Quantity = Utils.SharedGCExchangeListings[x.ItemID].Data.IsUnique ? 1 : 999;
                }
            }
            ImGuiEx.Tooltip("選擇此選項將自動填入所有可交換的武器與裝備；交換的武器與裝備將會立即籌備至軍隊，以最大化產生部隊點數。這些物品將會被放在清單末端，且僅在沒有其他可購買物品時才會購買。");
            if(ImGui.Selectable("加入所有缺少的物品"))
            {
                foreach(var x in Utils.SharedGCExchangeListings)
                {
                    if(!plan.Items.Any(i => i.ItemID == x.Key))
                    {
                        plan.Items.Add(new(x.Key, 0));
                    }
                }
            }
            if(ImGui.Selectable("將數量重設為 0"))
            {
                plan.Items.Each(x => x.Quantity = 0);
                plan.Items.Each(x => x.QuantitySingleTime = 0);
            }
            if(ImGui.Selectable("移除數量為 0 的物品"))
            {
                plan.Items.RemoveAll(x => x.Quantity == 0 && x.QuantitySingleTime == 0);
            }
            if(ImGuiEx.Selectable("清除清單(按住 CTRL + 左鍵)", enabled: ImGuiEx.Ctrl))
            {
                plan.Items.Clear();
            }
            ImGui.EndPopup();
        }
        if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.AngleDoubleDown, "Actions"))
        {
            ImGui.OpenPopup("Ex");
        }
        ImGui.SameLine();
        ImGuiEx.InputWithRightButtonsArea("Fltr2", () =>
        {
            ImGui.InputTextWithHint("##filter", "搜索...", ref getFilter(), 100);
        }, () =>
        {
            ImGui.Checkbox("僅顯示已選擇", ref onlySelected());
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100f);
            ImGuiEx.EnumCombo("##cat", ref SelectedCategory, nullName: "所有類別");
            ImGuiEx.Tooltip("類別");
        });



        DragDrop.Begin();
        if(ImGuiEx.BeginDefaultTable("GCDeliveryList", ["##dragDrop", "~Item", "GC", "Lv", "Price", "類別", "Keep", "One-Time", "##controls"]))
        {
            for(var i = 0; i < plan.Items.Count; i++)
            {
                var currentItem = plan.Items[i];
                var meta = Utils.SharedGCExchangeListings[currentItem.ItemID];
                if(onlySelected() && currentItem.Quantity == 0) continue;
                if(getFilter().Length > 0
                    && !meta.Data.Name.GetText().Contains(getFilter(), StringComparison.OrdinalIgnoreCase)
                    && !meta.Category.ToString().Equals(getFilter(), StringComparison.OrdinalIgnoreCase)
                    && !Utils.GCRanks[meta.MinPurchaseRank].Equals(getFilter(), StringComparison.OrdinalIgnoreCase)
                    ) continue;
                if(SelectedCategory != null && meta.Category != SelectedCategory.Value) continue;
                ImGui.PushID(currentItem.ID);
                ImGui.TableNextRow();
                DragDrop.SetRowColor(currentItem);
                ImGui.TableNextColumn();
                DragDrop.NextRow();
                if(ImGuiEx.IconButton(FontAwesomeIcon.AngleDoubleUp))
                {
                    new TickScheduler(() =>
                    {
                        plan.Items.Remove(currentItem);
                        plan.Items.Insert(0, currentItem);
                    });
                }
                ImGui.SameLine(0, 1);
                ImGuiEx.Tooltip("移至頂部");
                DragDrop.DrawButtonDummy(currentItem, plan.Items, i);
                ImGui.TableNextColumn();
                if(ThreadLoadImageHandler.TryGetIconTextureWrap(meta.Data.Icon, false, out var t))
                {
                    ImGui.Image(t.Handle, new(ImGui.GetFrameHeight()));
                    ImGui.SameLine();
                }
                ImGuiEx.TextV($"{meta.Data.Name.GetText()}");
                ImGui.TableNextColumn();
                foreach(var c in Enum.GetValues<GrandCompany>().Where(x => x != GrandCompany.Unemployed))
                {
                    if(ThreadLoadImageHandler.TryGetIconTextureWrap(60870 + (int)c, false, out var ctex))
                    {
                        var trans = !meta.Companies.Contains(c);
                        if(trans) ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.2f);
                        ImGui.Image(ctex.Handle, new(ImGui.GetFrameHeight()));
                        if(trans) ImGui.PopStyleVar();
                        ImGuiEx.Tooltip($"{c}" + (trans ? " (unavailable)" : ""));
                        ImGui.SameLine(0, 1);
                    }
                }
                ImGui.TableNextColumn();
                ImGuiEx.TextV($"{meta.Data.LevelItem.RowId}");
                ImGui.TableNextColumn();
                if(Svc.Data.GetExcelSheet<GrandCompanyRank>().TryGetRow(meta.MinPurchaseRank, out var rank) && ThreadLoadImageHandler.TryGetIconTextureWrap(rank.IconFlames, false, out var tex))
                {
                    ImGui.Image(tex.Handle, new(ImGui.GetFrameHeight()));
                    var rankName = Utils.GCRanks[meta.MinPurchaseRank];
                    ImGuiEx.Tooltip(rankName);
                    if(ImGuiEx.HoveredAndClicked()) getFilter() = rankName;
                    ImGui.SameLine();
                }
                ImGuiEx.TextV($"{meta.Seals}");
                ImGui.TableNextColumn();
                ImGuiEx.TextV($"{meta.Category}");
                if(ImGuiEx.HoveredAndClicked()) getFilter() = meta.Category.ToString();
                ImGui.TableNextColumn();
                if(currentItem.Data.Value.IsUnique)
                {
                    ImGuiEx.Checkbox("獨特的", ref currentItem.Quantity);
                }
                else
                {
                    ImGui.SetNextItemWidth(100f.Scale());
                    ImGui.InputInt("##qty", ref currentItem.Quantity.ValidateRange(0, int.MaxValue), 0, 0);
                }
                ImGuiEx.Tooltip("選擇背包中要維持的數量");
                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(100f.Scale());
                ImGui.InputInt("##qtyonetime", ref currentItem.QuantitySingleTime.ValidateRange(0, currentItem.Data.Value.IsUnique ? 1 : int.MaxValue), 0, 0);
                ImGuiEx.Tooltip("每次交換的數量：當使用此方案在任意角色上進行交換時，該數量將從此值中扣除。當數值降至 0 時，將自動恢復為\"維持\"數量。");
                ImGui.TableNextColumn();
                if(ImGuiEx.IconButton(FontAwesomeIcon.Clone))
                {
                    plan.Items.Insert(i + 1, currentItem.JSONClone());
                }
                ImGuiEx.Tooltip("複製此列表");
                ImGui.SameLine(0, 1);
                if(ImGuiEx.IconButton(FontAwesomeIcon.Trash))
                {
                    new TickScheduler(() => plan.Items.Remove(currentItem));
                }
                ImGuiEx.Tooltip($"如果清單中存在多個相同的物品，則將其從清單中刪除；如果清單中只有一個相同的物品，則將其數量設為 0。");
                ImGui.PopID();
            }
            ImGui.EndTable();
        }
        DragDrop.End();
        ImGui.PopID();
    }
}