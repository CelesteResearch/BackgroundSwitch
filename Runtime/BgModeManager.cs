using System;
using System.Collections;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Celeste.Core.Scheduling;
using Celeste.Game.Execution.Levels;
using Celeste.Mods.BackgroundSwitch.Components;
using Celeste.Mods.BackgroundSwitch.Entities;
using Celeste.Mods.BackgroundSwitch.Modules;
using Celeste.Mods.BackgroundSwitch.Triggers;
using Celeste.Physics.Collision.Shapes;
using Celeste.Simulation;
using Celeste.Simulation.Gameplay.Platforms.Foundation;

namespace Celeste.Mods.BackgroundSwitch.Runtime
{
    public sealed partial class BgModeManager : IDisposable
    {
        private static readonly ConditionalWeakTable<ISourceEngineContext, BgModeManager> Owners = new();

        public static BgModeManager For(ISourceEngineContext engine) =>
            Owners.GetValue(engine, e => new BgModeManager(e));

        public readonly ISourceEngineContext Engine;
        internal BackgroundSwitchModule Module;
        private bool bgMode;
        private SourceSolidEntityState bgSolidTiles;
        private GridColliderGeometry bgSolidTilesGrid;
        private readonly string listenerRegistryKey;

        private BgModeManager(ISourceEngineContext engine)
        {
            Engine = engine;
            listenerRegistryKey = ((Celeste.Scene.Tracking.ISourceSceneCatalogEnvironment)engine).SceneCatalog
                .Find(typeof(BgModeListener)).RegistryKey;
        }

        public bool BGMode
        {
            get => bgMode;
            set
            {
                if (bgMode == value) return;
                bgMode = value;
                if (Engine.Frame.Scenes.CurrentScene is not OriginalLevelSceneRoot level) return;
                level.Session.SetFlag("bg_mode", bgMode);
                level.SolidTiles.Collidable = !bgMode;
                bgSolidTiles.Collidable = bgMode;
                if (bgSolidTiles.Collider == null) bgSolidTiles.Collider = bgSolidTilesGrid;
                if (!bgMode) ResetDepth(level);
                foreach (var component in level.Scheduler.Components.GetTrackedComponents(listenerRegistryKey))
                    ((BgModeListener)component).OnChange(bgMode);
            }
        }

        private static void ResetDepth(OriginalLevelSceneRoot level)
        {
            var player = level.Scheduler.Entities.GetTrackedEntity<RoomPlayerSession>();
            if (player != null && player.Depth == 10001) player.Depth = 0;
        }

        public static IEnumerator FlipFlash(OriginalLevelSceneRoot level)
        {
            level.Shake(.1f);
            level.FormationBackdrop.Display = true;
            level.FormationBackdrop.Alpha = .4f;
            yield return .3f;
            level.FormationBackdrop.Display = false;
        }

        private GridColliderGeometry CreateBgtileGrid(OriginalLevelSceneRoot level)
        {
            var bounds = level.Bounds;
            var tileBounds = ((Celeste.Game.Content.Maps.OriginalChapterMapData)level.Session.MapData).TileBounds;
            var data = new bool[bounds.Width / 8, bounds.Height / 8];
            for (int i = 0; i < data.GetLength(0); i++)
            for (int j = 0; j < data.GetLength(1); j++)
                data[i, j] = level.BgData[i + bounds.Left / 8 - tileBounds.Left, j + bounds.Top / 8 - tileBounds.Top] !=
                             '0';
            return new GridColliderGeometry(8, 8, data);
        }

        private void PublishBackgroundSolid(OriginalLevelSceneRoot level)
        {
            bgSolidTilesGrid = CreateBgtileGrid(level);
            var position = new Vector2(level.Bounds.Left, level.Bounds.Top);
            bool hasEntity = level.Scheduler.Entities.Any(e => e is BGModeToggle || e is BGModeTrigger);
            bgSolidTiles =
                new SourceSolidEntityState(position, 1, 1, true, level.SolidWorldFactory.Create(position, true))
                {
                    AllowStaticMovers = false,
                    Collidable = bgMode,
                    Collider = bgMode || hasEntity ? bgSolidTilesGrid : null,
                    EnableAssistModeChecks = false
                };
            level.Scheduler.QueueSourceEntity(bgSolidTiles);
            level.Session.SetFlag("bg_mode", bgMode);
            level.SolidTiles.Collidable = !bgMode;
            if (!bgMode) ResetDepth(level);
        }
    }
}
