using System;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Runtime;
using Celeste.Core.Reflection.Interop;
using Celeste.Graphics.Animation;
using Celeste.Graphics.Content;
using Celeste.Mods.BackgroundSwitch.Interop;
using Celeste.Mods.BackgroundSwitch.Persistence;
using Celeste.Mods.BackgroundSwitch.Runtime;

namespace Celeste.Mods.BackgroundSwitch.Modules
{
    [RuntimeTypeAlias("Celeste.Mod.BGswitch.BGswitch")]
    public sealed class BackgroundSwitchModule : SourceEverestModule
    {
        private readonly BgModeManager manager;
        private IDisposable exports;
        public BackgroundSwitchSession Session => (BackgroundSwitchSession)_Session;
        public override Type SessionType => typeof(BackgroundSwitchSession);

        public BackgroundSwitchModule(ISourceEngineContext engine) : base(engine)
        {
            manager = BgModeManager.For(engine);
            manager.Module = this;
        }

        internal Sprite CreateSprite() =>
            ((ISourceGameGraphicsEnvironment)SourceEngine).GameGraphics.SpriteFactory.Create("BGswitch_toggle");

        public override void Load()
        {
            exports = ((ISourceModInteropEnvironment)SourceEngine).ModInterop.RegisterOwned(
                new SourceModInteropActualTypeBinding(typeof(BackgroundSwitchExports),
                    new BackgroundSwitchExports(manager)));
            manager.Load();
        }

        public override void Unload()
        {
            manager.Dispose();
            exports?.Dispose();
            exports = null;
        }
    }
}
