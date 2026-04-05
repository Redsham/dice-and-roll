using Settings.Views;
using VContainer;
using VContainer.Unity;


namespace Infrastructure.Scopes.Settings
{
	public class SettingsScope : LifetimeScope
	{
		protected override void Configure(IContainerBuilder builder)
		{
			// Register views
			builder.RegisterComponentInHierarchy<SettingsView>();
			
			// Register entry point
			builder.RegisterEntryPoint<SettingsEntryPoint>();
		}
	}
}