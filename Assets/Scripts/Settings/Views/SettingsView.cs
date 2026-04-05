using System;
using Cysharp.Threading.Tasks;
using Infrastructure.Services;
using R3;
using TriInspector;
using UI.Elements;
using UI.Elements.Abstract;
using UI.Elements.Other;
using UnityEngine;
using VContainer;


namespace Settings.Views
{
	public class SettingsView : MonoBehaviour
	{
		[Title("Controls")]
		[SerializeField] private UIButtonBase m_SaveButton;
		[SerializeField] private UIButtonBase m_ResetButton;

		[Title("Sections")]
		[SerializeField] private SettingsSection[] m_Sections;

		[Title("References")]
		[SerializeField] private GameObject m_Root;
		[SerializeField] private UIRatioGroup m_SectionsGroup;
		[SerializeField] private UIScreenSwitcher m_SectionsSwitcher;
		
		[Inject] private readonly PreferencesService m_Preferences;
		[Inject] private readonly IObjectResolver m_ObjectResolver;

		public void Init()
		{
			foreach (SettingsSection section in m_Sections) {
				m_ObjectResolver.Inject(section);
				
				section.InitPreferences();
				section.Init();
				section.Load();
			}
			
			m_SaveButton.OnClick.AddListener(Hide);
			m_ResetButton.OnClick.AddListener(ResetSection);
			m_SectionsGroup.SelectedIndex.Subscribe(index => m_SectionsSwitcher.Switch(index)).AddTo(this);
		}

		public void Show()
		{
			m_Root.SetActive(true);
		}
		public void Hide()
		{
			m_Preferences.Apply().Forget();
			m_Preferences.Save().Forget();
			m_Root.SetActive(false);
		}

		public void ResetSection()
		{
			SettingsSection section = m_Sections[m_SectionsGroup.SelectedIndex.CurrentValue];
			section.UntypedPreferences.New();
			section.UntypedPreferences.Apply();
			section.Load();
			
			Debug.Log($"[{nameof(SettingsView)}] Reset {section.UntypedPreferences.GetType().Name} to defaults");
		}
	}
}