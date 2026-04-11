using System;
using Gameplay.Navigation.Pathfinding.Providers;


namespace Gameplay.Navigation.Pathfinding
{
	public sealed class NavGridPathfinder
	{
		// === Runtime State ===

		private int[]           m_OpenHeap;
		private int[]           m_HeapPositions;
		private int[]           m_SearchMarks;
		private int[]           m_OpenMarks;
		private int[]           m_ClosedMarks;
		private int[]           m_Parents;
		private int[]           m_GScores;
		private int[]           m_HScores;
		private NavConnection[] m_ConnectionBuffer;

		private int m_OpenCount;
		private int m_SearchVersion;

		// === Setup ===

		public void EnsureCapacity(int nodeCount, int maxConnectionCount)
		{
			EnsureBuffer(ref m_OpenHeap,         nodeCount);
			EnsureBuffer(ref m_HeapPositions,    nodeCount, -1);
			EnsureBuffer(ref m_SearchMarks,      nodeCount);
			EnsureBuffer(ref m_OpenMarks,        nodeCount);
			EnsureBuffer(ref m_ClosedMarks,      nodeCount);
			EnsureBuffer(ref m_Parents,          nodeCount, -1);
			EnsureBuffer(ref m_GScores,          nodeCount);
			EnsureBuffer(ref m_HScores,          nodeCount);
			EnsureBuffer(ref m_ConnectionBuffer, maxConnectionCount);
		}

		// === Search ===

		public bool TryFindPath<TConnectionProvider, TWeightProvider>(
			NavGrid                 grid,
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
			EnsureConnectionBufferCapacity(connectionProvider.MaxConnectionCount);

			if (!IsValidIndex(grid, startIndex)) {
				result = new(NavPathStatus.InvalidStart, 0, 0, 0);
				return false;
			}

			if (!IsValidIndex(grid, goalIndex)) {
				result = new(NavPathStatus.InvalidGoal, 0, 0, 0);
				return false;
			}

			if (!grid.Nodes[startIndex].IsWalkable) {
				result = new(NavPathStatus.StartBlocked, 0, 0, 0);
				return false;
			}

			if (!grid.Nodes[goalIndex].IsWalkable) {
				result = new(NavPathStatus.GoalBlocked, 0, 0, 0);
				return false;
			}

			if (startIndex == goalIndex) {
				return TryWriteTrivialPath(startIndex, pathBuffer, out result);
			}

			int searchVersion = BeginSearch();
			m_OpenCount = 0;

			m_SearchMarks[startIndex] = searchVersion;
			m_Parents[startIndex]     = -1;
			m_GScores[startIndex]     = 0;
			m_HScores[startIndex]     = connectionProvider.EstimateCost(grid, startIndex, goalIndex);

			PushOpen(startIndex, searchVersion);

			while (m_OpenCount > 0) {
				int currentIndex = PopOpen();
				if (currentIndex == goalIndex) {
					return TryBuildPath(startIndex, goalIndex, pathBuffer, out result);
				}

				m_ClosedMarks[currentIndex] = searchVersion;
				ExpandNeighbors(grid, currentIndex, goalIndex, searchVersion, ref connectionProvider, ref weightProvider);
			}

			result = new(NavPathStatus.NoPath, 0, 0, 0);
			return false;
		}

		private void ExpandNeighbors<TConnectionProvider, TWeightProvider>(
			NavGrid                 grid,
			int                     currentIndex,
			int                     goalIndex,
			int                     searchVersion,
			ref TConnectionProvider connectionProvider,
			ref TWeightProvider     weightProvider
		)
			where TConnectionProvider : struct, INavConnectionProvider
			where TWeightProvider : struct, INavTraversalCostProvider
		{
			int connectionCount = connectionProvider.Collect(grid, currentIndex, m_ConnectionBuffer);
			for (int i = 0; i < connectionCount; i++) {
				NavConnection connection = m_ConnectionBuffer[i];
				TryRelaxEdge(grid, currentIndex, connection.TargetIndex, goalIndex, connection.BaseCost, searchVersion, ref connectionProvider, ref weightProvider);
			}
		}

		private void TryRelaxEdge<TConnectionProvider, TWeightProvider>(
			NavGrid                 grid,
			int                     fromIndex,
			int                     toIndex,
			int                     goalIndex,
			int                     baseCost,
			int                     searchVersion,
			ref TConnectionProvider connectionProvider,
			ref TWeightProvider     weightProvider
		)
			where TConnectionProvider : struct, INavConnectionProvider
			where TWeightProvider : struct, INavTraversalCostProvider
		{
			if (m_ClosedMarks[toIndex] == searchVersion) {
				return;
			}

			int previousIndex = m_Parents[fromIndex];
			if (!weightProvider.TryGetTraversalCost(grid, previousIndex, fromIndex, toIndex, baseCost, out int traversalCost)) {
				return;
			}

			int  candidateCost = m_GScores[fromIndex] + traversalCost;
			bool isKnownNode   = m_SearchMarks[toIndex] == searchVersion;

			if (isKnownNode && candidateCost >= m_GScores[toIndex]) {
				return;
			}

			m_SearchMarks[toIndex] = searchVersion;
			m_Parents[toIndex]     = fromIndex;
			m_GScores[toIndex]     = candidateCost;

			if (!isKnownNode) {
				m_HScores[toIndex] = connectionProvider.EstimateCost(grid, toIndex, goalIndex);
			}

			if (m_OpenMarks[toIndex] == searchVersion) {
				SiftUp(m_HeapPositions[toIndex]);
				return;
			}

			PushOpen(toIndex, searchVersion);
		}

		// === Path Output ===

		private bool TryWriteTrivialPath(int nodeIndex, int[] pathBuffer, out NavPathResult result)
		{
			if (pathBuffer == null || pathBuffer.Length == 0) {
				result = new(NavPathStatus.BufferTooSmall, 0, 0, 1);
				return false;
			}

			pathBuffer[0] = nodeIndex;
			result        = new(NavPathStatus.Found, 1, 0, 1);
			return true;
		}

		private bool TryBuildPath(int startIndex, int goalIndex, int[] pathBuffer, out NavPathResult result)
		{
			int pathLength   = 1;
			int currentIndex = goalIndex;

			while (currentIndex != startIndex) {
				currentIndex = m_Parents[currentIndex];
				pathLength++;
			}

			if (pathBuffer == null || pathBuffer.Length < pathLength) {
				result = new(NavPathStatus.BufferTooSmall, 0, m_GScores[goalIndex], pathLength);
				return false;
			}

			currentIndex = goalIndex;
			for (int writeIndex = pathLength - 1; writeIndex >= 0; writeIndex--) {
				pathBuffer[writeIndex] = currentIndex;
				currentIndex           = writeIndex > 0 ? m_Parents[currentIndex] : currentIndex;
			}

			result = new(NavPathStatus.Found, pathLength, m_GScores[goalIndex], pathLength);
			return true;
		}

		// === Heap ===

		private void PushOpen(int nodeIndex, int searchVersion)
		{
			int heapIndex = m_OpenCount++;
			m_OpenHeap[heapIndex]      = nodeIndex;
			m_HeapPositions[nodeIndex] = heapIndex;
			m_OpenMarks[nodeIndex]     = searchVersion;

			SiftUp(heapIndex);
		}

		private int PopOpen()
		{
			int rootIndex = m_OpenHeap[0];
			int lastIndex = --m_OpenCount;

			m_OpenMarks[rootIndex]     = 0;
			m_HeapPositions[rootIndex] = -1;

			if (lastIndex > 0) {
				int tailNode = m_OpenHeap[lastIndex];
				m_OpenHeap[0]             = tailNode;
				m_HeapPositions[tailNode] = 0;
				SiftDown(0);
			}

			return rootIndex;
		}

		private void SiftUp(int heapIndex)
		{
			while (heapIndex > 0) {
				int parentIndex = heapIndex - 1 >> 1;
				if (!IsHigherPriority(m_OpenHeap[heapIndex], m_OpenHeap[parentIndex])) {
					break;
				}

				SwapHeapEntries(heapIndex, parentIndex);
				heapIndex = parentIndex;
			}
		}

		private void SiftDown(int heapIndex)
		{
			while (true) {
				int leftChild = (heapIndex << 1) + 1;
				if (leftChild >= m_OpenCount) {
					return;
				}

				int rightChild = leftChild + 1;
				int bestChild  = leftChild;

				if (rightChild < m_OpenCount && IsHigherPriority(m_OpenHeap[rightChild], m_OpenHeap[leftChild])) {
					bestChild = rightChild;
				}

				if (!IsHigherPriority(m_OpenHeap[bestChild], m_OpenHeap[heapIndex])) {
					return;
				}

				SwapHeapEntries(heapIndex, bestChild);
				heapIndex = bestChild;
			}
		}

		private bool IsHigherPriority(int leftNode, int rightNode)
		{
			int leftScore  = m_GScores[leftNode]  + m_HScores[leftNode];
			int rightScore = m_GScores[rightNode] + m_HScores[rightNode];

			if (leftScore != rightScore) {
				return leftScore < rightScore;
			}

			if (m_HScores[leftNode] != m_HScores[rightNode]) {
				return m_HScores[leftNode] < m_HScores[rightNode];
			}

			return leftNode < rightNode;
		}

		private void SwapHeapEntries(int firstIndex, int secondIndex)
		{
			int firstNode  = m_OpenHeap[firstIndex];
			int secondNode = m_OpenHeap[secondIndex];

			m_OpenHeap[firstIndex]  = secondNode;
			m_OpenHeap[secondIndex] = firstNode;

			m_HeapPositions[firstNode]  = secondIndex;
			m_HeapPositions[secondNode] = firstIndex;
		}

		// === Runtime ===

		private int BeginSearch()
		{
			if (m_SearchVersion == int.MaxValue) {
				Array.Clear(m_SearchMarks, 0, m_SearchMarks.Length);
				Array.Clear(m_OpenMarks,   0, m_OpenMarks.Length);
				Array.Clear(m_ClosedMarks, 0, m_ClosedMarks.Length);

				for (int i = 0; i < m_HeapPositions.Length; i++) {
					m_HeapPositions[i] = -1;
				}

				m_SearchVersion = 1;
				return m_SearchVersion;
			}

			return ++m_SearchVersion;
		}

		private static bool IsValidIndex(NavGrid grid, int index)
		{
			return index >= 0 && index < grid.NodeCount;
		}

		private static void EnsureBuffer(ref int[] buffer, int size, int fillValue = 0)
		{
			if (buffer != null && buffer.Length == size) {
				return;
			}

			buffer = new int[size];
			if (fillValue == 0) {
				return;
			}

			for (int i = 0; i < buffer.Length; i++) {
				buffer[i] = fillValue;
			}
		}

		private static void EnsureBuffer(ref NavConnection[] buffer, int size)
		{
			if (buffer != null && buffer.Length >= size) {
				return;
			}

			buffer = new NavConnection[size];
		}

		private void EnsureConnectionBufferCapacity(int maxConnectionCount)
		{
			EnsureBuffer(ref m_ConnectionBuffer, maxConnectionCount);
		}
	}
}
