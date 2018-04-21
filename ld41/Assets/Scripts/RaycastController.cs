using UnityEngine;

public struct RaycastOrigins
{
	public Vector2 topLeft, topRight, bottomLeft, bottomRight;
}

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour {
	
	// Properties
	// =====================================================================

	public LayerMask     collisionMask;

	protected const float DST_BETWEEN_RAYS = 0.25f;
	protected const float SKIN_WIDTH = 0.015f;
	protected int horizontalRayCount;
	protected int verticalRayCount;

	[HideInInspector]
	public new BoxCollider2D collider;
	protected RaycastOrigins raycastOrigins;

	protected float horizontalRaySpacing;
	protected float verticalRaySpacing;
	
	// Unity
	// =====================================================================

	private void Awake ()
	{
		collider       = GetComponent<BoxCollider2D>();
		raycastOrigins = new RaycastOrigins();
	}

	protected virtual void Start ()
	{
		CalculateRaySpacing();
	}
	
	// Helpers
	// =====================================================================

	protected void UpdateRaycastOrigins ()
	{
		Bounds bounds = collider.bounds;
		bounds.Expand(SKIN_WIDTH * -2);

		raycastOrigins.bottomLeft  = new Vector2(bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
		raycastOrigins.topLeft     = new Vector2(bounds.min.x, bounds.max.y);
		raycastOrigins.topRight    = new Vector2(bounds.max.x, bounds.max.y);
	}

	private void CalculateRaySpacing ()
	{
		Bounds bounds = collider.bounds;
		bounds.Expand(SKIN_WIDTH * -2);

		float boundsWidth = bounds.size.x;
		float boundsHeight = bounds.size.y;

		horizontalRayCount = Mathf.RoundToInt(boundsHeight / DST_BETWEEN_RAYS);
		verticalRayCount   = Mathf.RoundToInt(boundsWidth / DST_BETWEEN_RAYS);

		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing   = bounds.size.x / (verticalRayCount - 1);
	}
	
}
