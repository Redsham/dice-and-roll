using System;
using System.Collections.Generic;
using UnityEngine;


namespace EditorTools.TextureTools
{
	[CreateAssetMenu(fileName = "TextureAtlasLayout", menuName = "Tools/Texture Tools/Atlas Layout")]
	public sealed class TextureAtlasLayoutAsset : ScriptableObject
	{
		[SerializeField] private Texture2D m_AtlasTexture;
		[SerializeField] private Vector2Int m_AtlasSize;
		[SerializeField] private List<TextureAtlasSpriteEntry> m_Entries = new();

		public Texture2D AtlasTexture => m_AtlasTexture;
		public Vector2Int AtlasSize => m_AtlasSize;
		public IReadOnlyList<TextureAtlasSpriteEntry> Entries => m_Entries;

		public void SetData(Texture2D atlasTexture, Vector2Int atlasSize, IReadOnlyList<TextureAtlasSpriteEntry> entries)
		{
			m_AtlasTexture = atlasTexture;
			m_AtlasSize = atlasSize;
			m_Entries = new List<TextureAtlasSpriteEntry>(entries);
		}
	}

	[Serializable]
	public struct TextureAtlasSpriteEntry
	{
		public string Id;
		public string SourceAssetPath;
		public Rect PixelRect;
		public Vector2 Pivot;
		public float RotationDegrees;
		public Vector4 UvRect;
	}
}
