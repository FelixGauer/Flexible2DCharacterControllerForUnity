using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private InputReader _inputReader;
	[SerializeField] private PlayerControllerStats _playerControllerStats;
	[SerializeField] public Transform spriteTransform;
	[SerializeField] private PhysicsHandler2D _physicsHandler2D;


	private CollisionsChecker _collisionsChecker;
	public Rigidbody2D _rigidbody;
	public CapsuleCollider2D _capsuleCollider;

	// InputReader parameters
	// private Vector2 _moveDirection;

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
	private bool IsFacingRight => _turnChecker.IsFacingRight;
	private bool _isSitting = false;
	
	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		_collisionsChecker = GetComponent<CollisionsChecker>();
		_capsuleCollider = GetComponentInChildren<CapsuleCollider2D>(); // TODO Мб создать класс который будет возвращать уменьшенный коллайдер
		_trailRenderer = GetComponent<TrailRenderer>();
		
		_turnChecker = new TurnChecker(); // FIXME
		
		_collisionsChecker.IsSitting = () => _isSitting;
		_collisionsChecker.IsFacingRight = () => _turnChecker.IsFacingRight;

		_jumpCoyoteTimer = new CountdownTimer(_playerControllerStats.CoyoteTime);
		_jumpBufferTimer = new CountdownTimer(_playerControllerStats.BufferTime);
		_wallJumpTimer = new CountdownTimer(_playerControllerStats.WallJumpTime);
		_dashTimer = new CountdownTimer(_playerControllerStats.DashTime);
		_crouchRollTimer = new CountdownTimer(_playerControllerStats.CrouchRollTime);

		playerPhysicsController = new PlayerPhysicsController(_rigidbody, _jumpCoyoteTimer, _jumpBufferTimer, _collisionsChecker, _playerControllerStats, this, _dashTimer, _turnChecker, _wallJumpTimer, _crouchRollTimer, _physicsHandler2D);
		
		_animator = GetComponentInChildren<Animator>(); // FIXME
		
		SetupStateMachine();
	}

	private void SetupStateMachine()
	{
	    _stateMachine = new StateMachine();

	    // Инициализация состояний
	    var idleState = new IdleState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D);
	    var locomotionState = new LocomotionState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D);
	    var runState = new RunState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D);
	    var idleCrouchState = new IdleCrouchState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D);
	    var crouchState = new CrouchState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D);
	    var jumpState = new JumpState(this, _animator, _inputReader, _playerControllerStats,_physicsHandler2D);
	    var fallState = new FallState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D);
	    var dashState = new DashState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D);
	    var crouchRollState = new CrouchRollState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D);
	    var wallSlideState = new WallSlideState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D);
	    var wallJumpState = new WallJumpState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D);
	    var runJumpState = new RunJumpState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D);
	    var runFallState = new RunFallState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D);

	    // Переходы из idleState
	    At(idleState, dashState, new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame));
	    At(idleState, jumpState, new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame));
	    At(idleState, idleCrouchState, new FuncPredicate(() => _inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection()[0] == 0));
	    At(idleState, crouchState, new FuncPredicate(() => _inputReader.GetCrouchState().IsHeld));
	    At(idleState, runState, new FuncPredicate(() => _inputReader.GetRunState().IsHeld && _inputReader.GetMoveDirection() != Vector2.zero));
	    At(idleState, locomotionState, new FuncPredicate(() => _inputReader.GetMoveDirection() != Vector2.zero));

	    // Переходы из locomotionState
	    At(locomotionState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded));
	    At(locomotionState, jumpState, new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame));
	    At(locomotionState, runState, new FuncPredicate(() => _inputReader.GetRunState().IsHeld && _collisionsChecker.IsGrounded));
	    At(locomotionState, idleCrouchState, new FuncPredicate(() => _inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection()[0] == 0 && Mathf.Abs(playerPhysicsController.PhysicsContext.MoveVelocity.x) < 0.1f));
	    At(locomotionState, crouchState, new FuncPredicate(() => _inputReader.GetCrouchState().IsHeld));
	    At(locomotionState, dashState, new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame));
	    At(locomotionState, idleState, new FuncPredicate(() => _inputReader.GetMoveDirection() == Vector2.zero && Mathf.Abs(_physicsHandler2D.GetVelocity().x) == 0f));

	    // Переходы из runState
	    At(runState, runJumpState, new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame));
	    At(runState, runFallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded));
	    At(runState, idleState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero && Mathf.Abs(_physicsHandler2D.GetVelocity().x) == 0f));
	    At(runState, idleState, new FuncPredicate(() => !_inputReader.GetRunState().IsHeld && _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero));
	    At(runState, locomotionState, new FuncPredicate(() => !_inputReader.GetRunState().IsHeld && _collisionsChecker.IsGrounded));
	    At(runState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded));
	    At(runState, dashState, new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame));
	    At(runState, jumpState, new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame)); // Оставлен для точного соответствия исходной логике
	    At(runState, crouchState, new FuncPredicate(() => _inputReader.GetCrouchState().IsHeld));

	    // Переходы из idleCrouchState
	    At(idleCrouchState, crouchState, new FuncPredicate(() => _inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection()[0] != 0));
	    At(idleCrouchState, jumpState, new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame));
	    At(idleCrouchState, idleState, new FuncPredicate(() => !_inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection()[0] == 0));
	    At(idleCrouchState, crouchRollState, new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame));

	    // Переходы из crouchState
	    At(crouchState, idleCrouchState, new FuncPredicate(() => _inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection()[0] == 0 && Mathf.Abs(_physicsHandler2D.GetVelocity().x) == 0f));
	    At(crouchState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded));
	    At(crouchState, jumpState, new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame));
	    At(crouchState, crouchRollState, new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame));
	    At(crouchState, runState, new FuncPredicate(() => _inputReader.GetRunState().IsHeld && !_inputReader.GetCrouchState().IsHeld && _collisionsChecker.IsGrounded && !_collisionsChecker.BumpedHead));
	    At(crouchState, idleState, new FuncPredicate(() => _inputReader.GetMoveDirection() == Vector2.zero && !_inputReader.GetCrouchState().IsHeld && _collisionsChecker.IsGrounded && !_collisionsChecker.BumpedHead));
	    At(crouchState, locomotionState, new FuncPredicate(() => !_inputReader.GetCrouchState().IsHeld && _collisionsChecker.IsGrounded && !_collisionsChecker.BumpedHead));

	    // Переходы из jumpState
	    At(jumpState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && playerPhysicsController.JumpModule.CanFall()));
	    // At(jumpState, jumpState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && input.JumpInputButtonState.WasPressedThisFrame));
	    At(jumpState, dashState, new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && playerPhysicsController.PhysicsContext.NumberAvailableDash > 0f));

	    // Переходы из fallState
	    At(fallState, jumpState, new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame && (_jumpCoyoteTimer.IsRunning || playerPhysicsController.PhysicsContext.NumberAvailableJumps > 0f)));
	    At(fallState, dashState, new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && playerPhysicsController.PhysicsContext.NumberAvailableDash > 0f));
	    At(fallState, jumpState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _jumpBufferTimer.IsRunning));
	    At(fallState, idleState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero && Mathf.Abs(_physicsHandler2D.GetVelocity().x) < 0.1f));
	    At(fallState, idleCrouchState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection()[0] == 0));
	    At(fallState, crouchState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetCrouchState().IsHeld));
	    At(fallState, runState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetRunState().IsHeld));
	    At(fallState, locomotionState, new FuncPredicate(() => _collisionsChecker.IsGrounded));
	    At(fallState, wallSlideState, new FuncPredicate(() => _collisionsChecker.IsTouchingWall));

	    // Переходы из dashState
	    At(dashState, idleState, new FuncPredicate(() => !_dashTimer.IsRunning && _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero));
	    At(dashState, runState, new FuncPredicate(() => !_dashTimer.IsRunning && _collisionsChecker.IsGrounded && _inputReader.GetRunState().IsHeld));
	    At(dashState, fallState, new FuncPredicate(() => !_dashTimer.IsRunning && !_collisionsChecker.IsGrounded));
	    At(dashState, wallSlideState, new FuncPredicate(() => _collisionsChecker.IsTouchingWall));
	    At(dashState, jumpState, new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame && playerPhysicsController.PhysicsContext.NumberAvailableJumps > 0f));
	    At(dashState, idleCrouchState, new FuncPredicate(() => !_dashTimer.IsRunning && _collisionsChecker.IsGrounded && _inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection()[0] == 0));
	    At(dashState, crouchState, new FuncPredicate(() => !_dashTimer.IsRunning && _collisionsChecker.IsGrounded && _inputReader.GetCrouchState().IsHeld));
	    At(dashState, locomotionState, new FuncPredicate(() => !_dashTimer.IsRunning && _collisionsChecker.IsGrounded));

	    // Переходы из crouchRollState
	    At(crouchRollState, idleState, new FuncPredicate(() => !_crouchRollTimer.IsRunning && !_inputReader.GetCrouchState().IsHeld));
	    At(crouchRollState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded));
	    At(crouchRollState, idleCrouchState, new FuncPredicate(() => !_crouchRollTimer.IsRunning && _inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection()[0] == 0));
	    At(crouchRollState, crouchState, new FuncPredicate(() => !_crouchRollTimer.IsRunning));

	    // Переходы из wallSlideState
	    At(wallSlideState, wallJumpState, new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame));
	    At(wallSlideState, idleState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero));
	    At(wallSlideState, locomotionState, new FuncPredicate(() => _collisionsChecker.IsGrounded));
	    At(wallSlideState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && !_collisionsChecker.IsTouchingWall));
	    At(wallSlideState, dashState, new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && playerPhysicsController.WallSlideModule.CalculateWallDirectionX() != _inputReader.GetMoveDirection().x));
	    At(wallSlideState, idleCrouchState, new FuncPredicate(() => _inputReader.GetCrouchState().IsHeld && _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection()[0] == 0));

	    // Переходы из wallJumpState
	    At(wallJumpState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && !_collisionsChecker.IsTouchingWall));

	    // Переходы из runJumpState
	    At(runJumpState, runFallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && playerPhysicsController.JumpModule.CanFall()));
	    // At(runJumpState, runFallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && ((playerPhysicsController.PhysicsContext.MoveVelocity.y < 0f && playerPhysicsController.JumpModule.CanFall()))));
	    At(runJumpState, dashState, new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && playerPhysicsController.PhysicsContext.NumberAvailableDash > 0f));

	    // Переходы из runFallState
	    At(runFallState, jumpState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _jumpBufferTimer.IsRunning && !_inputReader.GetRunState().IsHeld));
	    At(runFallState, fallState, new FuncPredicate(() => !_inputReader.GetRunState().IsHeld && !_collisionsChecker.IsGrounded));
	    At(runFallState, idleState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero));
	    At(runFallState, runJumpState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _jumpBufferTimer.IsRunning));
	    At(runFallState, runJumpState, new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame && (_jumpCoyoteTimer.IsRunning || playerPhysicsController.PhysicsContext.NumberAvailableJumps > 0f)));
	    At(runFallState, runJumpState, new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && playerPhysicsController.PhysicsContext.NumberAvailableDash > 0f)); //FIXME
	    At(runFallState, wallSlideState, new FuncPredicate(() => _collisionsChecker.IsTouchingWall));
	    At(runFallState, runState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetRunState().IsHeld));
		//TODO runfall to locomotion
	    // Установка начального состояния
	    _stateMachine.SetState(idleState);
	}

	void At(IState from, IState to, IPredicate condition) => _stateMachine.AddTransition(from, to, condition);
	void Any(IState to, IPredicate condition) => _stateMachine.AddAnyTransition(to, condition);

	private void Update() 
	{
		_stateMachine.Update();
		
		_turnChecker.TurnCheck(_inputReader.GetMoveDirection(), transform, playerPhysicsController.PhysicsContext.WasWallSliding);
		
		HandleTimers();
		Debbuging();
	}

	private void FixedUpdate()
	{
		_stateMachine.FixedUpdate();

		playerPhysicsController.BumpedHead();
		// playerPhysicsController.ApplyMovement();
	}

	private void LateUpdate()
	{
		// input.GetDashState().ResetFrameState();
		// input.GetJumpState().ResetFrameState();
		
		_inputReader.ResetFrameStates();
	}

	private void HandleTimers() // Сделать список
	{
		_jumpCoyoteTimer.Tick(Time.deltaTime);
		_jumpBufferTimer.Tick(Time.deltaTime);
		_wallJumpTimer.Tick(Time.deltaTime);
		_dashTimer.Tick(Time.deltaTime);
		_crouchRollTimer.Tick(Time.deltaTime);
	}
	
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
		if (_inputReader.GetMoveDirection() != Vector2.zero)
		{
			Debug.DrawLine(transform.position, transform.position + new Vector3(_inputReader.GetMoveDirection().x, 0, 0), Color.green);
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