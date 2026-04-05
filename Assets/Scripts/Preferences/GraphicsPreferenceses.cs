using Cysharp.Threading.Tasks;
using System;
using System.Reflection;
using Preferences.Ini;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


namespace Preferences
{
	/// <summary>
	/// Stores and applies PC graphics preferences.
	/// </summary>
	public class GraphicsPreferenceses : PreferencesCategory
	{
		// Data

		/// <summary>
		/// Preferred screen width in pixels.
		/// </summary>
		public int ResolutionWidth { get; set; }

		/// <summary>
		/// Preferred screen height in pixels.
		/// </summary>
		public int ResolutionHeight { get; set; }

		/// <summary>
		/// Preferred refresh rate in hertz.
		/// </summary>
		public int RefreshRate { get; set; }

		/// <summary>
		/// Preferred fullscreen mode.
		/// </summary>
		public FullScreenMode FullScreenMode { get; set; }

		/// <summary>
		/// VSync count for Unity quality settings.
		/// </summary>
		public int VSyncCount { get; set; }

		/// <summary>
		/// Target frame rate. A non-positive value means uncapped.
		/// </summary>
		public int TargetFrameRate { get; set; }

		/// <summary>
		/// Global texture mipmap limit.
		/// </summary>
		public int GlobalTextureMipmapLimit { get; set; }

		/// <summary>
		/// Global anisotropic filtering mode.
		/// </summary>
		public AnisotropicFiltering AnisotropicFiltering { get; set; }

		/// <summary>
		/// Global LOD bias.
		/// </summary>
		public float LodBias { get; set; }

		/// <summary>
		/// Global maximum LOD level.
		/// </summary>
		public int MaximumLodLevel { get; set; }

		/// <summary>
		/// Enables or disables soft particles.
		/// </summary>
		public bool SoftParticles { get; set; }

		/// <summary>
		/// Enables or disables realtime reflection probes.
		/// </summary>
		public bool RealtimeReflectionProbes { get; set; }

		/// <summary>
		/// Enables HDR in the active URP asset.
		/// </summary>
		public bool UseHdr { get; set; }

		/// <summary>
		/// MSAA sample count in the active URP asset.
		/// </summary>
		public int MsaaSamples { get; set; }

		/// <summary>
		/// URP render scale multiplier.
		/// </summary>
		public float RenderScale { get; set; }

		/// <summary>
		/// Enables camera opaque texture in URP.
		/// </summary>
		public bool RequireOpaqueTexture { get; set; }

		/// <summary>
		/// Enables camera depth texture in URP.
		/// </summary>
		public bool RequireDepthTexture { get; set; }

		/// <summary>
		/// Enables main light shadows in URP.
		/// </summary>
		public bool MainLightShadows { get; set; }

		/// <summary>
		/// Enables additional light shadows in URP.
		/// </summary>
		public bool AdditionalLightShadows { get; set; }

		/// <summary>
		/// Enables soft shadows in URP.
		/// </summary>
		public bool SoftShadows { get; set; }

		/// <summary>
		/// Shadow distance used by URP.
		/// </summary>
		public float ShadowDistance { get; set; }

		/// <summary>
		/// Number of shadow cascades used by URP.
		/// </summary>
		public int ShadowCascadeCount { get; set; }

		/// <summary>
		/// Maximum number of additional lights affecting a single object.
		/// </summary>
		public int AdditionalLightsPerObjectLimit { get; set; }

		/// <summary>
		/// Enables the SRP batcher in URP.
		/// </summary>
		public bool UseSrpBatcher { get; set; }

		/// <summary>
		/// Enables dynamic batching in URP.
		/// </summary>
		public bool UseDynamicBatching { get; set; }

		// Metadata

		protected override string SectionName => "Graphics";

		// Lifecycle

		/// <inheritdoc />
		public override void New()
		{
			CaptureUnitySettings();
			CapturePipelineSettings();
			Sanitize();
		}

		/// <inheritdoc />
		public override UniTask Apply()
		{
			Sanitize();
			ApplyQualitySettings();
			ApplyDisplaySettings();
			ApplyPipelineSettings();

			return UniTask.CompletedTask;
		}

		// Serialization

		protected override void Read(IniSectionReader reader)
		{
			ResolutionWidth                = reader.GetInt(nameof(ResolutionWidth),  ResolutionWidth);
			ResolutionHeight               = reader.GetInt(nameof(ResolutionHeight), ResolutionHeight);
			RefreshRate                    = reader.GetInt(nameof(RefreshRate),      RefreshRate);
			FullScreenMode                 = reader.GetEnum(nameof(FullScreenMode), FullScreenMode);
			VSyncCount                     = reader.GetInt(nameof(VSyncCount),               VSyncCount);
			TargetFrameRate                = reader.GetInt(nameof(TargetFrameRate),          TargetFrameRate);
			GlobalTextureMipmapLimit       = reader.GetInt(nameof(GlobalTextureMipmapLimit), GlobalTextureMipmapLimit);
			AnisotropicFiltering           = reader.GetEnum(nameof(AnisotropicFiltering), AnisotropicFiltering);
			LodBias                        = reader.GetFloat(nameof(LodBias), LodBias);
			MaximumLodLevel                = reader.GetInt(nameof(MaximumLodLevel), MaximumLodLevel);
			SoftParticles                  = reader.GetBool(nameof(SoftParticles),            SoftParticles);
			RealtimeReflectionProbes       = reader.GetBool(nameof(RealtimeReflectionProbes), RealtimeReflectionProbes);
			UseHdr                         = reader.GetBool(nameof(UseHdr),                   UseHdr);
			MsaaSamples                    = reader.GetInt(nameof(MsaaSamples), MsaaSamples);
			RenderScale                    = reader.GetFloat(nameof(RenderScale), RenderScale);
			RequireOpaqueTexture           = reader.GetBool(nameof(RequireOpaqueTexture),   RequireOpaqueTexture);
			RequireDepthTexture            = reader.GetBool(nameof(RequireDepthTexture),    RequireDepthTexture);
			MainLightShadows               = reader.GetBool(nameof(MainLightShadows),       MainLightShadows);
			AdditionalLightShadows         = reader.GetBool(nameof(AdditionalLightShadows), AdditionalLightShadows);
			SoftShadows                    = reader.GetBool(nameof(SoftShadows),            SoftShadows);
			ShadowDistance                 = reader.GetFloat(nameof(ShadowDistance), ShadowDistance);
			ShadowCascadeCount             = reader.GetInt(nameof(ShadowCascadeCount),             ShadowCascadeCount);
			AdditionalLightsPerObjectLimit = reader.GetInt(nameof(AdditionalLightsPerObjectLimit), AdditionalLightsPerObjectLimit);
			UseSrpBatcher                  = reader.GetBool(nameof(UseSrpBatcher),      UseSrpBatcher);
			UseDynamicBatching             = reader.GetBool(nameof(UseDynamicBatching), UseDynamicBatching);

			Sanitize();
		}

		protected override void Write(IniSectionWriter writer)
		{
			writer.Set(nameof(ResolutionWidth),                ResolutionWidth);
			writer.Set(nameof(ResolutionHeight),               ResolutionHeight);
			writer.Set(nameof(RefreshRate),                    RefreshRate);
			writer.Set(nameof(FullScreenMode),                 FullScreenMode);
			writer.Set(nameof(VSyncCount),                     VSyncCount);
			writer.Set(nameof(TargetFrameRate),                TargetFrameRate);
			writer.Set(nameof(GlobalTextureMipmapLimit),       GlobalTextureMipmapLimit);
			writer.Set(nameof(AnisotropicFiltering),           AnisotropicFiltering);
			writer.Set(nameof(LodBias),                        LodBias);
			writer.Set(nameof(MaximumLodLevel),                MaximumLodLevel);
			writer.Set(nameof(SoftParticles),                  SoftParticles);
			writer.Set(nameof(RealtimeReflectionProbes),       RealtimeReflectionProbes);
			writer.Set(nameof(UseHdr),                         UseHdr);
			writer.Set(nameof(MsaaSamples),                    NormalizeMsaa(MsaaSamples));
			writer.Set(nameof(RenderScale),                    RenderScale);
			writer.Set(nameof(RequireOpaqueTexture),           RequireOpaqueTexture);
			writer.Set(nameof(RequireDepthTexture),            RequireDepthTexture);
			writer.Set(nameof(MainLightShadows),               MainLightShadows);
			writer.Set(nameof(AdditionalLightShadows),         AdditionalLightShadows);
			writer.Set(nameof(SoftShadows),                    SoftShadows);
			writer.Set(nameof(ShadowDistance),                 ShadowDistance);
			writer.Set(nameof(ShadowCascadeCount),             NormalizeShadowCascades(ShadowCascadeCount));
			writer.Set(nameof(AdditionalLightsPerObjectLimit), AdditionalLightsPerObjectLimit);
			writer.Set(nameof(UseSrpBatcher),                  UseSrpBatcher);
			writer.Set(nameof(UseDynamicBatching),             UseDynamicBatching);
		}

		// Capture

		private void CaptureUnitySettings()
		{
			ResolutionWidth  = Mathf.Max(Screen.width,  1280);
			ResolutionHeight = Mathf.Max(Screen.height, 720);
			RefreshRate = Screen.currentResolution.refreshRateRatio.value > 0f
				              ? Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value)
				              : 60;
			FullScreenMode           = Screen.fullScreenMode;
			VSyncCount               = QualitySettings.vSyncCount;
			TargetFrameRate          = Application.targetFrameRate;
			GlobalTextureMipmapLimit = QualitySettings.globalTextureMipmapLimit;
			AnisotropicFiltering     = QualitySettings.anisotropicFiltering;
			LodBias                  = QualitySettings.lodBias;
			MaximumLodLevel          = QualitySettings.maximumLODLevel;
			SoftParticles            = QualitySettings.softParticles;
			RealtimeReflectionProbes = QualitySettings.realtimeReflectionProbes;
		}

		private void CapturePipelineSettings()
		{
			UniversalRenderPipelineAsset pipelineAsset = GetCurrentPipelineAsset();

			UseHdr                         = ReadMember(pipelineAsset, true, "supportsHDR", "m_SupportsHDR");
			MsaaSamples                    = NormalizeMsaa(ReadMember(pipelineAsset, 1,  "msaaSampleCount", "m_MSAA"));
			RenderScale                    = Mathf.Clamp(ReadMember(pipelineAsset,   1f, "renderScale",     "m_RenderScale"), 0.1f, 2f);
			RequireOpaqueTexture           = ReadMember(pipelineAsset, true, "supportsCameraOpaqueTexture",    "m_RequireOpaqueTexture");
			RequireDepthTexture            = ReadMember(pipelineAsset, true, "supportsCameraDepthTexture",     "m_RequireDepthTexture");
			MainLightShadows               = ReadMember(pipelineAsset, true, "supportsMainLightShadows",       "m_MainLightShadowsSupported");
			AdditionalLightShadows         = ReadMember(pipelineAsset, true, "supportsAdditionalLightShadows", "m_AdditionalLightShadowsSupported");
			SoftShadows                    = ReadMember(pipelineAsset, true, "supportsSoftShadows",            "m_SoftShadowsSupported");
			ShadowDistance                 = Mathf.Max(0f, ReadMember(pipelineAsset,           50f, "shadowDistance",           "m_ShadowDistance"));
			ShadowCascadeCount             = NormalizeShadowCascades(ReadMember(pipelineAsset, 4,   "shadowCascadeCount",       "m_ShadowCascadeCount"));
			AdditionalLightsPerObjectLimit = Mathf.Clamp(ReadMember(pipelineAsset,             4,   "maxAdditionalLightsCount", "m_AdditionalLightsPerObjectLimit"), 0, 8);
			UseSrpBatcher                  = ReadMember(pipelineAsset, true,  "useSRPBatcher",           "m_UseSRPBatcher");
			UseDynamicBatching             = ReadMember(pipelineAsset, false, "supportsDynamicBatching", "m_SupportsDynamicBatching");
		}

		// Apply

		private void ApplyQualitySettings()
		{
			QualitySettings.vSyncCount               = Mathf.Clamp(VSyncCount, 0, 4);
			QualitySettings.globalTextureMipmapLimit = Mathf.Max(0, GlobalTextureMipmapLimit);
			QualitySettings.anisotropicFiltering     = AnisotropicFiltering;
			QualitySettings.lodBias                  = Mathf.Max(0.1f, LodBias);
			QualitySettings.maximumLODLevel          = Mathf.Max(0,    MaximumLodLevel);
			QualitySettings.softParticles            = SoftParticles;
			QualitySettings.realtimeReflectionProbes = RealtimeReflectionProbes;
			Application.targetFrameRate              = TargetFrameRate <= 0 ? -1 : TargetFrameRate;
		}

		private void ApplyDisplaySettings()
		{
			Screen.SetResolution(
			                     Mathf.Max(320, ResolutionWidth),
			                     Mathf.Max(240, ResolutionHeight),
			                     FullScreenMode,
			                     CreateRefreshRate(RefreshRate));
		}

		private void ApplyPipelineSettings()
		{
			UniversalRenderPipelineAsset pipelineAsset = GetCurrentPipelineAsset();
			if (pipelineAsset == null) {
				return;
			}

			TrySetMember(pipelineAsset, UseHdr,                                            "supportsHDR",                    "m_SupportsHDR");
			TrySetMember(pipelineAsset, NormalizeMsaa(MsaaSamples),                        "msaaSampleCount",                "m_MSAA");
			TrySetMember(pipelineAsset, Mathf.Clamp(RenderScale, 0.1f, 2f),                "renderScale",                    "m_RenderScale");
			TrySetMember(pipelineAsset, RequireOpaqueTexture,                              "supportsCameraOpaqueTexture",    "m_RequireOpaqueTexture");
			TrySetMember(pipelineAsset, RequireDepthTexture,                               "supportsCameraDepthTexture",     "m_RequireDepthTexture");
			TrySetMember(pipelineAsset, MainLightShadows,                                  "supportsMainLightShadows",       "m_MainLightShadowsSupported");
			TrySetMember(pipelineAsset, AdditionalLightShadows,                            "supportsAdditionalLightShadows", "m_AdditionalLightShadowsSupported");
			TrySetMember(pipelineAsset, SoftShadows,                                       "supportsSoftShadows",            "m_SoftShadowsSupported");
			TrySetMember(pipelineAsset, Mathf.Max(0f, ShadowDistance),                     "shadowDistance",                 "m_ShadowDistance");
			TrySetMember(pipelineAsset, NormalizeShadowCascades(ShadowCascadeCount),       "shadowCascadeCount",             "m_ShadowCascadeCount");
			TrySetMember(pipelineAsset, Mathf.Clamp(AdditionalLightsPerObjectLimit, 0, 8), "maxAdditionalLightsCount",       "m_AdditionalLightsPerObjectLimit");
			TrySetMember(pipelineAsset, UseSrpBatcher,                                     "useSRPBatcher",                  "m_UseSRPBatcher");
			TrySetMember(pipelineAsset, UseDynamicBatching,                                "supportsDynamicBatching",        "m_SupportsDynamicBatching");
		}

		// Sanitizing

		private void Sanitize()
		{
			ResolutionWidth                = Mathf.Max(320, ResolutionWidth);
			ResolutionHeight               = Mathf.Max(240, ResolutionHeight);
			RefreshRate                    = Mathf.Max(0,   RefreshRate);
			VSyncCount                     = Mathf.Clamp(VSyncCount, 0, 4);
			GlobalTextureMipmapLimit       = Mathf.Max(0,    GlobalTextureMipmapLimit);
			LodBias                        = Mathf.Max(0.1f, LodBias);
			MaximumLodLevel                = Mathf.Max(0,    MaximumLodLevel);
			RenderScale                    = Mathf.Clamp(RenderScale, 0.1f, 2f);
			MsaaSamples                    = NormalizeMsaa(MsaaSamples);
			ShadowDistance                 = Mathf.Max(0f, ShadowDistance);
			ShadowCascadeCount             = NormalizeShadowCascades(ShadowCascadeCount);
			AdditionalLightsPerObjectLimit = Mathf.Clamp(AdditionalLightsPerObjectLimit, 0, 8);
		}

		// Helpers

		private static UniversalRenderPipelineAsset GetCurrentPipelineAsset()
		{
			PropertyInfo currentPipelineProperty = typeof(GraphicsSettings).GetProperty("currentRenderPipeline", BindingFlags.Public | BindingFlags.Static);
			if (currentPipelineProperty?.GetValue(null) is UniversalRenderPipelineAsset currentPipeline) {
				return currentPipeline;
			}

			PropertyInfo defaultPipelineProperty = typeof(GraphicsSettings).GetProperty("defaultRenderPipeline", BindingFlags.Public | BindingFlags.Static);
			if (defaultPipelineProperty?.GetValue(null) is UniversalRenderPipelineAsset defaultPipeline) {
				return defaultPipeline;
			}

			return null;
		}

		private static T ReadMember<T>(object target, T fallback, params string[] memberNames)
		{
			if (target == null) {
				return fallback;
			}

			foreach (string memberName in memberNames) {
				PropertyInfo property = target.GetType().GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (property != null && property.CanRead && TryConvert(property.GetValue(target), out T propertyValue)) {
					return propertyValue;
				}

				FieldInfo field = GetField(target.GetType(), memberName);
				if (field != null && TryConvert(field.GetValue(target), out T fieldValue)) {
					return fieldValue;
				}
			}

			return fallback;
		}

		private static void TrySetMember<T>(object target, T value, params string[] memberNames)
		{
			if (target == null) {
				return;
			}

			foreach (string memberName in memberNames) {
				PropertyInfo property = target.GetType().GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (property != null && property.CanWrite && TryConvert(value, property.PropertyType, out object convertedPropertyValue)) {
					property.SetValue(target, convertedPropertyValue);
					return;
				}

				FieldInfo field = GetField(target.GetType(), memberName);
				if (field != null && TryConvert(value, field.FieldType, out object convertedFieldValue)) {
					field.SetValue(target, convertedFieldValue);
					return;
				}
			}
		}

		private static FieldInfo GetField(Type type, string fieldName)
		{
			while (type != null) {
				FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (field != null) {
					return field;
				}

				type = type.BaseType;
			}

			return null;
		}

		private static bool TryConvert<T>(object value, out T converted)
		{
			if (TryConvert(value, typeof(T), out object boxed)) {
				converted = (T)boxed;
				return true;
			}

			converted = default;
			return false;
		}

		private static bool TryConvert(object value, Type targetType, out object converted)
		{
			if (value == null) {
				converted = null;
				return false;
			}

			Type sourceType = value.GetType();
			if (targetType.IsAssignableFrom(sourceType)) {
				converted = value;
				return true;
			}

			try {
				if (targetType.IsEnum) {
					converted = value is string enumText
						            ? Enum.Parse(targetType, enumText, true)
						            : Enum.ToObject(targetType, value);
					return true;
				}

				if (targetType == typeof(bool) && value is int intValue) {
					converted = intValue != 0;
					return true;
				}

				if (targetType == typeof(int) && value is bool boolValue) {
					converted = boolValue ? 1 : 0;
					return true;
				}

				converted = Convert.ChangeType(value, targetType, System.Globalization.CultureInfo.InvariantCulture);
				return true;
			} catch {
				converted = null;
				return false;
			}
		}

		private static int NormalizeMsaa(int value)
		{
			return value switch {
				>= 8 => 8,
				>= 4 => 4,
				>= 2 => 2,
				_    => 1
			};
		}

		private static int NormalizeShadowCascades(int value)
		{
			return value switch {
				>= 4 => 4,
				>= 2 => 2,
				_    => 1
			};
		}

		private static RefreshRate CreateRefreshRate(int refreshRate)
		{
			if (refreshRate <= 0) {
				return default;
			}

			return new RefreshRate {
				numerator   = (uint)refreshRate,
				denominator = 1u
			};
		}
	}
}