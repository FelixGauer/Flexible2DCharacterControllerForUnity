using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionsChecker : MonoBehaviour
{
	[SerializeField] PlayerControllerStats stats;

	public bool IsGrounded { get; private set; }
	public bool BumpedHead { get; private set; }

	// private Collider2D _feetCollider;
	// private Collider2D _bodyCollider;
	private RaycastHit2D _headHit;
	
	[SerializeField] private Collider2D FeetCollider;
	[SerializeField] private Collider2D BodyCollider;


	void Update()
	{
		CheckIsGrounded();
		CheckBumpedHead();
	}

	private void CheckIsGrounded()
	{
		var bounds = FeetCollider.bounds;
		Vector2 boxCastOrigin = new Vector2(bounds.center.x, bounds.min.y);
		Vector2 boxCastSize = new Vector2(bounds.size.x, stats.GroundDetectionRayLenght);

		IsGrounded = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, stats.GroundDetectionRayLenght, stats.GroundLayer).collider != null;
	}

	private void CheckBumpedHead()
	{
		var bounds = FeetCollider.bounds;

		Vector2 boxCastOrigin = new Vector2(bounds.center.x, BodyCollider.bounds.max.y);
		Vector2 boxCastSize = new Vector2(bounds.size.x * stats.HeadWidth, stats.HeadDetectionRayLength);

		_headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, stats.HeadDetectionRayLength, stats.GroundLayer);

		if (_headHit.collider != null)
		{
			BumpedHead = true;
		}
		else
		{
			BumpedHead = false;
		}
		// float headWidth = stats.HeadWidth;

		// Color rayColor;
		// if (BumpedHead)
		// {
		// 	rayColor = Color.green;
		// }
		// else
		// {
		// 	rayColor = Color.red;
		// }

		// Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y), Vector2.up * stats.HeadDetectionRayLength, rayColor);
		// Debug.DrawRay(new Vector2(boxCastOrigin.x + (boxCastSize.x / 2) * headWidth, boxCastOrigin.y), Vector2.up * stats.HeadDetectionRayLength, rayColor);
		// Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth + stats.HeadDetectionRayLength, boxCastOrigin.y), Vector2.right * boxCastSize.x * headWidth, rayColor);
	}

}
