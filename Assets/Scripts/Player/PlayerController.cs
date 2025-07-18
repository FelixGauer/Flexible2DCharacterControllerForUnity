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
	
	// #region Debbug
	//
	// //Debug var
	// private float maxYPosition;
	// private List<GameObject> markers = new List<GameObject>();
	// public GameObject markerPrefab;
	//
	// void AddJumpMarker()
	// {
	// 	// Получаем позицию конца линии TrailRenderer
	// 	Vector3 trailEndPosition = GetTrailEndPosition();
	//
	// 	// Спавним метку на этой позиции и сохраняем её в список markers
	// 	GameObject newMarker = Instantiate(markerPrefab, trailEndPosition, Quaternion.identity);
	// 	markers.Add(newMarker); // Добавляем созданную метку в список
	// }
	//
	// public void ClearMarkers()
	// {
	// 	foreach (GameObject marker in markers)
	// 	{
	// 		Destroy(marker);
	// 	}
	//
	// 	markers.Clear();
	// }
	//
	// Vector3 GetTrailEndPosition()
	// {
	// 	// Trail Renderer хранит все точки следа, берем последнюю из них
	// 	Vector3[] positions = new Vector3[_trailRenderer.positionCount];
	// 	_trailRenderer.GetPositions(positions);
	//
	// 	if (positions.Length > 0)
	// 	{
	// 		// Конец линии - это последняя точка в массиве
	// 		return positions[positions.Length - 1];
	// 	}
	// 	else
	// 	{
	// 		// Если точек нет, возвращаем позицию игрока
	// 		return transform.position;
	// 	}
	// }
	//
	// //TODO Дебаг для Grounded
	// private void Debbuging()
	// {
	// 	// Визуализация целевой скорости (Target Velocity)
	// 	// Debug.DrawLine(transform.position, transform.position + new Vector3(_targetVelocity.x, 0, 0), Color.red);
	//
	// 	// Визуализация текущего вектора скорости (_moveVelocity)
	// 	Debug.DrawLine(transform.position, transform.position + new Vector3(physicsHandler2D.GetVelocity().x, 0, 0), Color.blue);
	//
	// 	// Визуализация направления движения (_moveDirection)
	// 	if (inputReader.GetMoveDirection() != Vector2.zero)
	// 	{
	// 		Debug.DrawLine(transform.position, transform.position + new Vector3(inputReader.GetMoveDirection().x, 0, 0), Color.green);
	// 	}
	// 	else
	// 	{
	// 		// Визуализация замедления
	// 		if (physicsHandler2D.GetVelocity() != Vector2.zero)
	// 		{
	// 			Debug.DrawLine(transform.position, transform.position + new Vector3(physicsHandler2D.GetVelocity().x, 0, 0), Color.yellow);
	// 		}
	// 	}
	// }
	// #endregion
}