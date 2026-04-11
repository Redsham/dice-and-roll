using Gameplay.Navigation.Pathfinding;
using Gameplay.Navigation.Pathfinding.Providers;
using UnityEngine;
using Utilities;


namespace Gameplay.Navigation
{
	public class NavGrid : MonoBehaviour
	{
		// === Inspector ===

		[SerializeField] private int  m_Width  = 15;
		[SerializeField] private int  m_Height = 15;
		[SerializeField] private bool m_AllowCornerCutting;

		// === Grid ===

		public FlatArray2D<NavNode> Nodes;

		public int  Width              => m_Width;
		public int  Height             => m_Height;
		public int  NodeCount          => m_Width * m_Height;
		public bool AllowCornerCutting => m_AllowCornerCutting;

		// === Runtime ===

		private NavGridPathfinder m_Pathfinder;

		// === Unity ===

		private void Awake()
		{
			RebuildGrid();
		}

		private void OnValidate()
		{
			m_Width  = Mathf.Max(1, m_Width);
			m_Height = Mathf.Max(1, m_Height);
		}

		// === Setup ===

		public void RebuildGrid()
		{
			Nodes.Resize(m_Width, m_Height);

			for (int i = 0; i < Nodes.Data.Length; i++) {
				Nodes[i] = new(i, true);
			}

			EnsureRuntimeState();
		}

		public Vector3 GetCellWorldCorner(int x, int y)
		{
			return transform.position + transform.right * x + transform.forward * y;
		}

		public Vector3 GetCellWorldCenter(int x, int y)
		{
			return GetCellWorldCorner(x, y) + (transform.right + transform.forward) * 0.5f;
		}

		// === Queries ===

		public bool IsInBounds(int        x, int y) => x >= 0 && x < m_Width && y >= 0 && y < m_Height;
		public bool IsInBounds(Vector2Int coordinates) => IsInBounds(coordinates.x, coordinates.y);

		public int        ToIndex(int        x, int y) => x + y * m_Width;
		public int        ToIndex(Vector2Int coordinates) => ToIndex(coordinates.x, coordinates.y);
		public Vector2Int ToCoordinates(int  index)       => new(index % m_Width, index / m_Width);

		public bool TrySetWalkable(int index, bool isWalkable)
		{
			EnsureReady();

			if (!IsValidIndex(index)) {
				return false;
			}

			Nodes[index].IsWalkable = isWalkable;
			return true;
		}

		public bool TrySetWalkable(Vector2Int coordinates, bool isWalkable)
		{
			EnsureReady();
			return IsInBounds(coordinates) && TrySetWalkable(ToIndex(coordinates), isWalkable);
		}

		public void ClearOccupancy()
		{
			EnsureReady();

			for (int i = 0; i < Nodes.Data.Length; i++) {
				Nodes[i].Occupancy = NavCellOccupancy.Empty;
			}
		}

		public bool TrySetOccupancy(int index, NavCellOccupancy occupancy)
		{
			EnsureReady();

			if (!IsValidIndex(index)) {
				return false;
			}

			Nodes[index].Occupancy = occupancy;
			return true;
		}

		public bool TrySetOccupancy(Vector2Int coordinates, NavCellOccupancy occupancy)
		{
			EnsureReady();
			return IsInBounds(coordinates) && TrySetOccupancy(ToIndex(coordinates), occupancy);
		}

		public bool TryGetOccupancy(Vector2Int coordinates, out NavCellOccupancy occupancy)
		{
			EnsureReady();
			if (!IsInBounds(coordinates)) {
				occupancy = NavCellOccupancy.Empty;
				return false;
			}

			occupancy = Nodes[ToIndex(coordinates)].Occupancy;
			return true;
		}

		public bool TryFindPath(Vector2Int start, Vector2Int goal, int[] pathBuffer, out NavPathResult result)
		{
			NavDefaultTraversalCostProvider weights = default;
			return TryFindPath(start, goal, ref weights, pathBuffer, out result);
		}

		public bool TryFindPath(Vector2Int start, Vector2Int goal, Vector2Int[] pathBuffer, out NavPathResult result)
		{
			NavDefaultTraversalCostProvider weights = default;
			return TryFindPath(start, goal, ref weights, pathBuffer, out result);
		}

		public bool TryFindPath(int startIndex, int goalIndex, int[] pathBuffer, out NavPathResult result)
		{
			NavDefaultTraversalCostProvider weights = default;
			return TryFindPath(startIndex, goalIndex, ref weights, pathBuffer, out result);
		}

		public bool TryFindPath<TWeightProvider>(
			Vector2Int          start,
			Vector2Int          goal,
			ref TWeightProvider weightProvider,
			int[]               pathBuffer,
			out NavPathResult   result
		)
			where TWeightProvider : struct, INavTraversalCostProvider
		{
			EnsureReady();

			if (!IsInBounds(start)) {
				result = new(NavPathStatus.InvalidStart, 0, 0, 0);
				return false;
			}

			if (!IsInBounds(goal)) {
				result = new(NavPathStatus.InvalidGoal, 0, 0, 0);
				return false;
			}

			return TryFindPath(ToIndex(start), ToIndex(goal), ref weightProvider, pathBuffer, out result);
		}

		public bool TryFindPath<TWeightProvider>(
			Vector2Int          start,
			Vector2Int          goal,
			ref TWeightProvider weightProvider,
			Vector2Int[]        pathBuffer,
			out NavPathResult   result
		)
			where TWeightProvider : struct, INavTraversalCostProvider
		{
			int[] indexPathBuffer = pathBuffer == null ? null : new int[pathBuffer.Length];
			bool  hasPath         = TryFindPath(start, goal, ref weightProvider, indexPathBuffer, out result);
			if (!hasPath) {
				return false;
			}

			for (int i = 0; i < result.PathLength; i++) {
				pathBuffer[i] = ToCoordinates(indexPathBuffer[i]);
			}

			return true;
		}

		public bool TryFindPath<TWeightProvider>(
			int                 startIndex,
			int                 goalIndex,
			ref TWeightProvider weightProvider,
			int[]               pathBuffer,
			out NavPathResult   result
		)
			where TWeightProvider : struct, INavTraversalCostProvider
		{
			EnsureReady();
			NavOrthogonalConnectionProvider orthogonalConnections = default;
			return m_Pathfinder.TryFindPath(this, startIndex, goalIndex, ref orthogonalConnections, ref weightProvider, pathBuffer, out result);
		}

		public bool TryFindPath<TConnectionProvider, TWeightProvider>(
			Vector2Int              start,
			Vector2Int              goal,
			ref TConnectionProvider connectionProvider,
			ref TWeightProvider     weightProvider,
			int[]                   pathBuffer,
			out NavPathResult       result
		)
			where TConnectionProvider : struct, INavConnectionProvider
			where TWeightProvider : struct, INavTraversalCostProvider
		{
			EnsureReady();

			if (!IsInBounds(start)) {
				result = new(NavPathStatus.InvalidStart, 0, 0, 0);
				return false;
			}

			if (!IsInBounds(goal)) {
				result = new(NavPathStatus.InvalidGoal, 0, 0, 0);
				return false;
			}

			return TryFindPath(ToIndex(start), ToIndex(goal), ref connectionProvider, ref weightProvider, pathBuffer, out result);
		}

		public bool TryFindPath<TConnectionProvider, TWeightProvider>(
			int                     startIndex,
			int                     goalIndex,
			ref TConnectionProvider connectionProvider,
			ref TWeightProvider     weightProvider,
			int[]                   pathBuffer,
			out NavPathResult       result
		)
			where TConnectionProvider : struct, INavConnectionProvider
			where TWeightProvider : struct, INavTraversalCostProvider
		{
			EnsureReady();
			return m_Pathfinder.TryFindPath(this, startIndex, goalIndex, ref connectionProvider, ref weightProvider, pathBuffer, out result);
		}

		// === Runtime ===

		private void EnsureReady()
		{
			if (Nodes.Data == null || Nodes.Data.Length != NodeCount) {
				RebuildGrid();
				return;
			}

			EnsureRuntimeState();
		}

		private void EnsureRuntimeState()
		{
			m_Pathfinder ??= new();
			m_Pathfinder.EnsureCapacity(NodeCount, NAV_DIAGONAL_CONNECTION_PROVIDER_MAX_COUNT);
		}

		private bool IsValidIndex(int index)
		{
			return index >= 0 && index < NodeCount;
		}

		private const int NAV_DIAGONAL_CONNECTION_PROVIDER_MAX_COUNT = 8;
	}
}
