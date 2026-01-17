using AutoRetainer.Modules.Voyage;
using AutoRetainer.Scheduler.Tasks;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.UI.Overlays;
public unsafe class AutoBuyFuelOverlay : Window
{
    private float Height;
    private AutoBuyFuelOverlay() : base("AutoRetainer buy fuel window", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysUseWindowPadding | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoSavedSettings, true)
    {
        RespectCloseHotkey = false;
        IsOpen = true;
        P.WindowSystem.AddWindow(this);
    }

    public override void Draw()
    {
        if(TryGetAddonByName<AtkUnitBase>("部隊軍票商店", out var a) && IsAddonReady(a))
        {
            if(a->X != 0 || a->Y != 0)
            {
                Position = new(a->X, a->Y - Height);
            }
            if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.GasPump, "循環購買青磷水桶", !Utils.IsBusy)) TaskRecursivelyBuyFuel.Enqueue(true);
        }
        Height = ImGui.GetWindowSize().Y;
    }

    public override bool DrawConditions()
    {
        return VoyageUtils.Workshops.Contains(Svc.ClientState.TerritoryType) && TryGetAddonByName<AtkUnitBase>("部隊軍票商店", out var a) && IsAddonReady(a);
    }
}
