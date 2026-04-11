using System.Collections.Generic;
using Gameplay.Navigation;
using Gameplay.Nodes.Authoring;
using Gameplay.Nodes.Contracts;
using Gameplay.Nodes.Models;
using UnityEngine;


namespace Gameplay.Nodes.Runtime
{
	public sealed class LevelNodeService : ILevelNodeService
	{
		private readonly Dictionary<Vector2Int, NodeBehaviour> m_Nodes = new();

		private NavGrid m_CurrentGrid;

		public void BindLevel(NavGrid navGrid, NodeBehaviour[] nodes)
		{
			m_CurrentGrid = navGrid;
			m_Nodes.Clear();

			for (int i = 0; i < nodes.Length; i++) {
				NodeBehaviour node = nodes[i];
				node.ResetRuntimeState();
				m_Nodes[node.GridPosition] = node;
			}
		}

		public void ClearLevel()
		{
			m_CurrentGrid = null;
			m_Nodes.Clear();
		}

		public bool TryGetNode(Vector2Int cell, out NodeBehaviour node)
		{
			return m_Nodes.TryGetValue(cell, out node);
		}

		public NodeProjectileImpactInfo PreviewProjectileImpact(Vector2Int cell, int incomingDamage, out NavCellOccupancy occupancy)
		{
			if (m_CurrentGrid == null || !m_CurrentGrid.TryGetOccupancy(cell, out occupancy)) {
				occupancy = NavCellOccupancy.Empty;
				return default;
			}

			if (!m_Nodes.TryGetValue(cell, out NodeBehaviour node) || node is not INodeProjectileImpactHandler projectileImpactHandler) {
				return new(
				           consumedDamage: occupancy.StopsProjectileImmediately ? incomingDamage : 0,
				           stopsProjectile: occupancy.StopsProjectileImmediately,
				           canApplyDamage: false
				          );
			}

			NodeProjectileImpactInfo impactInfo = projectileImpactHandler.PreviewProjectileImpact(incomingDamage);
			return new(
			           impactInfo.ConsumedDamage,
			           impactInfo.StopsProjectile || occupancy.StopsProjectileImmediately,
			           impactInfo.CanApplyDamage
			          );
		}

		public void NotifyActorEntered(Vector2Int cell, GameObject actor)
		{
			if (!m_Nodes.TryGetValue(cell, out NodeBehaviour node)) {
				return;
			}

			if (node is INodeActorEnterHandler handler) {
				handler.OnActorEnter(new(actor, cell));
				SyncNode(cell, node);
			}
		}

		public void NotifyActorLeft(Vector2Int cell, GameObject actor)
		{
			if (!m_Nodes.TryGetValue(cell, out NodeBehaviour node)) {
				return;
			}

			if (node is INodeActorLeaveHandler handler) {
				handler.OnActorLeave(new(actor, cell));
				SyncNode(cell, node);
			}
		}

		public int ApplyDamage(Vector2Int cell, int damage, GameObject source = null)
		{
			if (damage <= 0 || !m_Nodes.TryGetValue(cell, out NodeBehaviour node) || node is not INodeDamageHandler damageHandler) {
				return 0;
			}

			NodeDamageResult result = damageHandler.ApplyDamage(new(source, cell, damage));
			SyncNode(cell, node);
			return result.ConsumedDamage;
		}

		private void SyncNode(Vector2Int cell, NodeBehaviour node)
		{
			if (m_CurrentGrid == null) {
				return;
			}

			NavCellOccupancy occupancy = node.CreateOccupancy();
			m_CurrentGrid.TrySetOccupancy(cell, occupancy);
		}
	}
}