using System;
using System.Collections.Generic;
using UnityEngine;


namespace EditorTools.TextureTools.Editor
{
	[CreateAssetMenu(fileName = "TextureAtlasComposerProfile", menuName = "Tools/Texture Tools/Atlas Composer Profile")]
	public sealed class TextureAtlasComposerProfile : ScriptableObject
	{
		public string OutputDirectory = "Assets/Art/UI/Atlases";
		public string OutputName = "UIAtlas";
		public int AtlasWidth = 2048;
		public int AtlasHeight = 2048;
		public int AutoLayoutPadding = 8;
		public bool AutoCreateSprites = true;
		public bool GenerateLayoutAsset = true;
		public bool OverwriteExisting = true;
		public FilterMode FilterMode = FilterMode.Bilinear;
		public TextureWrapMode WrapMode = TextureWrapMode.Clamp;
		public Color ClearColor = new(0f, 0f, 0f, 0f);
		public List<TextureAtlasSourceEntry> Entries = new();
	}

	[Serializable]
	public sealed class TextureAtlasSourceEntry
	{
		public bool Enabled = true;
		public string Id = "Texture";
		public Texture2D Texture;
		public RectInt Destination = new(0, 0, 256, 256);
		public Vector2 Pivot = new(0.5f, 0.5f);
		public float RotationDegrees;
		public int SortOrder;
		public bool PreserveAspect = true;
		public Color Tint = Color.white;

		public string DisplayName => string.IsNullOrWhiteSpace(Id) ? Texture != null ? Texture.name : "Texture" : Id;
	}
}
