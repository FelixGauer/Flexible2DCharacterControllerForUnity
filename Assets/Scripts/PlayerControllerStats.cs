using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerControllerStats", menuName = "PlayerControllerStats")]
public class PlayerControllerStats : ScriptableObject
{
	[Range(0f, 100f)] public float MoveSpeed = 12.5f;
	[Range(0f, 100f)] public float Acceleration = 5f;
	[Range(0f, 100f)] public float Deceleration = 20f;


	// TODO Сделать название сектора
	public LayerMask GroundLayer;
	public float GroundDetectionRayLenght = 0.02f;
	public float HeadDetectionRayLenght = 0.02f;
	[Range(0f, 2f)] public float HeadWidth = 0.75f;


	// public float Gravity { get; private set; }


	// TODO Переменные bool для дебага





	[Header("Jump")]
	public float JumpHeight = 6.5f;
	[Range(1f, 1.1f)] public float JumpHeightCompensationFactor = 1.054f;
	public float TimeTillJumpApex = 0.35f;
	[Range(0.01f, 5f)] public float GravityOnReleaseMultiplier = 2f;
	public float MaxFallSpeed = 26f;
	[Range(1, 5)] public int NumberOfJumpsAllowed = 2;

	[Header("Jump Cut")]
	[Range(0.02f, 0.3f)] public float TimeForUpwardsCancel = 0.027f;

	[Header("Jump Apex")]
	[Range(0.5f, 1f)] public float ApexThreshold = 0.97f;
	[Range(0.01f, 1f)] public float ApexHangTime = 0.075f;

	[Header("Jump Buffer")]
	[Range(0f, 1f)] public float JumpBufferTime = 0.125f;

	[Header("Jump Coyote Time")]
	[Range(0f, 1f)] public float JumpCoyoteTime = 0.1f;

	[Header("JumpVisualization Tool")]
	public bool ShowWalkJumpArc = false;
	public bool ShowRunJumpArc = false;
	public bool StopOnCollision = true;
	public bool DrawRight = true;
	[Range(5, 100)] public int ArcResolution = 20;
	[Range(0, 500)] public int VisualizationSteps = 90;

	public float Gravity { get; private set; }
	public float InitialJumpVelocity { get; private set; }
	public float AdjustedJumpHeight { get; private set; }

	private void OnValidate()
	{
		CalculateValues();
	}

	private void OnEnable()
	{
		CalculateValues();
	}

	private void CalculateValues()
	{
		AdjustedJumpHeight = JumpHeight * JumpHeightCompensationFactor;
		Gravity = -(2f * AdjustedJumpHeight) / Mathf.Pow(TimeTillJumpApex, 2f);
		InitialJumpVelocity = Mathf.Abs(Gravity) * TimeTillJumpApex;
	}


}
