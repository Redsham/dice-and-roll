using UnityEngine;


namespace Gameplay.Navigation
{
	public interface INavCellEntity
	{
		GameObject   Owner   { get; }
		Vector2Int   Cell    { get; }
		NavCellFlags Flags   { get; }
		bool         IsAlive { get; }

		int ApplyDamage(int damage, GameObject source = null);
	}

	public interface INavEntityService
	{
		bool TryGetEntity(Vector2Int coordinates, out INavCellEntity entity);
		bool TrySetEntity(Vector2Int coordinates, INavCellEntity entity);
		bool TryClearEntity(Vector2Int coordinates, INavCellEntity expectedEntity = null);
		bool TryMoveEntity(INavCellEntity entity, Vector2Int from, Vector2Int to);
	}
}
