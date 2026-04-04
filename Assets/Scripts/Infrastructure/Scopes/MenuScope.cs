using UI.Menu;
using VContainer;
using VContainer.Unity;


namespace Infrastructure.Scopes
{
	public class MenuScope : LifetimeScope
	{
		protected override void Configure(IContainerBuilder builder)
		{
			builder.RegisterComponentInHierarchy<MainMenuView>();
		}
	}
}