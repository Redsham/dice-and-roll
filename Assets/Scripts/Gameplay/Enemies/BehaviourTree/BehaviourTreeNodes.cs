using System;
using Gameplay.Enemies.Runtime;


namespace Gameplay.Enemies.BehaviourTree
{
	public sealed class SequenceNode : BehaviourTreeNode
	{
		public SequenceNode(string name, params BehaviourTreeNode[] children) : base(name)
		{
			for (int i = 0; i < children.Length; i++) {
				AddChild(children[i]);
			}
		}

		protected override BehaviourTreeNodeStatus OnTick(EnemyDecisionContext context)
		{
			for (int i = 0; i < Children.Count; i++) {
				BehaviourTreeNodeStatus status = Children[i].Tick(context);
				if (status != BehaviourTreeNodeStatus.Success) {
					return status;
				}
			}

			return BehaviourTreeNodeStatus.Success;
		}
	}

	public sealed class SelectorNode : BehaviourTreeNode
	{
		public SelectorNode(string name, params BehaviourTreeNode[] children) : base(name)
		{
			for (int i = 0; i < children.Length; i++) {
				AddChild(children[i]);
			}
		}

		protected override BehaviourTreeNodeStatus OnTick(EnemyDecisionContext context)
		{
			for (int i = 0; i < Children.Count; i++) {
				BehaviourTreeNodeStatus status = Children[i].Tick(context);
				if (status == BehaviourTreeNodeStatus.Success) {
					return status;
				}
			}

			return BehaviourTreeNodeStatus.Failure;
		}
	}

	public sealed class ConditionNode : BehaviourTreeNode
	{
		private readonly Func<EnemyDecisionContext, bool> m_Condition;

		public ConditionNode(string name, Func<EnemyDecisionContext, bool> condition) : base(name)
		{
			m_Condition = condition;
		}

		protected override BehaviourTreeNodeStatus OnTick(EnemyDecisionContext context)
		{
			return m_Condition(context) ? BehaviourTreeNodeStatus.Success : BehaviourTreeNodeStatus.Failure;
		}
	}

	public sealed class ActionNode : BehaviourTreeNode
	{
		private readonly Func<EnemyDecisionContext, bool> m_Action;

		public ActionNode(string name, Func<EnemyDecisionContext, bool> action) : base(name)
		{
			m_Action = action;
		}

		protected override BehaviourTreeNodeStatus OnTick(EnemyDecisionContext context)
		{
			return m_Action(context) ? BehaviourTreeNodeStatus.Success : BehaviourTreeNodeStatus.Failure;
		}
	}

	public sealed class BehaviourTreeRunner
	{
		private readonly BehaviourTreeNode      m_Root;
		private readonly BehaviourTreeDebugView m_DebugView = new();

		public BehaviourTreeRunner(BehaviourTreeNode root)
		{
			m_Root = root;
		}

		public BehaviourTreeDebugView DebugView => m_DebugView;

		public EnemyTurnAction Evaluate(EnemyDecisionContext context)
		{
			context.ResetAction();
			m_Root.Tick(context);
			m_DebugView.Clear();
			m_Root.CollectDebug(m_DebugView, 0);
			return context.SelectedAction;
		}
	}
}
