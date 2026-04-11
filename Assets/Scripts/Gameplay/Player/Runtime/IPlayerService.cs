using Cysharp.Threading.Tasks;
using Gameplay.Player.Authoring;
using Gameplay.Player.Domain;
using UnityEngine;


namespace Gameplay.Player.Runtime
{
	public interface IPlayerService
	{
		bool       HasPlayer     { get; }
		bool       IsRolling     { get; }
		bool       IsAlive       { get; }
		int        CurrentHealth { get; }
		int        MaxHealth     { get; }
		DiceState  State         { get; }
		Vector2Int Position      { get; }
		GameObject PlayerObject  { get; }

		void          BindPlayer(DiceBehaviour player, Vector2Int startPosition);
		void          ClearPlayer();
		int           ApplyDamage(int            damage, GameObject source = null);
		UniTask<bool> TryRollAsync(RollDirection direction);
		UniTask<bool> TryShootAsync(Vector3      aimPoint);
	}
}