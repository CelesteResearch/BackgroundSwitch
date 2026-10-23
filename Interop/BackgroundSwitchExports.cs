using System;
using Celeste.Core.Components;
using Celeste.Game.Execution.Levels;
using Celeste.Mods.BackgroundSwitch.Components;
using Celeste.Mods.BackgroundSwitch.Runtime;
using MonoMod.ModInterop;

namespace Celeste.Mods.BackgroundSwitch.Interop
{
    [ModExportName("BGswitch")]
    public sealed class BackgroundSwitchExports
    {
        private readonly BgModeManager manager;
        public BackgroundSwitchExports(BgModeManager manager) => this.manager = manager;
        public bool IsBGMode() => manager.BGMode;

        public void SetBGMode(bool bgMode = false, bool persistent = false)
        {
            if (manager.Engine.Frame.Scenes.CurrentScene is not OriginalLevelSceneRoot) return;
            manager.BGMode = bgMode;
            if (persistent) manager.Module.Session.BGMode = bgMode;
        }

        public SourceComponentState GetBGModeListener(Action<bool> action) => new BgModeListener(action);
    }
}
