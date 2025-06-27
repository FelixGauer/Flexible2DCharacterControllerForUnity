using UnityEngine;

// public class TurnChecker
// {
// 	public bool IsFacingRight { get; private set; } = true;
// 	
// 	private readonly Transform _transform;
//
// 	public TurnChecker(Transform transform)
// 	{
// 		_transform = transform;
// 	}
// 	
// 	// Универсальный скрипт, проверку на wasWallSliding делаю в самом классе плеера
// 	public void TurnCheck(Vector2 moveDirection)
// 	{
// 		if ((moveDirection.x < 0 && IsFacingRight) || (moveDirection.x > 0 && !IsFacingRight))
// 		{
// 			Turn(_transform);
// 		}
// 	}
// 	
//
// 	private void Turn(Transform transform)
// 	{
// 		IsFacingRight = !IsFacingRight;
// 		transform.Rotate(0f, 180f, 0f);
// 	}
// }

public class TurnChecker
{
	public bool IsFacingRight { get; private set; } = true;
    
	private readonly Transform _spriteTransform; // Ссылка на спрайт, а не на основной transform

	public TurnChecker(Transform spriteTransform)
	{
		_spriteTransform = spriteTransform;
	}
    
	public void TurnCheck(Vector2 moveDirection)
	{
		if ((moveDirection.x < 0 && IsFacingRight) || (moveDirection.x > 0 && !IsFacingRight))
		{
			Turn();
		}
	}
    
	private void Turn()
	{
		IsFacingRight = !IsFacingRight;
		// Используем scale вместо rotation - это не конфликтует с Squash and Stretch
		Vector3 scale = _spriteTransform.localScale;
		scale.x = IsFacingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
		_spriteTransform.localScale = scale;
	}
}
