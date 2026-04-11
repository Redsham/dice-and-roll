using Cysharp.Threading.Tasks;
using Infrastructure.Services;
using VContainer.Unity;


namespace Infrastructure.Scopes.Bootstrap
{
	public class BootstrapEntryPoint : IStartable
	{
		private readonly BootstrapService m_Bootstrap;

		public BootstrapEntryPoint(BootstrapService bootstrap) => m_Bootstrap = bootstrap;

		public void Start() => m_Bootstrap.Initialize().Forget();
	}
}