namespace AutoRetainer.UI.NeoUI;
public class LoginOverlay : NeoUIEntry
{
    public override string Path => "登入覆蓋介面";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
            .Section("登入覆蓋介面")
            .Checkbox("顯示登入覆蓋視窗", () => ref C.LoginOverlay)
            .Widget("登入視窗縮放倍率", (x) =>
            {
                ImGuiEx.SetNextItemWidthScaled(150f);
                if(ImGuiEx.SliderFloat(x, ref C.LoginOverlayScale.ValidateRange(0.1f, 5f), 0.2f, 2f)) P.LoginOverlay.bWidth = 0;
            })
            .Widget($"登入視窗按鈕間距", (x) =>
            {
                ImGuiEx.SetNextItemWidthScaled(150f);
                if(ImGuiEx.SliderFloat(x, ref C.LoginOverlayBPadding.ValidateRange(0.5f, 5f), 1f, 1.5f)) P.LoginOverlay.bWidth = 0;
            })
        .Checkbox("搜尋時顯示隱藏角色", () => ref C.LoginOverlayAllSearch)
        .SliderInt(150f, "Number of columns", () => ref C.NumLoginOverlayCols.ValidateRange(1, 10), 1, 10)
        .SliderFloat(150f, "Overlay height, %", () => ref C.LoginOverlayPercent.ValidateRange(20f, 100f), 20f, 100f);
}
