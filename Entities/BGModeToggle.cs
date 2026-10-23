using System.Numerics;
using Celeste.Core.Components;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Game.Execution.Levels;
using Celeste.Game.Execution.Processes;
using Celeste.Graphics.Animation;
using Celeste.Input.Gameplay.Feedback;
using Celeste.Mods.BackgroundSwitch.Components;
using Celeste.Mods.BackgroundSwitch.Runtime;
using Celeste.Physics.Collision.Shapes;
using Celeste.Scene.Entities;
using Celeste.Simulation;
using Celeste.Simulation.Gameplay.Player.Collision.Components;

namespace Celeste.Mods.BackgroundSwitch.Entities
{
    [CustomEntity("bgSwitch/bgModeToggle")]
    [RuntimeTypeAlias("Celeste.Mod.BGswitch.BGModeToggle")]
    public class BGModeToggle : SceneEntityState
    {
        private readonly bool onlyOn, onlyOff, persistent;
        private readonly Sprite sprite;
        private readonly BgModeManager manager;
        private float cooldownTimer;
        private bool playSounds;
        private bool Usable => (!onlyOn || !manager.BGMode) && (!onlyOff || manager.BGMode);

        public BGModeToggle(ISourceEngineContext engine, Vector2 position, bool onlyOn, bool onlyOff, bool persistent)
            : base(((ISourceEntitySourceDataEnvironment)engine).EntitySourceDataRegistration, position)
        {
            manager = BgModeManager.For(engine);
            this.onlyOn = onlyOn;
            this.onlyOff = onlyOff;
            this.persistent = persistent;
            Collider = new HitboxColliderGeometry(16, 20, -8, -8);
            Add(new BgModeListener(OnChangeMode));
            Add(new PlayerColliderComponentState(OnPlayer));
            Add(sprite = manager.Module.CreateSprite());
            Depth = 2000;
        }

        public BGModeToggle(ISourceEngineContext engine, EntityData data, Vector2 offset)
            : this(engine, data.Position + offset, data.Bool("onlyOn"), data.Bool("onlyOff"), data.Bool("persistent"))
        {
        }

        public override void Added(object scene)
        {
            base.Added(scene);
            sprite.Play(Usable ? (manager.BGMode ? "bgLoop" : "fgLoop") : (manager.BGMode ? "bgOffLoop" : "fgOffLoop"));
        }

        public override void Update()
        {
            base.Update();
            if (cooldownTimer > 0) cooldownTimer -= manager.Engine.DeltaTime;
        }

        private void OnChangeMode(bool bgMode)
        {
            var audio = ((IOriginalGameplayProcess)manager.Engine).AudioSession;
            if (playSounds)
                audio.Play(bgMode ? "event:/game/09_core/switch_to_cold" : "event:/game/09_core/switch_to_hot",
                    Position, ((IOriginalGameplayProcess)manager.Engine).SaveContext);
            sprite.Play(Usable ? (bgMode ? "bg" : "fg") : (bgMode ? "bgOff" : "fgOff"));
            if (!Usable && playSounds)
                audio.Play("event:/game/09_core/switch_dies", Position,
                    ((IOriginalGameplayProcess)manager.Engine).SaveContext);
            playSounds = false;
        }

        private void OnPlayer(RoomPlayerSession player)
        {
            if (!Usable || !(cooldownTimer <= 0)) return;
            playSounds = true;
            manager.BGMode = !manager.BGMode;
            if (persistent) manager.Module.Session.BGMode = manager.BGMode;
            ((ISourceRumbleEnvironment)manager.Engine).RumbleSession.Rumble(1, 1);
            ((ISourceFreezeEnvironment)manager.Engine).Freeze(.05f);
            Add(new SimulationCoroutine(manager.Engine, BgModeManager.FlipFlash((OriginalLevelSceneRoot)Scene)));
            cooldownTimer = 1;
        }
    }
}
