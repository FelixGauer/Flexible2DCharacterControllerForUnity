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
	public float HeadDetectionRayLenght = 0.02f;
	[Range(0f, 2f)] public float HeadWidth = 0.75f;

	[Header("Jump")]
	[Range(0f, 200f)] public float FallAcceleration = 110f; // Gravity
	[Range(0f, 200f)] public float MaxFallSpeed = 40f; 
	[Range(0f, 200f)] public float JumpPower = 36f; 
	[Range(0f, 200f)] public float JumpEndEarlyGravityModifier = 3f; 
	[Range(0f, 2f)] public float JumpBuffer = 0.2f; 
	[Range(-3f, 3f)] public float GroundGravity = -1.5f; 

}
