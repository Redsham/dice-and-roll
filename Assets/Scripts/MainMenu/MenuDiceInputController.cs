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

		private Vector2    _pressPointerPosition;
		private RaycastHit _pressHit;
		private bool       _isPointerDownOnDice;
		private bool       _isDragging;
		private Renderer[] _renderers;
		private Collider[] _colliders;

		private void Awake()
		{
			if (m_Rotator == null)
				m_Rotator = GetComponent<MenuDiceRotator>();

			if (m_Shooter == null)
				m_Shooter = GetComponent<MenuDiceShooter>();

			_renderers = GetComponentsInChildren<Renderer>();
			_colliders = GetComponentsInChildren<Collider>();
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
				_isPointerDownOnDice = true;
				_isDragging = false;
				_pressPointerPosition = pointerPosition;
				_pressHit = hit;
			}

			if (_isPointerDownOnDice && !_isDragging && mouse.leftButton.isPressed) {
				if ((pointerPosition - _pressPointerPosition).sqrMagnitude > m_ClickMaxDragPixels * m_ClickMaxDragPixels) {
					_isDragging = true;
					m_Rotator?.BeginDrag(_pressPointerPosition);
				}
			}

			if (_isDragging && mouse.leftButton.isPressed)
				m_Rotator?.DragTo(pointerPosition, inputCamera, deltaTime);

			if (_isPointerDownOnDice && mouse.leftButton.wasReleasedThisFrame) {
				if (_isDragging) {
					m_Rotator?.EndDrag();
				}
				else {
					m_Shooter?.Shoot(_pressHit);
				}

				_isPointerDownOnDice = false;
				_isDragging = false;
			}
		}

		private void OnDisable()
		{
			if (_isDragging)
				m_Rotator?.EndDrag();

			_isPointerDownOnDice = false;
			_isDragging = false;
		}

		private bool TryGetDiceHit(Vector2 pointerPosition, Camera inputCamera, out RaycastHit hit)
		{
			Ray ray = inputCamera.ScreenPointToRay(pointerPosition);
			if (_colliders != null && _colliders.Length > 0 && Physics.Raycast(ray, out hit, float.MaxValue))
				return hit.transform == transform || hit.transform.IsChildOf(transform);

			hit = default;
			return Vector2.Distance(pointerPosition, GetDiceScreenCenter(inputCamera)) <= m_DragRadiusPixels;
		}

		private Vector2 GetDiceScreenCenter(Camera inputCamera)
		{
			Vector3 worldCenter = transform.position;

			if (_renderers != null && _renderers.Length > 0) {
				Bounds bounds = _renderers[0].bounds;
				for (int i = 1; i < _renderers.Length; i++)
					bounds.Encapsulate(_renderers[i].bounds);

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
