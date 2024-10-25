using UnityEngine;

public class CollisionsChecker : MonoBehaviour
{
	[SerializeField] PlayerControllerStats stats;
	
	[SerializeField] private Collider2D FeetCollider;
	[SerializeField] private Collider2D BodyCollider;

	public bool IsGrounded { get; private set; }
	public bool BumpedHead { get; private set; }
	public bool IsTouchingWall { get; private set; }

	private PlayerController playerController;
	private RaycastHit2D _headHit;
	private RaycastHit2D _wallHit;
	private RaycastHit2D _lastWallHit;
	
	private void Awake()
	{
		playerController = GetComponent<PlayerController>();
	}

	void Update()
	{
		CheckIsGrounded();
		CheckBumpedHead();
		CheckTouchWall();
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
		float headWidth = stats.HeadWidth;

		Color rayColor;
		if (BumpedHead)
		{
			rayColor = Color.green;
		}
		else
		{
			rayColor = Color.red;
		}

		Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y), Vector2.up * stats.HeadDetectionRayLength, rayColor);
		Debug.DrawRay(new Vector2(boxCastOrigin.x + (boxCastSize.x / 2) * headWidth, boxCastOrigin.y), Vector2.up * stats.HeadDetectionRayLength, rayColor);
		Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth + stats.HeadDetectionRayLength, boxCastOrigin.y), Vector2.right * boxCastSize.x * headWidth, rayColor);
	}

	private void CheckTouchWall()
	{
		float originEndPoint = 0f;

		if (playerController.TurnChecker.IsFacingRight)
		{
			originEndPoint = BodyCollider.bounds.max.x;
		}
		else
		{
			originEndPoint = BodyCollider.bounds.min.x;
		}

		float adjustedHeight = BodyCollider.bounds.size.y * stats.WallDetectionRayHeightMultiplayer;

		Vector2 boxCastOrigin = new Vector2(originEndPoint, BodyCollider.bounds.center.y);
		Vector2 boxCastSize = new Vector2(stats.WallDetectionRayLength, adjustedHeight);

		_wallHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, transform.right, stats.WallDetectionRayLength, stats.GroundLayer);

		if (_wallHit.collider != null)
		{
			_lastWallHit = _wallHit;
			IsTouchingWall = true;
		}
		else
		{
			IsTouchingWall = false;
		}

		#region Debug

		Color rayColor;
		if (IsTouchingWall)
		{
			rayColor = Color.green;
		}
		else
		{
			rayColor = Color.red;
		}

		Vector2 boxBottomLeft = new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - boxCastSize.y / 2);
		Vector2 boxBottomRight = new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y - boxCastSize.y / 2);
		Vector2 boxTopLeft = new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y + boxCastSize.y / 2);
		Vector2 boxTopRight = new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y + boxCastSize.y / 2);

		Debug.DrawLine(boxBottomLeft, boxBottomRight, rayColor);
		Debug.DrawLine(boxBottomRight, boxTopRight, rayColor);
		Debug.DrawLine(boxTopRight, boxTopLeft, rayColor);
		Debug.DrawLine(boxTopLeft, boxBottomLeft, rayColor);

		#endregion
	}
}
