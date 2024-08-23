using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Camera))]
public class VCamera : MonoBehaviour
{
	[SerializeField]
	private VCameraTarget m_followTarget;
	public VCameraTarget followTarget 
	{
		get => m_followTarget;
		set {
			m_followTarget = value;

			SetFollowTarget(m_followTarget);
		}
	}

	[Header("Controls")]
	[Tooltip("Value on 0.0 to 1.0 distance to target Min to Max.")]
	[Range(0.0f, 1.0f)]
	public float distanceValue = 0.5f;
	public float distanceOffset;
	public Vector2 rotation;

	[Header("Options")]
	public bool centerScreenOffsetEnabled;
	public bool dampingPositionEnabled;
	public bool dampingRotationEnabled;
	public bool dampingDistanceEnabled;
	public bool limitRotationXEnabled;

	[Header("Params")]
	public CenterScreenOffset centerScreenOffset;
	public DampingPosition dampingPosition;
	public DampingRotation dampingRotation;
	public DampingDistance dampingDistance;
	public LimitRotationX limitRotationX = new LimitRotationX() { min = 0.0f, max = 90.0f };

	public Cinematic cinematic;

	public bool IsValid => m_followTarget != null;

	private Camera _camera;
	public Camera baseCamera => _camera;

	private Vector3 _positionHelper;
	private Vector2 _rotationHelper;
	private float _distanceHelper;

	private Vector3 _positionOffset = Vector3.zero;

	private Vector3 _velocityDampPosition = Vector3.zero;
	private Vector2 _velocityDampRotation = Vector2.zero;
	private float _velocityDampDistance = 0.0f;

	// UNITY EVENTS
	private void Awake()
	{
		_camera = GetComponent<Camera>();
		cinematic.vCamera = this;
	}

	private void Start()
	{
		Init();
	}
	
	private void LateUpdate()
	{
		if (IsValid)
		{
			UpdateCamera();
			cinematic.Update();
		}
	}

	public void Init()
	{
		SetFollowTarget(m_followTarget);
	}

	public void SetFollowTarget(VCameraTarget target)
	{
		m_followTarget = target;

		if (IsValid)
		{
			ResetHelpers();
			UpdateCamera();
		}
	}

	public Vector3 GetPositionOfTarget(VCameraTarget target)
	{
		float currentDistance = target.limitDistance.min + ((target.limitDistance.max - target.limitDistance.min) * _distanceHelper);
		Vector3 result = _positionHelper + _positionOffset - (transform.forward * (currentDistance + distanceOffset));

		return result;
	}

	private void ResetHelpers()
	{
		_positionHelper = m_followTarget.transform.position;
		_rotationHelper = rotation;
		_distanceHelper = distanceValue;

		_velocityDampPosition = Vector3.zero;
		_velocityDampRotation = Vector2.zero;
		_velocityDampDistance = 0.0f;
	}

	private void UpdateCamera()
	{
		UpdateCameraRotation();
		UpdateCameraPosition();
		UpdateCameraPositionOffset();
		UpdateCameraDistance();

		if (!cinematic.isAnimated)
		{
			transform.position = GetPositionOfTarget(m_followTarget);
		}
	}

	private void UpdateCameraPosition()
	{
		if (dampingPositionEnabled)
		{
			Vector3 followTargetHelperLocal = m_followTarget.transform.position - _positionHelper;

			followTargetHelperLocal = Quaternion.AngleAxis(-transform.eulerAngles.y, Vector3.up) * followTargetHelperLocal;

			followTargetHelperLocal.x = Mathf.SmoothDamp(followTargetHelperLocal.x, 0.0f, ref _velocityDampPosition.x, dampingPosition.dampingTime.x);
			followTargetHelperLocal.y = Mathf.SmoothDamp(followTargetHelperLocal.y, 0.0f, ref _velocityDampPosition.y, dampingPosition.dampingTime.y);
			followTargetHelperLocal.z = Mathf.SmoothDamp(followTargetHelperLocal.z, 0.0f, ref _velocityDampPosition.z, dampingPosition.dampingTime.z);

			followTargetHelperLocal = Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up) * followTargetHelperLocal;

			_positionHelper = m_followTarget.transform.position - followTargetHelperLocal;
		}
		else
		{
			_velocityDampPosition = Vector3.zero;
			_positionHelper = m_followTarget.transform.position;
		}
	}
	private void UpdateCameraRotation()
	{
		if (limitRotationXEnabled)
		{
			rotation.x = limitRotationX.ClampRotation(rotation.x);
		}

		if (dampingRotationEnabled)
		{
			_rotationHelper.x = Mathf.SmoothDamp(_rotationHelper.x, rotation.x, ref _velocityDampRotation.x, dampingRotation.dampingTime.x);
			_rotationHelper.y = Mathf.SmoothDamp(_rotationHelper.y, rotation.y, ref _velocityDampRotation.y, dampingRotation.dampingTime.y);
		}
		else
		{
			_velocityDampRotation = Vector2.zero;
			_rotationHelper = rotation;
		}

		transform.rotation = Quaternion.Euler(_rotationHelper.x, _rotationHelper.y, 0.0f);
	}
	private void UpdateCameraDistance()
	{
		if (dampingDistanceEnabled)
		{
			_distanceHelper = Mathf.SmoothDamp(_distanceHelper, distanceValue, ref _velocityDampDistance, dampingDistance.dampingTime);
		}
		else
		{
			_velocityDampDistance = 0.0f;
			_distanceHelper = distanceValue;
		}
	}
	private void UpdateCameraPositionOffset()
	{
		if (centerScreenOffsetEnabled)
		{
			_positionOffset = Quaternion.AngleAxis(transform.eulerAngles.x, Vector3.right) * new Vector3(centerScreenOffset.offsetPosition.x, centerScreenOffset.offsetPosition.y, 0.0f);
			_positionOffset = Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up) * _positionOffset;
		}
		else
		{
			_positionOffset = Vector3.zero;
		}
	}

#if UNITY_EDITOR
	// UNITY GIZMOS
	private void OnDrawGizmosSelected()
	{
		if (Application.isPlaying && IsValid)
		{
			// Draw center offset
			if (!_positionOffset.Equals(Vector3.zero))
			{
				Handles.color = Color.grey;
				Handles.DrawWireCube(m_followTarget.transform.position + _positionOffset, Vector3.one * 0.1f);
				Handles.DrawLine(m_followTarget.transform.position, m_followTarget.transform.position + _positionOffset, 1.0f);
			}

			// Draw min/max distance into camera
			Vector3 directionToCamera = Vector3.Normalize(_positionHelper + _positionOffset - transform.position);

			Handles.color = Color.grey;
			Handles.DrawWireCube(m_followTarget.transform.position, Vector3.one * 0.1f);
			Handles.DrawLine(_positionHelper + _positionOffset, _positionHelper + _positionOffset - (directionToCamera * (m_followTarget.limitDistance.min + distanceOffset)), 1.0f);
			
			Handles.color = Color.yellow;
			Handles.DrawLine(_positionHelper + _positionOffset - (directionToCamera * (m_followTarget.limitDistance.min + distanceOffset)), _positionHelper + _positionOffset - (directionToCamera * (m_followTarget.limitDistance.max + distanceOffset)), 1.0f);
			
			Handles.DrawWireDisc(_positionHelper + _positionOffset - (directionToCamera * (m_followTarget.limitDistance.min + distanceOffset)), directionToCamera, 0.1f, 1.0f);
			Handles.Label(_positionHelper + _positionOffset - (directionToCamera * (m_followTarget.limitDistance.min + distanceOffset)), "min dist: " + m_followTarget.limitDistance.min.ToString());
			Handles.DrawWireDisc(_positionHelper + _positionOffset - (directionToCamera * (m_followTarget.limitDistance.max + distanceOffset)), directionToCamera, 0.2f, 1.0f);
			Handles.Label(_positionHelper + _positionOffset - (directionToCamera * (m_followTarget.limitDistance.max + distanceOffset)), "max dist: " + m_followTarget.limitDistance.max.ToString());

			// Draw follow target helper
			Handles.color = Color.cyan;
			Handles.DrawWireCube(_positionHelper + _positionOffset, Vector3.one * 0.05f);
			Handles.DrawLine(m_followTarget.transform.position + _positionOffset, _positionHelper + _positionOffset, 1.0f);
		}
	}
#endif

	// PARAMS
	[Serializable]
	public struct CenterScreenOffset
	{
		public Vector2 offsetPosition;
	}

	[Serializable]
	public struct DampingPosition
	{
		public Vector3 dampingTime;
	}
	[Serializable]
	public struct DampingRotation
	{
		public Vector2 dampingTime;
	}
	[Serializable]
	public struct DampingDistance
	{
		public float dampingTime;
	}

	[Serializable]
	public struct LimitRotationX
	{
		public float min;
		public float max;

		public float ClampRotation(float degress) => Mathf.Clamp(degress, min, max);
	}
}


[Serializable]
public class Cinematic
{
	[Header("Options")]
	public bool noiseEnabled;
	public bool shakeEnabled;

	[Header("Params")]
	public PositionAnimation positionAnimation = new PositionAnimation() { easeAnimCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f), duration = 1.0f };
	public ZoomingAnimation zoomingAnimation = new ZoomingAnimation() { easeAnimCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f), duration = 1.0f };
	public Noise noise = new Noise() { frequency = 0.0f, amplitude = 0.0f };
	public Shake shake = new Shake() { magnitude = 0.0f, roughness = 0.0f };

	internal VCamera vCamera;

	public bool isAnimated => _isPositionAnimated || _isZoomingAnimated;

	private bool _isPositionAnimated;
	private bool _isZoomingAnimated;

	private float _positionAnimationTimer;
	private Vector3 _positionAnimationInValue;
	private float _zoomingAnimationTimer;
	private float _zoomingAnimationInValue;
	private float _zoomingAnimationOutValue;


	public void PositionToFollowTarget(VCameraTarget followTarget)
	{
		_isPositionAnimated = true;

		_positionAnimationTimer = 0.0f;
		_positionAnimationInValue = vCamera.transform.position;

		vCamera.SetFollowTarget(followTarget);
	}

	public void ZoomingToFollowTarget(float inValue, float outValue = 0.0f)
	{
		_isZoomingAnimated = true;

		_zoomingAnimationTimer = 0.0f;
		_zoomingAnimationInValue = vCamera.distanceOffset + inValue;
		_zoomingAnimationOutValue = vCamera.distanceOffset + outValue;
	}


	public void Update()
	{
		if (_isPositionAnimated)
		{
			_positionAnimationTimer += Time.deltaTime;
			vCamera.transform.position = Vector3.Lerp(_positionAnimationInValue, vCamera.GetPositionOfTarget(vCamera.followTarget), positionAnimation.easeAnimCurve.Evaluate(_positionAnimationTimer / positionAnimation.duration));
			
			if (_positionAnimationTimer >= positionAnimation.duration)
			{
				_isPositionAnimated = false;
				_positionAnimationTimer = 0.0f;
				_positionAnimationInValue = Vector3.zero;

				vCamera.transform.position = vCamera.GetPositionOfTarget(vCamera.followTarget);

				positionAnimation.OnAnimationDone?.Invoke();
			}
		}

		if (_isZoomingAnimated)
		{
			_zoomingAnimationTimer += Time.deltaTime;
			vCamera.distanceOffset = Mathf.Lerp(_zoomingAnimationInValue, _zoomingAnimationOutValue, zoomingAnimation.easeAnimCurve.Evaluate(_zoomingAnimationTimer / zoomingAnimation.duration));
			if (!_isPositionAnimated)
			{
				vCamera.transform.position = vCamera.GetPositionOfTarget(vCamera.followTarget);
			}
			
			if (_zoomingAnimationTimer >= zoomingAnimation.duration)
			{
				_isZoomingAnimated = false;
				_zoomingAnimationTimer = 0.0f;
				_zoomingAnimationInValue = 0.0f;

				vCamera.distanceOffset = _zoomingAnimationOutValue;

				_zoomingAnimationOutValue = 0.0f;

				zoomingAnimation.OnAnimationDone?.Invoke();
			}
		}



	}

	// PARAMS
	[Serializable]
	public struct PositionAnimation
	{
		public AnimationCurve easeAnimCurve;
		public float duration;

		[Header("Events")]
		public UnityEvent OnAnimationDone;
	}

	[Serializable]
	public struct ZoomingAnimation
	{
		public AnimationCurve easeAnimCurve;
		public float duration;

		[Header("Events")]
		public UnityEvent OnAnimationDone;
	}

	[Serializable]
	public struct Noise
	{
		public float frequency;
		public float amplitude;
	}

	[Serializable]
	public struct Shake
	{
		public float magnitude;
		public float roughness;
	}
}