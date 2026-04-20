using TriInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


namespace MainMenu
{
	public class MenuDiceInputController : MonoBehaviour
	{
		[Title("References")]
		[SerializeField] private MenuDiceRotator m_Rotator;
		[SerializeField] private MenuDiceShooter m_Shooter;
		[SerializeField] private Camera          m_InputCamera;

		[Title("Input")]
		[SerializeField] private float m_DragRadiusPixels = 180f;
		[SerializeField] private float m_ClickMaxDragPixels = 12f;

		private Vector2    m_PressPointerPosition;
		private RaycastHit m_PressHit;
		private bool       m_IsPointerDownOnDice;
		private bool       m_IsDragging;
		private Renderer[] m_Renderers;
		private Collider[] m_Colliders;

		private void Awake()
		{
			if (m_Rotator == null)
				m_Rotator = GetComponent<MenuDiceRotator>();

			if (m_Shooter == null)
				m_Shooter = GetComponent<MenuDiceShooter>();

			m_Renderers = GetComponentsInChildren<Renderer>();
			m_Colliders = GetComponentsInChildren<Collider>();
		}

		private void Update()
		{
			Camera inputCamera = GetInputCamera();
			Mouse mouse = Mouse.current;
			if (inputCamera == null || mouse == null)
				return;

			float deltaTime = Time.unscaledDeltaTime;
			Vector2 pointerPosition = mouse.position.ReadValue();

			if (mouse.leftButton.wasPressedThisFrame && !EventSystem.current.IsPointerOverGameObject() && TryGetDiceHit(pointerPosition, inputCamera, out RaycastHit hit)) {
				m_IsPointerDownOnDice = true;
				m_IsDragging = false;
				m_PressPointerPosition = pointerPosition;
				m_PressHit = hit;
			}

			if (m_IsPointerDownOnDice && !m_IsDragging && mouse.leftButton.isPressed) {
				if ((pointerPosition - m_PressPointerPosition).sqrMagnitude > m_ClickMaxDragPixels * m_ClickMaxDragPixels) {
					m_IsDragging = true;
					m_Rotator?.BeginDrag(m_PressPointerPosition);
				}
			}

			if (m_IsDragging && mouse.leftButton.isPressed)
				m_Rotator?.DragTo(pointerPosition, inputCamera, deltaTime);

			if (m_IsPointerDownOnDice && mouse.leftButton.wasReleasedThisFrame) {
				if (m_IsDragging) {
					m_Rotator?.EndDrag();
				}
				else {
					m_Shooter?.Shoot(m_PressHit);
				}

				m_IsPointerDownOnDice = false;
				m_IsDragging = false;
			}
		}

		private void OnDisable()
		{
			if (m_IsDragging)
				m_Rotator?.EndDrag();

			m_IsPointerDownOnDice = false;
			m_IsDragging = false;
		}

		private bool TryGetDiceHit(Vector2 pointerPosition, Camera inputCamera, out RaycastHit hit)
		{
			Ray ray = inputCamera.ScreenPointToRay(pointerPosition);
			if (m_Colliders is { Length: > 0 } && Physics.Raycast(ray, out hit, float.MaxValue))
				return hit.transform == transform || hit.transform.IsChildOf(transform);

			hit = default;
			return Vector2.Distance(pointerPosition, GetDiceScreenCenter(inputCamera)) <= m_DragRadiusPixels;
		}

		private Vector2 GetDiceScreenCenter(Camera inputCamera)
		{
			Vector3 worldCenter = transform.position;

			if (m_Renderers != null && m_Renderers.Length > 0) {
				Bounds bounds = m_Renderers[0].bounds;
				for (int i = 1; i < m_Renderers.Length; i++)
					bounds.Encapsulate(m_Renderers[i].bounds);

				worldCenter = bounds.center;
			}

			return inputCamera.WorldToScreenPoint(worldCenter);
		}

		private Camera GetInputCamera()
		{
			if (m_InputCamera != null)
				return m_InputCamera;

			return Camera.main;
		}

		private void OnValidate()
		{
			m_DragRadiusPixels = Mathf.Max(32f, m_DragRadiusPixels);
			m_ClickMaxDragPixels = Mathf.Max(0f, m_ClickMaxDragPixels);
		}
	}
}
