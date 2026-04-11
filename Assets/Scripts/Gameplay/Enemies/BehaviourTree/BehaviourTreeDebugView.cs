using System.Collections.Generic;


namespace Gameplay.Enemies.BehaviourTree
{
	public readonly struct BehaviourTreeDebugLine
	{
		public string Name { get; }
		public int Depth { get; }
		public BehaviourTreeNodeStatus Status { get; }

		public BehaviourTreeDebugLine(string name, int depth, BehaviourTreeNodeStatus status)
		{
			Name = name;
			Depth = depth;
			Status = status;
		}
	}

	public sealed class BehaviourTreeDebugView
	{
		private readonly List<BehaviourTreeDebugLine> m_Lines = new();
		public IReadOnlyList<BehaviourTreeDebugLine> Lines => m_Lines;

		public void Clear()
		{
			m_Lines.Clear();
		}

		public void Add(string name, int depth, BehaviourTreeNodeStatus status)
		{
			m_Lines.Add(new BehaviourTreeDebugLine(name, depth, status));
		}
	}
}
