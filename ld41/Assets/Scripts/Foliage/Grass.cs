using utils;
using UnityEngine;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace Foliage
{
	public class Grass : MonoBehaviour {
	
		// Properties
		// =====================================================================

		private const float      _BEND_FACTOR        = 0.25f;
		private const float      _BEND_FORCE_ON_EXIT = 0.1f;
		private       float      _colliderHalfWidth;
		private       float      _enterOffset;
		private       float      _exitOffset;
		private       bool       _isBending;
		private       bool       _isRebounding;
		private       MeshFilter _meshFilter;
		private       Spring     _spring;
	
		// Unity
		// =====================================================================

		private void Start ()
		{
			_colliderHalfWidth = GetComponent<BoxCollider2D>().bounds.size.x / 2;
			_meshFilter        = GetComponent<MeshFilter>();
			_spring            = new Spring();
		}

		private void Update ()
		{
			if (_isRebounding)
			{
				SetVertHorizontalOffset(_spring.Simulate());
			
				// Apply the spring until its acceleration dies down
				if (Mathf.Abs(_spring.acceleration) < 0.00005f)
				{
					// Reset to neutral (0)
					SetVertHorizontalOffset(0f);
					_isRebounding = false;
				}
			}
		}

		private void OnTriggerEnter2D (Collider2D col)
		{
			if (col.gameObject.layer == GameManager.PLAYER_LAYER)
			{
				_enterOffset = col.transform.position.x - transform.position.x;

				if (GameManager.i.player.velocity.y < -3f)
				{
					if (col.transform.position.x < transform.position.x)
					{
						_spring.ApplyAdditiveForce(_BEND_FORCE_ON_EXIT);
					}
					else
					{
						_spring.ApplyAdditiveForce(-_BEND_FORCE_ON_EXIT);
					}

					_isRebounding = true;
				}
			}
		}

		private void OnTriggerStay2D (Collider2D col)
		{
			if (col.gameObject.layer == GameManager.PLAYER_LAYER)
			{
				float offset = col.transform.position.x - transform.position.x;
				if (_isBending || Mathf.Sign(_enterOffset) != Mathf.Sign(offset))
				{
					_isRebounding = false;
					_isBending    = true;
				
					// Figure out how far we have moved into the trigger and
					// then map the offset to -1 to 1. 0 Would be neutral, -1 to
					// the left and +1 ro the right.
					float radius = _colliderHalfWidth + col.bounds.size.x * 0.5f;
					_exitOffset = MathHelpers.Map(offset, -radius, radius, -1f, 1f);
					SetVertHorizontalOffset(_exitOffset);
				}
			}
		}

		private void OnTriggerExit2D (Collider2D col)
		{
			if (col.gameObject.layer == GameManager.PLAYER_LAYER)
			{
				if (_isBending)
				{
					// Apply force in the opposite direction that we are
					// currently bending
					_spring.ApplyForceStartingAtPosition(
						_BEND_FORCE_ON_EXIT * Mathf.Sign(_exitOffset),
						_exitOffset
					);
				}

				_isBending    = false;
				_isRebounding = true;
			}
		}

		// Helpers
		// =====================================================================

		private void SetVertHorizontalOffset (float offset)
		{
			Vector3[] verts = _meshFilter.mesh.vertices;

			verts[1].x = 0.5f + offset * _BEND_FACTOR / transform.localScale.x;
			verts[3].x = -0.5f + offset * _BEND_FACTOR / transform.localScale.x;

			_meshFilter.mesh.vertices = verts;
		}
	
	}
}
