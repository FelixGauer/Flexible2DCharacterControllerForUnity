using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	[Header("References")]
	[SerializeField] public InputReader input;
	[SerializeField] public PlayerControllerStats stats; // FIXME private
	[SerializeField] public Transform spriteTransform;

	private CollisionsChecker _collisionsChecker;
	private Rigidbody2D _rigidbody;
	public CapsuleCollider2D _capsuleCollider;

	// InputReader parameters
	private Vector2 _moveDirection;

	//Debug var
	private float maxYPosition;
	private TrailRenderer _trailRenderer;
	private List<GameObject> markers = new List<GameObject>();

	// Timers
	private CountdownTimer _jumpCoyoteTimer;
	private CountdownTimer _jumpBufferTimer;
	private CountdownTimer _wallJumpTimer;
	private CountdownTimer _dashTimer;
	private CountdownTimer _crouchRollTimer;

	// State Machine Var
	private StateMachine _stateMachine;

	public PlayerPhysicsController playerPhysicsController;

	private Animator _animator;
	
	private TurnChecker _turnChecker;
	// private bool IsFacingRight => _turnChecker.IsFacingRight;
	private bool _isSitting = false;
	
	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		_collisionsChecker = GetComponent<CollisionsChecker>();
		_capsuleCollider = GetComponentInChildren<CapsuleCollider2D>();
		_trailRenderer = GetComponent<TrailRenderer>();
		
		_collisionsChecker.IsSitting = () => _isSitting;
		_collisionsChecker.IsFacingRight = () => _turnChecker.IsFacingRight;

		_jumpCoyoteTimer = new CountdownTimer(stats.CoyoteTime);
		_jumpBufferTimer = new CountdownTimer(stats.BufferTime);
		_wallJumpTimer = new CountdownTimer(stats.WallJumpTime);
		_dashTimer = new CountdownTimer(stats.DashTime);
		_crouchRollTimer = new CountdownTimer(stats.CrouchRollTime);

		_turnChecker = new TurnChecker(); // FIXME

		playerPhysicsController = new PlayerPhysicsController(_rigidbody, _jumpCoyoteTimer, _jumpBufferTimer, _collisionsChecker, stats, this, _dashTimer, _turnChecker, _wallJumpTimer, _crouchRollTimer);
		
		_animator = GetComponentInChildren<Animator>(); // FIXME
		
		SetupStateMachine();
	}

	private void SetupStateMachine()
	{
	    _stateMachine = new StateMachine();

	    // Инициализация состояний
	    var idleState = new IdleState(this, _animator);
	    var locomotionState = new LocomotionState(this, _animator);
	    var runState = new RunState(this, _animator);
	    var idleCrouchState = new IdleCrouchState(this, _animator);
	    var crouchState = new CrouchState(this, _animator);
	    var jumpState = new JumpState(this, _animator);
	    var fallState = new FallState(this, _animator);
	    var dashState = new DashState(this, _animator);
	    var crouchRollState = new CrouchRollState(this, _animator);
	    var wallSlideState = new WallSlideState(this, _animator);
	    var wallJumpState = new WallJumpState(this, _animator);
	    var runJumpState = new RunJumpState(this, _animator);
	    var runFallState = new RunFallState(this, _animator);

	    // Переходы из idleState
	    At(idleState, dashState, new FuncPredicate(() => input.DashInputButtonState.WasPressedThisFrame));
	    At(idleState, jumpState, new FuncPredicate(() => input.JumpInputButtonState.WasPressedThisFrame));
	    At(idleState, idleCrouchState, new FuncPredicate(() => input.CrouchInputButtonState.IsHeld && _moveDirection[0] == 0));
	    At(idleState, crouchState, new FuncPredicate(() => input.CrouchInputButtonState.IsHeld));
	    At(idleState, runState, new FuncPredicate(() => _moveDirection != Vector2.zero && input.RunInputButtonState.IsHeld));
	    At(idleState, locomotionState, new FuncPredicate(() => _moveDirection != Vector2.zero));

	    // Переходы из locomotionState
	    At(locomotionState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded));
	    At(locomotionState, jumpState, new FuncPredicate(() => input.JumpInputButtonState.WasPressedThisFrame));
	    At(locomotionState, runState, new FuncPredicate(() => input.RunInputButtonState.IsHeld && _collisionsChecker.IsGrounded));
	    At(locomotionState, idleCrouchState, new FuncPredicate(() => input.CrouchInputButtonState.IsHeld && _moveDirection[0] == 0 && Mathf.Abs(playerPhysicsController.PhysicsContext.MoveVelocity.x) < 0.1f));
	    At(locomotionState, crouchState, new FuncPredicate(() => input.CrouchInputButtonState.IsHeld));
	    At(locomotionState, dashState, new FuncPredicate(() => input.DashInputButtonState.WasPressedThisFrame));
	    At(locomotionState, idleState, new FuncPredicate(() => _moveDirection == Vector2.zero && Mathf.Abs(playerPhysicsController.PhysicsContext.MoveVelocity.x) < 0.1f));

	    // Переходы из runState
	    At(runState, runJumpState, new FuncPredicate(() => input.JumpInputButtonState.WasPressedThisFrame));
	    At(runState, runFallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded));
	    At(runState, idleState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _moveDirection == Vector2.zero && Mathf.Abs(playerPhysicsController.PhysicsContext.MoveVelocity.x) < 0.1f));
	    At(runState, idleState, new FuncPredicate(() => !input.RunInputButtonState.IsHeld && _collisionsChecker.IsGrounded && _moveDirection == Vector2.zero));
	    At(runState, locomotionState, new FuncPredicate(() => !input.RunInputButtonState.IsHeld && _collisionsChecker.IsGrounded));
	    At(runState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded));
	    At(runState, dashState, new FuncPredicate(() => input.DashInputButtonState.WasPressedThisFrame));
	    At(runState, jumpState, new FuncPredicate(() => input.JumpInputButtonState.WasPressedThisFrame)); // Оставлен для точного соответствия исходной логике
	    At(runState, crouchState, new FuncPredicate(() => input.CrouchInputButtonState.IsHeld));

	    // Переходы из idleCrouchState
	    At(idleCrouchState, crouchState, new FuncPredicate(() => input.CrouchInputButtonState.IsHeld && _moveDirection[0] != 0));
	    At(idleCrouchState, jumpState, new FuncPredicate(() => input.JumpInputButtonState.WasPressedThisFrame));
	    At(idleCrouchState, idleState, new FuncPredicate(() => !input.CrouchInputButtonState.IsHeld && _moveDirection[0] == 0));
	    At(idleCrouchState, crouchRollState, new FuncPredicate(() => input.DashInputButtonState.WasPressedThisFrame));

	    // Переходы из crouchState
	    At(crouchState, idleCrouchState, new FuncPredicate(() => input.CrouchInputButtonState.IsHeld && _moveDirection[0] == 0 && Mathf.Abs(playerPhysicsController.PhysicsContext.MoveVelocity.x) < 0.1f));
	    At(crouchState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded));
	    At(crouchState, jumpState, new FuncPredicate(() => input.JumpInputButtonState.WasPressedThisFrame));
	    At(crouchState, crouchRollState, new FuncPredicate(() => input.DashInputButtonState.WasPressedThisFrame));
	    At(crouchState, runState, new FuncPredicate(() => input.RunInputButtonState.IsHeld && !input.CrouchInputButtonState.IsHeld && _collisionsChecker.IsGrounded && !_collisionsChecker.BumpedHead));
	    At(crouchState, idleState, new FuncPredicate(() => _moveDirection == Vector2.zero && !input.CrouchInputButtonState.IsHeld && _collisionsChecker.IsGrounded && !_collisionsChecker.BumpedHead));
	    At(crouchState, locomotionState, new FuncPredicate(() => !input.CrouchInputButtonState.IsHeld && _collisionsChecker.IsGrounded && !_collisionsChecker.BumpedHead));

	    // Переходы из jumpState
	    At(jumpState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && playerPhysicsController.JumpModule.CanFall()));
	    // At(jumpState, jumpState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && input.JumpInputButtonState.WasPressedThisFrame));
	    At(jumpState, dashState, new FuncPredicate(() => input.DashInputButtonState.WasPressedThisFrame && playerPhysicsController.PhysicsContext.NumberAvailableDash > 0f));

	    // Переходы из fallState
	    At(fallState, jumpState, new FuncPredicate(() => input.JumpInputButtonState.WasPressedThisFrame && (_jumpCoyoteTimer.IsRunning || playerPhysicsController.PhysicsContext.NumberAvailableJumps > 0f)));
	    At(fallState, dashState, new FuncPredicate(() => input.DashInputButtonState.WasPressedThisFrame && playerPhysicsController.PhysicsContext.NumberAvailableDash > 0f));
	    At(fallState, jumpState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _jumpBufferTimer.IsRunning));
	    At(fallState, idleState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _moveDirection == Vector2.zero && Mathf.Abs(playerPhysicsController.PhysicsContext.MoveVelocity.x) < 0.1f));
	    At(fallState, idleCrouchState, new FuncPredicate(() => _collisionsChecker.IsGrounded && input.CrouchInputButtonState.IsHeld && _moveDirection[0] == 0));
	    At(fallState, crouchState, new FuncPredicate(() => _collisionsChecker.IsGrounded && input.CrouchInputButtonState.IsHeld));
	    At(fallState, runState, new FuncPredicate(() => _collisionsChecker.IsGrounded && input.RunInputButtonState.IsHeld));
	    At(fallState, locomotionState, new FuncPredicate(() => _collisionsChecker.IsGrounded));
	    At(fallState, wallSlideState, new FuncPredicate(() => _collisionsChecker.IsTouchingWall));

	    // Переходы из dashState
	    At(dashState, idleState, new FuncPredicate(() => !_dashTimer.IsRunning && _collisionsChecker.IsGrounded && _moveDirection == Vector2.zero));
	    At(dashState, runState, new FuncPredicate(() => !_dashTimer.IsRunning && _collisionsChecker.IsGrounded && input.RunInputButtonState.IsHeld));
	    At(dashState, fallState, new FuncPredicate(() => !_dashTimer.IsRunning && !_collisionsChecker.IsGrounded));
	    At(dashState, wallSlideState, new FuncPredicate(() => _collisionsChecker.IsTouchingWall));
	    At(dashState, jumpState, new FuncPredicate(() => input.JumpInputButtonState.WasPressedThisFrame && playerPhysicsController.PhysicsContext.NumberAvailableJumps > 0f));
	    At(dashState, idleCrouchState, new FuncPredicate(() => !_dashTimer.IsRunning && _collisionsChecker.IsGrounded && input.CrouchInputButtonState.IsHeld && _moveDirection[0] == 0));
	    At(dashState, crouchState, new FuncPredicate(() => !_dashTimer.IsRunning && _collisionsChecker.IsGrounded && input.CrouchInputButtonState.IsHeld));
	    At(dashState, locomotionState, new FuncPredicate(() => !_dashTimer.IsRunning && _collisionsChecker.IsGrounded));

	    // Переходы из crouchRollState
	    At(crouchRollState, idleState, new FuncPredicate(() => !_crouchRollTimer.IsRunning && !input.CrouchInputButtonState.IsHeld));
	    At(crouchRollState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded));
	    At(crouchRollState, idleCrouchState, new FuncPredicate(() => !_crouchRollTimer.IsRunning && input.CrouchInputButtonState.IsHeld && _moveDirection[0] == 0));
	    At(crouchRollState, crouchState, new FuncPredicate(() => !_crouchRollTimer.IsRunning));

	    // Переходы из wallSlideState
	    At(wallSlideState, wallJumpState, new FuncPredicate(() => input.JumpInputButtonState.WasPressedThisFrame));
	    At(wallSlideState, idleState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _moveDirection == Vector2.zero));
	    At(wallSlideState, locomotionState, new FuncPredicate(() => _collisionsChecker.IsGrounded));
	    At(wallSlideState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && !_collisionsChecker.IsTouchingWall));
	    At(wallSlideState, dashState, new FuncPredicate(() => input.DashInputButtonState.WasPressedThisFrame && playerPhysicsController.WallSlideModule.CalculateWallDirectionX() != _moveDirection.x));
	    At(wallSlideState, idleCrouchState, new FuncPredicate(() => input.CrouchInputButtonState.IsHeld && _collisionsChecker.IsGrounded && _moveDirection[0] == 0));

	    // Переходы из wallJumpState
	    At(wallJumpState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && !_collisionsChecker.IsTouchingWall));

	    // Переходы из runJumpState
	    At(runJumpState, runFallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && ((playerPhysicsController.PhysicsContext.MoveVelocity.y < 0f && playerPhysicsController.JumpModule.CanFall()))));
	    At(runJumpState, dashState, new FuncPredicate(() => input.DashInputButtonState.WasPressedThisFrame && playerPhysicsController.PhysicsContext.NumberAvailableDash > 0f));

	    // Переходы из runFallState
	    At(runFallState, jumpState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _jumpBufferTimer.IsRunning && !input.RunInputButtonState.IsHeld));
	    At(runFallState, fallState, new FuncPredicate(() => !input.RunInputButtonState.IsHeld && !_collisionsChecker.IsGrounded));
	    At(runFallState, idleState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _moveDirection == Vector2.zero));
	    At(runFallState, runJumpState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _jumpBufferTimer.IsRunning));
	    At(runFallState, runJumpState, new FuncPredicate(() => input.JumpInputButtonState.WasPressedThisFrame && (_jumpCoyoteTimer.IsRunning || playerPhysicsController.PhysicsContext.NumberAvailableJumps > 0f)));
	    At(runFallState, runJumpState, new FuncPredicate(() => input.DashInputButtonState.WasPressedThisFrame && playerPhysicsController.PhysicsContext.NumberAvailableDash > 0f));
	    At(runFallState, wallSlideState, new FuncPredicate(() => _collisionsChecker.IsTouchingWall));
	    At(runFallState, runState, new FuncPredicate(() => _collisionsChecker.IsGrounded && input.RunInputButtonState.IsHeld));

	    // Установка начального состояния
	    _stateMachine.SetState(idleState);
	}

	void At(IState from, IState to, IPredicate condition) => _stateMachine.AddTransition(from, to, condition);
	void Any(IState to, IPredicate condition) => _stateMachine.AddAnyTransition(to, condition);

	private void Update() 
	{
		_stateMachine.Update();
		
		_turnChecker.TurnCheck(_moveDirection, transform, playerPhysicsController.PhysicsContext.WasWallSliding);
		
		HandleTimers();
		Debbuging();
	}

	private void FixedUpdate()
	{
		_stateMachine.FixedUpdate();

		playerPhysicsController.BumpedHead();
		playerPhysicsController.ApplyMovement();
	}

	private void LateUpdate()
	{
		input.DashInputButtonState.ResetFrameState(); // FIXME WallJumpState
		input.JumpInputButtonState.ResetFrameState();
	}

	private void HandleTimers() // Сделать список
	{
		_jumpCoyoteTimer.Tick(Time.deltaTime);
		_jumpBufferTimer.Tick(Time.deltaTime);
		_wallJumpTimer.Tick(Time.deltaTime);
		_dashTimer.Tick(Time.deltaTime);
		_crouchRollTimer.Tick(Time.deltaTime);
	}

	#region OnEnableDisable
	
	void OnEnable()
	{
		input.Move += OnMove; // FIXME
	}

	void OnDisable()
	{
		input.Move -= OnMove; // FIXME
	}
	
	#endregion

	#region OnMethodActions

	private void OnMove(Vector2 moveDirection)
	{
		// _moveDirection = moveDirection;
		_moveDirection = input.Direction;

	}

	public Vector2 GetMoveDirection()
	{
		return _moveDirection;
	}
	
	#endregion

	#region Debbug
	public GameObject markerPrefab;

	void AddJumpMarker()
	{
		// Получаем позицию конца линии TrailRenderer
		Vector3 trailEndPosition = GetTrailEndPosition();

		// Спавним метку на этой позиции и сохраняем её в список markers
		GameObject newMarker = Instantiate(markerPrefab, trailEndPosition, Quaternion.identity);
		markers.Add(newMarker); // Добавляем созданную метку в список
	}

	public void ClearMarkers()
	{
		foreach (GameObject marker in markers)
		{
			Destroy(marker);
		}

		markers.Clear();
	}

	Vector3 GetTrailEndPosition()
	{
		// Trail Renderer хранит все точки следа, берем последнюю из них
		Vector3[] positions = new Vector3[_trailRenderer.positionCount];
		_trailRenderer.GetPositions(positions);

		if (positions.Length > 0)
		{
			// Конец линии - это последняя точка в массиве
			return positions[positions.Length - 1];
		}
		else
		{
			// Если точек нет, возвращаем позицию игрока
			return transform.position;
		}
	}

	//TODO Дебаг для Grounded
	private void Debbuging()
	{
		// Визуализация целевой скорости (Target Velocity)
		// Debug.DrawLine(transform.position, transform.position + new Vector3(_targetVelocity.x, 0, 0), Color.red);

		// Визуализация текущего вектора скорости (_moveVelocity)
		Debug.DrawLine(transform.position, transform.position + new Vector3(playerPhysicsController.PhysicsContext.MoveVelocity.x, 0, 0), Color.blue);

		// Визуализация направления движения (_moveDirection)
		if (_moveDirection != Vector2.zero)
		{
			Debug.DrawLine(transform.position, transform.position + new Vector3(_moveDirection.x, 0, 0), Color.green);
		}
		else
		{
			// Визуализация замедления
			if (playerPhysicsController.PhysicsContext.MoveVelocity != Vector2.zero)
			{
				Debug.DrawLine(transform.position, transform.position + new Vector3(playerPhysicsController.PhysicsContext.MoveVelocity.x, 0, 0), Color.yellow);
			}
		}
	}
	#endregion
}