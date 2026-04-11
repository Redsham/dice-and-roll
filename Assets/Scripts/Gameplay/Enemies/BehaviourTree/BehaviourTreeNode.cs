using System.Collections.Generic;


namespace Gameplay.Enemies.BehaviourTree
{
	public abstract class BehaviourTreeNode
	{
		// === Data ===

		private readonly List<BehaviourTreeNode> m_Children = new();

		protected BehaviourTreeNode(string name)
		{
			Name = name;
		}

		public string                           Name       { get; }
		public IReadOnlyList<BehaviourTreeNode> Children   => m_Children;
		public BehaviourTreeNodeStatus          LastStatus { get; private set; }

		// === Setup ===

		protected void AddChild(BehaviourTreeNode child)
		{
			if (child != null) {
				m_Children.Add(child);
			}
		}

		// === Runtime ===

		public BehaviourTreeNodeStatus Tick(EnemyDecisionContext context)
		{
			LastStatus = OnTick(context);
			return LastStatus;
		}

		public void CollectDebug(BehaviourTreeDebugView debugView, int depth)
		{
			debugView.Add(Name, depth, LastStatus);

			for (int i = 0; i < m_Children.Count; i++) {
				m_Children[i].CollectDebug(debugView, depth + 1);
			}
		}

		protected abstract BehaviourTreeNodeStatus OnTick(EnemyDecisionContext context);
	}
}