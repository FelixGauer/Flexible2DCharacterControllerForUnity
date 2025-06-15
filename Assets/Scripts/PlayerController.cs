using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private InputReader _inputReader;
	[SerializeField] private PlayerControllerStats _playerControllerStats;
	[SerializeField] private Transform _spriteTransform;
	[SerializeField] private CapsuleCollider2D _capsuleCollider;
	[SerializeField] private PhysicsHandler2D _physicsHandler2D;

	private PlayerTimerRegistry _playerTimerRegistry;
	private CollisionsChecker _collisionsChecker;
	private StateMachine _stateMachine;
	private ColliderSpriteResizer _colliderSpriteResizer;
	private TurnChecker _turnChecker;

	
	public Rigidbody2D _rigidbody;
	
	//Debug var
	private float maxYPosition;
	private TrailRenderer _trailRenderer;
	private List<GameObject> markers = new List<GameObject>();
	
	// State Machine Var

	public PlayerPhysicsController playerPhysicsController;
	
	private Animator _animator;
	
	private bool IsFacingRight => _turnChecker.IsFacingRight;
	private bool _isSitting = false;
	
	private PlayerStateMachineFactory _stateMachineFactory;

	
	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		_collisionsChecker = GetComponent<CollisionsChecker>();
		_capsuleCollider = GetComponentInChildren<CapsuleCollider2D>(); // TODO Мб создать класс который будет возвращать уменьшенный коллайдер
		_trailRenderer = GetComponent<TrailRenderer>();
		
		_turnChecker = new TurnChecker(this.transform); // FIXME
		
		_collisionsChecker.IsSitting = () => _isSitting;
		_collisionsChecker.IsFacingRight = () => _turnChecker.IsFacingRight;

		_playerTimerRegistry = new PlayerTimerRegistry(_playerControllerStats);
		// _colliderSpriteController = new ColliderSpriteController(_capsuleCollider, spriteTransform);

		_colliderSpriteResizer = new ColliderSpriteResizer(_capsuleCollider, _spriteTransform);

		playerPhysicsController = new PlayerPhysicsController(_rigidbody, _collisionsChecker, _playerControllerStats, this, _turnChecker, _physicsHandler2D, _playerTimerRegistry, _colliderSpriteResizer);
		
		_animator = GetComponentInChildren<Animator>(); // FIXME
		
		// SetupStateMachine();

		InitializeStateMachine();
	}
	
	private void InitializeStateMachine()
	{
		_stateMachineFactory = new PlayerStateMachineFactory(
			this,
			_animator,
			_inputReader,
			_playerControllerStats,
			_physicsHandler2D,
			_turnChecker,
			_collisionsChecker,
			_playerTimerRegistry,
			playerPhysicsController
		);

		_stateMachine = _stateMachineFactory.CreateStateMachine();
	}
	
	private void Update() 
	{
		_stateMachine.Update();
		
		// _turnChecker.TurnCheck(_inputReader.GetMoveDirection());
		
		HandleTimers();
		
		Debbuging();
	}

	private void FixedUpdate()
	{
		_stateMachine.FixedUpdate();

		// playerPhysicsController.BumpedHead();
		// playerPhysicsController.ApplyMovement();
	}

	private void LateUpdate()
	{
		_inputReader.ResetFrameStates();
	}

	private void HandleTimers() // Сделать список
	{
		_playerTimerRegistry.UpdateAll();
	}
	
	private void SetupStateMachine()
	{
	    _stateMachine = new StateMachine();

	    // Инициализация состояний
	    var idleState = new IdleState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D, _turnChecker);
	    var locomotionState = new LocomotionState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D, _turnChecker);
	    var runState = new RunState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D, _turnChecker);
	    var idleCrouchState = new IdleCrouchState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D, _turnChecker);
	    var crouchState = new CrouchState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D, _turnChecker);
	    var jumpState = new JumpState(this, _animator, _inputReader, _playerControllerStats,_physicsHandler2D, _turnChecker);
	    var fallState = new FallState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D, _turnChecker);
	    var dashState = new DashState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D, _turnChecker);
	    var crouchRollState = new CrouchRollState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D, _turnChecker);
	    var wallSlideState = new WallSlideState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D, _turnChecker);
	    var wallJumpState = new WallJumpState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D, _turnChecker);
	    var runJumpState = new RunJumpState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D, _turnChecker);
	    var runFallState = new RunFallState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D, _turnChecker);

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
	    At(locomotionState, idleCrouchState, new FuncPredicate(() => _inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection()[0] == 0 && Mathf.Abs(_physicsHandler2D.GetVelocity().x) == 0f));
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
	    At(idleCrouchState, crouchState, new FuncPredicate(() => (_inputReader.GetCrouchState().IsHeld || _collisionsChecker.BumpedHead) && _inputReader.GetMoveDirection()[0] != 0));
	    At(idleCrouchState, jumpState, new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame && !_collisionsChecker.BumpedHead));
	    At(idleCrouchState, idleState, new FuncPredicate(() => !_inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection()[0] == 0 && !_collisionsChecker.BumpedHead));
	    At(idleCrouchState, crouchRollState, new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame));

	    // Переходы из crouchState
	    At(crouchState, idleCrouchState, new FuncPredicate(() => (_inputReader.GetCrouchState().IsHeld || _collisionsChecker.BumpedHead) && _inputReader.GetMoveDirection()[0] == 0 && Mathf.Abs(_physicsHandler2D.GetVelocity().x) == 0f));
	    At(crouchState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded));
	    At(crouchState, jumpState, new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame));
	    At(crouchState, crouchRollState, new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame));
	    At(crouchState, runState, new FuncPredicate(() => _inputReader.GetRunState().IsHeld && !_inputReader.GetCrouchState().IsHeld && _collisionsChecker.IsGrounded && !_collisionsChecker.BumpedHead));
	    At(crouchState, idleState, new FuncPredicate(() => _inputReader.GetMoveDirection() == Vector2.zero && !_inputReader.GetCrouchState().IsHeld && _collisionsChecker.IsGrounded && !_collisionsChecker.BumpedHead));
	    At(crouchState, locomotionState, new FuncPredicate(() => !_inputReader.GetCrouchState().IsHeld && _collisionsChecker.IsGrounded && !_collisionsChecker.BumpedHead));

	    // Переходы из jumpState
	    At(jumpState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && (playerPhysicsController.JumpModule.CanFall() || _collisionsChecker.BumpedHead)));
	    // At(jumpState, jumpState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && input.JumpInputButtonState.WasPressedThisFrame));
	    At(jumpState, dashState, new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && playerPhysicsController.DashModule.CanDash()));

	    // Переходы из fallState
	    At(fallState, jumpState, new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame && (_playerTimerRegistry.jumpCoyoteTimer.IsRunning || playerPhysicsController.CanMultiJump())));
	    At(fallState, dashState, new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && playerPhysicsController.DashModule.CanDash()));
	    At(fallState, jumpState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _playerTimerRegistry.jumpBufferTimer.IsRunning));
	    At(fallState, idleState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero && Mathf.Abs(_physicsHandler2D.GetVelocity().x) < 0.1f));
	    At(fallState, idleCrouchState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection()[0] == 0));
	    At(fallState, crouchState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetCrouchState().IsHeld));
	    At(fallState, runState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetRunState().IsHeld));
	    At(fallState, locomotionState, new FuncPredicate(() => _collisionsChecker.IsGrounded));
	    At(fallState, wallSlideState, new FuncPredicate(() => _collisionsChecker.IsTouchingWall));

	    // Переходы из dashState
	    At(dashState, idleState, new FuncPredicate(() => !_playerTimerRegistry.dashTimer.IsRunning && _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero));
	    At(dashState, runState, new FuncPredicate(() => !_playerTimerRegistry.dashTimer.IsRunning && _collisionsChecker.IsGrounded && _inputReader.GetRunState().IsHeld));
	    At(dashState, fallState, new FuncPredicate(() => !_playerTimerRegistry.dashTimer.IsRunning && !_collisionsChecker.IsGrounded));
	    At(dashState, wallSlideState, new FuncPredicate(() => _collisionsChecker.IsTouchingWall));
	    At(dashState, jumpState, new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame && playerPhysicsController.JumpModule.CanMultiJump()));
	    At(dashState, idleCrouchState, new FuncPredicate(() => !_playerTimerRegistry.dashTimer.IsRunning && _collisionsChecker.IsGrounded && _inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection()[0] == 0));
	    At(dashState, crouchState, new FuncPredicate(() => !_playerTimerRegistry.dashTimer.IsRunning && _collisionsChecker.IsGrounded && _inputReader.GetCrouchState().IsHeld));
	    At(dashState, locomotionState, new FuncPredicate(() => !_playerTimerRegistry.dashTimer.IsRunning && _collisionsChecker.IsGrounded));

	    // Переходы из crouchRollState
	    At(crouchRollState, idleState, new FuncPredicate(() => !_playerTimerRegistry.crouchRollTimer.IsRunning && !_inputReader.GetCrouchState().IsHeld && !_collisionsChecker.BumpedHead));
	    At(crouchRollState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded));
	    At(crouchRollState, idleCrouchState, new FuncPredicate(() => !_playerTimerRegistry.crouchRollTimer.IsRunning && (_inputReader.GetCrouchState().IsHeld || _collisionsChecker.BumpedHead) && _inputReader.GetMoveDirection()[0] == 0));
	    At(crouchRollState, crouchState, new FuncPredicate(() => !_playerTimerRegistry.crouchRollTimer.IsRunning));

	    // Переходы из wallSlideState
	    At(wallSlideState, wallJumpState, new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame));
	    At(wallSlideState, idleState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero));
	    At(wallSlideState, locomotionState, new FuncPredicate(() => _collisionsChecker.IsGrounded));
	    // At(wallSlideState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && !_collisionsChecker.IsTouchingWall));
	    At(wallSlideState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && (_playerTimerRegistry.wallJumpTimer.IsFinished || !_collisionsChecker.IsTouchingWall)));
	    At(wallSlideState, dashState, new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && playerPhysicsController.WallSlideModule.CalculateWallDirectionX() != _inputReader.GetMoveDirection().x));
	    // At(wallSlideState, dashState, new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && playerPhysicsController.WallSlideModule.CalculateWallDirectionX() != _inputReader.GetMoveDirection().x));
	    At(wallSlideState, idleCrouchState, new FuncPredicate(() => _inputReader.GetCrouchState().IsHeld && _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection()[0] == 0));

	    // Переходы из wallJumpState
	    At(wallJumpState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && !_collisionsChecker.IsTouchingWall));

	    // Переходы из runJumpState
	    At(runJumpState, runFallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && playerPhysicsController.JumpModule.CanFall()));
	    // At(runJumpState, runFallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && ((playerPhysicsController.PhysicsContext.MoveVelocity.y < 0f && playerPhysicsController.JumpModule.CanFall()))));
	    At(runJumpState, dashState, new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && playerPhysicsController.DashModule.CanDash()));

	    // Переходы из runFallState
	    At(runFallState, jumpState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _playerTimerRegistry.jumpBufferTimer.IsRunning && !_inputReader.GetRunState().IsHeld));
	    At(runFallState, fallState, new FuncPredicate(() => !_inputReader.GetRunState().IsHeld && !_collisionsChecker.IsGrounded));
	    At(runFallState, idleState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero));
	    At(runFallState, runJumpState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _playerTimerRegistry.jumpBufferTimer.IsRunning));
	    At(runFallState, runJumpState, new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame && (_playerTimerRegistry.jumpCoyoteTimer.IsRunning || playerPhysicsController.JumpModule.CanMultiJump())));
	    At(runFallState, runJumpState, new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && playerPhysicsController.DashModule.CanDash())); //FIXME
	    At(runFallState, wallSlideState, new FuncPredicate(() => _collisionsChecker.IsTouchingWall));
	    At(runFallState, runState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetRunState().IsHeld));
		//TODO runfall to locomotion
	    // Установка начального состояния
	    _stateMachine.SetState(idleState);
	}

	void At(IState from, IState to, IPredicate condition) => _stateMachine.AddTransition(from, to, condition);
	void Any(IState to, IPredicate condition) => _stateMachine.AddAnyTransition(to, condition);
	
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


public abstract class StateMachineFactory<TStates> where TStates : class, new()
{
	protected TStates States { get; private set; }
	protected StateMachine stateMachine;

	public StateMachine CreateStateMachine()
	{
		stateMachine = new StateMachine();
        
		// Создаем состояния
		States = CreateStates();
        
		// Настраиваем переходы
		SetupTransitions(States);
        
		// Устанавливаем начальное состояние
		var initialState = GetInitialState(States);
		stateMachine.SetState(initialState);
        
		return stateMachine;
	}

	// Абстрактные методы, которые должны быть реализованы в наследниках
	protected abstract TStates CreateStates();
	protected abstract void SetupTransitions(TStates states);
	protected abstract IState GetInitialState(TStates states);

	// Утилитарный метод для добавления переходов
	protected void AddTransition(IState from, IState to, Func<bool> condition)
	{
		stateMachine.AddTransition(from, to, new FuncPredicate(condition));
	}
	
	protected void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
	protected void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);
}
public class PlayerStates
{
	public IState IdleState { get; set; }
	public IState LocomotionState { get; set; }
	public IState RunState { get; set; }
	public IState IdleCrouchState { get; set; }
	public IState CrouchState { get; set; }
	public IState JumpState { get; set; }
	public IState FallState { get; set; }
	public IState DashState { get; set; }
	public IState CrouchRollState { get; set; }
	public IState WallSlideState { get; set; }
	public IState WallJumpState { get; set; }
	public IState RunJumpState { get; set; }
	public IState RunFallState { get; set; }
}

public class PlayerStateMachineFactory : StateMachineFactory<PlayerStates>
{
	private readonly PlayerController _playerController;
	private readonly Animator _animator;
	private readonly InputReader _inputReader;
	private readonly PlayerControllerStats _playerControllerStats;
	private readonly PhysicsHandler2D _physicsHandler2D;
	private readonly TurnChecker _turnChecker;
	private readonly CollisionsChecker _collisionsChecker;
	private readonly PlayerTimerRegistry _playerTimerRegistry;
	private readonly PlayerPhysicsController _playerPhysicsController;

	public PlayerStateMachineFactory(
		PlayerController playerController,
		Animator animator,
		InputReader inputReader,
		PlayerControllerStats playerControllerStats,
		PhysicsHandler2D physicsHandler2D,
		TurnChecker turnChecker,
		CollisionsChecker collisionsChecker,
		PlayerTimerRegistry playerTimerRegistry,
		PlayerPhysicsController playerPhysicsController)
	{
		_playerController = playerController;
		_animator = animator;
		_inputReader = inputReader;
		_playerControllerStats = playerControllerStats;
		_physicsHandler2D = physicsHandler2D;
		_turnChecker = turnChecker;
		_collisionsChecker = collisionsChecker;
		_playerTimerRegistry = playerTimerRegistry;
		_playerPhysicsController = playerPhysicsController;
	}

	protected override PlayerStates CreateStates()
	{
		return new PlayerStates
		{
			IdleState = new IdleState(_playerController, _animator, _inputReader, _playerControllerStats,
				_physicsHandler2D, _turnChecker),
			LocomotionState = new LocomotionState(_playerController, _animator, _inputReader, _playerControllerStats,
				_physicsHandler2D, _turnChecker),
			RunState = new RunState(_playerController, _animator, _inputReader, _playerControllerStats,
				_physicsHandler2D, _turnChecker),
			IdleCrouchState = new IdleCrouchState(_playerController, _animator, _inputReader, _playerControllerStats,
				_physicsHandler2D, _turnChecker),
			CrouchState = new CrouchState(_playerController, _animator, _inputReader, _playerControllerStats,
				_physicsHandler2D, _turnChecker),
			JumpState = new JumpState(_playerController, _animator, _inputReader, _playerControllerStats,
				_physicsHandler2D, _turnChecker),
			FallState = new FallState(_playerController, _animator, _inputReader, _playerControllerStats,
				_physicsHandler2D, _turnChecker),
			DashState = new DashState(_playerController, _animator, _inputReader, _playerControllerStats,
				_physicsHandler2D, _turnChecker),
			CrouchRollState = new CrouchRollState(_playerController, _animator, _inputReader, _playerControllerStats,
				_physicsHandler2D, _turnChecker),
			WallSlideState = new WallSlideState(_playerController, _animator, _inputReader, _playerControllerStats,
				_physicsHandler2D, _turnChecker),
			WallJumpState = new WallJumpState(_playerController, _animator, _inputReader, _playerControllerStats,
				_physicsHandler2D, _turnChecker),
			RunJumpState = new RunJumpState(_playerController, _animator, _inputReader, _playerControllerStats,
				_physicsHandler2D, _turnChecker),
			RunFallState = new RunFallState(_playerController, _animator, _inputReader, _playerControllerStats,
				_physicsHandler2D, _turnChecker)
		};
	}

	protected override IState GetInitialState(PlayerStates states)
	{
		return states.IdleState;
	}

	protected override void SetupTransitions(PlayerStates states)
	{
		SetupIdleTransitions(states);
		SetupLocomotionTransitions(states);
		SetupRunTransitions(states);
		SetupIdleCrouchTransitions(states);
		SetupCrouchTransitions(states);
		SetupJumpTransitions(states);
		SetupFallTransitions(states);
		SetupDashTransitions(states);
		SetupCrouchRollTransitions(states);
		SetupWallSlideTransitions(states);
		SetupWallJumpTransitions(states);
		SetupRunJumpTransitions(states);
		SetupRunFallTransitions(states);
	}

	private void SetupIdleTransitions(PlayerStates states)
	{
		At(states.IdleState, states.DashState,
			new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame));
		At(states.IdleState, states.JumpState,
			new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame));
		At(states.IdleState, states.IdleCrouchState,
			new FuncPredicate(() => _inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection()[0] == 0));
		At(states.IdleState, states.CrouchState,
			new FuncPredicate(() => _inputReader.GetCrouchState().IsHeld));
		At(states.IdleState, states.RunState,
			new FuncPredicate(() => _inputReader.GetRunState().IsHeld && _inputReader.GetMoveDirection() != Vector2.zero));
		At(states.IdleState, states.LocomotionState,
			new FuncPredicate(() => _inputReader.GetMoveDirection() != Vector2.zero));
	}

	private void SetupLocomotionTransitions(PlayerStates states)
	{
		At(states.LocomotionState, states.FallState,
			new FuncPredicate(() => !_collisionsChecker.IsGrounded));
		At(states.LocomotionState, states.JumpState,
			new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame));
		At(states.LocomotionState, states.RunState,
			new FuncPredicate(() => _inputReader.GetRunState().IsHeld && _collisionsChecker.IsGrounded));
		At(states.LocomotionState, states.IdleCrouchState,
			new FuncPredicate(() => _inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection()[0] == 0 && Mathf.Abs(_physicsHandler2D.GetVelocity().x) == 0f));
		At(states.LocomotionState, states.CrouchState,
			new FuncPredicate(() => _inputReader.GetCrouchState().IsHeld));
		At(states.LocomotionState, states.DashState,
			new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame));
		At(states.LocomotionState, states.IdleState,
			new FuncPredicate(() => _inputReader.GetMoveDirection() == Vector2.zero && Mathf.Abs(_physicsHandler2D.GetVelocity().x) == 0f));
	}

	private void SetupRunTransitions(PlayerStates states)
	{
		At(states.RunState, states.RunJumpState,
			new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame));
		At(states.RunState, states.RunFallState,
			new FuncPredicate(() => !_collisionsChecker.IsGrounded));
		At(states.RunState, states.IdleState,
			new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero && Mathf.Abs(_physicsHandler2D.GetVelocity().x) == 0f));
		At(states.RunState, states.IdleState,
			new FuncPredicate(() => !_inputReader.GetRunState().IsHeld && _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero));
		At(states.RunState, states.LocomotionState,
			new FuncPredicate(() => !_inputReader.GetRunState().IsHeld && _collisionsChecker.IsGrounded));
		At(states.RunState, states.FallState,
			new FuncPredicate(() => !_collisionsChecker.IsGrounded));
		At(states.RunState, states.DashState,
			new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame));
		At(states.RunState, states.JumpState,
			new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame));
		At(states.RunState, states.CrouchState,
			new FuncPredicate(() => _inputReader.GetCrouchState().IsHeld));
	}

	private void SetupIdleCrouchTransitions(PlayerStates states)
	{
		At(states.IdleCrouchState, states.CrouchState,
			new FuncPredicate(() => (_inputReader.GetCrouchState().IsHeld || _collisionsChecker.BumpedHead) && _inputReader.GetMoveDirection()[0] != 0));
		At(states.IdleCrouchState, states.JumpState,
			new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame && !_collisionsChecker.BumpedHead));
		At(states.IdleCrouchState, states.IdleState,
			new FuncPredicate(() => !_inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection()[0] == 0 && !_collisionsChecker.BumpedHead));
		At(states.IdleCrouchState, states.CrouchRollState,
			new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame));
	}

	private void SetupCrouchTransitions(PlayerStates states)
	{
		At(states.CrouchState, states.IdleCrouchState,
			new FuncPredicate(() => (_inputReader.GetCrouchState().IsHeld || _collisionsChecker.BumpedHead) &&
			                        _inputReader.GetMoveDirection()[0] == 0 && Mathf.Abs(_physicsHandler2D.GetVelocity().x) == 0f));
		At(states.CrouchState, states.FallState,
			new FuncPredicate(() => !_collisionsChecker.IsGrounded));
		At(states.CrouchState, states.JumpState,
			new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame));
		At(states.CrouchState, states.CrouchRollState,
			new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame));
		At(states.CrouchState, states.RunState,
			new FuncPredicate(() => _inputReader.GetRunState().IsHeld && !_inputReader.GetCrouchState().IsHeld &&
			                        _collisionsChecker.IsGrounded && !_collisionsChecker.BumpedHead));
		At(states.CrouchState, states.IdleState,
			new FuncPredicate(() => _inputReader.GetMoveDirection() == Vector2.zero && !_inputReader.GetCrouchState().IsHeld &&
			                        _collisionsChecker.IsGrounded && !_collisionsChecker.BumpedHead));
		At(states.CrouchState, states.LocomotionState,
			new FuncPredicate(() => !_inputReader.GetCrouchState().IsHeld && _collisionsChecker.IsGrounded &&
			                        !_collisionsChecker.BumpedHead));
	}

	private void SetupJumpTransitions(PlayerStates states)
	{
		At(states.JumpState, states.FallState,
			new FuncPredicate(() => !_collisionsChecker.IsGrounded &&
			                        (_playerPhysicsController.JumpModule.CanFall() || _collisionsChecker.BumpedHead)));
		At(states.JumpState, states.DashState,
			new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && _playerPhysicsController.DashModule.CanDash()));
	}

	private void SetupFallTransitions(PlayerStates states)
	{
		At(states.FallState, states.JumpState,
			new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame && (_playerTimerRegistry.jumpCoyoteTimer.IsRunning || _playerPhysicsController.CanMultiJump())));
		At(states.FallState, states.DashState,
			new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && _playerPhysicsController.DashModule.CanDash()));
		At(states.FallState, states.JumpState,
			new FuncPredicate(() => _collisionsChecker.IsGrounded && _playerTimerRegistry.jumpBufferTimer.IsRunning));
		At(states.FallState, states.IdleState,
			new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero && Mathf.Abs(_physicsHandler2D.GetVelocity().x) < 0.1f));
		At(states.FallState, states.IdleCrouchState,
			new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection()[0] == 0));
		At(states.FallState, states.CrouchState,
			new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetCrouchState().IsHeld));
		At(states.FallState, states.RunState,
			new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetRunState().IsHeld));
		At(states.FallState, states.LocomotionState,
			new FuncPredicate(() => _collisionsChecker.IsGrounded));
		At(states.FallState, states.WallSlideState,
			new FuncPredicate(() => _collisionsChecker.IsTouchingWall));
	}

	private void SetupDashTransitions(PlayerStates states)
	{
		At(states.DashState, states.IdleState,
			new FuncPredicate(() => !_playerTimerRegistry.dashTimer.IsRunning && _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero));
		At(states.DashState, states.RunState,
			new FuncPredicate(() => !_playerTimerRegistry.dashTimer.IsRunning && _collisionsChecker.IsGrounded && _inputReader.GetRunState().IsHeld));
		At(states.DashState, states.FallState,
			new FuncPredicate(() => !_playerTimerRegistry.dashTimer.IsRunning && !_collisionsChecker.IsGrounded));
		At(states.DashState, states.WallSlideState,
			new FuncPredicate(() => _collisionsChecker.IsTouchingWall));
		At(states.DashState, states.JumpState,
			new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame && _playerPhysicsController.JumpModule.CanMultiJump()));
		At(states.DashState, states.IdleCrouchState,
			new FuncPredicate(() => !_playerTimerRegistry.dashTimer.IsRunning && _collisionsChecker.IsGrounded && _inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection()[0] == 0));
		At(states.DashState, states.CrouchState,
			new FuncPredicate(() => !_playerTimerRegistry.dashTimer.IsRunning && _collisionsChecker.IsGrounded && _inputReader.GetCrouchState().IsHeld));
		At(states.DashState, states.LocomotionState,
			new FuncPredicate(() => !_playerTimerRegistry.dashTimer.IsRunning && _collisionsChecker.IsGrounded));
	}

	private void SetupCrouchRollTransitions(PlayerStates states)
	{
		At(states.CrouchRollState, states.IdleState,
			new FuncPredicate(() => !_playerTimerRegistry.crouchRollTimer.IsRunning && !_inputReader.GetCrouchState().IsHeld && !_collisionsChecker.BumpedHead));
		At(states.CrouchRollState, states.FallState,
			new FuncPredicate(() => !_collisionsChecker.IsGrounded));
		At(states.CrouchRollState, states.IdleCrouchState,
			new FuncPredicate(() => !_playerTimerRegistry.crouchRollTimer.IsRunning && (_inputReader.GetCrouchState().IsHeld || _collisionsChecker.BumpedHead) && _inputReader.GetMoveDirection()[0] == 0));
		At(states.CrouchRollState, states.CrouchState,
			new FuncPredicate(() => !_playerTimerRegistry.crouchRollTimer.IsRunning));
	}

	private void SetupWallSlideTransitions(PlayerStates states)
	{
		At(states.WallSlideState, states.WallJumpState,
			new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame));
		At(states.WallSlideState, states.IdleState,
			new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero));
		At(states.WallSlideState, states.LocomotionState,
			new FuncPredicate(() => _collisionsChecker.IsGrounded));
		At(states.WallSlideState, states.FallState,
			new FuncPredicate(() => !_collisionsChecker.IsGrounded && (_playerTimerRegistry.wallJumpTimer.IsFinished || !_collisionsChecker.IsTouchingWall)));
		At(states.WallSlideState, states.DashState,
			new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && _playerPhysicsController.WallSlideModule.CalculateWallDirectionX() != _inputReader.GetMoveDirection().x));
		At(states.WallSlideState, states.IdleCrouchState,
			new FuncPredicate(() => _inputReader.GetCrouchState().IsHeld && _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection()[0] == 0));
	}

	private void SetupWallJumpTransitions(PlayerStates states)
	{
		At(states.WallJumpState, states.FallState,
			new FuncPredicate(() => !_collisionsChecker.IsGrounded && !_collisionsChecker.IsTouchingWall));
	}

	private void SetupRunJumpTransitions(PlayerStates states)
	{
		At(states.RunJumpState, states.RunFallState,
			new FuncPredicate(() => !_collisionsChecker.IsGrounded && _playerPhysicsController.JumpModule.CanFall()));
		At(states.RunJumpState, states.DashState,
			new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && _playerPhysicsController.DashModule.CanDash()));
	}

	private void SetupRunFallTransitions(PlayerStates states)
	{
		At(states.RunFallState, states.JumpState,
			new FuncPredicate(() => _collisionsChecker.IsGrounded && _playerTimerRegistry.jumpBufferTimer.IsRunning && !_inputReader.GetRunState().IsHeld));
		At(states.RunFallState, states.RunJumpState,
			new FuncPredicate(() => _collisionsChecker.IsGrounded && _playerTimerRegistry.jumpBufferTimer.IsRunning));
		At(states.RunFallState, states.RunJumpState,
			new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame && (_playerTimerRegistry.jumpCoyoteTimer.IsRunning || _playerPhysicsController.JumpModule.CanMultiJump())));
		At(states.RunFallState, states.DashState,
			new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && _playerPhysicsController.DashModule.CanDash())); // FIXME: Возможно, это должно быть dash
		At(states.RunFallState, states.FallState,
			new FuncPredicate(() => !_inputReader.GetRunState().IsHeld && !_collisionsChecker.IsGrounded));
		At(states.RunFallState, states.IdleState,
			new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero));
		At(states.RunFallState, states.WallSlideState,
			new FuncPredicate(() => _collisionsChecker.IsTouchingWall));
		At(states.RunFallState, states.RunState,
			new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetRunState().IsHeld));
	}
}