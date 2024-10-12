using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionsChecker : MonoBehaviour
{
	[SerializeField] PlayerControllerStats stats;
	
	public bool IsGrounded { get; private set; }
	
	private Collider2D _feetCollider;
	
	private void Awake()
	{
		_feetCollider = GetComponentInChildren<BoxCollider2D>();
	}

	void Update()
	{
		CheckIsGrounded();
	}
	
	private void CheckIsGrounded()
	{
		var bounds = _feetCollider.bounds;
		Vector2 boxCastOrigin = new Vector2(bounds.center.x, bounds.min.y);
		Vector2 boxCastSize = new Vector2(bounds.size.x, stats.GroundDetectionRayLenght);

		IsGrounded = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, stats.GroundDetectionRayLenght, stats.GroundLayer).collider != null;
	}
	
}
