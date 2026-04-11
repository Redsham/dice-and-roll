using Gameplay.Enemies.Authoring;
using Gameplay.Enemies.BehaviourTree;
using Gameplay.Player.Domain;
using UnityEngine;


namespace Gameplay.Enemies.Runtime
{
	public static class EnemyBehaviourTreeFactory
	{
		public static BehaviourTreeRunner Create(EnemyRuntimeHandle enemy)
		{
			return enemy.Kind switch
			{
				EnemyKind.Pawn => new BehaviourTreeRunner(CreatePawnTree(enemy)),
				EnemyKind.Mortar => new BehaviourTreeRunner(CreateMortarTree(enemy)),
				_ => new BehaviourTreeRunner(new ActionNode("Idle", context => {
					context.SelectAction(EnemyTurnAction.Wait());
					return true;
				}))
			};
		}

		private static BehaviourTreeNode CreatePawnTree(EnemyRuntimeHandle enemy)
		{
			return new SelectorNode(
				"Pawn Root",
				new SequenceNode(
					"Shoot When Ready",
					new ConditionNode("In Range", context => IsPawnInRange(enemy, context)),
					new ActionNode("Shoot Or Rotate", context => {
						if (!context.TryGetPrimaryDirectionToPlayer(out RollDirection direction)) {
							return false;
						}

						context.SelectAction(enemy.State.Facing == direction
							? EnemyTurnAction.Shoot(context.PlayerService.Position)
							: EnemyTurnAction.Rotate(direction));
						return true;
					})
				),
				new ActionNode("Move Towards Player", context => {
					if (!context.TryGetPrimaryDirectionToPlayer(out RollDirection direction)) {
						context.SelectAction(EnemyTurnAction.Wait());
						return true;
					}

					context.SelectAction(enemy.State.Facing == direction
						? EnemyTurnAction.Move(direction)
						: EnemyTurnAction.Rotate(direction));
					return true;
				})
			);
		}

		private static BehaviourTreeNode CreateMortarTree(EnemyRuntimeHandle enemy)
		{
			return new SelectorNode(
				"Mortar Root",
				new SequenceNode(
					"Fire Pending Strike",
					new ConditionNode("Has Pending Strike", _ => enemy.PendingMortarCell.HasValue),
					new ConditionNode("Countdown Complete", _ => enemy.MortarTurnsUntilImpact <= 0),
					new ActionNode("Fire Strike", context => {
						context.SelectAction(EnemyTurnAction.Shoot(enemy.PendingMortarCell.GetValueOrDefault()));
						return true;
					})
				),
				new SequenceNode(
					"Acquire Strike Target",
					new ConditionNode("No Pending Strike", _ => !enemy.PendingMortarCell.HasValue),
					new ActionNode("Select Target", context => TrySelectMortarTarget(enemy, context))
				),
				new ActionNode("Retreat Or Wait", context => {
					if (TryGetDirectionAwayFromPlayer(enemy, context, out RollDirection direction)) {
						context.SelectAction(enemy.State.Facing == direction
							? EnemyTurnAction.Move(direction)
							: EnemyTurnAction.Rotate(direction));
						return true;
					}

					context.SelectAction(EnemyTurnAction.Wait());
					return true;
				})
			);
		}

		private static bool IsPawnInRange(EnemyRuntimeHandle enemy, EnemyDecisionContext context)
		{
			if (enemy.Behaviour is not PawnEnemyBehaviour pawn) {
				return false;
			}

			Vector2Int delta = context.GetDeltaToPlayer();
			if (delta.x != 0 && delta.y != 0) {
				return false;
			}

			int straightDistance = Mathf.Abs(delta.x) + Mathf.Abs(delta.y);
			return straightDistance <= pawn.Config.ShootRange && straightDistance > 0;
		}

		private static bool TrySelectMortarTarget(EnemyRuntimeHandle enemy, EnemyDecisionContext context)
		{
			if (enemy.Behaviour is not MortarEnemyBehaviour mortar) {
				return false;
			}

			int radius = Mathf.Max(1, mortar.Config.BombardmentRadius);
			for (int attempt = 0; attempt < 12; attempt++) {
				Vector2Int randomOffset = new(
					Random.Range(-radius, radius + 1),
					Random.Range(-radius, radius + 1)
				);

				if (randomOffset == Vector2Int.zero || randomOffset.magnitude > radius + 0.01f) {
					continue;
				}

				Vector2Int targetCell = context.PlayerService.Position + randomOffset;
				if (!context.NavigationService.TryGetOccupancy(targetCell, out _)) {
					continue;
				}

				enemy.ScheduleMortarStrike(targetCell, mortar.Config.BombardmentIntervalTurns);
				context.SelectAction(EnemyTurnAction.Wait());
				return true;
			}

			context.SelectAction(EnemyTurnAction.Wait());
			return true;
		}

		private static bool TryGetDirectionAwayFromPlayer(EnemyRuntimeHandle enemy, EnemyDecisionContext context, out RollDirection direction)
		{
			Vector2Int delta = enemy.State.Position - context.PlayerService.Position;
			if (delta == Vector2Int.zero) {
				direction = RollDirection.North;
				return false;
			}

			if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y)) {
				direction = delta.x >= 0 ? RollDirection.East : RollDirection.West;
				return true;
			}

			direction = delta.y >= 0 ? RollDirection.North : RollDirection.South;
			return true;
		}
	}
}
