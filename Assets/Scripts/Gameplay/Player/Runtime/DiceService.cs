using Cysharp.Threading.Tasks;
using Gameplay.Player.Authoring;
using Gameplay.Player.Domain;
using Gameplay.Player.Presentation;
using Gameplay.World.Runtime;
using UnityEngine;


namespace Gameplay.Player.Runtime
{
	public sealed class DiceService : IPlayerService
	{
		private readonly INavigationService m_NavigationService;

		private DiceBehaviour  m_Player;
		private IDiceView      m_DiceView;
		private DiceController m_Controller;

		public DiceService(INavigationService navigationService)
		{
			m_NavigationService = navigationService;
		}

		public bool HasPlayer => m_Player != null;
		public DiceState State => m_Controller.State;
		public bool IsRolling { get; private set; }

		public void BindPlayer(DiceBehaviour player, Vector2Int startPosition)
		{
			m_Player = player;
			m_DiceView = player.View;

			DiceState initialState = DiceState.Create(startPosition);
			m_Controller = new DiceController(initialState);

			m_DiceView.Initialize();
			m_DiceView.Snap(initialState, m_NavigationService.Basis);
		}

		public void ClearPlayer()
		{
			if (m_Player != null) {
				Object.Destroy(m_Player.gameObject);
			}

			m_Player = null;
			m_DiceView = null;
			m_Controller = null;
			IsRolling = false;
		}

		public async UniTask<bool> TryRollAsync(RollDirection direction)
		{
			if (IsRolling || m_Controller == null || !m_NavigationService.HasLevel) {
				return false;
			}

			DiceState nextState = m_Controller.PreviewRoll(direction);
			if (!m_NavigationService.CanOccupy(nextState.Position)) {
				return false;
			}

			IsRolling = true;

			try {
				DiceState currentState = m_Controller.State;
				DiceState appliedState = m_Controller.Roll(direction);
				await m_DiceView.PlayRollAsync(currentState, appliedState, direction, m_NavigationService.Basis);
				return true;
			}
			finally {
				IsRolling = false;
			}
		}
	}
}
