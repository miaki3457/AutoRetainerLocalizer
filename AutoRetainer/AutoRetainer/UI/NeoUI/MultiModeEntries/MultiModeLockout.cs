using ECommons.ExcelServices;

namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeLockout : NeoUIEntry
{
    public override string Path => "Multi Mode/Region Lock";

    private int Num = 12;

    public override void Draw()
    {
        ImGuiEx.TextV("For");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(150f);
        ImGui.InputInt("小時內...", ref Num.ValidateRange(1, 10000));
        foreach(var x in Enum.GetValues<ExcelWorldHelper.Region>())
        {
            if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Lock, $"...不登入到 {x} 區域"))
            {
                C.LockoutTime[x] = DateTimeOffset.Now.ToUnixTimeSeconds() + Num * 60 * 60;
            }
        }
        if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Unlock, "移除所有鎖定"))
        {
            C.LockoutTime.Clear();
        }
    }
}
