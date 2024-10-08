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


	// InputReader parameters
	private Vector2 _moveDirection;
	private bool _jumpPerforemd;

	// CollisionChecks parameters
	private RaycastHit2D _groundHit;
	private RaycastHit2D _headHit;
	private bool _isGrounded;
	private bool _bumbedHead;

	//
	private Vector2 _moveVelocity;
	private Vector2 targetVelocity;

	//TODO Реализовать дебаг функции для всех вопросов 

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		_capsuleCollider = GetComponentInChildren<CapsuleCollider2D>();
		_feetCollider = GetComponentInChildren<BoxCollider2D>();
	}

	private void Update()
	{
		Debbuging();
	}

	private void FixedUpdate()
	{
		CollisionChecks();
		HandleMovement();

		ApplyMovement();
	}

	private void ApplyMovement()
	{
		_rigidbody.velocity = _moveVelocity;
		// _rigidbody.velocity = new Vector2(_moveVelocity.x, _rigidbody.velocity.y);
	}

	private void HandleMovement()
	{
		targetVelocity = _moveDirection != Vector2.zero
			? new Vector2(_moveDirection.x, 0f) * stats.MoveSpeed
			: Vector2.zero;

		float smoothFactor = _moveDirection != Vector2.zero
			? stats.Acceleration
			: stats.Deceleration;

		// MoveToWord 
		_moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, smoothFactor * Time.fixedDeltaTime);
		// _rigidbody.velocity = new Vector2(_moveVelocity.x, _rigidbody.velocity.y);

		// Debug.Log(_moveVelocity);
	}

	#region CollisionChecks

	private void CollisionChecks()
	{
		IsGrounded();
	}

	private void IsGrounded()
	{
		var bounds = _feetCollider.bounds;
		Vector2 boxCastOrigin = new Vector2(bounds.center.x, bounds.min.y);
		Vector2 boxCastSize = new Vector2(bounds.size.x, stats.GroundDetectionRayLenght);

		_isGrounded = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, stats.GroundDetectionRayLenght, stats.GroundLayer).collider != null;
	}

	#endregion

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


	//TODO Дебаг для Grounded
	private void Debbuging()
	{
		// Визуализация целевой скорости (Target Velocity)
		Debug.DrawLine(transform.position, transform.position + new Vector3(targetVelocity.x, 0, 0), Color.red);

		// Визуализация текущего вектора скорости (_moveVelocity)
		Debug.DrawLine(transform.position, transform.position + new Vector3(_moveVelocity.x, 0, 0), Color.blue);

		// Визуализация направления движения (_moveDirection)
		if (_moveDirection != Vector2.zero)
		{
			Debug.DrawLine(transform.position, transform.position + new Vector3(_moveDirection.x, 0, 0), Color.green);
		}
		else
		{
			// Визуализация замедления
			if (_moveVelocity != Vector2.zero)
			{
				Debug.DrawLine(transform.position, transform.position + new Vector3(_moveVelocity.x, 0, 0), Color.yellow);
			}
		}
	}
}

