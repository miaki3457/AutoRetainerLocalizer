using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI;
public sealed unsafe class UserInterface : NeoUIEntry
{
    public override string Path => "用戶介面";

    public override NuiBuilder Builder => new NuiBuilder()

        .Section("用戶介面")
        .Checkbox("匿名化僱員", () => ref C.NoNames, "僱員名稱將在常規UI元素中被隱藏（調試選單和插件日誌中仍可見）。啟用此選項後，不同外掛程式介面中的角色和僱員編號可能不一致")
        .Checkbox("在僱員介面顯示快捷選單", () => ref C.UIBar)
        .Checkbox("顯示僱員詳細資訊", () => ref C.ShowAdditionalInfo, "在主介面顯示僱員裝等/獲得力/鑑別力及其當前探險名稱")
        .Widget("按下 ESC 鍵時不關閉 AutoRetainer 視窗", (x) =>
        {
            if(ImGui.Checkbox(x, ref C.IgnoreEsc)) Utils.ResetEscIgnoreByWindows();
        })
        .Checkbox("在狀態列僅顯示最高優先級圖標", () => ref C.StatusBarMSI)
        .SliderInt(120f, "Status bar icon size", () => ref C.StatusBarIconWidth, 32, 128)
        .Checkbox("遊戲啟動時開啟 AutoRetainer 視窗", () => ref C.DisplayOnStart)
        //.Checkbox("Skip item sell/trade confirmation while plugin is active", () => ref C.SkipItemConfirmations)
        .Checkbox("啟用標題介面按鈕（需重啟插件）", () => ref C.UseTitleScreenButton)
        .Checkbox("隱藏角色搜尋", () => ref C.NoCharaSearch)
        .Checkbox("不為已完成角色顯示背景閃爍", () => ref C.NoGradient)
        .Checkbox("不警告同一目錄運行多個遊戲", () => ref C.No2ndInstanceNotify, "這將使AutoRetainer在第二個遊戲客戶端中自動跳過加載，除非在主要客戶端中停用此選項")

        .Section("僱員標籤頁角色排序")
        .Checkbox("啟用", () => ref C.EnableRetainerSort)
        .TextWrapped("此排序僅影響視覺顯示順序，不會影響角色處理邏輯。")
        .Widget(() => UIUtils.DrawSortableEnumList("rorder", C.RetainersVisualOrders))

        .Section("遠航探索標籤頁角色排序")
        .Checkbox("啟用", () => ref C.EnableDeployablesSort)
        .TextWrapped("此排序僅影響視覺顯示順序，不會影響角色處理邏輯。")
        .Widget(() => UIUtils.DrawSortableEnumList("dorder", C.DeployablesVisualOrders));



}