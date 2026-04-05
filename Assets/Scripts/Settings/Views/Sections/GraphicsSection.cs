using System;
using System.Collections.Generic;
using Preferences;
using R3;
using TMPro;
using TriInspector;
using UI.Elements;
using UnityEngine;


namespace Settings.Views.Sections
{
	/// <summary>
	/// UI section for common graphics settings.
	/// </summary>
	public class GraphicsSection : SettingsSection<GraphicsPreferenceses>
	{
		// Display

		[Title("Display")]
		[SerializeField] private TMP_Dropdown m_ResolutionDropdown;
		[SerializeField] private TMP_Dropdown m_FrameRateDropdown;
		[SerializeField] private UIRatioGroup m_FullscreenGroup;
		[SerializeField] private UIRatioGroup m_VSyncGroup;

		// Quality

		[Title("Quality")]
		[SerializeField] private UIRatioGroup m_AntiAliasingGroup;
		[SerializeField] private UIRatioGroup m_HdrGroup;
		[SerializeField] private UIRatioGroup m_ShadowsGroup;
		[SerializeField] private UIRatioGroup m_SoftShadowsGroup;
		[SerializeField] private UISlider     m_RenderScaleSlider;
		[SerializeField] private UISlider     m_ShadowDistanceSlider;

		private readonly List<ResolutionOption> m_ResolutionOptions = new();
		private readonly List<int> m_FrameRateOptions = new() {
			-1,
			30,
			60,
			120,
			144,
			165,
			240
		};

		// Lifecycle

		public override void Build()
		{
			BuildResolutionOptions();
			BuildFrameRateOptions();
		}

		public override void Load()
		{
			LoadResolution();
			LoadFullscreen();
			LoadVSync();
			LoadFrameRate();
			LoadAntiAliasing();
			LoadOnOffGroup(m_HdrGroup, Preferences.UseHdr);
			LoadShadows();
			LoadOnOffGroup(m_SoftShadowsGroup, Preferences.SoftShadows);
			LoadSlider(m_RenderScaleSlider,    Preferences.RenderScale,    0.5f, 1.5f);
			LoadSlider(m_ShadowDistanceSlider, Preferences.ShadowDistance, 0f,   100f);
		}

		public override void Bind()
		{
			if (m_ResolutionDropdown != null) {
				m_ResolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
			}

			if (m_FullscreenGroup != null) {
				m_FullscreenGroup.OnSelected += index => {
					Preferences.FullScreenMode = GetFullscreenMode(index);
				};
			}

			if (m_VSyncGroup != null) {
				m_VSyncGroup.OnSelected += index => {
					Preferences.VSyncCount = index == 1 ? 1 : 0;
				};
			}

			if (m_FrameRateDropdown != null) {
				m_FrameRateDropdown.onValueChanged.AddListener(index => {
					Preferences.TargetFrameRate = GetFrameRate(index);
				});
			}

			if (m_AntiAliasingGroup != null) {
				m_AntiAliasingGroup.OnSelected += index => {
					Preferences.MsaaSamples = GetMsaaSamples(index);
				};
			}

			if (m_HdrGroup != null) {
				m_HdrGroup.OnSelected += index => {
					Preferences.UseHdr = index == 1;
				};
			}

			if (m_ShadowsGroup != null) {
				m_ShadowsGroup.OnSelected += ApplyShadowMode;
			}

			if (m_SoftShadowsGroup != null) {
				m_SoftShadowsGroup.OnSelected += index => {
					Preferences.SoftShadows = index == 1;
				};
			}

			if (m_RenderScaleSlider != null) {
				m_RenderScaleSlider.Value.Subscribe(value => {
					Preferences.RenderScale = Mathf.Lerp(0.5f, 1.5f, value);
				}).AddTo(this);
			}

			if (m_ShadowDistanceSlider != null) {
				m_ShadowDistanceSlider.Value.Subscribe(value => {
					Preferences.ShadowDistance = Mathf.Lerp(0f, 100f, value);
				}).AddTo(this);
			}
		}

		// Build

		private void BuildResolutionOptions()
		{
			if (m_ResolutionDropdown == null) {
				return;
			}

			m_ResolutionOptions.Clear();
			m_ResolutionDropdown.ClearOptions();

			HashSet<string> uniqueOptions = new();
			foreach (Resolution resolution in Screen.resolutions) {
				ResolutionOption option = new(resolution.width, resolution.height);
				if (!uniqueOptions.Add(option.Key)) {
					continue;
				}

				m_ResolutionOptions.Add(option);
				m_ResolutionDropdown.options.Add(new TMP_Dropdown.OptionData(option.Label));
			}

			if (m_ResolutionOptions.Count == 0) {
				ResolutionOption fallback = new(Screen.width, Screen.height);
				m_ResolutionOptions.Add(fallback);
				m_ResolutionDropdown.options.Add(new TMP_Dropdown.OptionData(fallback.Label));
			}

			m_ResolutionDropdown.RefreshShownValue();
		}

		private void BuildFrameRateOptions()
		{
			if (m_FrameRateDropdown == null) {
				return;
			}

			m_FrameRateDropdown.ClearOptions();
			foreach (int frameRate in m_FrameRateOptions) {
				string label = frameRate <= 0 ? "Unlimited" : $"{frameRate} FPS";
				m_FrameRateDropdown.options.Add(new TMP_Dropdown.OptionData(label));
			}

			m_FrameRateDropdown.RefreshShownValue();
		}

		// Load

		private void LoadResolution()
		{
			if (m_ResolutionDropdown == null || m_ResolutionOptions.Count == 0) {
				return;
			}

			int selectedIndex = 0;
			int bestScore     = int.MaxValue;

			for (int index = 0; index < m_ResolutionOptions.Count; index++) {
				ResolutionOption option = m_ResolutionOptions[index];
				int score = Mathf.Abs(option.Width - Preferences.ResolutionWidth) * 10
				            + Mathf.Abs(option.Height - Preferences.ResolutionHeight) * 10;

				if (score < bestScore) {
					bestScore     = score;
					selectedIndex = index;
				}
			}

			m_ResolutionDropdown.SetValueWithoutNotify(selectedIndex);
		}

		private void LoadFullscreen()
		{
			if (m_FullscreenGroup == null) {
				return;
			}

			m_FullscreenGroup.Select(GetFullscreenIndex(Preferences.FullScreenMode), false);
		}

		private void LoadVSync()
		{
			if (m_VSyncGroup == null) {
				return;
			}

			m_VSyncGroup.Select(Preferences.VSyncCount > 0 ? 1 : 0, false);
		}

		private void LoadFrameRate()
		{
			if (m_FrameRateDropdown == null) {
				return;
			}

			int index = m_FrameRateOptions.IndexOf(Preferences.TargetFrameRate);
			m_FrameRateDropdown.SetValueWithoutNotify(index >= 0 ? index : 0);
		}

		private void LoadAntiAliasing()
		{
			if (m_AntiAliasingGroup == null) {
				return;
			}

			m_AntiAliasingGroup.Select(GetMsaaIndex(Preferences.MsaaSamples), false);
		}

		private void LoadShadows()
		{
			if (m_ShadowsGroup == null) {
				return;
			}

			int index = !Preferences.MainLightShadows
				            ? 0
				            : Preferences.ShadowCascadeCount >= 4 ? 2 : 1;

			m_ShadowsGroup.Select(index, false);
		}

		private static void LoadOnOffGroup(UIRatioGroup group, bool value)
		{
			if (group == null) {
				return;
			}

			group.Select(value ? 1 : 0, false);
		}

		private static void LoadSlider(UISlider slider, float value, float min, float max)
		{
			if (slider == null) {
				return;
			}

			float normalized = Mathf.InverseLerp(min, max, value);
			slider.SetValue(normalized);
		}

		// Events

		private void OnResolutionChanged(int index)
		{
			if (index < 0 || index >= m_ResolutionOptions.Count) {
				return;
			}

			ResolutionOption option = m_ResolutionOptions[index];
			Preferences.ResolutionWidth  = option.Width;
			Preferences.ResolutionHeight = option.Height;
			Preferences.RefreshRate      = ResolveRefreshRate(option.Width, option.Height, Preferences.RefreshRate);
		}

		private void ApplyShadowMode(int index)
		{
			switch (index) {
				case 0:
					Preferences.MainLightShadows       = false;
					Preferences.AdditionalLightShadows = false;
					break;
				case 1:
					Preferences.MainLightShadows       = true;
					Preferences.AdditionalLightShadows = false;
					Preferences.ShadowCascadeCount     = 1;
					break;
				default:
					Preferences.MainLightShadows       = true;
					Preferences.AdditionalLightShadows = true;
					Preferences.ShadowCascadeCount     = 4;
					break;
			}
		}

		// Helpers

		private static int GetFullscreenIndex(FullScreenMode mode)
		{
			return mode switch {
				FullScreenMode.Windowed            => 0,
				FullScreenMode.FullScreenWindow    => 1,
				FullScreenMode.ExclusiveFullScreen => 2,
				FullScreenMode.MaximizedWindow     => 0,
				_                                  => 1
			};
		}

		private static FullScreenMode GetFullscreenMode(int index)
		{
			return index switch {
				0 => FullScreenMode.Windowed,
				1 => FullScreenMode.FullScreenWindow,
				2 => FullScreenMode.ExclusiveFullScreen,
				_ => FullScreenMode.FullScreenWindow
			};
		}

		private int GetFrameRate(int index)
		{
			if (index < 0 || index >= m_FrameRateOptions.Count) {
				return -1;
			}

			return m_FrameRateOptions[index];
		}

		private static int GetMsaaIndex(int samples)
		{
			return samples switch {
				>= 8 => 3,
				>= 4 => 2,
				>= 2 => 1,
				_    => 0
			};
		}

		private static int GetMsaaSamples(int index)
		{
			return index switch {
				1 => 2,
				2 => 4,
				3 => 8,
				_ => 1
			};
		}

		private readonly struct ResolutionOption
		{
			public readonly int Width;
			public readonly int Height;

			public string Key => $"{Width}x{Height}";
			public string Label => Key;

			public ResolutionOption(int width, int height)
			{
				Width  = width;
				Height = height;
			}
		}

		private static int ResolveRefreshRate(int width, int height, int preferredRefreshRate)
		{
			int resolvedRefreshRate = 0;
			int bestScore           = int.MaxValue;

			foreach (Resolution resolution in Screen.resolutions) {
				if (resolution.width != width || resolution.height != height) {
					continue;
				}

				int refreshRate = Mathf.RoundToInt((float)resolution.refreshRateRatio.value);
				int score       = Mathf.Abs(refreshRate - preferredRefreshRate);

				if (score < bestScore || score == bestScore && refreshRate > resolvedRefreshRate) {
					bestScore           = score;
					resolvedRefreshRate = refreshRate;
				}
			}

			return resolvedRefreshRate > 0
				? resolvedRefreshRate
				: Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value);
		}
	}
}
