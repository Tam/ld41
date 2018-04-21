using System.Collections.Generic;
using UnityEngine;
// ReSharper disable CompareOfFloatsByEqualityOperator

internal struct PassengerMovement
{
	public Transform transform;
	public Vector3 velocity;
	public bool standingOnPlatform;
	public bool moveBeforePlatform;

	public PassengerMovement (
		Transform _transform,
		Vector3   _velocity,
		bool      _standingOnPlatform,
		bool      _moveBeforePlatform
	) {
		transform = _transform;
		velocity = _velocity;
		standingOnPlatform = _standingOnPlatform;
		moveBeforePlatform = _moveBeforePlatform;
	}
}

public class PlatformController : RaycastController {
	
	// Properties
	// =====================================================================

	public LayerMask passengerMask;
	public bool cyclic;
	public float speed = 2.5f;
	[Range(0, 2)]
	public float easeAmount = 0.75f;
	public float waitTime;
	public Vector3[] localWaypoints;

	private float _nextMoveTime;
	private int _fromWaypointIndex;
	private float _percentBetweenWaypoints;
	private Vector3[] _globalWaypoints;
	private List<PassengerMovement> _passengerMovement;
	private Dictionary<Transform, Controller2D> _passengerDictionary =
		new Dictionary<Transform, Controller2D>();
	
	// Unity
	// =====================================================================

	protected override void Start ()
	{
		base.Start();
		
		_globalWaypoints = new Vector3[localWaypoints.Length];
		for (int i = 0; i < localWaypoints.Length; i++)
			_globalWaypoints[i] = localWaypoints[i] + transform.position;
	}

	private void Update ()
	{
		UpdateRaycastOrigins();
		
		Vector3 velocity = CalculatePlatformMovement();
		
		CalculatePassengerMovement(velocity);
		
		MovePassengers(true);
		transform.Translate(velocity);
		MovePassengers(false);
	}

	private void OnDrawGizmos ()
	{
		if (localWaypoints != null)
		{
			Gizmos.color = Color.red;
			float size = 0.3f;

			for (int i = 0; i < localWaypoints.Length; i++)
			{
				Vector3 globalWaypointPosition = 
					Application.isPlaying 
						? _globalWaypoints[i] 
						: localWaypoints[i] + transform.position;
				Gizmos.DrawLine(
					globalWaypointPosition - Vector3.up * size, 
					globalWaypointPosition + Vector3.up * size
				);
				Gizmos.DrawLine(
					globalWaypointPosition - Vector3.left * size, 
					globalWaypointPosition + Vector3.left * size
				);
			}
		}
	}

	// Actions
	// =====================================================================

	private float Ease (float x)
	{
		float a = easeAmount + 1;
		return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
	}

	private Vector3 CalculatePlatformMovement ()
	{
		if (Time.time < _nextMoveTime) 
			return Vector3.zero;
		
		_fromWaypointIndex %= _globalWaypoints.Length;
		int toWaypointIndex = (_fromWaypointIndex + 1) % _globalWaypoints.Length;
		float distanceBetweenWaypoints = Vector3.Distance(
			_globalWaypoints[_fromWaypointIndex],
			_globalWaypoints[toWaypointIndex]
		);

		_percentBetweenWaypoints += Time.deltaTime * speed / distanceBetweenWaypoints;
		_percentBetweenWaypoints = Mathf.Clamp01(_percentBetweenWaypoints);

		float easedPercentBetweenWaypoints = Ease(_percentBetweenWaypoints);

		Vector3 newPos = Vector3.Lerp(
			_globalWaypoints[_fromWaypointIndex],
			_globalWaypoints[toWaypointIndex],
			easedPercentBetweenWaypoints
		);

		if (_percentBetweenWaypoints >= 1)
		{
			_percentBetweenWaypoints = 0;
			_fromWaypointIndex++;

			if (!cyclic)
			{
				if (_fromWaypointIndex >= _globalWaypoints.Length - 1)
				{
					_fromWaypointIndex = 0;
					System.Array.Reverse(_globalWaypoints);
				}
			}

			_nextMoveTime = Time.time + waitTime;
		}

		return newPos - transform.position;
	}

	private void MovePassengers (bool beforeMovePlatform)
	{
		foreach (PassengerMovement passenger in _passengerMovement)
		{
			if (!_passengerDictionary.ContainsKey(passenger.transform))
			{
				_passengerDictionary.Add(
					passenger.transform, 
					passenger.transform.GetComponent<Controller2D>()
				);
			}
			
			if (passenger.moveBeforePlatform == beforeMovePlatform)
			{
				_passengerDictionary[passenger.transform].Move(
					passenger.velocity,
					passenger.standingOnPlatform
				);
			}
		}
	}

	private void CalculatePassengerMovement (Vector3 velocity)
	{
		HashSet<Transform> movedPassengers = new HashSet<Transform>();
		_passengerMovement = new List<PassengerMovement>();
		
		float directionX = Mathf.Sign(velocity.x);
		float directionY = Mathf.Sign(velocity.y);
		
		// Vertically moving platform
		if (velocity.y != 0)
		{
			float rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;

			for (int i = 0; i < verticalRayCount; i++)
			{
				Vector2 rayOrigin = directionY == -1
					? raycastOrigins.bottomLeft
					: raycastOrigins.topLeft;

				rayOrigin += Vector2.right * (verticalRaySpacing * i);

				RaycastHit2D hit = Physics2D.Raycast(
					rayOrigin,
					Vector2.up * directionY,
					rayLength,
					passengerMask
				);

				if (hit && hit.distance != 0)
				{
					if (!movedPassengers.Contains(hit.transform))
					{
						movedPassengers.Add(hit.transform);
						float pushX = directionY == 1 ? velocity.x : 0;
						float pushY = velocity.y
									  - (hit.distance - SKIN_WIDTH)
									  * directionY;

						_passengerMovement.Add(
							new PassengerMovement(
								hit.transform,
								new Vector3(pushX, pushY),
								directionY == 1,
								true
							)
						);
					}
				}
			}
		}
		
		// Horizontally moving platform
		if (velocity.x != 0)
		{
			float rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;

			for (int i = 0; i < horizontalRayCount; i++)
			{
				Vector2 rayOrigin = directionX == -1
					? raycastOrigins.bottomLeft
					: raycastOrigins.bottomRight;

				rayOrigin += Vector2.up * (horizontalRaySpacing * i);

				RaycastHit2D hit = Physics2D.Raycast(
					rayOrigin,
					Vector2.right * directionX,
					rayLength,
					passengerMask
				);
				
				if (hit && hit.distance != 0)
				{
					if (!movedPassengers.Contains(hit.transform))
					{
						movedPassengers.Add(hit.transform);
						float pushX = velocity.x - (hit.distance - SKIN_WIDTH) 
									  * directionX;
						float pushY = -SKIN_WIDTH;

						_passengerMovement.Add(
							new PassengerMovement(
								hit.transform,
								new Vector3(pushX, pushY),
								false,
								true
							)
						);
					}
				}
			}
		}
		
		// Passenger on top of a horizontally or downward moving platform
		if (directionY == -1 || velocity.y == 0 && velocity.x != 0)
		{
			float rayLength = SKIN_WIDTH * 2;

			for (int i = 0; i < verticalRayCount; i++)
			{
				Vector2 rayOrigin = 
					raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);

				RaycastHit2D hit = Physics2D.Raycast(
					rayOrigin,
					Vector2.up,
					rayLength,
					passengerMask
				);

				if (hit && hit.distance != 0)
				{
					if (!movedPassengers.Contains(hit.transform))
					{
						movedPassengers.Add(hit.transform);
						float pushX = velocity.x;
						float pushY = velocity.y;

						_passengerMovement.Add(
							new PassengerMovement(
								hit.transform,
								new Vector3(pushX, pushY),
								true,
								false
							)
						);
					}
				}
			}
		}
	}
	
}
