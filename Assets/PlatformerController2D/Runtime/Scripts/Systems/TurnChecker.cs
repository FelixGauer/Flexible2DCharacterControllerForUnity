using UnityEngine;

namespace PlatformerController2D.Runtime.Scripts.Systems
{
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
		
			Vector3 scale = _spriteTransform.localScale;
			scale.x = IsFacingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
		
			_spriteTransform.localScale = scale;
		}
	}
}
