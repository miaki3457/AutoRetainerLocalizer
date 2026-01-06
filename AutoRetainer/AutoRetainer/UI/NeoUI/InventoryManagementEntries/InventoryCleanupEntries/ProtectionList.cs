namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.InventoryCleanupEntries;
public class ProtectionList : InventoryManagementBase
{
    public override string Name { get; } = "Inventory Cleanup/Protection List";
    private InventoryManagementCommon InventoryManagementCommon = new();
    private ProtectionList()
    {
        DisplayPriority = -1;
        Builder = InventoryCleanupCommon.CreateCleanupHeaderBuilder()
            .Section(Name)
            .TextWrapped("即使這些物品包含在其他任何清單中，AutoRetainer 也不會將其出售、分解、丟棄、或籌備給軍隊")
            .Widget(() => InventoryManagementCommon.DrawListNew(
                itemId => InventoryCleanupCommon.SelectedPlan.AddItemToList(IMListKind.Protect, itemId, out _),
                itemId => InventoryCleanupCommon.SelectedPlan.IMProtectList.Remove(itemId), InventoryCleanupCommon.SelectedPlan.IMProtectList))
            .Separator()
            .Widget(() =>
            {
                InventoryManagementCommon.ImportBlacklistFromArDiscard();
            });
    }

}