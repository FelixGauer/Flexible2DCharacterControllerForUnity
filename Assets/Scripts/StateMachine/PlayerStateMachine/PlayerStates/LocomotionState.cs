using System.Net.NetworkInformation;
using UnityEngine;

public class LocomotionState : BaseState
{
	
	private float baseAnimationSpeed = 1f; // Базовая скорость анимации
	private float baseMovementSpeed = 3f;  // Базовая скорость движения
	
	
	public LocomotionState(PlayerController player, Animator animator, InputReader inputReader,
		PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D, TurnChecker turnChecker) :
		base(player, animator, inputReader, playerControllerStats, physicsHandler2D, turnChecker) { }
	
	public override void OnEnter()
	{
		Debug.Log("MoveEnter");
		
		animator.Play("Walk");
	}

	public override void Update()
	{
		turnChecker.TurnCheck(inputReader.GetMoveDirection());
		
		// Получаем текущую скорость движения
		float currentSpeed = physicsHandler2D.GetVelocity().magnitude;
        
		// Нормализуем скорость (от 0 до 1 или больше)
		float normalizedSpeed = currentSpeed / baseMovementSpeed;
        
		// Передаем параметр в Animator
		animator.SetFloat("Speed", normalizedSpeed);
	}

	public override void FixedUpdate()
	{
		var moveVelocity = player.playerPhysicsController.MovementModule.HandleMovement(physicsHandler2D.GetVelocity(), inputReader.GetMoveDirection(), playerControllerStats.MoveSpeed, playerControllerStats.WalkAcceleration, playerControllerStats.WalkDeceleration); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
		moveVelocity.y = playerControllerStats.GroundGravity;
		physicsHandler2D.AddVelocity(moveVelocity);
	}

	public override void OnExit()
	{
		animator.SetFloat("Speed", 0f);
	}
}
