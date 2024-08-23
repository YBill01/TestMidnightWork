using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(VCamera))]
public class VCameraController : MonoBehaviour
{
	[SerializeField]
	private Vector2 rotationMultiplier = new Vector2(0.1f, 0.1f);
	[SerializeField]
	private float scrollMultiplier = 0.01f;

	[Space(10)]
	[SerializeField]
	private VCameraTarget cameraTarget;
	[SerializeField]
	private Vector3 moveSizeBounds;
	[SerializeField]
	private float moveMultiplierMin = 0.01f;
	[SerializeField]
	private float moveMultiplierMax = 0.1f;

	private VCamera _vCamera;

	private InputControls _inputControls;

	private bool _isPointerLeftDown;
	private bool _isPointerMiddleDown;
	private bool _isPointerRightDown;

	private bool _isPointerOverGameObject;

	// UNITY EVENTS
	private void Awake()
	{
		_vCamera = GetComponent<VCamera>();

		_inputControls = new InputControls();
	}

	private void Update()
	{
		_isPointerOverGameObject = EventSystem.current.IsPointerOverGameObject();
	}

	private void OnEnable()
	{
		App.Instance.Services.Get<EventsService>().GameplayInitialized += OnGameplayInitialized;
		App.Instance.Services.Get<EventsService>().GameplayStart += OnGameplayStart;

		_inputControls.Camera.PointerPositionDelta.performed += OnPointerPositionDelta;

		_inputControls.Camera.PointerLeftDown.performed += OnPointerLeftDown;
		_inputControls.Camera.PointerLeftDown.canceled += OnPointerLeftDown;
		_inputControls.Camera.PointerMiddleDown.performed += OnPointerMiddleDown;
		_inputControls.Camera.PointerMiddleDown.canceled += OnPointerMiddleDown;
		_inputControls.Camera.PointerRightDown.performed += OnPointerRightDown;
		_inputControls.Camera.PointerRightDown.canceled += OnPointerRightDown;

		_inputControls.Camera.PointerScrollDelta.performed += OnPointerScroll;
	}
	private void OnDisable()
	{
		App.Instance.Services.Get<EventsService>().GameplayInitialized -= OnGameplayInitialized;
		App.Instance.Services.Get<EventsService>().GameplayStart -= OnGameplayStart;

		_inputControls.Camera.PointerPositionDelta.performed -= OnPointerPositionDelta;

		_inputControls.Camera.PointerLeftDown.performed -= OnPointerLeftDown;
		_inputControls.Camera.PointerLeftDown.canceled -= OnPointerLeftDown;
		_inputControls.Camera.PointerMiddleDown.performed -= OnPointerMiddleDown;
		_inputControls.Camera.PointerMiddleDown.canceled -= OnPointerMiddleDown;
		_inputControls.Camera.PointerRightDown.performed -= OnPointerRightDown;
		_inputControls.Camera.PointerRightDown.canceled -= OnPointerRightDown;

		_inputControls.Camera.PointerScrollDelta.performed -= OnPointerScroll;
	}

	// HANDLERS
	private void OnGameplayInitialized()
	{
		_inputControls.Disable();
	}
	private void OnGameplayStart()
	{
		_inputControls.Enable();
	}

	private void OnPointerPositionDelta(InputAction.CallbackContext context)
	{
		Vector2 delta = context.ReadValue<Vector2>();

		if (_isPointerLeftDown)
		{
			_vCamera.rotation.x -= delta.y * rotationMultiplier.y;
			_vCamera.rotation.y += delta.x * rotationMultiplier.x;
		}

		if (_isPointerMiddleDown || _isPointerRightDown)
		{
			Vector3 moveDirectionX = _vCamera.transform.right * -delta.x;
			Vector3 moveDirectionY = _vCamera.transform.up * -delta.y;

			cameraTarget.transform.position += (moveDirectionX + moveDirectionY) * (moveMultiplierMin + (_vCamera.distanceValue * moveMultiplierMax));

			Vector3 position = cameraTarget.transform.position;

			cameraTarget.transform.position = new Vector3
			{
				x = Math.Clamp(position.x, -moveSizeBounds.x / 2, moveSizeBounds.x / 2),
				y = Math.Clamp(position.y, -moveSizeBounds.y / 2, moveSizeBounds.y / 2),
				z = Math.Clamp(position.z, -moveSizeBounds.z / 2, moveSizeBounds.z / 2)
			};
		}
	}
	private void OnPointerLeftDown(InputAction.CallbackContext context)
	{
		_isPointerLeftDown = false;

		if (_isPointerOverGameObject)
		{
			return;
		}

		_isPointerLeftDown = context.ReadValueAsButton();
	}
	private void OnPointerMiddleDown(InputAction.CallbackContext context)
	{
		_isPointerMiddleDown = false;

		if (_isPointerOverGameObject)
		{
			return;
		}

		_isPointerMiddleDown = context.ReadValueAsButton();
	}
	private void OnPointerRightDown(InputAction.CallbackContext context)
	{
		_isPointerRightDown = false;

		if (_isPointerOverGameObject)
		{
			return;
		}

		_isPointerRightDown = context.ReadValueAsButton();
	}
	private void OnPointerScroll(InputAction.CallbackContext context)
	{
		if (_isPointerOverGameObject)
		{
			return;
		}

		_vCamera.distanceValue = Math.Clamp(_vCamera.distanceValue - context.ReadValue<Vector2>().y * scrollMultiplier, 0, 1);
	}

#if UNITY_EDITOR
	// UNITY GIZMOS
	private void OnDrawGizmosSelected()
	{
		Handles.color = new Color(0.75f, 0.25f, 0.125f, 1.0f);
		Handles.DrawWireCube(Vector3.zero, moveSizeBounds);
		Handles.color = Color.white;
	}
#endif
}