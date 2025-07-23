using UnityEngine;

public class MovementLogic
{
    private readonly PlayerControllerStats _stats;
    
    public MovementLogic(PlayerControllerStats stats)
    {
        _stats = stats;
    }
    
    public bool ShouldRun(InputButtonState runState)
    {
        if (_stats.movementSettings == null) return runState.IsHeld;
        
        return _stats.movementSettings.defaultMovementMode switch
        {
            MovementMode.Walk => runState.IsHeld,
            MovementMode.Run => !runState.IsHeld,
            _ => runState.IsHeld
        };
    }
    
    public bool ShouldWalk(InputButtonState runState) => !ShouldRun(runState);
}



[System.Serializable]
public class MovementSettings
{
    [Header("Movement Mode")]
    [Tooltip("Walk: ходьба по умолчанию, зажать клавишу для бега\nRun: бег по умолчанию, зажать клавишу для ходьбы")]
    public MovementMode defaultMovementMode = MovementMode.Run;
}

public enum MovementMode
{
    [Tooltip("Ходьба по умолчанию, зажать клавишу для бега")]
    Walk,
    
    [Tooltip("Бег по умолчанию, зажать клавишу для ходьбы")]
    Run
}