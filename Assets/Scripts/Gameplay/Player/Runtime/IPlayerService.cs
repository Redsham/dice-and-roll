using Cysharp.Threading.Tasks;
using Gameplay.Player.Authoring;
using Gameplay.Player.Domain;
using UnityEngine;


namespace Gameplay.Player.Runtime
{
	public interface IPlayerService
	{
		bool      HasPlayer { get; }
		bool      IsRolling { get; }
		DiceState State     { get; }

		void BindPlayer(DiceBehaviour player, Vector2Int startPosition);
		void ClearPlayer();
		UniTask<bool> TryRollAsync(RollDirection direction);
	}
}
