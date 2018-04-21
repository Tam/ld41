using UnityEngine;
// ReSharper disable CompareOfFloatsByEqualityOperator

internal struct FocusArea
{
	public Vector2 centre;
	public Vector2 velocity;
	private float _left, _right, _top, _bottom;

	public FocusArea (Bounds targetBounds, Vector2 size)
	{
		_left = targetBounds.center.x - size.x / 2;
		_right = targetBounds.center.x + size.x / 2;
		_bottom = targetBounds.min.y;
		_top = targetBounds.min.y + size.y;

		velocity = Vector2.zero;
		
		centre = new Vector2(
			(_left + _right) / 2,
			(_top + _bottom) / 2
		);
	}

	public void Update (Bounds targetBounds)
	{
		float shiftX = 0;
		
		if (targetBounds.min.x < _left)
			shiftX = targetBounds.min.x - _left;
		else if (targetBounds.max.x > _right)
			shiftX = targetBounds.max.x - _right;

		_left += shiftX;
		_right += shiftX;
		
		float shiftY = 0;
		
		if (targetBounds.min.y < _bottom)
			shiftY = targetBounds.min.y - _bottom;
		else if (targetBounds.max.y > _top)
			shiftY = targetBounds.max.y - _top;

		_bottom += shiftY;
		_top += shiftY;
		
		centre = new Vector2(
			(_left + _right) / 2,
			(_top + _bottom) / 2
		);
		
		velocity = new Vector2(shiftX, shiftY);
	}
}

public class CameraFollow : MonoBehaviour {
	
	// Properties
	// =====================================================================

	public Controller2D target;
	public Vector2 focusAreaSize;
	public float verticalOffset;
	public float lookAheadDstX;
	public float lookSmoothTimeX;
	public float verticalSmoothTime;

	private FocusArea _focusArea;
	private float _currentLookAheadX;
	private float _targetLookAheadX;
	private float _lookAheadDirX;
	private float _smoothLookVelocityX;
	private float _smoothVelocityY;
	private bool _lookAheadStopped;
	
	// Unity
	// =====================================================================

	private void Start ()
	{
		_focusArea = new FocusArea(target.collider.bounds, focusAreaSize);
	}

	private void LateUpdate ()
	{
		_focusArea.Update(target.collider.bounds);
		
		Vector2 focusPosition = _focusArea.centre + Vector2.up * verticalOffset;

		if (_focusArea.velocity.x != 0)
		{
			_lookAheadDirX = Mathf.Sign(_focusArea.velocity.x);

			if (
				Mathf.Sign(target.playerInput.x) == Mathf.Sign(_focusArea.velocity.x) 
				&& target.playerInput.x != 0
			) {
				_lookAheadStopped = false;
				_targetLookAheadX = _lookAheadDirX * lookAheadDstX;
			}
			else
			{
				if (!_lookAheadStopped)
				{
					_lookAheadStopped = true;
					_targetLookAheadX =
						_currentLookAheadX
						+ (_lookAheadDirX * lookAheadDstX - _currentLookAheadX) / 4f;
				}
			}
		}

		_currentLookAheadX = Mathf.SmoothDamp(
			_currentLookAheadX,
			_targetLookAheadX,
			ref _smoothLookVelocityX,
			lookSmoothTimeX
		);

		focusPosition.y = Mathf.SmoothDamp(
			transform.position.y,
			focusPosition.y,
			ref _smoothVelocityY,
			verticalSmoothTime
		);
		focusPosition += Vector2.right * _currentLookAheadX;
		
		transform.position = (Vector3)focusPosition + Vector3.forward * -10;
	}

	private void OnDrawGizmos ()
	{
		Gizmos.color = new Color(1, 0, 1, 0.5f);
		Gizmos.DrawCube(_focusArea.centre, focusAreaSize);
	}
}
