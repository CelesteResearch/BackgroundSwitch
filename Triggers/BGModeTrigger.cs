using System.Numerics;
using Celeste.Core.Components;
using Celeste.Core.Mathematics.Extrema;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Game.Execution.Levels;
using Celeste.Input.Gameplay.Feedback;
using Celeste.Mods.BackgroundSwitch.Runtime;
using Celeste.Simulation;
using Celeste.Simulation.Gameplay.Triggers.Foundation;

namespace Celeste.Mods.BackgroundSwitch.Triggers
{
    [CustomEntity("bgSwitch/bgTrigger")]
    [RuntimeTypeAlias("Celeste.Mod.BGswitch.BGModeTrigger")]
    public class BGModeTrigger : SourceTriggerEntityState
    {
        private readonly BgModeManager manager;
        private readonly bool mode, persistent, playEffects;

        public BGModeTrigger(ISourceEngineContext engine, EntityData data, Vector2 offset)
            : base(((ISourceEntitySourceDataEnvironment)engine).EntitySourceDataRegistration, data.Position + offset,
                data.Width, data.Height, ((ISourceExtremaEnvironment)engine).ExtremaArithmetic)
        {
            manager = BgModeManager.For(engine);
            mode = data.Bool("mode", true);
            persistent = data.Has("persistent") ? data.Bool("persistent") : data.Bool("persistant", true);
            playEffects = data.Bool("playEffects", true);
        }

        public override void OnEnter(RoomPlayerSession player)
        {
            if (manager.BGMode == mode) return;
            manager.BGMode = mode;
            if (persistent) manager.Module.Session.BGMode = mode;
            if (!playEffects) return;
            ((ISourceRumbleEnvironment)manager.Engine).RumbleSession.Rumble(1, 1);
            ((ISourceFreezeEnvironment)manager.Engine).Freeze(.05f);
            Add(new SimulationCoroutine(manager.Engine, BgModeManager.FlipFlash((OriginalLevelSceneRoot)Scene)));
        }
    }
}
