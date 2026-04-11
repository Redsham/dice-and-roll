using Preferences;
using R3;
using UI.Elements;
using UnityEngine;


namespace Settings.Views.Sections
{
	public class AudioSection : SettingsSection<AudioPreferenceses>
	{
		[SerializeField] private UISlider m_MasterVolume;
		[SerializeField] private UISlider m_UIVolume;
		[SerializeField] private UISlider m_MusicVolume;
		[SerializeField] private UISlider m_EffectVolume;


		public override void Bind()
		{
			m_MasterVolume.Value.Subscribe(value => {
				Preferences.MasterVolume = value;
				NotifyPreferencesChanged();
			}).AddTo(this);

			m_UIVolume.Value.Subscribe(value => {
				Preferences.UIVolume = value;
				NotifyPreferencesChanged();
			}).AddTo(this);

			m_MusicVolume.Value.Subscribe(value => {
				Preferences.MusicVolume = value;
				NotifyPreferencesChanged();
			}).AddTo(this);

			m_EffectVolume.Value.Subscribe(value => {
				Preferences.EffectsVolume = value;
				NotifyPreferencesChanged();
			}).AddTo(this);
		}

		public override void Load()
		{
			m_MasterVolume.SetValue(Preferences.MasterVolume);
			m_UIVolume.SetValue(Preferences.UIVolume);
			m_MusicVolume.SetValue(Preferences.MusicVolume);
			m_EffectVolume.SetValue(Preferences.EffectsVolume);
		}
	}
}