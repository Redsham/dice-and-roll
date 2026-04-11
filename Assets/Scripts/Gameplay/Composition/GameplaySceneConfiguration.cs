using Gameplay.Levels.Data;
using Gameplay.Player.Authoring;
using TriInspector;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Gameplay.Composition
{
	public sealed class GameplaySceneConfiguration : MonoBehaviour
	{
		[field: SerializeField, Required] public  LevelAsset       InitialLevel { get; private set; }
		[field: SerializeField, Required] public  DiceBehaviour    PlayerPrefab { get; private set; }
		[field: SerializeField, Required] public  InputActionAsset InputActions { get; private set; }
		[SerializeField]                  private Transform        m_LevelParent;
		[SerializeField]                  private Transform        m_ActorParent;

		public Transform LevelParent => m_LevelParent != null ? m_LevelParent : transform;
		public Transform ActorParent => m_ActorParent != null ? m_ActorParent : transform;

		private void Reset()
		{
			m_LevelParent = transform;
			m_ActorParent = transform;
		}
	}
}