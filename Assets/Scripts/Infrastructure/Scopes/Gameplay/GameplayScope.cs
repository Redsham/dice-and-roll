using Gameplay.Camera.Abstractions;
using Gameplay.Camera.Runtime;
using Gameplay.Composition;
using Gameplay.Flow.Input;
using Gameplay.Flow.Loop;
using Gameplay.Flow.Scenario;
using Gameplay.Flow.Sequences;
using Gameplay.Flow.Spawning;
using Gameplay.Flow.Transitions;
using Gameplay.Flow.Turns;
using Gameplay.Levels.Runtime;
using Gameplay.Player.Runtime;
using Gameplay.World.Runtime;
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
			       .As<ICameraGridOrientation>();

			builder.Register<NavigationService>(Lifetime.Scoped).As<INavigationService>();
			builder.Register<LevelService>(Lifetime.Scoped).As<ILevelService>();
			builder.Register<DiceService>(Lifetime.Scoped).As<IPlayerService>();

			builder.Register<SingleLevelSequence>(Lifetime.Scoped).As<ILevelSequence>();
			builder.Register<NullEnemySpawner>(Lifetime.Scoped).As<IEnemySpawner>();
			builder.Register<NullLocationTransitionService>(Lifetime.Scoped).As<ILocationTransitionService>();
			builder.Register<KeyboardPlayerTurnSource>(Lifetime.Scoped).As<IPlayerTurnSource>();
			builder.Register<PlayerMovementTurn>(Lifetime.Scoped).As<IGameTurn>();
			builder.Register<DefaultGameplayLoop>(Lifetime.Scoped).As<IGameplayLoop>();
			builder.Register<DefaultGameplayScenario>(Lifetime.Scoped).As<IGameplayScenario>();

			builder.RegisterEntryPoint<GameplayCameraInput>();
			builder.RegisterEntryPoint<GameplayEntryPoint>();
		}
	}
}
