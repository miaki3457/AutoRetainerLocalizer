using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRetainer.Services;

public class PluginTerminator
{
    private PluginTerminator()
    {
    }

    public void OnUpdate()
    {
        if(BossMod.Available)
        {
            if(BossMod.Presets_GetActiveList().Count > 0)
            {
                BossMod.Presets_SetActiveList([]);
                PluginLog.Debug($"Bossmod presets shutdown");
            }
        }
    }
}
