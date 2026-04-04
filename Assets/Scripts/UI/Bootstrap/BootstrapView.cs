using Infrastructure.Services;
using R3;
using TMPro;
using UnityEngine;
using VContainer;


namespace UI.Bootstrap
{
	public class BootstrapView : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI m_MessageText;

		[Inject]
		private readonly BootstrapService m_Bootstrap;

		private void Start()
		{
			m_Bootstrap.Message.Subscribe(message =>
			{
				m_MessageText.text = message;
			}).AddTo(this);
		}
	}
}