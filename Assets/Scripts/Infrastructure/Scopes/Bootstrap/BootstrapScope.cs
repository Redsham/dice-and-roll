using UI.Bootstrap;
using VContainer;
using VContainer.Unity;


namespace Infrastructure.Scopes.Bootstrap
{
    public class BootstrapScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // Register components
            builder.RegisterComponentInHierarchy<BootstrapView>();
            
            // Register entry point
            builder.RegisterEntryPoint<BootstrapEntryPoint>();
        }
    }
}
