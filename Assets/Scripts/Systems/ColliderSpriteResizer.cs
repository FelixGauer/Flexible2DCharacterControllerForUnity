using System;
using UnityEngine;
using UnityEngine.Serialization;

public enum ResizeTarget
{
    Both,
    ColliderOnly,
    SpriteOnly
}

public class ColliderSpriteResizer : MonoBehaviour
{
    [SerializeField] private CapsuleCollider2D capsuleCollider;
    [SerializeField] private Transform spriteTransform;
    [SerializeField] private ResizeTarget resizeTarget;

    private Vector2 _normalColliderSize;
    private Vector2 _normalSpriteScale;
    private Vector2 _normalSpritePosition;
    private Vector2 _normalColliderOffset;

    private void Awake()
    {
        _normalColliderSize = capsuleCollider.size;
        _normalColliderOffset = capsuleCollider.offset; 
        _normalSpriteScale = spriteTransform.localScale;
        _normalSpritePosition = spriteTransform.localPosition;
    }
    
    public void SetSize(float height, float offset, ResizeTarget? target = null)
    {
        ResizeTarget actualTarget = target ?? resizeTarget;
        float currentScaleX = Mathf.Abs(_normalSpriteScale.x) * GetCurrentScaleXSign();

        switch (actualTarget)
        {
            case ResizeTarget.Both:
                // Коллайдер - абсолютные размеры

                capsuleCollider.size = new Vector2(_normalColliderSize.x, height);
                capsuleCollider.offset = new Vector2(_normalColliderOffset.x, offset);
                
                // Спрайт - относительное масштабирование
                float scaleMultiplier = height / _normalColliderSize.y;
                spriteTransform.localScale = new Vector2(currentScaleX, _normalSpriteScale.y * scaleMultiplier);
                spriteTransform.localPosition = new Vector2(_normalSpritePosition.x, _normalSpritePosition.y + offset);
                break;
            
            case ResizeTarget.ColliderOnly:
                capsuleCollider.size = new Vector2(_normalColliderSize.x, height);
                capsuleCollider.offset = new Vector2(_normalColliderOffset.x, offset);
                break;
            
            case ResizeTarget.SpriteOnly:
                // Спрайт - относительное масштабирование
                float spriteScaleMultiplier = height / _normalColliderSize.y;
                spriteTransform.localScale = new Vector2(currentScaleX, _normalSpriteScale.y * spriteScaleMultiplier);
                spriteTransform.localPosition = new Vector2(_normalSpritePosition.x, _normalSpritePosition.y + offset);
                break;
        }
    }
    
    public void ResetToNormal()
    {
        capsuleCollider.size = _normalColliderSize;
        capsuleCollider.offset = _normalColliderOffset; 
        
        float currentScaleX = Mathf.Abs(_normalSpriteScale.x) * GetCurrentScaleXSign();

        // spriteTransform.localScale = _normalSpriteScale;
        spriteTransform.localScale = new Vector2(currentScaleX, _normalSpriteScale.y);
        spriteTransform.localPosition = _normalSpritePosition;
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
        capsuleCollider.size = colliderSize;
        capsuleCollider.offset = colliderOffset;
        spriteTransform.localScale = spriteScale;
        spriteTransform.localPosition = spritePosition;
    }

    public Vector2 GetNormalColliderSize() => _normalColliderSize;
    public Vector2 GetCurrentColliderSize() => capsuleCollider.size;
    public Vector2 GetNormalSpritePosition() => _normalSpritePosition;
    
    private float GetCurrentScaleXSign()
    {
        return spriteTransform.localScale.x >= 0 ? 1f : -1f;
    }
}

