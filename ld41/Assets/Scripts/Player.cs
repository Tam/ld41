using UnityEngine;
// ReSharper disable CompareOfFloatsByEqualityOperator

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour {
	
	// Parameters
	// =====================================================================

	public float maxJumpHeight = 3.5f;
	public float minJumpHeight = 1f;
	public float timeToJumpApex = 0.3f;
	public float wallSlideSpeedMax = 3f;
	private float wallStickTime = 0.1f;
	public Vector2 wallJumpClimb;
	public Vector2 wallJumpOff;
	public Vector2 wallLeap;
	
	private float _moveSpeed = 10;
	private float _accelerationTimeAirborne = 0.1f;
	private float _accelerationTimeGrounded = 0.05f;

	private float _velocityXSmoothing;
	private float _gravity;
	private float _maxJumpVelocity;
	private float _minJumpVelocity;
	private float _timeToWallUnstick;

	private bool _wallSliding;
	private int _wallDirX;
	
	private Controller2D _controller;
	private Vector3 _velocity;
	private Vector2 _directionalInput;

	public Vector3 velocity
	{
		get { return _velocity; }
	}

	// Unity
	// =====================================================================

	private void Start()
	{
		_controller = GetComponent<Controller2D>();

		_gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
		_maxJumpVelocity = Mathf.Abs(_gravity) * timeToJumpApex;
		_minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(_gravity) * minJumpHeight);
	}

	private void Update ()
	{
		CalculateVelocity();
		HandleWallSliding();
		
		_controller.Move(_velocity * Time.deltaTime, _directionalInput);

		if (_controller.collisions.above || _controller.collisions.below)
		{
			if (_controller.collisions.slidingDownMaxSlope)
			{
				_velocity.y += _controller.collisions.slopeNormal.y
							   * -_gravity
							   * Time.deltaTime;
			}
			else
			{
				_velocity.y = 0;
			}
		}
	}
	
	// Actions
	// =====================================================================

	public void SetDirectionalInput (Vector2 input)
	{
		_directionalInput = input;
	}

	private void HandleWallSliding ()
	{
		_wallSliding = false;
		_wallDirX = _controller.collisions.left ? -1 : 1;

		if (
			(_controller.collisions.left || _controller.collisions.right)
			&& !_controller.collisions.below
			&& _velocity.y < 0
		) {
			_wallSliding = true;

			if (_velocity.y < -wallSlideSpeedMax)
				_velocity.y = -wallSlideSpeedMax;

			if (_timeToWallUnstick > 0)
			{
				_velocityXSmoothing = 0;
				_velocity.x         = 0;
				
				if (_directionalInput.x != _wallDirX && _directionalInput.x != 0)
				{
					_timeToWallUnstick -= Time.deltaTime;
				}
				else
				{
					_timeToWallUnstick = wallStickTime;
				}
			}
			else
			{
				_timeToWallUnstick = wallStickTime;
			}
		}
	}
	
	// Events
	// =====================================================================

	public void OnJumpInputDown ()
	{
		if (_wallSliding)
		{
			if (_wallDirX == Mathf.Round(_directionalInput.x))
			{
				_velocity.x = -_wallDirX * wallJumpClimb.x;
				_velocity.y = wallJumpClimb.y;
			}
			else if (_directionalInput.x == 0)
			{
				_velocity.x = -_wallDirX * wallJumpOff.x;
				_velocity.y = wallJumpOff.y;
			}
			else
			{
				_velocity.x = -_wallDirX * wallLeap.x;
				_velocity.y = wallLeap.y;
			}
		}

		if (_controller.collisions.below)
		{
			if (_controller.collisions.slidingDownMaxSlope)
			{
				// If not jumping against max slope
				if (
					_directionalInput.x
					!= -Mathf.Sign(_controller.collisions.slopeNormal.x)
				) {
					_velocity.y = 
						_maxJumpVelocity * _controller.collisions.slopeNormal.y;
					_velocity.x = 
						_maxJumpVelocity * _controller.collisions.slopeNormal.x;
				}
			}
			else
			{
				_velocity.y = _maxJumpVelocity;
			}
		}
	}

	public void OnJumpInputUp ()
	{
		if (_velocity.y > _minJumpVelocity)
			_velocity.y = _minJumpVelocity;
	}
	
	// Helpers
	// =====================================================================

	private void CalculateVelocity ()
	{
		float targetVelocityX = _directionalInput.x * _moveSpeed;
		
		_velocity.x = Mathf.SmoothDamp(
			_velocity.x,
			targetVelocityX,
			ref _velocityXSmoothing,
			_controller.collisions.below 
				? _accelerationTimeGrounded 
				: _accelerationTimeAirborne
		);
		
		_velocity.y += _gravity * Time.deltaTime;
	}

}
