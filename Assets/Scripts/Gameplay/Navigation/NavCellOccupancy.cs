namespace Gameplay.Navigation
{
	public struct NavCellOccupancy
	{
		public NavCellOccupancyType Type;

		public bool BlocksMovement => Type is NavCellOccupancyType.StaticProp or NavCellOccupancyType.DestructibleProp or NavCellOccupancyType.Actor;
		public bool CanReceiveDamage => Type is NavCellOccupancyType.DestructibleProp or NavCellOccupancyType.DecorativeDestructibleProp or NavCellOccupancyType.Actor;
		public bool StopsProjectileImmediately => Type == NavCellOccupancyType.StaticProp;

		public static NavCellOccupancy Empty => new() {
			Type = NavCellOccupancyType.Empty
		};
	}
}
