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

	// TODO Переменные bool для дебага

}
