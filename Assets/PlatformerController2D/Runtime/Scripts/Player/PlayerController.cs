using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private InputReader inputReader;
	[SerializeField] private PlayerControllerStats playerControllerStats;
	[SerializeField] private ColliderSpriteResizer colliderSpriteResizer;
	[SerializeField] private PhysicsHandler2D physicsHandler2D;
	[SerializeField] private CollisionsChecker collisionsChecker;

	[SerializeField] private Animator animator;
	
	[SerializeField] private Transform transformForTurn;

	private AnimationController _animationController;
	private TurnChecker _turnChecker;
	private PlayerTimerRegistry _playerTimerRegistry;
	private MovementLogic _movementLogic;
	
	private PlayerModulesFactory _playerModulesFactory;
	private PlayerModules _playerModules;
	
	private StateMachine _stateMachine;
	private PlayerStateMachineFactory _stateMachineFactory;

	
	private void Awake()
	{
		_turnChecker = new TurnChecker(transformForTurn);
		
		collisionsChecker.IsFacingRight = () => _turnChecker.IsFacingRight; 
		
		_playerTimerRegistry = new PlayerTimerRegistry(playerControllerStats);
		_movementLogic = new MovementLogic(playerControllerStats);
		
		_animationController = new AnimationController(animator);
		
		_playerModulesFactory  = new PlayerModulesFactory();
		_playerModules = _playerModulesFactory.CreateModules(playerControllerStats, collisionsChecker, _playerTimerRegistry, _turnChecker, colliderSpriteResizer);

		InitializeStateMachine();
	}
	
	private void InitializeStateMachine()
	{
		_stateMachineFactory = new PlayerStateMachineFactory(
			inputReader,
			playerControllerStats,
			physicsHandler2D,
			_turnChecker,
			collisionsChecker,
			_playerTimerRegistry,
			_animationController,
			_movementLogic,
			_playerModules
		);

		_stateMachine = _stateMachineFactory.CreateStateMachine();
	}
	
	private void Update() 
	{
		_stateMachine.Update();
		
		HandleTimers();
		// Debbuging();
	}

	private void FixedUpdate()
	{
		_stateMachine.FixedUpdate();
		
		inputReader.JumpResetFrameStates();
	}

	private void LateUpdate()
	{
		inputReader.ResetFrameStates();
	}

	private void HandleTimers() 
	{
		_playerTimerRegistry.UpdateAll();
	}
	
}