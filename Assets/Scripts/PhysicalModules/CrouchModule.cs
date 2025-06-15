public class CrouchModule
{
    private readonly PhysicsContext _physicsContext;
    private readonly PlayerControllerStats _playerControllerStats;
    private readonly CollisionsChecker _collisionsChecker;
    private readonly ColliderSpriteResizer _colliderSpriteResizer;

    // private Vector2 normalHeight => _capsuleCollider.size;

    public CrouchModule(PlayerControllerStats playerControllerStats, CollisionsChecker collisionsChecker, ColliderSpriteResizer colliderSpriteResizer) 
    {
        _playerControllerStats = playerControllerStats;

        _collisionsChecker = collisionsChecker;
        
        _colliderSpriteResizer = colliderSpriteResizer;
    }
	
    // public void OnEnterCrouch() // TODO Delete
    // {
    //     SetCrouchState(true);
    // }
	   //
    // // Метод вызываемый при выходе из состояния приседа
    // public void OnExitCrouch(InputButtonState crouchRollButtonState)
    // {
    //     if (crouchRollButtonState.WasPressedThisFrame) return;
		  //
    //     SetCrouchState(false);
    // }
    
    public void SetCrouchState(bool isCrouching)
    {
        // Устанавливаем состояние сидения в CollisionsChecker
        _collisionsChecker.IsSitting = () => isCrouching;
        
        // Используем универсальные методы ColliderSpriteResizer
        if (isCrouching)
        {
            _colliderSpriteResizer.SetSize(
                _playerControllerStats.CrouchHeight, 
                -_playerControllerStats.CrouchOffset
            );
        }
        else
        {
            _colliderSpriteResizer.ResetToNormal();
        }
    }

    // Метод который регулирует высоту спрайта и коллайдера в зависимости сидит ли персонаж или стоит
    // public void SetCrouchState(bool isCrouching)
    // {
    //     _collisionsChecker.IsSitting = () => true && isCrouching;
    //             
    //     // Еси персонаж сидит его высота равна высоте приседа, если нет обычной высоте
    //     var height = isCrouching ? _playerControllerStats.CrouchHeight : normalHeight.x;
    //     // Еси персонаж сидит его оффсет равен оффсету приседа, если нет то нулю
    //     var offset = isCrouching ? -_playerControllerStats.CrouchOffset : 0;
    //
    //     // Настройка коллайдера
    //     _capsuleCollider.size = new Vector2(_capsuleCollider.size.x, height);
    //     _capsuleCollider.offset = new Vector2(_capsuleCollider.offset.x, offset);
    //     
    //             
    //     // Настройка спрайта
    //     _spriteTransform.localScale = isCrouching ? new Vector2(1f, _playerControllerStats.CrouchHeight) : Vector2.one;
    //     _spriteTransform.localPosition = isCrouching ? new Vector2(_spriteTransform.localPosition.x, offset) : Vector2.zero;
    // }
}

// public class ColliderSpriteController
// {
//     private readonly CapsuleCollider2D _collider;
//     private readonly Transform _spriteTransform;
//
//     public ColliderSpriteController(CapsuleCollider2D collider, Transform spriteTransform)
//     {
//         _collider = collider;
//         _spriteTransform = spriteTransform;
//     }
//     
//     public void SetColliderSize(float width, float height, float offsetY)
//     {
//         _collider.size = new Vector2(width, height);
//         _collider.offset = new Vector2(_collider.offset.x, offsetY);
//     }
//
//     public void SetSpriteScale(float scaleX, float scaleY)
//     {
//         _spriteTransform.localScale = new Vector2(scaleX, scaleY);
//     }
//
//     public void SetSpritePosition(float posX, float posY)
//     {
//         _spriteTransform.localPosition = new Vector2(posX, posY);
//     }
// }
