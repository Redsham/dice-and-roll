using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorTools.SceneSwitcher.Editor
{
	[EditorToolbarElement(ElementId, typeof(SceneView))]
	internal sealed class SceneSwitcherDropdown : EditorToolbarDropdown
	{
		public const string ElementId = "DiceAndRoll/Scene Switcher";

		public SceneSwitcherDropdown()
		{
			text = BuildLabel();
			tooltip = "Quick scene switcher";
			clicked += ShowScenePopup;

			EditorSceneManager.activeSceneChangedInEditMode += OnActiveSceneChanged;
			EditorApplication.projectChanged += Refresh;
		}

		private void ShowScenePopup()
		{
			SceneSwitcherPopupWindow.ShowFor(this);
		}

		private void OnActiveSceneChanged(UnityEngine.SceneManagement.Scene _, UnityEngine.SceneManagement.Scene __)
		{
			Refresh();
		}

		private void Refresh()
		{
			text = BuildLabel();
		}

		private static string BuildLabel()
		{
			SceneAssetInfo activeScene = SceneCatalog.TryGetActiveScene();
			if (activeScene == null) {
				return "Scene";
			}

			if (string.Equals(activeScene.GroupPath, "Scenes", StringComparison.Ordinal)) {
				return activeScene.DisplayName;
			}

			return $"{activeScene.GroupSegments[^1]} / {activeScene.DisplayName}";
		}
	}

	[Overlay(typeof(SceneView), "Scene Switcher", true)]
	[Icon("Packages/com.unity.collab-proxy/Editor/PlasticSCM/Assets/icon-menu.png")]
	internal sealed class SceneSwitcherOverlay : ToolbarOverlay
	{
		public SceneSwitcherOverlay() : base(SceneSwitcherDropdown.ElementId)
		{
		}
	}

	internal sealed class SceneSwitcherPopupWindow : EditorWindow
	{
		private const float WindowWidth = 360f;
		private const float WindowHeight = 420f;

		private readonly List<SceneCatalogEntry> visibleEntries = new List<SceneCatalogEntry>();
		private ListView listView;
		private ToolbarSearchField searchField;
		private string searchTerm = string.Empty;

		public static void ShowFor(VisualElement anchor)
		{
			SceneSwitcherPopupWindow window = CreateInstance<SceneSwitcherPopupWindow>();
			window.titleContent = new GUIContent("Scene Switcher");
			window.ShowAsDropDown(BuildAnchorRect(anchor), new Vector2(WindowWidth, WindowHeight));
		}

		private static Rect BuildAnchorRect(VisualElement anchor)
		{
			Rect bound = anchor.worldBound;
			Vector2 screenPosition = anchor.panel?.contextType == ContextType.Editor
				? GUIUtility.GUIToScreenPoint(new Vector2(bound.xMin, bound.yMax))
				: new Vector2(bound.xMin, bound.yMax);

			return new Rect(screenPosition.x, screenPosition.y, Mathf.Max(bound.width, 220f), 18f);
		}

		private void CreateGUI()
		{
			rootVisualElement.style.flexDirection = FlexDirection.Column;
			rootVisualElement.style.paddingLeft = 6f;
			rootVisualElement.style.paddingRight = 6f;
			rootVisualElement.style.paddingTop = 6f;
			rootVisualElement.style.paddingBottom = 6f;

			searchField = new ToolbarSearchField();
			searchField.RegisterValueChangedCallback(OnSearchChanged);
			rootVisualElement.Add(searchField);

			listView = new ListView();
			listView.style.flexGrow = 1f;
			listView.selectionType = SelectionType.Single;
			listView.fixedItemHeight = 22f;
			listView.makeItem = MakeItem;
			listView.bindItem = BindItem;
			listView.selectionChanged += OnSelectionChanged;
			rootVisualElement.Add(listView);

			RefreshEntries();
			searchField.Focus();
		}

		private static VisualElement MakeItem()
		{
			Label label = new Label();
			label.style.unityTextAlign = TextAnchor.MiddleLeft;
			label.style.paddingLeft = 6f;
			label.style.paddingRight = 6f;
			return label;
		}

		private void BindItem(VisualElement element, int index)
		{
			Label label = (Label)element;
			SceneCatalogEntry entry = visibleEntries[index];
			label.text = entry.IsScene
				? $"{new string(' ', Mathf.Max(0, entry.Depth) * 2)}{entry.Scene.DisplayName}"
				: $"{new string(' ', Mathf.Max(0, entry.Depth) * 2)}{entry.Name}";
			label.style.unityFontStyleAndWeight = entry.IsScene ? FontStyle.Normal : FontStyle.Bold;
		}

		private void OnSearchChanged(ChangeEvent<string> evt)
		{
			searchTerm = evt.newValue ?? string.Empty;
			RefreshEntries();
		}

		private void RefreshEntries()
		{
			if (listView == null) {
				return;
			}

			visibleEntries.Clear();
			SceneCatalog.PopulateFlatList(visibleEntries, searchTerm);
			listView.itemsSource = visibleEntries;
			listView.Rebuild();
		}

		private void OnSelectionChanged(IEnumerable<object> selection)
		{
			foreach (object item in selection) {
				if (item is not SceneCatalogEntry entry || !entry.IsScene) {
					continue;
				}

				if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
					listView.ClearSelection();
					return;
				}

				EditorSceneManager.OpenScene(entry.Scene.AssetPath, OpenSceneMode.Single);
				Close();
				return;
			}
		}
	}

	internal sealed class SceneCatalogEntry
	{
		public SceneCatalogEntry(string name, int depth, SceneAssetInfo scene = null)
		{
			Name = name;
			Depth = depth;
			Scene = scene;
		}

		public string Name { get; }
		public int Depth { get; }
		public SceneAssetInfo Scene { get; }
		public bool IsScene => Scene != null;
	}

	internal sealed class SceneTreeNode
	{
		public SceneTreeNode(string name, SceneAssetInfo scene = null)
		{
			Name = name;
			Scene = scene;
			Children = new List<SceneTreeNode>();
		}

		public string Name { get; }
		public SceneAssetInfo Scene { get; }
		public List<SceneTreeNode> Children { get; }
	}

	internal sealed class SceneAssetInfo
	{
		public SceneAssetInfo(string assetPath)
		{
			AssetPath = assetPath.Replace('\\', '/');
			DisplayName = System.IO.Path.GetFileNameWithoutExtension(AssetPath);

			string directory = System.IO.Path.GetDirectoryName(AssetPath)?.Replace('\\', '/') ?? string.Empty;
			string[] pathSegments = directory.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
			GroupSegments = GetRelativeGroupSegments(pathSegments);
			GroupPath = GroupSegments.Length == 0 ? "Scenes" : string.Join(" / ", GroupSegments);
		}

		public string AssetPath { get; }
		public string DisplayName { get; }
		public string GroupPath { get; }
		public string[] GroupSegments { get; }

		public bool Matches(string term)
		{
			if (string.IsNullOrWhiteSpace(term)) {
				return true;
			}

			return DisplayName.Contains(term, StringComparison.OrdinalIgnoreCase)
				|| GroupPath.Contains(term, StringComparison.OrdinalIgnoreCase)
				|| AssetPath.Contains(term, StringComparison.OrdinalIgnoreCase);
		}

		private static string[] GetRelativeGroupSegments(string[] pathSegments)
		{
			List<string> relativeSegments = new List<string>(pathSegments.Length);
			for (int index = 0; index < pathSegments.Length; index++) {
				string segment = pathSegments[index];
				if (index == 0 && string.Equals(segment, "Assets", StringComparison.OrdinalIgnoreCase)) {
					continue;
				}

				if (index == 1 && string.Equals(segment, "Scenes", StringComparison.OrdinalIgnoreCase)) {
					continue;
				}

				relativeSegments.Add(segment);
			}

			return relativeSegments.ToArray();
		}
	}

	internal static class SceneCatalog
	{
		private const string ScenesRoot = "Assets/Scenes";

		public static SceneAssetInfo TryGetActiveScene()
		{
			string path = EditorSceneManager.GetActiveScene().path;
			return string.IsNullOrWhiteSpace(path) ? null : new SceneAssetInfo(path);
		}

		public static void PopulateFlatList(List<SceneCatalogEntry> target, string searchTerm)
		{
			foreach (SceneTreeNode node in BuildTree()) {
				AddNode(target, node, 0, searchTerm);
			}
		}

		private static void AddNode(List<SceneCatalogEntry> target, SceneTreeNode node, int depth, string searchTerm)
		{
			if (node.Scene != null) {
				if (node.Scene.Matches(searchTerm)) {
					target.Add(new SceneCatalogEntry(node.Scene.DisplayName, depth, node.Scene));
				}

				return;
			}

			int countBefore = target.Count;
			int folderIndex = target.Count;
			target.Add(new SceneCatalogEntry(node.Name, depth));

			foreach (SceneTreeNode child in node.Children) {
				AddNode(target, child, depth + 1, searchTerm);
			}

			if (target.Count == countBefore + 1) {
				target.RemoveAt(folderIndex);
			}
		}

		private static List<SceneTreeNode> BuildTree()
		{
			List<SceneAssetInfo> scenes = GetScenes();
			SceneTreeNode root = new SceneTreeNode("Scenes");

			foreach (SceneAssetInfo scene in scenes) {
				SceneTreeNode current = root;
				foreach (string segment in scene.GroupSegments) {
					SceneTreeNode next = current.Children.Find(child => child.Scene == null && child.Name == segment);
					if (next == null) {
						next = new SceneTreeNode(segment);
						current.Children.Add(next);
					}

					current = next;
				}

				current.Children.Add(new SceneTreeNode(scene.DisplayName, scene));
			}

			SortNodes(root.Children);
			return root.Children;
		}

		private static List<SceneAssetInfo> GetScenes()
		{
			string[] guids = AssetDatabase.FindAssets("t:Scene", new[] {ScenesRoot});
			List<SceneAssetInfo> scenes = new List<SceneAssetInfo>(guids.Length);

			foreach (string guid in guids) {
				string path = AssetDatabase.GUIDToAssetPath(guid);
				if (string.IsNullOrWhiteSpace(path)) {
					continue;
				}

				scenes.Add(new SceneAssetInfo(path));
			}

			scenes.Sort(CompareScenes);
			return scenes;
		}

		private static int CompareScenes(SceneAssetInfo left, SceneAssetInfo right)
		{
			int groupCompare = EditorUtility.NaturalCompare(left.GroupPath, right.GroupPath);
			if (groupCompare != 0) {
				return groupCompare;
			}

			return EditorUtility.NaturalCompare(left.DisplayName, right.DisplayName);
		}

		private static void SortNodes(List<SceneTreeNode> nodes)
		{
			nodes.Sort(CompareNodes);
			foreach (SceneTreeNode node in nodes) {
				if (node.Children.Count > 0) {
					SortNodes(node.Children);
				}
			}
		}

		private static int CompareNodes(SceneTreeNode left, SceneTreeNode right)
		{
			if (left.Scene == null && right.Scene != null) {
				return -1;
			}

			if (left.Scene != null && right.Scene == null) {
				return 1;
			}

			string leftValue = left.Scene?.DisplayName ?? left.Name;
			string rightValue = right.Scene?.DisplayName ?? right.Name;
			return EditorUtility.NaturalCompare(leftValue, rightValue);
		}
	}
}
