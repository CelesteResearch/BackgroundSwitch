using System;
using Celeste.Core.Components;
using Celeste.Core.Reflection.Runtime;
using Celeste.Scene.Tracking.Attributes;

namespace Celeste.Mods.BackgroundSwitch.Components
{
    [SceneTracked(false)]
    [RuntimeTypeAlias("Celeste.Mod.BGswitch.BGModeListener")]
    public class BgModeListener : SourceComponentState
    {
        public Action<bool> OnChange;
        public BgModeListener(Action<bool> onChange) : base(false, false) => OnChange = onChange;
    }
}
