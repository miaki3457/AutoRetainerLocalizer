namespace AutoRetainer.UI.NeoUI.AdvancedEntries;
public class LogTab : NeoUIEntry
{
    public override string Path => "進階設定/日誌";

    public override void Draw()
    {
        InternalLog.PrintImgui();
    }
}
