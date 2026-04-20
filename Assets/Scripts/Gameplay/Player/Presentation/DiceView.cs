using Cysharp.Threading.Tasks;
using Gameplay.Navigation.Tracing;
using Gameplay.Player.Domain;
using Gameplay.Player.Domain.Combat;
using Gameplay.Player.Presentation.Combat;
using Gameplay.Player.Runtime;
using Gameplay.World.Runtime;
using TriInspector;
using UnityEngine;
using VContainer;


namespace Gameplay.Player.Presentation
{
	public class DiceView : MonoBehaviour
	{
		#region Fields

		private readonly DiceRotator m_Rotator = new();

		// === References ===
		[Title("References")]
		[field: SerializeField, Required] public Transform Model { get;         private set; }
		[field: SerializeField, Required] public DiceShotBurstView Burst { get; private set; }
		[field: SerializeField, Required] public DiceAudio         Audio { get; private set; }

		[SerializeField] private DiceShotDirectionView m_ShotDirectionView;

		private DiceState m_CurrentState;
		private GridBasis m_GridBasis;
		private int       m_ShootRange;
		private bool      m_IsInitialized;
		private bool      m_SuppressShotPreview;

		[Inject] private readonly DiceShotAimService m_ShotAimService;

		#endregion

		// === Lifecycle ===

		public void Initialize(int shootRange)
		{
			m_ShootRange = shootRange;

			m_Rotator.Initialize(transform, Model);
			Burst.Initialize(this);

			m_IsInitialized = true;
		}
		private void Update()
		{
			if (!m_IsInitialized || m_SuppressShotPreview) {
				return;
			}

			RefreshShotDirectionPreview(true);
		}
		
		// === Roll & Snap ===
		
		public async UniTask PlayRollAsync(DiceState fromState, DiceState toState, RollDirection direction, GridBasis gridBasis)
		{
			SetShotPreviewVisible(false);
			Audio?.PlayRoll();
			await m_Rotator.RollAsync(fromState, toState, direction, gridBasis);

			m_SuppressShotPreview = false;
			Snap(toState, gridBasis);
		}
		public void Snap(DiceState state, GridBasis gridBasis)
		{
			m_CurrentState = state;
			m_GridBasis    = gridBasis;

			transform.SetPositionAndRotation(gridBasis.GetCellCenter(state.Position) + gridBasis.Up * 0.5f, gridBasis.ToWorldRotation(Quaternion.identity));
			Transform modelTransform = Model != null ? Model : transform;
			modelTransform.SetLocalPositionAndRotation(Vector3.zero, state.Orientation.GetRotation());
			RefreshShotDirectionPreview(false);
		}


		// === Shot Preview ===

		public void HideShotPreview() => SetShotPreviewVisible(false);
		public void ShowShotPreview()
		{
			m_SuppressShotPreview = false;
			RefreshShotDirectionPreview(false);
		}

		private void RefreshShotDirectionPreview(bool animateOnDirectionChange)
		{
			if (m_ShotDirectionView == null) return;

			if (!m_ShotAimService.TryResolvePointerShotTrace(m_CurrentState, m_ShootRange, out DiceShotDefinition shot, out GridTraceResult traceResult)) {
				m_ShotDirectionView.Hide();
				return;
			}

			int visibleDistance = Mathf.Max(0, traceResult.ExitedBounds ? traceResult.Distance - 1 : traceResult.Distance);
			if (visibleDistance <= 0) {
				m_ShotDirectionView.Hide();
				return;
			}

			int damage = m_CurrentState.Orientation.GetFaceValue(shot.Face);
			m_ShotDirectionView.Show(shot.Direction, visibleDistance, damage, m_GridBasis.CellSize, animateOnDirectionChange);
		}
		private void SetShotPreviewVisible(bool visible)
		{
			m_SuppressShotPreview = !visible;

			if (visible) {
				RefreshShotDirectionPreview(false);
				return;
			}

			m_ShotDirectionView?.Hide();
		}
	}
}