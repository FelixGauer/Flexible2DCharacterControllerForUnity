using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	[SerializeField] InputReader input;

	private Rigidbody2D _rigidbody;
	private BoxCollider2D _collider;


	private Vector2 _moveDirection;
	private bool _jumpPerforemd;


	[Header("Movement Settings")]
	private Vector2 _moveVelocity;
	private float _moveSpeed = 12.5f;
	private float _acceleration = 5f;
	private float _deacceleration = 20f;


	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		_collider = GetComponent<BoxCollider2D>();
	}

	private void FixedUpdate()
	{
		// HandleMovementVer1();
		// HandleMovementVer2();
		// HandleMovementVer3();
		HandleMovementVer4();

		// ApplyMovement();
	}

	// private void ApplyMovement()
	// {
	// 	// _rigidbody.velocity = _moveVelocity;
	// 	_rigidbody.velocity = new Vector2(_moveVelocity.x, _rigidbody.velocity.y);
	// }

	private void HandleMovementVer1()
	{
		if (_moveDirection != Vector2.zero)
		{
			Vector2 targetVelocity = Vector2.zero;
			targetVelocity = new Vector2(_moveDirection.x, 0f) * _moveSpeed;

			_moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, _acceleration * Time.fixedDeltaTime);
			// _moveVelocity = Vector2.MoveTowards(_moveVelocity, targetVelocity, 5f * Time.fixedDeltaTime);

			_rigidbody.velocity = new Vector2(_moveVelocity.x, _rigidbody.velocity.y);
		}
		else if (_moveDirection == Vector2.zero)
		{
			_moveVelocity = Vector2.Lerp(_moveVelocity, Vector2.zero, _deacceleration * Time.fixedDeltaTime);
			// _moveVelocity = Vector2.MoveTowards(_moveVelocity, Vector2.zero, 20f * Time.fixedDeltaTime);
			_rigidbody.velocity = new Vector2(_moveVelocity.x, _rigidbody.velocity.y);
		}

		Debug.Log(_moveVelocity);
	}

	private void HandleMovementVer2()
	{
		float targetSpeed = (_moveDirection != Vector2.zero) ? _moveSpeed : 0f;
		Vector2 targetVelocity = new Vector2(_moveDirection.x, 0f) * targetSpeed;

		_moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, (_moveDirection.magnitude > 0 ? _acceleration : _deacceleration) * Time.fixedDeltaTime);

		_rigidbody.velocity = new Vector2(_moveVelocity.x, _rigidbody.velocity.y);

		Debug.Log(_moveVelocity);
	}

	private void HandleMovementVer3()
	{
		float targetSpeed = _moveDirection.magnitude > 0 ? _moveSpeed : 0f;
		Vector2 targetVelocity = _moveDirection.normalized * targetSpeed;

		_moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, ((_moveDirection != Vector2.zero) ? _acceleration : _deacceleration) * Time.fixedDeltaTime);

		_rigidbody.velocity = new Vector2(_moveVelocity.x, _rigidbody.velocity.y);

		Debug.Log(_moveVelocity);
	}

	private void HandleMovementVer4()
	{
		Vector2 targetVelocity = _moveDirection != Vector2.zero
			? new Vector2(_moveDirection.x, 0f) * _moveSpeed
			: Vector2.zero;

		float smoothFactor = _moveDirection != Vector2.zero 
			? _acceleration 
			: _deacceleration;

		_moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, smoothFactor * Time.fixedDeltaTime);
		_rigidbody.velocity = new Vector2(_moveVelocity.x, _rigidbody.velocity.y);

		Debug.Log(_moveVelocity);
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
