using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
	// TODO Анимации в хеши, скорость анимации зависит от скорости
	// TODO В каждом сосотоянии заменить вызов анимации с аниматора на аниматор контроллер
	
	[Header("References")]
	[SerializeField] private InputReader inputReader;
	[SerializeField] private PlayerControllerStats playerControllerStats;
	[SerializeField] private Transform spriteTransform;
	[SerializeField] private CapsuleCollider2D capsuleCollider;
	[SerializeField] private PhysicsHandler2D physicsHandler2D;
	[SerializeField] private Animator animator;
	
	private PlayerTimerRegistry _playerTimerRegistry;
	private CollisionsChecker _collisionsChecker;
	private StateMachine _stateMachine;
	private ColliderSpriteResizer _colliderSpriteResizer;
	private TurnChecker _turnChecker;
	private PlayerStateMachineFactory _stateMachineFactory;
	private AnimationController _animationController;

	public PlayerPhysicsController playerPhysicsController;
	
	private void Awake()
	{
		_collisionsChecker = GetComponent<CollisionsChecker>();
		capsuleCollider = GetComponentInChildren<CapsuleCollider2D>();
		_trailRenderer = GetComponent<TrailRenderer>();
		// _animator = GetComponentInChildren<Animator>(); // FIXME
		
		_turnChecker = new TurnChecker(this.transform); // FIXME
		_playerTimerRegistry = new PlayerTimerRegistry(playerControllerStats);
		_colliderSpriteResizer = new ColliderSpriteResizer(capsuleCollider, spriteTransform);
		playerPhysicsController = new PlayerPhysicsController(_collisionsChecker, playerControllerStats, _turnChecker, _playerTimerRegistry, _colliderSpriteResizer);

		_animationController = new AnimationController(animator);
		
		_collisionsChecker.IsFacingRight = () => _turnChecker.IsFacingRight; 

		InitializeStateMachine();
	}
	
	private void InitializeStateMachine()
	{
		_stateMachineFactory = new PlayerStateMachineFactory(
			this,
			animator,
			inputReader,
			playerControllerStats,
			physicsHandler2D,
			_turnChecker,
			_collisionsChecker,
			_playerTimerRegistry,
			playerPhysicsController,
			_animationController
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
	}

	private void LateUpdate()
	{
		inputReader.ResetFrameStates();
	}

	private void HandleTimers() // Сделать список
	{
		_playerTimerRegistry.UpdateAll();
	}
	
	// private void SetupStateMachine()
	// {
	//     _stateMachine = new StateMachine();
	//
	//     // Инициализация состояний
	//     var idleState = new IdleState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D, _turnChecker);
	//     var locomotionState = new LocomotionState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D, _turnChecker);
	//     var runState = new RunState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D, _turnChecker);
	//     var idleCrouchState = new IdleCrouchState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D, _turnChecker);
	//     var crouchState = new CrouchState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D, _turnChecker);
	//     var jumpState = new JumpState(this, _animator, _inputReader, _playerControllerStats,_physicsHandler2D, _turnChecker);
	//     var fallState = new FallState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D, _turnChecker);
	//     var dashState = new DashState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D, _turnChecker);
	//     var crouchRollState = new CrouchRollState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D, _turnChecker);
	//     var wallSlideState = new WallSlideState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D, _turnChecker);
	//     var wallJumpState = new WallJumpState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D, _turnChecker);
	//     var runJumpState = new RunJumpState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D, _turnChecker);
	//     var runFallState = new RunFallState(this, _animator, _inputReader, _playerControllerStats, _physicsHandler2D, _turnChecker);
	//
	//     // Переходы из idleState
	//     At(idleState, dashState, new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame));
	//     At(idleState, jumpState, new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame));
	//     At(idleState, idleCrouchState, new FuncPredicate(() => _inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection()[0] == 0));
	//     At(idleState, crouchState, new FuncPredicate(() => _inputReader.GetCrouchState().IsHeld));
	//     At(idleState, runState, new FuncPredicate(() => _inputReader.GetRunState().IsHeld && _inputReader.GetMoveDirection() != Vector2.zero));
	//     At(idleState, locomotionState, new FuncPredicate(() => _inputReader.GetMoveDirection() != Vector2.zero));
	//
	//     // Переходы из locomotionState
	//     At(locomotionState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded));
	//     At(locomotionState, jumpState, new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame));
	//     At(locomotionState, runState, new FuncPredicate(() => _inputReader.GetRunState().IsHeld && _collisionsChecker.IsGrounded));
	//     At(locomotionState, idleCrouchState, new FuncPredicate(() => _inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection()[0] == 0 && Mathf.Abs(_physicsHandler2D.GetVelocity().x) == 0f));
	//     At(locomotionState, crouchState, new FuncPredicate(() => _inputReader.GetCrouchState().IsHeld));
	//     At(locomotionState, dashState, new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame));
	//     At(locomotionState, idleState, new FuncPredicate(() => _inputReader.GetMoveDirection() == Vector2.zero && Mathf.Abs(_physicsHandler2D.GetVelocity().x) == 0f));
	//
	//     // Переходы из runState
	//     At(runState, runJumpState, new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame));
	//     At(runState, runFallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded));
	//     At(runState, idleState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero && Mathf.Abs(_physicsHandler2D.GetVelocity().x) == 0f));
	//     At(runState, idleState, new FuncPredicate(() => !_inputReader.GetRunState().IsHeld && _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero));
	//     At(runState, locomotionState, new FuncPredicate(() => !_inputReader.GetRunState().IsHeld && _collisionsChecker.IsGrounded));
	//     At(runState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded));
	//     At(runState, dashState, new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame));
	//     At(runState, jumpState, new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame)); // Оставлен для точного соответствия исходной логике
	//     At(runState, crouchState, new FuncPredicate(() => _inputReader.GetCrouchState().IsHeld));
	//
	//     // Переходы из idleCrouchState
	//     At(idleCrouchState, crouchState, new FuncPredicate(() => (_inputReader.GetCrouchState().IsHeld || _collisionsChecker.BumpedHead) && _inputReader.GetMoveDirection()[0] != 0));
	//     At(idleCrouchState, jumpState, new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame && !_collisionsChecker.BumpedHead));
	//     At(idleCrouchState, idleState, new FuncPredicate(() => !_inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection()[0] == 0 && !_collisionsChecker.BumpedHead));
	//     At(idleCrouchState, crouchRollState, new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame));
	//
	//     // Переходы из crouchState
	//     At(crouchState, idleCrouchState, new FuncPredicate(() => (_inputReader.GetCrouchState().IsHeld || _collisionsChecker.BumpedHead) && _inputReader.GetMoveDirection()[0] == 0 && Mathf.Abs(_physicsHandler2D.GetVelocity().x) == 0f));
	//     At(crouchState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded));
	//     At(crouchState, jumpState, new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame));
	//     At(crouchState, crouchRollState, new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame));
	//     At(crouchState, runState, new FuncPredicate(() => _inputReader.GetRunState().IsHeld && !_inputReader.GetCrouchState().IsHeld && _collisionsChecker.IsGrounded && !_collisionsChecker.BumpedHead));
	//     At(crouchState, idleState, new FuncPredicate(() => _inputReader.GetMoveDirection() == Vector2.zero && !_inputReader.GetCrouchState().IsHeld && _collisionsChecker.IsGrounded && !_collisionsChecker.BumpedHead));
	//     At(crouchState, locomotionState, new FuncPredicate(() => !_inputReader.GetCrouchState().IsHeld && _collisionsChecker.IsGrounded && !_collisionsChecker.BumpedHead));
	//
	//     // Переходы из jumpState
	//     At(jumpState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && (playerPhysicsController.JumpModule.CanFall() || _collisionsChecker.BumpedHead)));
	//     // At(jumpState, jumpState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && input.JumpInputButtonState.WasPressedThisFrame));
	//     At(jumpState, dashState, new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && playerPhysicsController.DashModule.CanDash()));
	//
	//     // Переходы из fallState
	//     At(fallState, jumpState, new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame && (_playerTimerRegistry.jumpCoyoteTimer.IsRunning || playerPhysicsController.CanMultiJump())));
	//     At(fallState, dashState, new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && playerPhysicsController.DashModule.CanDash()));
	//     At(fallState, jumpState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _playerTimerRegistry.jumpBufferTimer.IsRunning));
	//     At(fallState, idleState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero && Mathf.Abs(_physicsHandler2D.GetVelocity().x) < 0.1f));
	//     At(fallState, idleCrouchState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection()[0] == 0));
	//     At(fallState, crouchState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetCrouchState().IsHeld));
	//     At(fallState, runState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetRunState().IsHeld));
	//     At(fallState, locomotionState, new FuncPredicate(() => _collisionsChecker.IsGrounded));
	//     At(fallState, wallSlideState, new FuncPredicate(() => _collisionsChecker.IsTouchingWall));
	//
	//     // Переходы из dashState
	//     At(dashState, idleState, new FuncPredicate(() => !_playerTimerRegistry.dashTimer.IsRunning && _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero));
	//     At(dashState, runState, new FuncPredicate(() => !_playerTimerRegistry.dashTimer.IsRunning && _collisionsChecker.IsGrounded && _inputReader.GetRunState().IsHeld));
	//     At(dashState, fallState, new FuncPredicate(() => !_playerTimerRegistry.dashTimer.IsRunning && !_collisionsChecker.IsGrounded));
	//     At(dashState, wallSlideState, new FuncPredicate(() => _collisionsChecker.IsTouchingWall));
	//     At(dashState, jumpState, new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame && playerPhysicsController.JumpModule.CanMultiJump()));
	//     At(dashState, idleCrouchState, new FuncPredicate(() => !_playerTimerRegistry.dashTimer.IsRunning && _collisionsChecker.IsGrounded && _inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection()[0] == 0));
	//     At(dashState, crouchState, new FuncPredicate(() => !_playerTimerRegistry.dashTimer.IsRunning && _collisionsChecker.IsGrounded && _inputReader.GetCrouchState().IsHeld));
	//     At(dashState, locomotionState, new FuncPredicate(() => !_playerTimerRegistry.dashTimer.IsRunning && _collisionsChecker.IsGrounded));
	//
	//     // Переходы из crouchRollState
	//     At(crouchRollState, idleState, new FuncPredicate(() => !_playerTimerRegistry.crouchRollTimer.IsRunning && !_inputReader.GetCrouchState().IsHeld && !_collisionsChecker.BumpedHead));
	//     At(crouchRollState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded));
	//     At(crouchRollState, idleCrouchState, new FuncPredicate(() => !_playerTimerRegistry.crouchRollTimer.IsRunning && (_inputReader.GetCrouchState().IsHeld || _collisionsChecker.BumpedHead) && _inputReader.GetMoveDirection()[0] == 0));
	//     At(crouchRollState, crouchState, new FuncPredicate(() => !_playerTimerRegistry.crouchRollTimer.IsRunning));
	//
	//     // Переходы из wallSlideState
	//     At(wallSlideState, wallJumpState, new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame));
	//     At(wallSlideState, idleState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero));
	//     At(wallSlideState, locomotionState, new FuncPredicate(() => _collisionsChecker.IsGrounded));
	//     // At(wallSlideState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && !_collisionsChecker.IsTouchingWall));
	//     At(wallSlideState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && (_playerTimerRegistry.wallJumpTimer.IsFinished || !_collisionsChecker.IsTouchingWall)));
	//     At(wallSlideState, dashState, new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && playerPhysicsController.WallSlideModule.CurrentWallDirection != _inputReader.GetMoveDirection().x));
	//     // At(wallSlideState, dashState, new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && playerPhysicsController.WallSlideModule.CalculateWallDirectionX() != _inputReader.GetMoveDirection().x));
	//     At(wallSlideState, idleCrouchState, new FuncPredicate(() => _inputReader.GetCrouchState().IsHeld && _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection()[0] == 0));
	//
	//     // Переходы из wallJumpState
	//     At(wallJumpState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && !_collisionsChecker.IsTouchingWall));
	//
	//     // Переходы из runJumpState
	//     At(runJumpState, runFallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && playerPhysicsController.JumpModule.CanFall()));
	//     // At(runJumpState, runFallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && ((playerPhysicsController.PhysicsContext.MoveVelocity.y < 0f && playerPhysicsController.JumpModule.CanFall()))));
	//     At(runJumpState, dashState, new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && playerPhysicsController.DashModule.CanDash()));
	//
	//     // Переходы из runFallState
	//     At(runFallState, jumpState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _playerTimerRegistry.jumpBufferTimer.IsRunning && !_inputReader.GetRunState().IsHeld));
	//     At(runFallState, fallState, new FuncPredicate(() => !_inputReader.GetRunState().IsHeld && !_collisionsChecker.IsGrounded));
	//     At(runFallState, idleState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero));
	//     At(runFallState, runJumpState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _playerTimerRegistry.jumpBufferTimer.IsRunning));
	//     At(runFallState, runJumpState, new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame && (_playerTimerRegistry.jumpCoyoteTimer.IsRunning || playerPhysicsController.JumpModule.CanMultiJump())));
	//     At(runFallState, runJumpState, new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && playerPhysicsController.DashModule.CanDash())); //FIXME
	//     At(runFallState, wallSlideState, new FuncPredicate(() => _collisionsChecker.IsTouchingWall));
	//     At(runFallState, runState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetRunState().IsHeld));
	// 	//TODO runfall to locomotion
	//     // Установка начального состояния
	//     _stateMachine.SetState(idleState);
	// }
	//
	// void At(IState from, IState to, IPredicate condition) => _stateMachine.AddTransition(from, to, condition);
	// void Any(IState to, IPredicate condition) => _stateMachine.AddAnyTransition(to, condition);
	
	//Debug var
	private float maxYPosition;
	private TrailRenderer _trailRenderer;
	private List<GameObject> markers = new List<GameObject>();
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
		Debug.DrawLine(transform.position, transform.position + new Vector3(physicsHandler2D.GetVelocity().x, 0, 0), Color.blue);

		// Визуализация направления движения (_moveDirection)
		if (inputReader.GetMoveDirection() != Vector2.zero)
		{
			Debug.DrawLine(transform.position, transform.position + new Vector3(inputReader.GetMoveDirection().x, 0, 0), Color.green);
		}
		else
		{
			// Визуализация замедления
			if (physicsHandler2D.GetVelocity() != Vector2.zero)
			{
				Debug.DrawLine(transform.position, transform.position + new Vector3(physicsHandler2D.GetVelocity().x, 0, 0), Color.yellow);
			}
		}
	}
	#endregion
}