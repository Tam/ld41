using UnityEngine;
// ReSharper disable CompareOfFloatsByEqualityOperator

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour {
	
	// Parameters
	// =====================================================================

	
	public Controller2D controller;
	public float maxJumpHeight = 3.5f;
	public float minJumpHeight = 1f;
	public float timeToJumpApex = 0.3f;
	public float wallSlideSpeedMax = 3f;
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
	private float _wallStickTime = 0.1f;
	private float _timeToWallUnstick;

	public bool wallSliding;
	private int _wallDirX;
	
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
		controller = GetComponent<Controller2D>();

		_gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
		_maxJumpVelocity = Mathf.Abs(_gravity) * timeToJumpApex;
		_minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(_gravity) * minJumpHeight);
	}

	private void Update ()
	{
		CalculateVelocity();
		HandleWallSliding();
		
		controller.Move(_velocity * Time.deltaTime, _directionalInput);

		if (controller.collisions.above || controller.collisions.below)
		{
			if (controller.collisions.slidingDownMaxSlope)
			{
				_velocity.y += controller.collisions.slopeNormal.y
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
		wallSliding = false;
		_wallDirX = controller.collisions.left ? -1 : 1;

		if (
			(controller.collisions.left || controller.collisions.right)
			&& !controller.collisions.below
			&& _velocity.y < 0
		) {
			wallSliding = true;

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
					_timeToWallUnstick = _wallStickTime;
				}
			}
			else
			{
				_timeToWallUnstick = _wallStickTime;
			}
		}
	}
	
	// Events
	// =====================================================================

	public void OnJumpInputDown ()
	{
		if (wallSliding)
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

		if (controller.collisions.slidingDownMaxSlope)
		{
			// If not jumping against max slope
			if (
				_directionalInput.x
				!= -Mathf.Sign(controller.collisions.slopeNormal.x)
			) {
				_velocity.y = 
					_maxJumpVelocity * controller.collisions.slopeNormal.y;
				_velocity.x = 
					_maxJumpVelocity * controller.collisions.slopeNormal.x;
			}
		}
		else
		{
			_velocity.y = _maxJumpVelocity;
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
			controller.collisions.below 
				? _accelerationTimeGrounded 
				: _accelerationTimeAirborne
		);
		
		_velocity.y += _gravity * Time.deltaTime;
	}

}
