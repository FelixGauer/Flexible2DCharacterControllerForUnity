using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerControllerStats", menuName = "PlayerControllerStats")]
public class PlayerControllerStats : ScriptableObject
{
	// [Tooltip("Set this to the layer your player is on")]
	
	[Header("Movement")] 
	[Range(0f, 100f)] public float MoveSpeed = 12.5f;
	[Range(0f, 100f)] public float Acceleration = 5f;
	[Range(0f, 100f)] public float Deceleration = 20f;

	[Header("Collision Check")] 
	public LayerMask GroundLayer;
	public float GroundDetectionRayLenght = 0.02f;
	public float HeadDetectionRayLength = 0.02f;
	[Range(0f, 5f)] public float HeadWidth = 0.75f;

	[Header("Jump")]
	[Range(0f, 100f)] public float maxJumpHeight = 5f;
	[Range(0f, 100f)] public float minJumpHeight = 2f;
	[Range(0f, 5f)] public float timeTillJumpApex = 0.35f;
	[Range(0f, 5f)] public float jumpHeightCompensationFactor = 1.06f;
	[Range(0f, 5f)] public float GravityMultiplayer = 1.5f;
	[Range(0f, 5f)] public float CoyoteTime = 1.5f;
	[Range(0f, 5f)] public float BufferTime = 0.2f;
	
	// Dont Use
	[Range(0f, 200f)] public float MaxFallSpeed = 40f; 
	[Range(0f, 200f)] public float JumpEndEarlyGravityModifier = 3f; 
	[Range(-3f, 3f)] public float GroundGravity = -1.5f; 

}
