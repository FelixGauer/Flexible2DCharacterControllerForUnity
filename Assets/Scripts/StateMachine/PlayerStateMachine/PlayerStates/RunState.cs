using UnityEngine;


public class RunState : BaseState
{
	// private Vector2 _moveVelocity;
	
	private float baseAnimationSpeed = 1f; // Базовая скорость анимации
	private float baseMovementSpeed = 12f;  // Базовая скорость движения
	
	public RunState(PlayerController player, Animator animator, InputReader inputReader, PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D, TurnChecker turnChecker) :
		base(player, animator, inputReader, playerControllerStats, physicsHandler2D, turnChecker) { }

	public override void OnEnter()
	{		
		Debug.Log("RunState");
		
		animator.Play("Run");
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
		var moveVelocity = player.playerPhysicsController.MovementModule.HandleMovement(physicsHandler2D.GetVelocity(), inputReader.GetMoveDirection(), playerControllerStats.RunSpeed, playerControllerStats.RunAcceleration, playerControllerStats.RunDeceleration); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
		physicsHandler2D.AddVelocity(moveVelocity);
	}
	
	public override void OnExit()
	{
		animator.SetFloat("Speed", 0f);
	}
}
