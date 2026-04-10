namespace Gameplay.Navigation.Pathfinding.Providers
{
	public interface INavConnectionProvider
	{
		int MaxConnectionCount { get; }
		int EstimateCost(in NavGrid grid, int fromIndex, int             goalIndex);
		int Collect(in      NavGrid grid, int nodeIndex, NavConnection[] buffer);
	}
}