using Celeste.Framework.Diagnostics.Commands;
using Celeste.Mods.BackgroundSwitch.Interop;
using Celeste.Mods.BackgroundSwitch.Runtime;

namespace Celeste.Mods.BackgroundSwitch.Diagnostics
{
    public static class BackgroundSwitchCommands
    {
        [SourceCommand("bg_mode", "[BGswitch] Sets the bg mode of the level")]
        public static void SetBGMode(bool bgMode = false, bool persistent = false)
            => new BackgroundSwitchExports(BgModeManager.For(SourceCommandExecutionScope.CurrentEngine)).SetBGMode(
                bgMode, persistent);
    }
}
