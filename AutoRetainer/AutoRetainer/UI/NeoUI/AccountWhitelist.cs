using ECommons.GameHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI;
public sealed unsafe class AccountWhitelist : NeoUIEntry
{
    public override void Draw()
    {
        ImGuiEx.TextWrapped($"您可以設定帳戶白名單。當您使用非白名單帳號登入時，AutoRetainer將不會記錄任何角色、僱員或潛水艇資訊。");
        if(C.WhitelistedAccounts.Count == 0)
        {
            ImGuiEx.TextWrapped(EColor.GreenBright, "目前白名單狀態：已停用。要啟用，請添加一些帳號。");
        }
        else
        {
            ImGuiEx.TextWrapped(EColor.YellowBright, "目前白名單狀態：已啟用。要停用，請移除所有帳號。");
        }

        foreach(var x in C.WhitelistedAccounts)
        {
            ImGui.PushID(x.ToString());
            if(ImGuiEx.IconButton(FontAwesomeIcon.Trash))
            {
                new TickScheduler(() => C.WhitelistedAccounts.Remove(x));
            }
            ImGui.SameLine();
            ImGuiEx.TextV($"帳號 {x}");
            ImGui.PopID();
        }
    }
}