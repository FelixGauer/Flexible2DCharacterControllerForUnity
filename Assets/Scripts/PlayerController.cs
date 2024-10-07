using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.Scripting.APIUpdating;

public class PlayerController : MonoBehaviour
{
	[Header("References")]
	[SerializeField] InputReader input;
	[SerializeField] PlayerControllerStats stats; // TODO rename
	private Rigidbody2D _rigidbody;

	private CapsuleCollider2D _capsuleCollider;
	private BoxCollider2D _feetCollider;

	private Vector2 _moveDirection;
	private bool _jumpPerforemd;
	private Vector2 _moveVelocity;

	// TODO Add Region or header
	private RaycastHit2D _groundHit;
	private RaycastHit2D _headHit;
	private bool _isGrounded;
	private bool _bumbedHead;

	//TODO Реализовать дебаг функции для всех вопросов 



	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		_capsuleCollider = GetComponentInChildren<CapsuleCollider2D>();
		_feetCollider = GetComponentInChildren<BoxCollider2D>();
	}

	private void FixedUpdate()
	{
		CollisionChecks();
		HandleMovement();

		ApplyMovement();
		
		Debug.Log(_isGrounded);

	}

	private void ApplyMovement()
	{
		// _rigidbody.velocity = _moveVelocity;
		_rigidbody.velocity = new Vector2(_moveVelocity.x, _rigidbody.velocity.y);
	}

	private void HandleMovement()
	{
		Vector2 targetVelocity = _moveDirection != Vector2.zero
			? new Vector2(_moveDirection.x, 0f) * stats.MoveSpeed
			: Vector2.zero;

		float smoothFactor = _moveDirection != Vector2.zero
			? stats.Acceleration
			: stats.Deceleration;

		// MoveToWord Test
		_moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, smoothFactor * Time.fixedDeltaTime);
		// _rigidbody.velocity = new Vector2(_moveVelocity.x, _rigidbody.velocity.y);

		// Debug.Log(_moveVelocity);
	}

	private void CollisionChecks()
	{
		IsGroundedVer2();
	}

	private void IsGroundedVer2()
	{
		var bounds = _feetCollider.bounds;
		Vector2 boxCastOrigin = new Vector2(bounds.center.x, bounds.min.y);
		Vector2 boxCastSize = new Vector2(bounds.size.x, stats.GroundDetectionRayLenght);

		_isGrounded = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, stats.GroundDetectionRayLenght, stats.GroundLayer).collider != null;
	}

	private void IsGrounded()
	{
		Vector2 boxCastOrigin = new Vector2(_feetCollider.bounds.center.x, _feetCollider.bounds.min.y);
		Vector2 boxCastSize = new Vector2(_feetCollider.bounds.size.x, stats.GroundDetectionRayLenght);

		_groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, stats.GroundDetectionRayLenght, stats.GroundLayer);

		if (_groundHit.collider != null)
		{
			_isGrounded = true;
		}
		else
		{
			_isGrounded = false;
		}


		// Color rayColor;
		// if (_isGrounded)
		// {
		// 	rayColor = Color.green;
		// }
		// else
		// {
		// 	rayColor = Color.red;
		// }

		// Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * stats.GroundDetectionRayLenght, rayColor);
		// Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * stats.GroundDetectionRayLenght, rayColor);
		// Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - stats.GroundDetectionRayLenght), Vector2.right * boxCastSize.y, rayColor);


		// Debug.Log(_groundHit.collider);

	}

	#region OnEnableDisable
	void OnEnable()
	{
		input.Move += OnMove;
		input.Jump += OnJump;
	}

	void OnDisable()
	{
		input.Move -= OnMove;
		input.Jump -= OnJump;
	}
	#endregion

	#region OnMethodActions
	private void OnMove(Vector2 moveDirection)
	{
		_moveDirection = moveDirection;
	}

	private void OnJump(bool performed)
	{
		if (performed)
			_jumpPerforemd = true;
		else
			_jumpPerforemd = false;

		// Debug.Log(performed);
	}
	#endregion
}
