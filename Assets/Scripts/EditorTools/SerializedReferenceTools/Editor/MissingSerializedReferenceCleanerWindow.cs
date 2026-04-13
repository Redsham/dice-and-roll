using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace EditorTools.SerializedReferenceTools.Editor
{
	public sealed class MissingSerializedReferenceCleanerWindow : EditorWindow
	{
		private static readonly Regex ManagedReferenceOverrideRegex = new Regex(
			@"propertyPath:\s*'?\s*managedReferences\[(?<rid>-?\d+)\]\s*'?\s*\r?\n\s*value:\s*(?<type>[^\r\n]+)",
			RegexOptions.Compiled);

		private static readonly Regex ManagedReferenceRegistryRegex = new Regex(
			@"-\s+rid:\s*(?<rid>\d+)\s*\r?\n\s*type:\s*\{class:\s*(?<class>[^,}]+),\s*ns:\s*(?<ns>[^,}]*),\s*asm:\s*(?<asm>[^}]+)\}",
			RegexOptions.Compiled);

		private Vector2 m_Scroll;
		private string m_Status = "Ready.";
		private List<ScanHit> m_LastScanResults = new List<ScanHit>();

		[MenuItem("Tools/Serialization/Clean Missing Serialized References")]
		public static void Open()
		{
			MissingSerializedReferenceCleanerWindow window = GetWindow<MissingSerializedReferenceCleanerWindow>();
			window.titleContent = new GUIContent("SR Cleaner");
			window.minSize = new Vector2(760f, 420f);
			window.Show();
		}

		private void OnGUI()
		{
			EditorGUILayout.LabelField("Missing SerializedReference Cleaner", EditorStyles.boldLabel);
			EditorGUILayout.HelpBox(
				"Инструмент ищет битые managed references в YAML сцен и префабов и удаляет их из файла. Если сцена сейчас открыта, cleaner её пропустит.",
				MessageType.Info);

			using (new EditorGUILayout.HorizontalScope()) {
				if (GUILayout.Button("Scan Selection", GUILayout.Height(28f))) {
					m_LastScanResults = ScanAssets(CollectSelectionAssetPaths());
				}

				if (GUILayout.Button("Clean Selection", GUILayout.Height(28f))) {
					CleanAssets(CollectSelectionAssetPaths());
				}
			}

			using (new EditorGUILayout.HorizontalScope()) {
				if (GUILayout.Button("Scan Project", GUILayout.Height(28f))) {
					m_LastScanResults = ScanAssets(CollectProjectAssetPaths());
				}

				if (GUILayout.Button("Clean Project", GUILayout.Height(28f))) {
					CleanAssets(CollectProjectAssetPaths());
				}
			}

			EditorGUILayout.Space(8f);
			EditorGUILayout.HelpBox(m_Status, MessageType.None);

			EditorGUILayout.Space(8f);
			EditorGUILayout.LabelField("Found", EditorStyles.boldLabel);
			m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);

			if (m_LastScanResults.Count == 0) {
				EditorGUILayout.LabelField("No broken managed references found.", EditorStyles.miniLabel);
			}
			else {
				foreach (IGrouping<string, ScanHit> group in m_LastScanResults.GroupBy(x => x.AssetPath).OrderBy(x => x.Key)) {
					EditorGUILayout.LabelField(group.Key, EditorStyles.boldLabel);

					foreach (ScanHit hit in group.OrderBy(x => x.ReferenceId)) {
						EditorGUILayout.LabelField("RID " + hit.ReferenceId + " | " + hit.TypeName, EditorStyles.miniLabel);
					}

					EditorGUILayout.Space(4f);
				}
			}

			EditorGUILayout.EndScrollView();
		}

		private void CleanAssets(IReadOnlyList<string> assetPaths)
		{
			List<ScanHit> hits = ScanAssets(assetPaths);
			m_LastScanResults = hits;

			if (hits.Count == 0) {
				m_Status = "Nothing to clean.";
				return;
			}

			int changedAssets = 0;
			int removedEntries = 0;
			int skippedLoadedScenes = 0;

			try {
				foreach (IGrouping<string, ScanHit> group in hits.GroupBy(x => x.AssetPath)) {
					string assetPath = group.Key;
					if (IsLoadedScene(assetPath)) {
						skippedLoadedScenes++;
						continue;
					}

					if (!TryCleanAssetYaml(assetPath, group.ToList(), out int removedInAsset)) {
						continue;
					}

					changedAssets++;
					removedEntries += removedInAsset;
				}
			}
			finally {
				AssetDatabase.Refresh();
			}

			m_LastScanResults = ScanAssets(assetPaths);
			m_Status = "Cleaned " + removedEntries + " entries in " + changedAssets + " assets."
				+ (skippedLoadedScenes > 0 ? " Skipped open scenes: " + skippedLoadedScenes + "." : string.Empty)
				+ " Remaining: " + m_LastScanResults.Count + ".";
		}

		private List<ScanHit> ScanAssets(IReadOnlyList<string> assetPaths)
		{
			List<ScanHit> hits = new List<ScanHit>();

			for (int i = 0; i < assetPaths.Count; i++) {
				string assetPath = assetPaths[i];
				EditorUtility.DisplayProgressBar("Scanning", assetPath, (float)i / Mathf.Max(1, assetPaths.Count));

				if (!File.Exists(assetPath)) {
					continue;
				}

				string text;
				try {
					text = File.ReadAllText(assetPath);
				}
				catch (Exception exception) {
					Debug.LogWarning("[MissingSerializedReferenceCleaner] Failed to read '" + assetPath + "': " + exception.Message);
					continue;
				}

				foreach (Match match in ManagedReferenceOverrideRegex.Matches(text)) {
					long rid;
					if (!long.TryParse(match.Groups["rid"].Value, out rid) || rid < 0) {
						continue;
					}

					string typeName = match.Groups["type"].Value.Trim();
					if (ResolveManagedReferenceType(typeName) != null) {
						continue;
					}

					hits.Add(new ScanHit(assetPath, rid, typeName));
				}

				foreach (Match match in ManagedReferenceRegistryRegex.Matches(text)) {
					long rid;
					if (!long.TryParse(match.Groups["rid"].Value, out rid) || rid < 0) {
						continue;
					}

					string className = match.Groups["class"].Value.Trim();
					string namespaceName = match.Groups["ns"].Value.Trim();
					string assemblyName = match.Groups["asm"].Value.Trim();
					string fullTypeName = string.IsNullOrEmpty(namespaceName) ? className : namespaceName + "." + className;

					if (ResolveManagedReferenceType(fullTypeName, assemblyName) != null) {
						continue;
					}

					hits.Add(new ScanHit(assetPath, rid, assemblyName + " " + fullTypeName));
				}
			}

			EditorUtility.ClearProgressBar();
			m_Status = hits.Count == 0
				? "Scan complete. Nothing broken found."
				: "Scan complete. Found " + hits.Count + " broken entries in " + hits.Select(x => x.AssetPath).Distinct().Count() + " assets.";
			return hits;
		}

		private static bool TryCleanAssetYaml(string assetPath, IReadOnlyList<ScanHit> hits, out int removedEntries)
		{
			removedEntries = 0;

			if (hits == null || hits.Count == 0 || !File.Exists(assetPath)) {
				return false;
			}

			string[] lines;
			try {
				lines = File.ReadAllLines(assetPath);
			}
			catch (Exception exception) {
				Debug.LogWarning("[MissingSerializedReferenceCleaner] Failed to read YAML from '" + assetPath + "': " + exception.Message);
				return false;
			}

			HashSet<long> brokenIds = new HashSet<long>(hits.Select(x => x.ReferenceId));
			List<string> rewritten = RewriteYaml(lines, brokenIds, ref removedEntries);
			if (removedEntries == 0) {
				return false;
			}

			try {
				File.WriteAllLines(assetPath, rewritten.ToArray());
				return true;
			}
			catch (Exception exception) {
				Debug.LogWarning("[MissingSerializedReferenceCleaner] Failed to write YAML to '" + assetPath + "': " + exception.Message);
				removedEntries = 0;
				return false;
			}
		}

		private static List<string> RewriteYaml(IReadOnlyList<string> lines, HashSet<long> brokenIds, ref int removedEntries)
		{
			List<string> result = new List<string>(lines.Count);
			bool insideRefIds = false;
			int refIdsIndent = -1;

			for (int i = 0; i < lines.Count; i++) {
				string line = lines[i];
				string trimmed = line.Trim();
				int indent = GetIndentWidth(line);

				if (trimmed == "RefIds:") {
					insideRefIds = true;
					refIdsIndent = indent;
					result.Add(line);
					continue;
				}

				if (insideRefIds && !string.IsNullOrWhiteSpace(trimmed) && indent <= refIdsIndent && !trimmed.StartsWith("- rid:", StringComparison.Ordinal)) {
					insideRefIds = false;
				}

				if (insideRefIds) {
					long rid;
					if (TryParseRidLine(trimmed, out rid) && brokenIds.Contains(rid)) {
						removedEntries++;
						i = SkipRefIdsBlock(lines, i + 1, indent) - 1;
						continue;
					}
				}

				if (trimmed.StartsWith("- target:", StringComparison.Ordinal)) {
					List<string> block = new List<string>();
					int blockIndent = indent;
					int blockStart = i;

					while (i < lines.Count) {
						string currentLine = lines[i];
						if (i > blockStart && currentLine.TrimStart().StartsWith("- target:", StringComparison.Ordinal) && GetIndentWidth(currentLine) == blockIndent) {
							i--;
							break;
						}

						block.Add(currentLine);
						i++;
					}

					if (ShouldRemovePropertyBlock(block, brokenIds)) {
						removedEntries++;
						continue;
					}

					result.AddRange(block);
					continue;
				}

				long inlineRid;
				if (TryParseRidLine(trimmed, out inlineRid) && brokenIds.Contains(inlineRid)) {
					result.Add(BuildRidLine(line, 0));
					removedEntries++;
					continue;
				}

				result.Add(line);
			}

			return result;
		}

		private static bool ShouldRemovePropertyBlock(IReadOnlyList<string> block, HashSet<long> brokenIds)
		{
			string propertyPath = null;
			string value = null;

			for (int i = 0; i < block.Count; i++) {
				string trimmed = block[i].Trim();
				if (trimmed.StartsWith("propertyPath:", StringComparison.Ordinal)) {
					propertyPath = trimmed.Substring("propertyPath:".Length).Trim().Trim('\'');
				}
				else if (trimmed.StartsWith("value:", StringComparison.Ordinal)) {
					value = trimmed.Substring("value:".Length).Trim();
				}
			}

			if (string.IsNullOrEmpty(propertyPath)) {
				return false;
			}

			foreach (long brokenId in brokenIds) {
				string rid = brokenId.ToString();

				if (propertyPath == "managedReferences[" + rid + "]") {
					return true;
				}

				if (propertyPath.StartsWith("managedReferences[" + rid + "].", StringComparison.Ordinal)) {
					return true;
				}

				if (value == rid && propertyPath.Contains(".Array.data[")) {
					return true;
				}
			}

			return false;
		}

		private static int SkipRefIdsBlock(IReadOnlyList<string> lines, int startIndex, int ridIndent)
		{
			for (int i = startIndex; i < lines.Count; i++) {
				string line = lines[i];
				string trimmed = line.Trim();
				if (string.IsNullOrWhiteSpace(trimmed)) {
					continue;
				}

				int indent = GetIndentWidth(line);
				if (indent == ridIndent && trimmed.StartsWith("- rid:", StringComparison.Ordinal)) {
					return i;
				}

				if (indent < ridIndent) {
					return i;
				}
			}

			return lines.Count;
		}

		private static bool TryParseRidLine(string trimmedLine, out long rid)
		{
			rid = 0;
			if (!trimmedLine.StartsWith("- rid:", StringComparison.Ordinal)) {
				return false;
			}

			return long.TryParse(trimmedLine.Substring("- rid:".Length).Trim(), out rid);
		}

		private static string BuildRidLine(string originalLine, long rid)
		{
			return new string(' ', GetIndentWidth(originalLine)) + "- rid: " + rid;
		}

		private static int GetIndentWidth(string line)
		{
			int indent = 0;
			while (indent < line.Length && line[indent] == ' ') {
				indent++;
			}

			return indent;
		}

		private static bool IsLoadedScene(string assetPath)
		{
			if (!assetPath.EndsWith(".unity", StringComparison.OrdinalIgnoreCase)) {
				return false;
			}

			Scene scene = SceneManager.GetSceneByPath(assetPath);
			return scene.IsValid() && scene.isLoaded;
		}

		private static List<string> CollectProjectAssetPaths()
		{
			HashSet<string> result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			foreach (string guid in AssetDatabase.FindAssets("t:Scene")) {
				result.Add(AssetDatabase.GUIDToAssetPath(guid));
			}

			foreach (string guid in AssetDatabase.FindAssets("t:Prefab")) {
				result.Add(AssetDatabase.GUIDToAssetPath(guid));
			}

			return result.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
		}

		private static List<string> CollectSelectionAssetPaths()
		{
			HashSet<string> result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			foreach (string guid in Selection.assetGUIDs) {
				string path = AssetDatabase.GUIDToAssetPath(guid);
				if (string.IsNullOrEmpty(path)) {
					continue;
				}

				if (AssetDatabase.IsValidFolder(path)) {
					foreach (string folderGuid in AssetDatabase.FindAssets("t:Scene", new[] { path })) {
						result.Add(AssetDatabase.GUIDToAssetPath(folderGuid));
					}

					foreach (string folderGuid in AssetDatabase.FindAssets("t:Prefab", new[] { path })) {
						result.Add(AssetDatabase.GUIDToAssetPath(folderGuid));
					}

					continue;
				}

				if (path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)) {
					result.Add(path);
				}
			}

			return result.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
		}

		private static Type ResolveManagedReferenceType(string typeDescriptor)
		{
			int separatorIndex = typeDescriptor.IndexOf(' ');
			if (separatorIndex <= 0 || separatorIndex >= typeDescriptor.Length - 1) {
				return null;
			}

			string assemblyName = typeDescriptor.Substring(0, separatorIndex).Trim();
			string fullTypeName = typeDescriptor.Substring(separatorIndex + 1).Trim();
			return ResolveManagedReferenceType(fullTypeName, assemblyName);
		}

		private static Type ResolveManagedReferenceType(string fullTypeName, string assemblyName)
		{
			if (string.IsNullOrWhiteSpace(fullTypeName) || string.IsNullOrWhiteSpace(assemblyName)) {
				return null;
			}

			Type directType = Type.GetType(fullTypeName + ", " + assemblyName, false);
			if (directType != null) {
				return directType;
			}

			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
				if (!string.Equals(assembly.GetName().Name, assemblyName, StringComparison.Ordinal)) {
					continue;
				}

				Type resolvedType = assembly.GetType(fullTypeName, false);
				if (resolvedType != null) {
					return resolvedType;
				}
			}

			return null;
		}

		private struct ScanHit
		{
			public readonly string AssetPath;
			public readonly long ReferenceId;
			public readonly string TypeName;

			public ScanHit(string assetPath, long referenceId, string typeName)
			{
				AssetPath = assetPath;
				ReferenceId = referenceId;
				TypeName = typeName;
			}
		}
	}
}
