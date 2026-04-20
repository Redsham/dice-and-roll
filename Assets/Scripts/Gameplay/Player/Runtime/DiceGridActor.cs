using Gameplay.Navigation;
using UnityEngine;


namespace Gameplay.Player.Runtime
{
	public sealed class DiceGridActor : INavCellEntity
	{
		private readonly DiceService m_Owner;

		public DiceGridActor(DiceService owner)
		{
			m_Owner = owner;
		}

		public NavCellEntityLayer Layer => NavCellEntityLayer.Actor;
		public GameObject   Owner   => m_Owner.PlayerObject;
		public Vector2Int   Cell    => m_Owner.Position;
		public NavCellFlags Flags   => NavCellFlags.BlocksMovement | NavCellFlags.BlocksTrace;
		public bool         IsAlive => m_Owner.IsAlive;

		public int ApplyDamage(int damage, GameObject source = null) => m_Owner.ApplyDamage(damage, source);
	}
}
