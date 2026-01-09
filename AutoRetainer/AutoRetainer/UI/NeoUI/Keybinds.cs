namespace AutoRetainer.UI.NeoUI;
public class Keybinds : NeoUIEntry
{
    public override string Path => "快捷鍵設定";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("訪問傳喚鈴/管制面板的快捷鍵")
        .Widget("使用傳喚鈴/管制面板時，暫時防止 AutoRetainer 自動啟動", (x) =>
        {
            UIUtils.DrawKeybind(x, ref C.Suppress);
        })
        .Widget("暫時設定為僅領取模式，防止在當前循環中分派任務/暫時將潛艇模式設定為僅結算", (x) =>
        {
            UIUtils.DrawKeybind(x, ref C.TempCollectB);
        })

        .Section("僱員快速動作")
        .Widget("出售物品", (x) => UIUtils.QRA(x, ref C.SellKey))
        .Widget("存放物品", (x) => UIUtils.QRA(x, ref C.EntrustKey))
        .Widget("取回物品", (x) => UIUtils.QRA(x, ref C.RetrieveKey))
        .Widget("上架出售", (x) => UIUtils.QRA(x, ref C.SellMarketKey));
}
