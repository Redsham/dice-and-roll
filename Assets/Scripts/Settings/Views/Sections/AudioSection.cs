using Preferences;
using R3;
using UI.Elements;
using UnityEngine;


namespace Settings.Views.Sections
{
	public class AudioSection : SettingsSection<AudioPreferenceses>
	{
		[SerializeField] private UISlider m_MasterVolume;
		[SerializeField] private UISlider m_MusicVolume;
		[SerializeField] private UISlider m_EffectVolume;


		public override void Init()
		{
			m_MasterVolume.Value.Subscribe(value => { Preferences.MasterVolume  = value; }).AddTo(this);
			m_MusicVolume.Value.Subscribe(value => { Preferences.MusicVolume    = value; }).AddTo(this);
			m_EffectVolume.Value.Subscribe(value => { Preferences.EffectsVolume = value; }).AddTo(this);
		}

		public override void Load()
		{
			m_MasterVolume.SetValue(Preferences.MasterVolume);
			m_MusicVolume.SetValue(Preferences.MusicVolume);
			m_EffectVolume.SetValue(Preferences.EffectsVolume);
		}
	}
}