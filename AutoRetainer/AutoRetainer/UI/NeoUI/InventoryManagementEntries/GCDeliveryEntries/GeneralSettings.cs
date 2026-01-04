using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.GCDeliveryEntries;
public sealed unsafe class GeneralSettings : InventoryManagementBase
{
    public override string Name { get; } = "Grand Company Delivery/General Settings";

    public override NuiBuilder Builder => new NuiBuilder()
        .Section("一般設定")
        .Checkbox("啟用自動籌備交換", () => ref C.AutoGCContinuation)
        .TextWrapped($"""
            When Expert Delivery Continuation is enabled:
            - The plugin will automatically spend available Grand Company Seals to purchase items from the configured Exchange List.
            - If the Exchange List is empty, only Ventures will be purchased.
            - Make sure that "Delivery Mode" is not set to "Disabled" in "Character Configuration" section

            After seals have been spent:
            - Expert Delivery will resume automatically.
            - The process will repeat until there are no eligible items left to deliver or no seals remaining.
            """)

        .Section("多角色模式籌備交換")
        .TextWrapped($"""
        When enabled:
        - Characters with teleportation enabled will automatically deliver items for expert delivery and buy items according to exchange plan, if their rank is sufficient, during multi mode.
        """)
        .Checkbox("啟用多角色籌備交換", () => ref C.FullAutoGCDelivery)
        .Checkbox("僅在工作台未鎖定時觸發", () => ref C.FullAutoGCDeliveryOnlyWsUnlocked)
        .InputInt(150f, "Inventory slots remaining to trigger delivery, less or equal", () => ref C.FullAutoGCDeliveryInventory, "Only primary inventory is accounted for, not armory")
        .Checkbox("當當探險幣耗盡時觸發", () => ref C.FullAutoGCDeliveryDeliverOnVentureExhaust, "此選項可能導致每次登入時都會前往軍隊兌換。請確保已設置足夠探險幣的方案。")
        .Indent()
        .InputInt(150f, "Ventures remaining to trigger delivery, less or equal", () => ref C.FullAutoGCDeliveryDeliverOnVentureLessThan)
        .Unindent()
        .Checkbox("優先使用軍票加成票券，如果可用的話", () => ref C.FullAutoGCDeliveryUseBuffItem)
        .Checkbox("優先使用部隊軍票加成BUFF，如果可用的話", () => ref C.FullAutoGCDeliveryUseBuffFCAction)
        .Checkbox("籌備交換後傳送回房屋/旅館", () => ref C.TeleportAfterGCExchange)
        .Indent()
        .Checkbox("僅在多角色模式啟動時", () => ref C.TeleportAfterGCExchangeMulti)
        .Unindent()
        ;
}