public class CrouchModule
{
    private readonly PlayerControllerStats _playerControllerStats;
    private readonly CollisionsChecker _collisionsChecker;
    private readonly ColliderSpriteResizer _colliderSpriteResizer;

    public CrouchModule(PlayerControllerStats playerControllerStats, CollisionsChecker collisionsChecker, ColliderSpriteResizer colliderSpriteResizer) 
    {
        _playerControllerStats = playerControllerStats;
        _collisionsChecker = collisionsChecker;
        _colliderSpriteResizer = colliderSpriteResizer;
    }
    
    public void SetCrouchState(bool isCrouching)
    {
        _collisionsChecker.IsSitting = () => isCrouching;
        
        if (isCrouching)
        {
            _colliderSpriteResizer.SetSize(_playerControllerStats.CrouchHeight, -_playerControllerStats.CrouchOffset);
        }
        else
        {
            _colliderSpriteResizer.ResetToNormal();
        }
    }
}
