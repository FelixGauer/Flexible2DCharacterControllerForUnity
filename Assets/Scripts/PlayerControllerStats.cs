using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerControllerStats", menuName = "PlayerControllerStats")]
public class PlayerControllerStats : ScriptableObject
{
	// [Tooltip("Set this to the layer your player is on")]

	[Header("Movement")]
	[Range(0f, 100f)] public float MoveSpeed = 12.5f;
	[Range(0f, 100f)] public float WalkAcceleration = 5f;
	[Range(0f, 100f)] public float WalkDeceleration = 20f;
	[Range(0f, 100f)] public float RunAcceleration = 15f;
	[Range(0f, 100f)] public float RunDeceleration = 30f;
	[Range(0f, 100f)] public float CrouchAcceleration = 15f;
	[Range(0f, 100f)] public float CrouchDeceleration = 30f;

	[Header("Collision Check")]
	public LayerMask GroundLayer;
	public float GroundDetectionRayLenght = 0.02f;
	public float HeadDetectionRayLength = 0.02f;
	[Range(0f, 5f)] public float HeadWidth = 0.75f;
	public float WallDetectionRayLength	 = 0.125f;
	[Range(0.01f, 3f)]public float WallDetectionRayHeightMultiplayer = 0.9f;

	[Header("Jump")]
	[Range(0f, 100f)] public float maxJumpHeight = 5f;
	[Range(0f, 100f)] public float minJumpHeight = 2f;
	[Range(0f, 100f)] public float airAcceleration = 30f;
	[Range(0f, 100f)] public float airDeceleration = 5f;
	[Range(0f, 50f)] public float MaxNumberJumps = 2f;
	[Range(0f, 5f)] public float timeTillJumpApex = 0.35f;
	[Range(0f, 5f)] public float jumpHeightCompensationFactor = 1.06f;
	[Range(0f, 5f)] public float JumpGravityMultiplayer = 1.5f;
	[Range(0f, 5f)] public float FallGravityMultiplayer = 1.5f;
	
	[Header("JumpHang")]
	public float jumpHangTimeThreshold = 1f;
	public float jumpHangGravityMult = 0.5f;
	
	[Header("WallSlide/Jump")]
	public float StartVelocityWallSlide = 3f;
	public float WallSlideSpeedMax = 5f;
	// public float WallSlideGravityMultiplayer = 1f;
	public float WallSlideDeceleration = 0.1f;
	public Vector2 WallJumpClimb;
	public Vector2 WallJumpOff;
	public Vector2 WallLeap;
	
	[Header("Dash")]
	[Range(0f, 200f)] public float DashVelocity = 15f;
	
	public float MaxNumberDash = 2f;
	public float DashGravityMultiplayer = 0f;	
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
	
	[Header("Run")]
	public float RunSpeed = 20f;
	
	
	[Header("Timers")]
	public float CoyoteTime = 1.5f;
	public float BufferTime = 0.2f;
	public float WallJumpTime = 0.25f;
	public float DashTime = 0.11f;
	public float CrouchRollTime = 0.1f;
	
	[Header("Fall")]
	[Range(0f, 100f)] public float maxFallSpeed = 20f;
	[Range(-3f, 3f)] public float GroundGravity = -1.5f;
}
