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
    [SerializeField] private Collider2D targetCollider;
    [SerializeField] private Transform spriteTransform;
    [SerializeField] private ResizeTarget resizeTarget;

    private Vector2 _normalColliderSize;
    private Vector2 _normalSpriteScale;
    private Vector2 _normalSpritePosition;
    private Vector2 _normalColliderOffset;

    private void Awake()
    {
        if (targetCollider == null)
            targetCollider = GetComponent<Collider2D>();
            
        _normalColliderSize = GetColliderSize();
        _normalColliderOffset = GetColliderOffset();
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
                SetColliderSize(new Vector2(_normalColliderSize.x, height));
                SetColliderOffset(new Vector2(_normalColliderOffset.x, offset));
                
                float scaleMultiplier = height / _normalColliderSize.y;
                spriteTransform.localScale = new Vector2(currentScaleX, _normalSpriteScale.y * scaleMultiplier);
                spriteTransform.localPosition = new Vector2(_normalSpritePosition.x, _normalSpritePosition.y + offset);
                break;
            
            case ResizeTarget.ColliderOnly:
                SetColliderSize(new Vector2(_normalColliderSize.x, height));
                SetColliderOffset(new Vector2(_normalColliderOffset.x, offset));
                break;
            
            case ResizeTarget.SpriteOnly:
                float spriteScaleMultiplier = height / _normalColliderSize.y;
                spriteTransform.localScale = new Vector2(currentScaleX, _normalSpriteScale.y * spriteScaleMultiplier);
                spriteTransform.localPosition = new Vector2(_normalSpritePosition.x, _normalSpritePosition.y + offset);
                break;
        }
    }
    
    public void ResetToNormal()
    {
        SetColliderSize(_normalColliderSize);
        SetColliderOffset(_normalColliderOffset);
        
        float currentScaleX = Mathf.Abs(_normalSpriteScale.x) * GetCurrentScaleXSign();
        spriteTransform.localScale = new Vector2(currentScaleX, _normalSpriteScale.y);
        spriteTransform.localPosition = _normalSpritePosition;
    }

    public void SetSizeSmooth(float height, float offset, float duration)
    {
        SetSize(height, offset);
    }
    
    public void SetCustomSize(Vector2 colliderSize, Vector2 spriteScale, Vector2 spritePosition, Vector2 colliderOffset)
    {
        SetColliderSize(colliderSize);
        SetColliderOffset(colliderOffset);
        spriteTransform.localScale = spriteScale;
        spriteTransform.localPosition = spritePosition;
    }

    public Vector2 GetNormalColliderSize() => _normalColliderSize;
    public Vector2 GetCurrentColliderSize() => GetColliderSize();
    public Vector2 GetNormalSpritePosition() => _normalSpritePosition;
    
    private float GetCurrentScaleXSign()
    {
        return spriteTransform.localScale.x >= 0 ? 1f : -1f;
    }
    
    private Vector2 GetColliderSize()
    {
        return targetCollider switch
        {
            BoxCollider2D box => box.size,
            CapsuleCollider2D capsule => capsule.size,
            CircleCollider2D circle => new Vector2(circle.radius * 2, circle.radius * 2),
            _ => Vector2.one
        };
    }
    
    private void SetColliderSize(Vector2 size)
    {
        switch (targetCollider)
        {
            case BoxCollider2D box:
                box.size = size;
                break;
            case CapsuleCollider2D capsule:
                capsule.size = size;
                break;
            case CircleCollider2D circle:
                circle.radius = Mathf.Max(size.x, size.y) / 2f;
                break;
        }
    }
    
    private Vector2 GetColliderOffset()
    {
        return targetCollider switch
        {
            BoxCollider2D box => box.offset,
            CapsuleCollider2D capsule => capsule.offset,
            CircleCollider2D circle => circle.offset,
            _ => Vector2.zero
        };
    }
    
    private void SetColliderOffset(Vector2 offset)
    {
        switch (targetCollider)
        {
            case BoxCollider2D box:
                box.offset = offset;
                break;
            case CapsuleCollider2D capsule:
                capsule.offset = offset;
                break;
            case CircleCollider2D circle:
                circle.offset = offset;
                break;
        }
    }
}