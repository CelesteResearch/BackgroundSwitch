using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using Celeste.Everest.Runtime.Levels.Update;
using Celeste.Everest.Runtime.Levels.Transitions;
using Celeste.Game.Content.Maps.Levels;
using Celeste.Game.Execution.Levels;
using Celeste.Game.Execution.Levels.Transitions;
using Celeste.Simulation;
using Celeste.Simulation.Gameplay.Player.Intro;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Mono.Cecil.Cil;

namespace Celeste.Mods.BackgroundSwitch.Runtime
{
    public sealed partial class BgModeManager
    {
        private readonly List<IDisposable> hooks = new();

        public void Load()
        {
            hooks.Add(new Hook(typeof(OriginalLevelSceneRoot).GetMethod(nameof(OriginalLevelSceneRoot.LoadLevel)),
                (Action<Action<OriginalLevelSceneRoot, PlayerIntroType, bool>, OriginalLevelSceneRoot, PlayerIntroType,
                    bool>)LoadLevel));
            hooks.Add(new Hook(typeof(RoomPlayerSession).GetMethod(nameof(RoomPlayerSession.Update)),
                (Action<Action<RoomPlayerSession>, RoomPlayerSession>)PlayerUpdate));
            ((IEverestLevelEntityLoadEnvironment)Engine).LevelUpdateEvents.OnTransitionTo += Transition;
            var type = typeof(EverestLevelTransitionPolicy).GetNestedType("TransitionRoutine", BindingFlags.NonPublic);
            var sequenceField = type.GetField("sequence", BindingFlags.NonPublic | BindingFlags.Instance);
            hooks.Add(new ILHook(type.GetMethod("MoveNext"), il =>
            {
                var cursor = new ILCursor(il);
                cursor.GotoNext(MoveType.After, i => i.MatchLdstr("Celeste"),
                    i => i.MatchCall(typeof(string), "op_Equality"));
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, sequenceField);
                cursor.Emit(OpCodes.Callvirt,
                    typeof(OriginalLevelTransitionSequence).GetProperty(nameof(OriginalLevelTransitionSequence.Level))
                        .GetMethod);
                cursor.EmitDelegate<Func<bool, OriginalLevelSceneRoot, bool>>((vanilla, level) =>
                    vanilla && (!ReferenceEquals(level.EngineContext, Engine) || !bgMode));
            }));
        }

        private void LoadLevel(Action<OriginalLevelSceneRoot, PlayerIntroType, bool> original,
            OriginalLevelSceneRoot level, PlayerIntroType intro, bool fromLoader)
        {
            if (!ReferenceEquals(level.EngineContext, Engine))
            {
                original(level, intro, fromLoader);
                return;
            }

            if (level.Session.JustStarted && level.Session.FirstLevel) Module.Session.BGMode = false;
            bgMode = Module.Session.BGMode;
            original(level, intro, fromLoader);
            PublishBackgroundSolid(level);
        }

        private void PlayerUpdate(Action<RoomPlayerSession> original, RoomPlayerSession player)
        {
            original(player);
            if (ReferenceEquals(player.EngineContext, Engine) && bgMode && player.Depth == 0) player.Depth = 10001;
        }

        private void Transition(OriginalLevelSceneRoot level, OriginalLevelRoomData next, Vector2 direction)
        {
            if (ReferenceEquals(level.EngineContext, Engine)) Module.Session.BGMode = bgMode;
        }

        public void Dispose()
        {
            foreach (var hook in hooks) hook.Dispose();
            hooks.Clear();
            ((IEverestLevelEntityLoadEnvironment)Engine).LevelUpdateEvents.OnTransitionTo -= Transition;
        }
    }
}
