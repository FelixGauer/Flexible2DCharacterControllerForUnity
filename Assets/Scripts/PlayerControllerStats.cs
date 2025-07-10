using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "PlayerControllerStats", menuName = "PlayerControllerStats")]
public class PlayerControllerStats : ScriptableObject
{
	// [Tooltip("Set this to the layer your player is on")]
	
	[Header("Movement Settings")]
	public MovementSettings movementSettings = new MovementSettings();


	public bool ResetNumberJumpOnWall;
	public bool ResetNumberDashOnWall;

	
	[Header("Movement")]
	[Range(0f, 200f)] public float MoveSpeed = 12.5f;
	[Range(0f, 200f)] public float WalkAcceleration = 5f;
	[Range(0f, 200f)] public float WalkDeceleration = 20f;
	
	[Header("Run")]
	[Range(0f, 200f)] public float RunSpeed = 20f;
	[Range(0f, 200f)] public float RunAcceleration = 15f;
	[Range(0f, 200f)] public float RunDeceleration = 30f;

	[Header("Jump")]
	[Range(0f, 100f)] public float MaxJumpHeight = 5f;
	[Range(0f, 100f)] public float MinJumpHeight = 2f;
	[Range(0f, 5f)] public float TimeTillJumpApex = 0.35f;
	[Range(0f, 5f)] public float JumpHeightCompensationFactor = 1.06f;
	
	[Header("MultiJump")]
	[Range(0f, 50f)] public float MaxNumberJumps = 2f;

	[Header("JumpGravity")]
	[Range(0f, 5f)] public float JumpGravityMultiplayer = 1.5f;
	[Range(0f, 5f)] public float FallGravityMultiplayer = 1.5f;
	
	[Header("Jump/Air Movement")]
	[Range(0f, 200f)] public float AirAcceleration = 30f;
	[Range(0f, 200f)] public float AirDeceleration = 5f;
	[Range(0f, 200f)] public float RunAirAcceleration = 30f;
	[Range(0f, 200f)] public float RunAirDeceleration = 5f;
	
	[Header("JumpHang")]
	public float JumpHangTimeThreshold = 1f;
	public float JumpHangGravityMultiplier = 0.5f;
	
	[Header("WallSlide")]
	public float StartVelocityWallSlide = 3f;
	public float WallSlideSpeedMax = 5f;
	// public float WallSlideGravityMultiplayer = 1f;
	public float WallSlideDeceleration = 0.1f;

	[Header("WallJump")] 
	public bool CanWallJumpTowardsTheWall;
	public Vector2 WallJumpClimb;
	public Vector2 WallJumpOff;
	public Vector2 WallLeap;
	[Range(0f, 200f)] public float WallFallAirAcceleration = 30f;
	[Range(0f, 200f)] public float WallFallAirDeceleration = 5f;
	[Range(0f, 200f)] public float WallFallSpeed = 5f;
	
	[Header("Dash")]
	[Range(0f, 200f)] public float DashVelocity = 15f;
	public float MaxNumberDash = 2f;
	// public float DashGravityMultiplayer = 0f;
	[Range(0f, 200f)] public float DashFallSpeed = 5f;
	[Range(0f, 200f)] public float DashFallAirAcceleration = 30f;
	[Range(0f, 200f)] public float DashFallAirDeceleration = 5f;
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
	public float CrouchMoveSpeed = 7f;
	public float CrouchHeight = 0.6f;
	public float CrouchOffset = 0.2f;
	public float CrouchRollVelocity = 10f;
	[Range(0f, 200f)] public float CrouchAcceleration = 15f;
	[Range(0f, 200f)] public float CrouchDeceleration = 30f;

	[Header("Timers")]
	public float CoyoteTime = 1.5f;
	public float BufferTime = 0.2f;
	public float WallFallTime = 0.25f;
	public float DashTime = 0.11f;
	public float CrouchRollTime = 0.1f;
	public float WallJumpTime = 0.05f;

	[Header("Fall")]
	[Range(0f, 100f)] public float MaxFallSpeed = 20f;
	
	[Range(-3f, 3f)] public float GroundGravity = -1.5f;
	[Range(0, 200f)] public float MaxUpwardSpeed = 50f;

	[Header("Animation")]
	public float BaseMovementAnimationSpeed = 3f;
	public float BaseRunAnimationSpeed = 12f;
	public float BaseCrouchAnimationSpeed = 7f;

	[Header("Collision Check")]
	public float GroundDetectionRayLenght = 0.02f;
	public float HeadDetectionRayLength = 0.02f;
	[Range(0f, 5f)] public float HeadWidth = 0.75f;
	public float WallDetectionRayLength	 = 0.125f;
	[Range(0.01f, 3f)]public float WallDetectionRayHeightMultiplayer = 0.9f;
	public LayerMask GroundLayer;
	
	
	
		
	public float AdjustedJumpHeight => MaxJumpHeight * JumpHeightCompensationFactor;  
	public float Gravity => 2f * AdjustedJumpHeight / MathF.Pow(TimeTillJumpApex, 2f);  
	public float MaxJumpVelocity => Gravity * TimeTillJumpApex;  
	public float MinJumpVelocity => Mathf.Sqrt(2 * MinJumpHeight * Gravity);
}
