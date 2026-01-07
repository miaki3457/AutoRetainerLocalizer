using AutoRetainerAPI.Configuration;
using ECommons.GameHelpers;

namespace AutoRetainer.UI.Statistics;
public sealed class FcDataManager
{
    private FcDataManager() { }

    public void Draw()
    {
        ImGui.Checkbox($"每30小時更新", ref C.UpdateStaleFCData);
        ImGui.SameLine();
        if(ImGuiEx.Button("更新", Player.Interactable))
        {
            S.FCPointsUpdater.ScheduleUpdateIfNeeded(true);
        }
        ImGui.SameLine();
        ImGui.Checkbox($"僅顯示錢包部隊", ref C.DisplayOnlyWalletFC);
        if(ImGui.BeginTable("FCData", 5, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
        {
            ImGui.TableSetupColumn($"名稱", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn($"角色");
            ImGui.TableSetupColumn($"Gil");
            ImGui.TableSetupColumn($"部隊戰績");
            ImGui.TableSetupColumn($"##control");
            ImGui.TableHeadersRow();

            var totalGil = 0L;
            var totalPoint = 0L;

            var i = 0;
            foreach(var x in C.FCData)
            {
                if(x.Key == 0) continue;
                if(!x.Value.GilCountsTowardsChara && C.DisplayOnlyWalletFC) continue;
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGuiEx.TextV(C.NoNames ? $"Free company {++i}" : x.Value.Name);

                ImGui.TableNextColumn();
                foreach(var c in C.OfflineData.Where(z => z.FCID == x.Key))
                {
                    ImGuiEx.Text(x.Value.HolderChara == c.CID && x.Value.GilCountsTowardsChara ? EColor.GreenBright : null, Censor.Character(c.Name, c.World));
                    if(ImGuiEx.HoveredAndClicked("左鍵 - 重新登入此角色"))
                    {
                        Svc.Commands.ProcessCommand($"/ays relog {c.Name}@{c.World}");
                    }
                    if(x.Value.GilCountsTowardsChara)
                    {
                        if(ImGuiEx.HoveredAndClicked("右鍵 - 設為Gil持有者", ImGuiMouseButton.Right))
                        {
                            x.Value.HolderChara = c.CID;
                        }
                    }
                }

                ImGui.TableNextColumn();
                if(x.Value.LastGilUpdate != -1 && x.Value.LastGilUpdate != 0)
                {
                    ImGuiEx.Text($"{x.Value.Gil:N0}");
                    totalGil += x.Value.Gil;
                    ImGuiEx.Tooltip($"Last updated {UpdatedWhen(x.Value.LastGilUpdate)}. Ctrl + click to reset");
                    if(ImGuiEx.HoveredAndClicked() && ImGuiEx.Ctrl)
                    {
                        x.Value.LastGilUpdate = -1;
                        x.Value.Gil = 0;
                    }
                }
                else
                {
                    ImGuiEx.Text($"未知");
                }

                ImGui.TableNextColumn();
                if(x.Value.FCPointsLastUpdate != 0)
                {
                    ImGuiEx.Text($"{x.Value.FCPoints:N0}");
                    totalPoint += x.Value.FCPoints;
                    ImGuiEx.Tooltip($"Last updated {UpdatedWhen(x.Value.FCPointsLastUpdate)}");
                }
                else
                {
                    ImGuiEx.Text($"未知");
                }

                ImGui.TableNextColumn();
                ImGui.PushFont(UiBuilder.IconFont);
                ImGuiEx.ButtonCheckbox($"\uf555##FC{x.Key}", ref x.Value.GilCountsTowardsChara, EColor.Green);
                ImGui.PopFont();
                ImGuiEx.Tooltip("標記此部隊為錢包部隊。Gil顯示標籤頁將包含此部隊的Gil。");
                ImGui.SameLine();
                if(ImGuiEx.IconButton(FontAwesomeIcon.Trash, $"{x.Key}Dele", enabled: ImGuiEx.Ctrl))
                {
                    new TickScheduler(() => C.FCData.Remove(x));
                }

                ImGuiEx.Tooltip($"按住CTRL並點擊刪除此部隊。注意：如果重新登入此部隊，它會再次出現。");
            }

            ImGui.TableNextRow();
            ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, EColor.GreenDark.ToUint());
            ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg1, EColor.GreenDark.ToUint());
            ImGui.TableNextColumn();
            ImGuiEx.Text($"TOTAL");
            ImGui.TableNextColumn();
            ImGui.TableNextColumn();
            ImGuiEx.Text($"{totalGil:N0}");
            ImGui.TableNextColumn();
            ImGuiEx.Text($"{totalPoint:N0}");

            ImGui.EndTable();
        }


        string UpdatedWhen(long time)
        {
            var diff = DateTimeOffset.Now.ToUnixTimeMilliseconds() - time;
            if(diff < 1000L * 60) return "just now";
            if(diff < 1000L * 60 * 60) return $"{(int)(diff / 1000 / 60)} minute(s) ago";
            if(diff < 1000L * 60 * 60 * 60) return $"{(int)(diff / 1000 / 60 / 60)} hour(s) ago";
            return $"{(int)(diff / 1000 / 60 / 60 / 24)} day(s) ago";
        }
    }

    public OfflineCharacterData GetHolderChara(ulong fcid, FCData data)
    {
        if(C.OfflineData.TryGetFirst(x => x.FCID == fcid && x.CID == data.HolderChara, out var chara))
        {
            return chara;
        }
        else if(C.OfflineData.TryGetFirst(x => x.FCID == fcid, out var fchara))
        {
            data.HolderChara = fchara.CID;
            return fchara;
        }
        return null;
    }
}
