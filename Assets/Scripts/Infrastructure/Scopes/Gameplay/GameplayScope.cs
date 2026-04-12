using Gameplay.Actors.Runtime;
using Gameplay.Camera.Abstractions;
using Gameplay.Camera.Runtime;
using Gameplay.Composition;
using Gameplay.Enemies.Runtime;
using Gameplay.Flow.GameState;
using Gameplay.Flow.Input;
using Gameplay.Flow.Loop;
using Gameplay.Flow.Scenario;
using Gameplay.Flow.Sequences;
using Gameplay.Flow.Spawning;
using Gameplay.Flow.Spawning.Runtime;
using Gameplay.Flow.Transitions;
using Gameplay.Flow.Turns;
using Gameplay.Flow.Turns.Enemies;
using Gameplay.Levels.Runtime;
using Gameplay.Navigation;
using Gameplay.Nodes.Runtime;
using Gameplay.Player.Runtime;
using Gameplay.World.Runtime;
using UI.Gameplay;
using VContainer;
using VContainer.Unity;


namespace Infrastructure.Scopes.Gameplay
{
	public sealed class GameplayScope : LifetimeScope
	{
		protected override void Configure(IContainerBuilder builder)
		{
			builder.RegisterComponentInHierarchy<GameplaySceneConfiguration>();
			builder.RegisterComponentInHierarchy<DynamicCameraRig>()
			       .As<IGameCameraController>()
			       .As<ICameraGridOrientation>()
			       .As<ICameraScreenProjector>();

			builder.Register<CombatResolverService>(Lifetime.Scoped).As<ICombatResolverService>();
			builder.Register<GameplayStateService>(Lifetime.Scoped).As<IGameplayStateService>();
			builder.Register<NavigationService>(Lifetime.Scoped).As<INavigationService>().As<INavEntityService>();
			builder.Register<LevelNodeService>(Lifetime.Scoped).As<ILevelNodeService>();
			builder.Register<LevelService>(Lifetime.Scoped).As<ILevelService>();
			builder.Register<DiceService>(Lifetime.Scoped).As<IPlayerService>();
			builder.Register<EnemyService>(Lifetime.Scoped).As<IEnemyService>();

			builder.Register<SingleLevelSequence>(Lifetime.Scoped).As<ILevelSequence>();
			builder.Register<RandomEnemySpawner>(Lifetime.Scoped).As<IEnemySpawner>();
			builder.Register<NullLocationTransitionService>(Lifetime.Scoped).As<ILocationTransitionService>();
			builder.Register<KeyboardPlayerTurnSource>(Lifetime.Scoped).As<IPlayerTurnSource>();
			builder.Register<PlayerCommandTurn>(Lifetime.Scoped).As<IGameTurn>();
			builder.Register<EnemyTurnExecutor>(Lifetime.Scoped).As<IEnemyTurnExecutor>();
			builder.Register<DefaultGameplayLoop>(Lifetime.Scoped).As<IGameplayLoop>();
			builder.Register<DefaultGameplayScenario>(Lifetime.Scoped).As<IGameplayScenario>();

			builder.RegisterEntryPoint<GameplayCameraInput>();
			builder.RegisterEntryPoint<GameplayHudEntryPoint>();
			builder.RegisterEntryPoint<GameplayEntryPoint>();
		}
	}
}
