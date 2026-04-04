using Infrastructure.Services;
using LitMotion;
using LitMotion.Extensions;
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
			m_Bootstrap.Message.Subscribe(UpdateMessage).AddTo(this);
		}

		private void UpdateMessage(string msg)
		{
			m_MessageText.text = msg;
			LMotion.Punch.Create(Vector2.one, Vector2.one * 0.2f, 0.25f)
			       .WithFrequency(2)
			       .WithDampingRatio(0.5f)
			       .WithEase(Ease.OutBack)
			       .BindToLocalScaleXY(m_MessageText.transform);
		}
	}
}