using UnityEngine;

public class TurnChecker
{
	public bool IsFacingRight { get; private set; } = true;
	
	// public void TurnCheck(Vector2 moveDirection, Transform transform, bool wasWallSliding)
	// {
	// 	if (wasWallSliding) return;
	//
	// 	if ((moveDirection.x < 0 && IsFacingRight) || (moveDirection.x > 0 && !IsFacingRight))
	// 	{
	// 		Turn(transform);
	// 	}
	// }

	private readonly Transform _transform;

	public TurnChecker(Transform transform)
	{
		_transform = transform;
	}
	
	// Универсальный скрипт, проверку на wasWallSliding делаю в самом классе плеера
	public void TurnCheck(Vector2 moveDirection)
	{
		if ((moveDirection.x < 0 && IsFacingRight) || (moveDirection.x > 0 && !IsFacingRight))
		{
			Turn(_transform);
		}
	}
	
	
	// public void TurnCheck(Vector2 moveDirection, Transform transform)
	// {
	// 	if ((moveDirection.x < 0 && IsFacingRight) || (moveDirection.x > 0 && !IsFacingRight))
	// 	{
	// 		Turn(transform);
	// 	}
	// }

	private void Turn(Transform transform)
	{
		IsFacingRight = !IsFacingRight;
		transform.Rotate(0f, 180f, 0f);
	}
}
