using Audio.UI;
using Infrastructure.Services;
using Infrastructure.Services.Scenes;
using TriInspector;
using UI.Effects;
using UnityEngine;
using VContainer;
using VContainer.Unity;


namespace Infrastructure.Scopes.Root
{
	public class RootScope : LifetimeScope
	{
		[SerializeField, Required] private UISoundsSettings m_UISoundsSettings;

		protected override void Configure(IContainerBuilder builder)
		{
			DontDestroyOnLoad(this);

			// Register instances
			builder.RegisterInstance(m_UISoundsSettings);

			// Register services
			builder.Register<SceneService>(Lifetime.Singleton);
			builder.Register<PreferencesService>(Lifetime.Singleton);

			builder.Register<UISounds>(Lifetime.Singleton);
			builder.Register<UIFade>(Lifetime.Singleton);

			builder.Register<BootstrapService>(Lifetime.Singleton);

			// Register entry point
			builder.RegisterEntryPoint<RootEntryPoint>();
		}
	}
}