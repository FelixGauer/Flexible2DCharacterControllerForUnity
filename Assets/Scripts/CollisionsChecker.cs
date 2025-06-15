using UnityEngine;

public class CollisionsChecker : MonoBehaviour
{
	[SerializeField] PlayerControllerStats stats;

	[SerializeField] private Collider2D FeetCollider;
	[SerializeField] private Collider2D BodyCollider;

	public bool IsGrounded { get; private set; }
	public bool BumpedHead { get; private set; }
	public bool IsTouchingWall { get; private set; }

	private RaycastHit2D _headHit;
	private RaycastHit2D _wallHit;
	private RaycastHit2D _lastWallHit;
	
	// Делегаты для получения состояния персонажа
	public System.Func<bool> IsSitting;
	public System.Func<bool> IsFacingRight;

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
		float currentHeadDetectionRayLength = IsSitting() ? 0.4f : stats.HeadDetectionRayLength; // FIXME Заменить 0.4

		Vector2 boxCastOrigin = new Vector2(bounds.center.x, BodyCollider.bounds.max.y);
		Vector2 boxCastSize = new Vector2(bounds.size.x * stats.HeadWidth, stats.HeadDetectionRayLength);

		_headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, currentHeadDetectionRayLength, stats.GroundLayer);
		BumpedHead = _headHit.collider != null;

		Color rayColor = BumpedHead ? Color.green : Color.red;

		Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * stats.HeadWidth, boxCastOrigin.y), Vector2.up * currentHeadDetectionRayLength, rayColor);
		Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2 * stats.HeadWidth, boxCastOrigin.y), Vector2.up * currentHeadDetectionRayLength, rayColor);
		Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * stats.HeadWidth, currentHeadDetectionRayLength + boxCastOrigin.y), Vector2.right * boxCastSize.x * stats.HeadWidth, rayColor);
	}

	private void CheckTouchWall()
	{
		float originEndPoint = IsFacingRight() ? BodyCollider.bounds.max.x : BodyCollider.bounds.min.x;
		float adjustedHeight = BodyCollider.bounds.size.y * stats.WallDetectionRayHeightMultiplayer;

		Vector2 boxCastOrigin = new Vector2(originEndPoint, BodyCollider.bounds.center.y);
		Vector2 boxCastSize = new Vector2(stats.WallDetectionRayLength, adjustedHeight);

		_wallHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, transform.right, stats.WallDetectionRayLength, stats.GroundLayer);

		// IsTouchingWall = _wallHit.collider != null;
		// if (IsTouchingWall) _lastWallHit = _wallHit;
		
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

		Color rayColor = IsTouchingWall ? Color.green : Color.red;

		// Отрисовка отладочного бокса
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
