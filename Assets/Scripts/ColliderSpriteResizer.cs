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
    
    // public void SetSize(float height, float offset, ResizeTarget target = ResizeTarget.Both)
    public void SetSize(float height, float offset, ResizeTarget? target = null)
    {
        ResizeTarget actualTarget = target ?? resizeTarget;
        switch (actualTarget)
        {
            case ResizeTarget.Both:
                capsuleCollider.size = new Vector2(_normalColliderSize.x, height);
                capsuleCollider.offset = new Vector2(_normalColliderOffset.x, offset);
                spriteTransform.localScale = new Vector2(_normalSpriteScale.x, height);
                spriteTransform.localPosition = new Vector2(_normalSpritePosition.x, _normalSpritePosition.y + offset);
                break;
            
            case ResizeTarget.ColliderOnly:
                capsuleCollider.size = new Vector2(_normalColliderSize.x, height);
                capsuleCollider.offset = new Vector2(_normalColliderOffset.x, offset);
                break;
            
            case ResizeTarget.SpriteOnly:
                spriteTransform.localScale = new Vector2(_normalSpriteScale.x, height);
                spriteTransform.localPosition = new Vector2(_normalSpritePosition.x, _normalSpritePosition.y + offset);
                break;
        }
    }

    public void ResetToNormal()
    {
        capsuleCollider.size = _normalColliderSize;
        capsuleCollider.offset = _normalColliderOffset; 
        
        spriteTransform.localScale = _normalSpriteScale;
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
}