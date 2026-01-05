using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.GCDeliveryEntries;
public sealed unsafe class GeneralSettings : InventoryManagementBase
{
    public override string Name { get; } = "大國聯防軍 - 一般設定";

    public override NuiBuilder Builder => new NuiBuilder()
        .Section("一般設定")
        .Checkbox("啟用自動籌備交換", () => ref C.AutoGCContinuation)
        .TextWrapped($"""
啟用自動籌備交換後:
- 插件會自動使用軍票交換已設定的兌換清單中的物品。
- 若清單為空，則只會交換探險幣。
- 請確認在角色設定中，\"交付模式\"未設為\"停用\"(Disabled)。

軍票兌換完成後:
- 將繼續執行籌備稀有品。
- 該流程將重複至沒有可籌備的物品或是軍票使用完畢。
            """)

        .Section("多角色模式籌備交換")
        .TextWrapped($"""
啟用後:
- 在多角色模式下，啟用傳送的角色會自動進行專家委託並根據兌換方案購買物品（前提是角色軍階足夠）。
        """)
        .Checkbox("啟用多角色籌備交換", () => ref C.FullAutoGCDelivery)
        .Checkbox("僅在工作台未鎖定時觸發", () => ref C.FullAutoGCDeliveryOnlyWsUnlocked)
        .InputInt(150f, "觸發籌備的剩餘背包格數 (小於或等於)", () => ref C.FullAutoGCDeliveryInventory, "僅計算主要背包，不包含兵裝庫")
        .Checkbox("當當探險幣耗盡時觸發", () => ref C.FullAutoGCDeliveryDeliverOnVentureExhaust, "此選項可能導致每次登入時都會前往軍隊兌換。請確保已設置足夠探險幣的方案。")
        .Indent()
        .InputInt(150f, "觸發籌備的剩餘探險幣數量 (小於或等於)", () => ref C.FullAutoGCDeliveryDeliverOnVentureLessThan)
        .Unindent()
        .Checkbox("優先使用軍票加成票券，如果可用的話", () => ref C.FullAutoGCDeliveryUseBuffItem)
        .Checkbox("優先使用部隊軍票加成BUFF，如果可用的話", () => ref C.FullAutoGCDeliveryUseBuffFCAction)
        .Checkbox("籌備交換後傳送回房屋/旅館", () => ref C.TeleportAfterGCExchange)
        .Indent()
        .Checkbox("僅在多角色模式啟動時", () => ref C.TeleportAfterGCExchangeMulti)
        .Unindent()
        ;
}