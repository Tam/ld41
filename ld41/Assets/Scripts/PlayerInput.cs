using UnityEngine;
// ReSharper disable CompareOfFloatsByEqualityOperator

[RequireComponent(typeof(Player))]
public class PlayerInput : MonoBehaviour
{

	// Properties
	// =====================================================================
	
	private Player _player;
	private bool _isJumpPressed;
	private bool _jumpWasPressed;
	private int _jumpCount;
	
	// Unity
	// =====================================================================

	private void Start ()
	{
		_player = GetComponent<Player>();
	}

	private void Update ()
	{
		Vector2 directionalInput = new Vector2(
			Input.GetAxisRaw("Horizontal"),
			Input.GetAxisRaw("Vertical")
		);
		
		_player.SetDirectionalInput(directionalInput);

		if (_player.controller.collisions.below || _player.wallSliding)
			_jumpCount = 0;

		if (GetJump() && _jumpCount < 2)
		{
			_jumpCount++;
			_player.OnJumpInputDown();
		}

		if (GetJumpReleased())
			_player.OnJumpInputUp();
	}
	
	// Helpers
	// =====================================================================

	/// <summary>
	/// Prevent the player from holding down jump to jump constantly
	/// </summary>
	/// <returns></returns>
	private bool GetJump ()
	{
		if (Input.GetAxisRaw("Jump") != 0)
		{
			if (!_isJumpPressed)
			{
				_isJumpPressed  = true;
				_jumpWasPressed = false;
				return true;
			}
		}

		if (Input.GetAxisRaw("Jump") == 0)
		{
			_isJumpPressed  = false;
			_jumpWasPressed = true;
		}

		return false;
	}

	private bool GetJumpReleased ()
	{
		if (_jumpWasPressed)
		{
			_jumpWasPressed = false;
			return true;
		}

		return false;
	}
	
}
