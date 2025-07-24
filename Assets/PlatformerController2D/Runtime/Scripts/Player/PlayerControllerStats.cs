using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "PlayerControllerStats", menuName = "PlayerControllerStats")]
public class PlayerControllerStats : ScriptableObject
{
	[Header("Movement Settings")]
	public MovementSettings movementSettings = new MovementSettings();

	public bool ResetNumberJumpOnWall = true;
	public bool ResetNumberDashOnWall = true;
	public bool CanJumpInTheAirTowardsTheWall = true;

	[Header("Movement")]
	[Range(0f, 200f)] public float WalkSpeed = 3f;
	[Range(0f, 200f)] public float WalkAcceleration = 150f;
	[Range(0f, 200f)] public float WalkDeceleration = 150f;
	
	[Header("Run")]
	[Range(0f, 200f)] public float RunSpeed = 15f;
	[Range(0f, 200f)] public float RunAcceleration = 150f;
	[Range(0f, 200f)] public float RunDeceleration = 150f;

	[Header("Jump")]
	[Range(0f, 100f)] public float MaxJumpHeight = 3.5f;
	[Range(0f, 100f)] public float MinJumpHeight = 1f;
	[Range(0f, 5f)] public float TimeTillJumpApex = 0.3f;
	[Range(0f, 5f)] public float JumpHeightCompensationFactor = 1.06f;
	
	[Header("JumpGravity")]
	[Range(0f, 5f)] public float JumpGravityMultiplayer = 1f;
	[Range(0f, 5f)] public float FallGravityMultiplayer = 1.2f;
	
	[Header("JumpHang")]
	public float JumpHangTimeThreshold = 1f;
	public float JumpHangGravityMultiplier = 0.5f;
	
	[Header("MultiJump")]
	[Range(0f, 50f)] public float MaxNumberJumps = 2f;
	
	[Header("Jump/Air Movement")]
	[Range(0f, 200f)] public float AirAcceleration = 100f;
	[Range(0f, 200f)] public float AirDeceleration = 5f;
	[Range(0f, 200f)] public float RunAirAcceleration = 150f;
	[Range(0f, 200f)] public float RunAirDeceleration = 23f;
	[Range(0f, 200f)] public float WallFallSpeed = 10f;
	[Range(0f, 200f)] public float WallFallAirAcceleration = 150f;
	[Range(0f, 200f)] public float WallFallAirDeceleration = 23f;
	[Range(0f, 200f)] public float DashFallSpeed = 15f;
	[Range(0f, 200f)] public float DashFallAirAcceleration = 80f;
	[Range(0f, 200f)] public float DashFallAirDeceleration = 80f;
	
	
	[Header("WallSlide")]
	public float StartVelocityWallSlide = 2.5f;
	public float WallSlideSpeedMax = 3.5f;
	// public float WallSlideGravityMultiplayer = 1f;
	public float WallSlideDeceleration = 5f;

	[Header("WallJump")] 
	public Vector2 WallJumpClimb = new Vector2(20f,25f);
	public Vector2 WallJumpOff = new Vector2(5f, 8f);
	public Vector2 WallLeap = new Vector2(40f, 15f);

	
	[Header("Dash")]
	[Range(0f, 200f)] public float DashVelocity = 30f;
	public float MaxNumberDash = 2f;
	// public float DashGravityMultiplayer = 0f;

	public readonly Vector2[] DashDirections = new Vector2[]
	{
		new Vector2(0, 0), // Nothing
		new Vector2(1, 0), // Right
		new Vector2(1, 1).normalized, // Top-Right
		new Vector2(0, 1), // Up
		new Vector2(-1, 1).normalized, // Top-Left
		new Vector2(-1, 0), // Left
		new Vector2(-1, -1).normalized, // Bot-Left
		new Vector2(0, -1), // Down
		new Vector2(1, -1).normalized, // Bot-Right
	};
	
	[Header("Crouch/CrouchRoll")]
	public float CrouchMoveSpeed = 5f;
	public float CrouchHeight = 0.6f;
	public float CrouchOffset = 0.2f;
	public float CrouchRollVelocity = 10f;
	[Range(0f, 200f)] public float CrouchAcceleration = 150f;
	[Range(0f, 200f)] public float CrouchDeceleration = 150f;

	[Header("Timers")]
	public float CoyoteTime = 0.13f;
	public float BufferTime = 0.2f;
	public float WallFallTime = 0.2f;
	public float DashTime = 0.11f;
	public float CrouchRollTime = 0.25f;
	public float WallJumpTime = 0.05f;

	[Header("Fall")]
	[Range(0f, 100f)] public float MaxFallSpeed = 30f;
	[Range(0, 200f)] public float MaxUpwardSpeed = 50f;
	[Range(-3f, 3f)] public float GroundGravity = -1.5f;
	
	[Header("Animation")]
	public float BaseMovementAnimationSpeed = 3f;
	public float BaseRunAnimationSpeed = 12f;
	public float BaseCrouchAnimationSpeed = 5f;

	[Header("Collision Check")]
	public float GroundDetectionRayLenght = 0.02f;
	public float HeadDetectionRayLength = 0.02f;
	[Range(0f, 5f)] public float HeadWidth = 1f;
	public float WallDetectionRayLength	 = 0.015f;
	[Range(0.01f, 3f)]public float WallDetectionRayHeightMultiplayer = 0.9f;
	public LayerMask GroundLayer;
	
	
	public float AdjustedJumpHeight => MaxJumpHeight * JumpHeightCompensationFactor;  
	public float Gravity => 2f * AdjustedJumpHeight / MathF.Pow(TimeTillJumpApex, 2f);  
	public float MaxJumpVelocity => Gravity * TimeTillJumpApex;  
	public float MinJumpVelocity => Mathf.Sqrt(2 * MinJumpHeight * Gravity);
}
