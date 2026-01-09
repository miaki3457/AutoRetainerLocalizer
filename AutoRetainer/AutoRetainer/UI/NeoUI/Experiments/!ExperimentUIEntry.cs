namespace AutoRetainer.UI.NeoUI.Experiments;
public abstract class ExperimentUIEntry : NeoUIEntry
{
    public virtual string Name => GetType().Name;
    public override string Path => $"實驗性功能/{Name}";
}
