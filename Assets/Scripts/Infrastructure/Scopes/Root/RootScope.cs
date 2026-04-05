using Infrastructure.Services;
using Infrastructure.Services.Scenes;
using UI.Effects;
using VContainer;
using VContainer.Unity;


namespace Infrastructure.Scopes.Root
{
	public class RootScope : LifetimeScope
	{
		protected override void Configure(IContainerBuilder builder)
		{
			DontDestroyOnLoad(this);

			// Register services
			builder.Register<SceneService>(Lifetime.Singleton);
			builder.Register<PreferencesService>(Lifetime.Singleton);
			builder.Register<UIFade>(Lifetime.Singleton);
			builder.Register<BootstrapService>(Lifetime.Singleton);

			// Register entry point
			builder.RegisterEntryPoint<RootEntryPoint>();
		}
	}
}