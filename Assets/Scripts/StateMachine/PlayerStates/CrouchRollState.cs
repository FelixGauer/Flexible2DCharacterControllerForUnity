using System.Linq;
using UnityEngine;


public class CrouchRollState : BaseState
{
	public CrouchRollState(PlayerController player, Animator animator) 
		: base(player, animator)
	{
		// найдём клип по имени (можно оптимизировать и вынести на инициализацию)
		_clip = animator.runtimeAnimatorController
			.animationClips
			.First(c => c.name == "CrouchRoll");
        
		// желаемая длительность — из ваших Stats
		_desiredDuration = player.stats.CrouchRollTime;
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

	public override void FixedUpdate()
	{
		player.playerPhysicsController.CrouchRollModule.CrouchRoll();
	}

	public override void OnExit()
	{
		animator.SetFloat(SpeedParam, 1f);

		player.playerPhysicsController.CrouchRollModule.StopCrouchRoll();
	}
	
	public static float Map(float value, float min1, float max1, float min2, float max2, bool clamp = false)
	{
		float val = min2 + (max2 - min2) * ((value - min1) / (max1 - min1));
        
		return clamp ? Mathf.Clamp(val, Mathf.Min(min2, max2), Mathf.Min(min2, max2)) : val;
	}
}
