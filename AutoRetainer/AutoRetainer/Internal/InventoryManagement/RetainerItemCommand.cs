namespace AutoRetainer.Internal.InventoryManagement;

public enum RetainerItemCommand : long
{
    RetrieveFromRetainer = 0,
    EntrustToRetainer = 1,
    RetrieveQuantity = 3,
    EntrustQuantity = 4,
    HaveRetainerSellItem = 5,
}
