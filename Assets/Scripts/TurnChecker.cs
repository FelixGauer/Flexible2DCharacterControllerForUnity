using UnityEngine;

public class TurnChecker : MonoBehaviour
{
	public bool IsFacingRight { get; private set; } = true;
	
	public void TurnCheck(Vector2 moveDirection, Transform transform, bool wasWallSliding)
	{
		if (wasWallSliding) return;

		if ((moveDirection.x < 0 && IsFacingRight) || (moveDirection.x > 0 && !IsFacingRight))
		{
			Turn(transform);
		}
	}

	private void Turn(Transform transform)
	{
		IsFacingRight = !IsFacingRight;
		transform.Rotate(0f, 180f, 0f);
	}
}
