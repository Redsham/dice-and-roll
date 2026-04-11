using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace Audio.UI
{
	[CreateAssetMenu(fileName = "UISoundSettings", menuName = "Game/Audio/UI Sound Settings")]
	public class UISoundsSettings : ScriptableObject
	{
		// Sub-types

		[Serializable]
		public class UISound
		{
			[field: SerializeField]                    public UISoundsCue                Cue    { get; private set; }
			[field: SerializeField]                    public AssetReferenceT<AudioClip> Sound  { get; private set; }
			[field: SerializeField, Range(0.0f, 1.0f)] public float                      Volume { get; private set; } = 1.0f;
		}


		// Accessors

		public IReadOnlyList<UISound> Sounds => m_Sounds;
		public int                    Size   => m_Sounds.Count;
		public UIAudioSource          Source => m_Source;


		// Fields

		[SerializeField] private List<UISound> m_Sounds = new();
		[SerializeField] private UIAudioSource m_Source;
	}
}