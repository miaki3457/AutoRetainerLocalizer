using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRetainer.UI.NeoUI.MultiModeEntries;

public class MultiModeDisableRender : NeoUIEntry
{
    public override string Path => "多角色模式/關閉畫面渲染";

    public override NuiBuilder Builder => new NuiBuilder()
        .Section("關閉畫面渲染")
        .Checkbox("多角色模式下關閉畫面渲染", () => ref C.MultiDisableRender, "多角色模式下不再渲染遊戲世界，以降低資源占用")
        .Checkbox("只在夜間模式啟用", () => ref C.MultiDisableRenderNightModeOnly)
        .Checkbox("僅在遊戲視窗非作用中時", () => ref C.MultiDisableRenderOnlyInactive);
}
