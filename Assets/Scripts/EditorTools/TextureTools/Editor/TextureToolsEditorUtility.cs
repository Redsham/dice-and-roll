using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace EditorTools.TextureTools.Editor
{
	internal static class TextureToolsEditorUtility
	{
		private static readonly int[] MaxTextureSizes = { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384 };

		public static Texture2D CreateReadableCopy(Texture source)
		{
			if (source == null)
				return null;

			int width = Mathf.Max(1, source.width);
			int height = Mathf.Max(1, source.height);
			RenderTexture descriptor = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			RenderTexture previous = RenderTexture.active;
			Graphics.Blit(source, descriptor);
			RenderTexture.active = descriptor;

			Texture2D readable = new(width, height, TextureFormat.RGBA32, false, false);
			readable.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			readable.Apply(false, false);

			RenderTexture.active = previous;
			RenderTexture.ReleaseTemporary(descriptor);
			return readable;
		}

		public static Texture2D ResizeTexture(Texture source, int targetWidth, int targetHeight, FilterMode filterMode)
		{
			if (source == null)
				return null;

			targetWidth = Mathf.Max(1, targetWidth);
			targetHeight = Mathf.Max(1, targetHeight);

			RenderTexture descriptor = RenderTexture.GetTemporary(targetWidth, targetHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			RenderTexture previous = RenderTexture.active;
			FilterMode previousFilterMode = source.filterMode;
			source.filterMode = filterMode;
			Graphics.Blit(source, descriptor);
			source.filterMode = previousFilterMode;
			RenderTexture.active = descriptor;

			Texture2D resized = new(targetWidth, targetHeight, TextureFormat.RGBA32, false, false);
			resized.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
			resized.Apply(false, false);

			RenderTexture.active = previous;
			RenderTexture.ReleaseTemporary(descriptor);
			return resized;
		}

		public static void EnsureFolderExists(string assetFolderPath)
		{
			if (string.IsNullOrWhiteSpace(assetFolderPath))
				return;

			assetFolderPath = assetFolderPath.Replace("\\", "/");
			if (AssetDatabase.IsValidFolder(assetFolderPath))
				return;

			string[] parts = assetFolderPath.Split('/');
			string current = parts[0];
			for (int i = 1; i < parts.Length; i++)
			{
				string next = $"{current}/{parts[i]}";
				if (!AssetDatabase.IsValidFolder(next))
					AssetDatabase.CreateFolder(current, parts[i]);
				current = next;
			}
		}

		public static string CombineAssetPath(string folder, string fileName)
		{
			folder = folder.Replace("\\", "/").TrimEnd('/');
			return $"{folder}/{fileName}";
		}

		public static string SanitizeFileName(string fileName)
		{
			if (string.IsNullOrWhiteSpace(fileName))
				return "TextureAsset";

			foreach (char invalid in Path.GetInvalidFileNameChars())
				fileName = fileName.Replace(invalid, '_');

			return fileName.Trim();
		}

		public static string GetAssetPath(UnityEngine.Object asset)
		{
			return asset == null ? string.Empty : AssetDatabase.GetAssetPath(asset);
		}

		public static List<Texture2D> CollectTextures(IReadOnlyList<UnityEngine.Object> sources, bool includeSubfolders)
		{
			HashSet<string> assetPaths = new(StringComparer.OrdinalIgnoreCase);
			List<Texture2D> textures = new();

			if (sources == null)
				return textures;

			for (int i = 0; i < sources.Count; i++)
			{
				UnityEngine.Object source = sources[i];
				if (source == null)
					continue;

				string path = AssetDatabase.GetAssetPath(source);
				if (string.IsNullOrEmpty(path))
					continue;

				if (AssetDatabase.IsValidFolder(path))
				{
					string[] searchRoots = includeSubfolders ? new[] { path } : Array.Empty<string>();
					if (!includeSubfolders)
					{
						string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { path });
						for (int guidIndex = 0; guidIndex < guids.Length; guidIndex++)
						{
							string texturePath = AssetDatabase.GUIDToAssetPath(guids[guidIndex]);
							if (Path.GetDirectoryName(texturePath)?.Replace("\\", "/") != path)
								continue;
							AddTexture(texturePath, assetPaths, textures);
						}

						continue;
					}

					string[] folderGuids = AssetDatabase.FindAssets("t:Texture2D", searchRoots);
					for (int guidIndex = 0; guidIndex < folderGuids.Length; guidIndex++)
						AddTexture(AssetDatabase.GUIDToAssetPath(folderGuids[guidIndex]), assetPaths, textures);

					continue;
				}

				AddTexture(path, assetPaths, textures);
			}

			textures.Sort((left, right) => string.CompareOrdinal(left.name, right.name));
			return textures;
		}

		public static Vector2Int CalculateTargetSize(int sourceWidth, int sourceHeight, TextureResizeProfile profile)
		{
			sourceWidth = Mathf.Max(1, sourceWidth);
			sourceHeight = Mathf.Max(1, sourceHeight);

			Vector2 rawSize = profile.ScalingMode switch
			{
				TextureResizeScalingMode.Exact => new Vector2(Mathf.Max(1, profile.Width), Mathf.Max(1, profile.Height)),
				TextureResizeScalingMode.FitWithin => CalculateFitWithin(sourceWidth, sourceHeight, Mathf.Max(1, profile.Width), Mathf.Max(1, profile.Height)),
				TextureResizeScalingMode.LongSide => CalculateUniform(sourceWidth, sourceHeight, Mathf.Max(1, profile.LongSide), true),
				TextureResizeScalingMode.ShortSide => CalculateUniform(sourceWidth, sourceHeight, Mathf.Max(1, profile.ShortSide), false),
				TextureResizeScalingMode.Percent => new Vector2(
					Mathf.Max(1, Mathf.RoundToInt(sourceWidth * Mathf.Max(1, profile.Percent) / 100f)),
					Mathf.Max(1, Mathf.RoundToInt(sourceHeight * Mathf.Max(1, profile.Percent) / 100f))),
				_ => new Vector2(sourceWidth, sourceHeight)
			};

			int width = ApplyPowerOfTwo(Mathf.RoundToInt(rawSize.x), profile.PowerOfTwoMode);
			int height = ApplyPowerOfTwo(Mathf.RoundToInt(rawSize.y), profile.PowerOfTwoMode);
			return new Vector2Int(Mathf.Max(1, width), Mathf.Max(1, height));
		}

		public static int ClosestSupportedMaxSize(int value)
		{
			value = Mathf.Clamp(value, MaxTextureSizes[0], MaxTextureSizes[^1]);

			int bestValue = MaxTextureSizes[0];
			int bestDistance = int.MaxValue;
			for (int i = 0; i < MaxTextureSizes.Length; i++)
			{
				int candidate = MaxTextureSizes[i];
				int distance = Mathf.Abs(candidate - value);
				if (distance >= bestDistance)
					continue;

				bestDistance = distance;
				bestValue = candidate;
			}

			return bestValue;
		}

		private static void AddTexture(string path, ISet<string> assetPaths, ICollection<Texture2D> textures)
		{
			if (string.IsNullOrEmpty(path) || !assetPaths.Add(path))
				return;

			Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
			if (texture != null)
				textures.Add(texture);
		}

		private static Vector2 CalculateFitWithin(int sourceWidth, int sourceHeight, int maxWidth, int maxHeight)
		{
			float scale = Mathf.Min(maxWidth / (float)sourceWidth, maxHeight / (float)sourceHeight);
			scale = Mathf.Max(scale, 1f / Mathf.Max(sourceWidth, sourceHeight));
			return new Vector2(sourceWidth * scale, sourceHeight * scale);
		}

		private static Vector2 CalculateUniform(int sourceWidth, int sourceHeight, int targetSide, bool useLongSide)
		{
			int referenceSide = useLongSide ? Mathf.Max(sourceWidth, sourceHeight) : Mathf.Min(sourceWidth, sourceHeight);
			float scale = targetSide / (float)Mathf.Max(1, referenceSide);
			return new Vector2(sourceWidth * scale, sourceHeight * scale);
		}

		private static int ApplyPowerOfTwo(int value, TextureResizePowerOfTwoMode mode)
		{
			return mode switch
			{
				TextureResizePowerOfTwoMode.Nearest => Mathf.ClosestPowerOfTwo(Mathf.Max(1, value)),
				TextureResizePowerOfTwoMode.Floor => Mathf.NextPowerOfTwo(Mathf.Max(1, value)) > value
					? Mathf.NextPowerOfTwo(Mathf.Max(1, value)) / 2
					: Mathf.NextPowerOfTwo(Mathf.Max(1, value)),
				TextureResizePowerOfTwoMode.Ceil => Mathf.NextPowerOfTwo(Mathf.Max(1, value)),
				_ => Mathf.Max(1, value)
			};
		}
	}
}
