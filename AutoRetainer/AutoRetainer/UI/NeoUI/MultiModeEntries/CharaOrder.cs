using AutoRetainerAPI.Configuration;

namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class CharaOrder : NeoUIEntry
{
    public override string Path => "多角色模式/排除與排序";

    private static string Search = "";
    private static ImGuiEx.RealtimeDragDrop<OfflineCharacterData> DragDrop = new("CharaOrder", x => x.Identity);

    public override bool NoFrame { get; set; } = true;

    public override void Draw()
    {
        C.OfflineData.RemoveAll(x => C.Blacklist.Any(z => z.CID == x.CID));
        var b = new NuiBuilder()
        .Section("角色排序")
        .Widget("在此處可對角色進行排序。這將影響多角色模式處理它們的順序，以及它們在插件介面和登入覆蓋層中的顯示順序。", (x) =>
        {
            ImGuiEx.TextWrapped($"在此處可對角色進行排序。這將影響多角色模式處理它們的順序，以及它們在插件介面和登入覆蓋層中的顯示順序。");
            ImGui.SetNextItemWidth(150f);
            ImGui.InputText($"搜索", ref Search, 50);
            DragDrop.Begin();
            if(ImGui.BeginTable("CharaOrderTable", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.NoSavedSettings | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit))
            {
                ImGui.TableSetupColumn("##ctrl");
                ImGui.TableSetupColumn("角色", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("Toggles");
                ImGui.TableSetupColumn("Deletion");
                ImGui.TableHeadersRow();

                for(var index = 0; index < C.OfflineData.Count; index++)
                {
                    var chr = C.OfflineData[index];
                    ImGui.PushID(chr.Identity);
                    ImGui.TableNextRow();
                    DragDrop.SetRowColor(chr.Identity);
                    ImGui.TableNextColumn();
                    DragDrop.NextRow();
                    DragDrop.DrawButtonDummy(chr, C.OfflineData, index);
                    ImGui.TableNextColumn();
                    ImGuiEx.TextV((Search != "" && ($"{chr.Name}@{chr.World}").Contains(Search, StringComparison.OrdinalIgnoreCase)) ? ImGuiColors.ParsedGreen : (Search == "" ? null : ImGuiColors.DalamudGrey3), Censor.Character(chr.Name, chr.World));
                    ImGui.TableNextColumn();
                    if(ImGuiEx.ButtonCheckbox(FontAwesomeIcon.Users, ref chr.ExcludeRetainer, inverted: true))
                    {
                        chr.Enabled = false;
                        C.SelectedRetainers.Remove(chr.CID);
                    }
                    ImGuiEx.Tooltip("Enable retainers");
                    ImGuiEx.DragDropRepopulate("EnRet", chr.ExcludeRetainer, ref chr.ExcludeRetainer);
                    ImGui.SameLine();
                    if(ImGuiEx.ButtonCheckbox(FontAwesomeIcon.Ship, ref chr.ExcludeWorkshop, inverted: true))
                    {
                        chr.WorkshopEnabled = false;
                        chr.EnabledSubs.Clear();
                        chr.EnabledAirships.Clear();
                    }
                    ImGuiEx.Tooltip("Enable deployables");
                    ImGuiEx.DragDropRepopulate("EnDep", chr.ExcludeWorkshop, x =>
                    {
                        chr.ExcludeWorkshop = x;
                        if(!x)
                        {
                            chr.EnabledSubs.Clear();
                            chr.EnabledAirships.Clear();
                        }
                    });
                    ImGui.SameLine();
                    ImGuiEx.ButtonCheckbox(FontAwesomeIcon.DoorOpen, ref chr.ExcludeOverlay, inverted: true);
                    ImGuiEx.Tooltip("Display on login overlay");
                    ImGuiEx.DragDropRepopulate("EnLog", chr.ExcludeOverlay, ref chr.ExcludeOverlay);
                    ImGui.SameLine();
                    ImGuiEx.ButtonCheckbox(FontAwesomeIcon.Coins, ref chr.NoGilTrack, inverted: true);
                    ImGuiEx.Tooltip("Count gil on this character towards total");
                    ImGuiEx.DragDropRepopulate("EnGil", chr.NoGilTrack, ref chr.NoGilTrack);
                    ImGui.SameLine();
                    ImGuiEx.ButtonCheckbox(FontAwesomeIcon.GasPump, ref chr.AutoFuelPurchase, color:ImGuiColors.TankBlue);
                    ImGuiEx.Tooltip("Allow this character to purchase fuel from workshop");
                    ImGuiEx.DragDropRepopulate("EnFuel", chr.AutoFuelPurchase, ref chr.AutoFuelPurchase);
                    ImGui.TableNextColumn();
                    if(ImGuiEx.IconButton(FontAwesomeIcon.UserMinus))
                    {
                        chr.ClearFCData();
                    }
                    ImGuiEx.Tooltip("Reset FC data and deployable data for this character. It will regenerate once you log in and access workshop panel.");
                    ImGui.SameLine();
                    if(ImGuiEx.IconButton(FontAwesomeIcon.Trash, enabled: ImGuiEx.Ctrl))
                    {
                        new TickScheduler(() => C.OfflineData.Remove(chr));
                    }
                    ImGuiEx.Tooltip($"按住CTRL + 左鍵以刪除儲存的角色資料。重新登入後會自動重建。");
                    ImGui.SameLine();
                    if(ImGuiEx.IconButton("", enabled: ImGuiEx.Ctrl))
                    {
                        C.Blacklist.Add((chr.CID, chr.Name));
                    }
                    ImGuiEx.Tooltip($"按住CTRL + 左鍵以永久刪除角色數據，該角色將完全排除在AutoRetainer的處理範圍之外。");

                    ImGui.PopID();
                }

                ImGui.EndTable();
            }
            DragDrop.End();
        });


        if(C.Blacklist.Count != 0)
        {
            b = b.Section("已排除角色")
                .Widget(() =>
                {
                    for(var i = 0; i < C.Blacklist.Count; i++)
                    {
                        var d = C.Blacklist[i];
                        ImGuiEx.TextV($"{d.Name} ({d.CID:X16})");
                        ImGui.SameLine();
                        if(ImGui.Button($"刪除##bl{i}"))
                        {
                            C.Blacklist.RemoveAt(i);
                            C.SelectedRetainers.Remove(d.CID);
                            break;
                        }
                    }
                });
        }

        b.Draw();
    }
}
