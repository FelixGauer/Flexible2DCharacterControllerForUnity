using UnityEngine;

public class ColliderSpriteResizer
{
    private readonly CapsuleCollider2D _capsuleCollider;
    private readonly Transform _spriteTransform;
    private readonly Vector2 _normalColliderSize;
    private readonly Vector2 _normalSpriteScale;
    private readonly Vector2 _normalSpritePosition;

    public ColliderSpriteResizer(CapsuleCollider2D capsuleCollider, Transform spriteTransform)
    {
        _capsuleCollider = capsuleCollider;
        _spriteTransform = spriteTransform;
        
        // Сохраняем изначальные значения
        _normalColliderSize = _capsuleCollider.size;
        _normalSpriteScale = _spriteTransform.localScale;
        _normalSpritePosition = _spriteTransform.localPosition;
    }

    public void SetSize(float height, float offset)
    {
        // Настройка коллайдера
        _capsuleCollider.size = new Vector2(_normalColliderSize.x, height);
        _capsuleCollider.offset = new Vector2(_capsuleCollider.offset.x, offset);
        
        // Настройка спрайта
        _spriteTransform.localScale = new Vector2(_normalSpriteScale.x, height);
        _spriteTransform.localPosition = new Vector2(_spriteTransform.localPosition.x, offset);
    }

    public void ResetToNormal()
    {
        // Возвращаем к изначальным значениям
        _capsuleCollider.size = _normalColliderSize;
        _capsuleCollider.offset = new Vector2(_capsuleCollider.offset.x, 0);
        
        _spriteTransform.localScale = _normalSpriteScale;
        _spriteTransform.localPosition = new Vector2(_spriteTransform.localPosition.x, _normalSpritePosition.y);
    }

    // Методы для плавного изменения размеров
    public void SetSizeSmooth(float height, float offset, float duration)
    {
        // Можно добавить корутину для плавного изменения
        // Пока что просто вызываем SetSize
        SetSize(height, offset);
    }
    public void SetCustomSize(Vector2 colliderSize, Vector2 spriteScale, Vector2 spritePosition, Vector2 colliderOffset)
    {
        _capsuleCollider.size = colliderSize;
        _capsuleCollider.offset = colliderOffset;
        _spriteTransform.localScale = spriteScale;
        _spriteTransform.localPosition = spritePosition;
    }

    public Vector2 GetNormalColliderSize() => _normalColliderSize;
    public Vector2 GetCurrentColliderSize() => _capsuleCollider.size;
}