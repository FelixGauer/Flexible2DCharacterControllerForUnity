using System.Linq;
using UnityEngine;


public class CrouchRollState : BaseState
{
	public CrouchRollState(PlayerController player, Animator animator, InputReader inputReader, PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D, TurnChecker turnChecker) :
		base(player, animator, inputReader, playerControllerStats, physicsHandler2D, turnChecker) 
	{
		// найдём клип по имени (можно оптимизировать и вынести на инициализацию)
		_clip = animator.runtimeAnimatorController
			.animationClips
			.First(c => c.name == "CrouchRoll");
        
		// желаемая длительность — из ваших Stats
		_desiredDuration = playerControllerStats.CrouchRollTime;
	}

	private static readonly int SpeedParam = Animator.StringToHash("CrouchRollSpeedMultiplier");
	private AnimationClip _clip;
	private float _desiredDuration;

	
	public override void OnEnter()
	{		
		Debug.Log("CrouchRollState");
		
		// animator.Play("CrouchSlide", 0, 0f);

		float speedMult = _clip.length / _desiredDuration;
		animator.SetFloat(SpeedParam, speedMult);
		animator.Play("CrouchRoll", 0, 0f);
		
		
		player.playerPhysicsController.CrouchRollModule.StartCrouchRoll();
	}
	
	public override void Update()
	{
		// player.playerPhysicsController.CrouchRollModule.UpdateTimer();
		turnChecker.TurnCheck(inputReader.GetMoveDirection());

	}

	private Vector2 _moveVelocity;

	public override void FixedUpdate()
	{
		_moveVelocity.x = player.playerPhysicsController.CrouchRollModule.CrouchRoll(physicsHandler2D.GetVelocity()).x;
		physicsHandler2D.AddVelocity(_moveVelocity);
	}
	


	public override void OnExit()
	{
		animator.SetFloat(SpeedParam, 1f);
		
		// player._physicsHandler2D.AddVelocity(_moveVelocity);
		
		player.playerPhysicsController.CrouchModule.SetCrouchState(false);

		player.playerPhysicsController.CrouchRollModule.StopCrouchRoll();
	}

}
