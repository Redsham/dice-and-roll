using System.IO;
using System.Reflection;
using Gameplay.Enemies.Authoring;
using Gameplay.Enemies.Configs;
using Gameplay.Enemies.Presentation;
using TriInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;


namespace Gameplay.Enemies.Editor
{
	public static class EnemyContentGenerator
	{
		// === Paths ===

		private const string PrefabFolderPath = "Assets/Prefabs/Gameplay/Enemies";
		private const string MaterialFolderPath = "Assets/Prefabs/Gameplay/Enemies/Materials";
		private const string ConfigFolderPath = "Assets/Settings/Gameplay/Enemies";

		private const string PawnPrefabPath = "Assets/Prefabs/Gameplay/Enemies/Pawn.prefab";
		private const string MortarPrefabPath = "Assets/Prefabs/Gameplay/Enemies/Mortar.prefab";
		private const string PawnConfigPath = "Assets/Settings/Gameplay/Enemies/PawnEnemyConfig.asset";
		private const string MortarConfigPath = "Assets/Settings/Gameplay/Enemies/MortarEnemyConfig.asset";

		// === Menu ===

		[MenuItem("Game/Gameplay/Enemies/Rebuild Base Enemy Content")]
		public static void RebuildAll()
		{
			EnsureFolder("Assets/Prefabs/Gameplay", "Enemies");
			EnsureFolder("Assets/Prefabs/Gameplay/Enemies", "Materials");
			EnsureFolder("Assets/Settings/Gameplay", "Enemies");

			PawnEnemyConfig pawnConfig = LoadOrCreateConfig<PawnEnemyConfig>(PawnConfigPath, ConfigurePawnConfig);
			MortarEnemyConfig mortarConfig = LoadOrCreateConfig<MortarEnemyConfig>(MortarConfigPath, ConfigureMortarConfig);

			Material pawnMaterial = LoadOrCreateMaterial("Enemy_Pawn_Mat", new Color(0.78f, 0.43f, 0.28f));
			Material pawnAccentMaterial = LoadOrCreateMaterial("Enemy_Pawn_Accent_Mat", new Color(0.22f, 0.16f, 0.15f));
			Material mortarMaterial = LoadOrCreateMaterial("Enemy_Mortar_Mat", new Color(0.37f, 0.56f, 0.37f));
			Material mortarAccentMaterial = LoadOrCreateMaterial("Enemy_Mortar_Accent_Mat", new Color(0.18f, 0.2f, 0.18f));
			Material markerMaterial = LoadOrCreateMaterial("Enemy_Mortar_Marker_Mat", new Color(0.95f, 0.32f, 0.16f, 0.85f));

			BuildPawnPrefab(pawnConfig, pawnMaterial, pawnAccentMaterial);
			BuildMortarPrefab(mortarConfig, mortarMaterial, mortarAccentMaterial, markerMaterial);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			Debug.Log("Base enemy content was rebuilt.");
		}

		// === Configs ===

		private static T LoadOrCreateConfig<T>(string assetPath, System.Action<T> configure) where T : EnemyConfig
		{
			T config = AssetDatabase.LoadAssetAtPath<T>(assetPath);
			if (config == null) {
				config = ScriptableObject.CreateInstance<T>();
				AssetDatabase.CreateAsset(config, assetPath);
			}

			configure(config);
			EditorUtility.SetDirty(config);
			return config;
		}

		private static void ConfigurePawnConfig(PawnEnemyConfig config)
		{
			SetAutoProperty(config, "Id", "enemy_pawn");
			SetAutoProperty(config, "DisplayName", new LocalizedString("Enemies", "enemy_pawn_name"));
			SetAutoProperty(config, "Description", new LocalizedString("Enemies", "enemy_pawn_description"));
			SetAutoProperty(config, "MaxHealth", 4);
			SetAutoProperty(config, "ContactDamage", 1);
			SetAutoProperty(config, "MoveDuration", 0.22f);
			SetAutoProperty(config, "RotateDuration", 0.17f);
			SetAutoProperty(config, "SpawnDuration", 0.3f);
			SetAutoProperty(config, "ShootRange", 2);
			SetAutoProperty(config, "ShootDamage", 1);
		}

		private static void ConfigureMortarConfig(MortarEnemyConfig config)
		{
			SetAutoProperty(config, "Id", "enemy_mortar");
			SetAutoProperty(config, "DisplayName", new LocalizedString("Enemies", "enemy_mortar_name"));
			SetAutoProperty(config, "Description", new LocalizedString("Enemies", "enemy_mortar_description"));
			SetAutoProperty(config, "MaxHealth", 5);
			SetAutoProperty(config, "ContactDamage", 1);
			SetAutoProperty(config, "MoveDuration", 0.24f);
			SetAutoProperty(config, "RotateDuration", 0.18f);
			SetAutoProperty(config, "SpawnDuration", 0.36f);
			SetAutoProperty(config, "BombardmentIntervalTurns", 3);
			SetAutoProperty(config, "BombardmentRadius", 2);
			SetAutoProperty(config, "BombardmentDamage", 3);
			SetAutoProperty(config, "PreferredDistance", 4);
		}

		// === Prefabs ===

		private static void BuildPawnPrefab(PawnEnemyConfig config, Material primaryMaterial, Material accentMaterial)
		{
			GameObject root = new("Pawn");

			try {
				PawnEnemyBehaviour behaviour = root.AddComponent<PawnEnemyBehaviour>();
				EnemyView view = root.AddComponent<EnemyView>();

				GameObject modelRoot = new("ModelRoot");
				modelRoot.transform.SetParent(root.transform, false);

				GameObject body = CreatePrimitive("Body", PrimitiveType.Capsule, modelRoot.transform, primaryMaterial);
				body.transform.localScale = new Vector3(0.72f, 0.85f, 0.72f);
				body.transform.localPosition = new Vector3(0.0f, 0.42f, 0.0f);

				GameObject head = CreatePrimitive("Head", PrimitiveType.Sphere, modelRoot.transform, accentMaterial);
				head.transform.localScale = Vector3.one * 0.32f;
				head.transform.localPosition = new Vector3(0.0f, 1.05f, 0.18f);

				GameObject muzzle = CreatePrimitive("Muzzle", PrimitiveType.Cube, modelRoot.transform, accentMaterial);
				muzzle.transform.localScale = new Vector3(0.15f, 0.15f, 0.6f);
				muzzle.transform.localPosition = new Vector3(0.0f, 0.72f, 0.42f);

				SetAutoProperty(behaviour, "View", view);
				SetAutoProperty(behaviour, "Config", config);
				SetAutoProperty(view, "m_ModelRoot", modelRoot.transform);

				SavePrefab(root, PawnPrefabPath);
			}
			finally {
				Object.DestroyImmediate(root);
			}
		}

		private static void BuildMortarPrefab(MortarEnemyConfig config, Material primaryMaterial, Material accentMaterial, Material markerMaterial)
		{
			GameObject root = new("Mortar");

			try {
				MortarEnemyBehaviour behaviour = root.AddComponent<MortarEnemyBehaviour>();
				EnemyView view = root.AddComponent<EnemyView>();

				GameObject modelRoot = new("ModelRoot");
				modelRoot.transform.SetParent(root.transform, false);

				GameObject baseBody = CreatePrimitive("Base", PrimitiveType.Cylinder, modelRoot.transform, primaryMaterial);
				baseBody.transform.localScale = new Vector3(0.7f, 0.25f, 0.7f);
				baseBody.transform.localPosition = new Vector3(0.0f, 0.26f, 0.0f);

				GameObject barrelPivot = new("BarrelPivot");
				barrelPivot.transform.SetParent(modelRoot.transform, false);
				barrelPivot.transform.localPosition = new Vector3(0.0f, 0.62f, 0.0f);
				barrelPivot.transform.localEulerAngles = new Vector3(-38.0f, 0.0f, 0.0f);

				GameObject barrel = CreatePrimitive("Barrel", PrimitiveType.Cylinder, barrelPivot.transform, accentMaterial);
				barrel.transform.localScale = new Vector3(0.16f, 0.55f, 0.16f);
				barrel.transform.localRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
				barrel.transform.localPosition = new Vector3(0.0f, 0.1f, 0.2f);

				GameObject ring = CreatePrimitive("Ring", PrimitiveType.Cylinder, modelRoot.transform, accentMaterial);
				ring.transform.localScale = new Vector3(0.48f, 0.04f, 0.48f);
				ring.transform.localPosition = new Vector3(0.0f, 0.52f, 0.0f);

				GameObject marker = CreateMarkerRoot(root.transform, markerMaterial);
				MortarAimMarkerView markerView = marker.AddComponent<MortarAimMarkerView>();
				SetAutoProperty(markerView, "m_Root", marker);

				SetAutoProperty(behaviour, "View", view);
				SetAutoProperty(behaviour, "Config", config);
				SetAutoProperty(behaviour, "AimMarker", markerView);
				SetAutoProperty(view, "m_ModelRoot", modelRoot.transform);

				SavePrefab(root, MortarPrefabPath);
			}
			finally {
				Object.DestroyImmediate(root);
			}
		}

		private static GameObject CreateMarkerRoot(Transform parent, Material material)
		{
			GameObject markerRoot = new("AimMarker");
			markerRoot.transform.SetParent(parent, false);
			markerRoot.SetActive(false);

			GameObject ring = CreatePrimitive("Ring", PrimitiveType.Cylinder, markerRoot.transform, material);
			ring.transform.localScale = new Vector3(0.85f, 0.01f, 0.85f);
			ring.transform.localPosition = new Vector3(0.0f, 0.02f, 0.0f);

			GameObject center = CreatePrimitive("Center", PrimitiveType.Sphere, markerRoot.transform, material);
			center.transform.localScale = new Vector3(0.16f, 0.04f, 0.16f);
			center.transform.localPosition = new Vector3(0.0f, 0.03f, 0.0f);

			return markerRoot;
		}

		private static GameObject CreatePrimitive(string name, PrimitiveType primitiveType, Transform parent, Material material)
		{
			GameObject primitive = GameObject.CreatePrimitive(primitiveType);
			primitive.name = name;
			primitive.transform.SetParent(parent, false);

			Collider collider = primitive.GetComponent<Collider>();
			if (collider != null) {
				Object.DestroyImmediate(collider);
			}

			MeshRenderer renderer = primitive.GetComponent<MeshRenderer>();
			if (renderer != null) {
				renderer.sharedMaterial = material;
				renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
				renderer.receiveShadows = true;
			}

			return primitive;
		}

		private static void SavePrefab(GameObject root, string prefabPath)
		{
			PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
		}

		// === Materials ===

		private static Material LoadOrCreateMaterial(string fileName, Color color)
		{
			string assetPath = Path.Combine(MaterialFolderPath, $"{fileName}.mat").Replace("\\", "/");
			Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
			Shader shader = Shader.Find("Universal Render Pipeline/Lit");
			if (shader == null) {
				shader = Shader.Find("Standard");
			}

			if (material == null) {
				material = new Material(shader);
				AssetDatabase.CreateAsset(material, assetPath);
			}

			material.shader = shader;
			material.name = fileName;
			if (material.HasProperty("_BaseColor")) {
				material.SetColor("_BaseColor", color);
			}

			if (material.HasProperty("_Color")) {
				material.SetColor("_Color", color);
			}

			EditorUtility.SetDirty(material);
			return material;
		}

		// === Helpers ===

		private static void SetAutoProperty(object target, string propertyName, object value)
		{
			FieldInfo field = null;
			for (System.Type currentType = target.GetType(); currentType != null; currentType = currentType.BaseType) {
				field = currentType.GetField($"<{propertyName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
				        ?? currentType.GetField(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
				if (field != null) {
					break;
				}
			}

			field?.SetValue(target, value);
		}

		private static void EnsureFolder(string parentFolder, string folderName)
		{
			string fullPath = $"{parentFolder}/{folderName}";
			if (!AssetDatabase.IsValidFolder(fullPath)) {
				AssetDatabase.CreateFolder(parentFolder, folderName);
			}
		}
	}
}
