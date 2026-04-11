using System.Collections.Generic;
using UnityEngine;


namespace EditorTools.TextureTools.Editor
{
	[CreateAssetMenu(fileName = "TextureResizeProfile", menuName = "Tools/Texture Tools/Resize Profile")]
	public sealed class TextureResizeProfile : ScriptableObject
	{
		public TextureResizeOperationMode OperationMode = TextureResizeOperationMode.ImportMaxSize;
		public TextureResizeScalingMode ScalingMode = TextureResizeScalingMode.FitWithin;
		public TextureResizePowerOfTwoMode PowerOfTwoMode = TextureResizePowerOfTwoMode.None;
		public string OutputDirectory = "Assets/Art/UI/Resized";
		public bool IncludeSubfolders = true;
		public bool OverwriteExisting = false;
		public bool PreserveTextureType = true;
		public int Width = 1024;
		public int Height = 1024;
		public int LongSide = 1024;
		public int ShortSide = 512;
		public int Percent = 50;
		public List<Object> Sources = new();
	}

	public enum TextureResizeOperationMode
	{
		ImportMaxSize = 0,
		BakeCopies = 1
	}

	public enum TextureResizeScalingMode
	{
		Exact = 0,
		FitWithin = 1,
		LongSide = 2,
		ShortSide = 3,
		Percent = 4
	}

	public enum TextureResizePowerOfTwoMode
	{
		None = 0,
		Nearest = 1,
		Floor = 2,
		Ceil = 3
	}
}
