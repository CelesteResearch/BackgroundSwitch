using Celeste.Core.Reflection.Runtime;
using Celeste.Everest.Modules.Persistence;

namespace Celeste.Mods.BackgroundSwitch.Persistence
{
    [RuntimeTypeAlias("Celeste.Mod.BGswitch.BGswitchSession")]
    public class BackgroundSwitchSession : EverestModuleSession
    {
        public bool BGMode;
    }
}
