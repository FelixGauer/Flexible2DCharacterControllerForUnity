using UnityEngine;

public class CrouchModule
{
    private readonly PhysicsContext _physicsContext;
    private readonly PlayerControllerStats _playerControllerStats;
    private readonly CapsuleCollider2D _capsuleCollider;
    private readonly Transform _spriteTransform;

    private Vector2 normalHeight => _capsuleCollider.size;

    public CrouchModule(PhysicsContext physicsContext, PlayerControllerStats playerControllerStats, CapsuleCollider2D capsuleCollider, Transform spriteTransform) 
    {
        _physicsContext = physicsContext;
        _playerControllerStats = playerControllerStats;
        _capsuleCollider = capsuleCollider;
        _spriteTransform = spriteTransform;
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

    // Метод который регулирует высоту спрайта и коллайдера в зависимости сидит ли персонаж или стоит
    public void SetCrouchState(bool isCrouching)
    {
        // Еси персонаж сидит его высота равна высоте приседа, если нет обычной высоте
        var height = isCrouching ? _playerControllerStats.CrouchHeight : normalHeight.x;
        // Еси персонаж сидит его оффсет равен оффсету приседа, если нет то нулю
        var offset = isCrouching ? -_playerControllerStats.CrouchOffset : 0;

        // Настройка коллайдера
        _capsuleCollider.size = new Vector2(_capsuleCollider.size.x, height);
        _capsuleCollider.offset = new Vector2(_capsuleCollider.offset.x, offset);

        // Настройка спрайта
        _spriteTransform.localScale = isCrouching ? new Vector2(1f, _playerControllerStats.CrouchHeight) : Vector2.one;
        _spriteTransform.localPosition = isCrouching ? new Vector2(_spriteTransform.localPosition.x, offset) : Vector2.zero;
    }
}