public class PlayerStateContext
{
    public InputReader InputReader { get; }
    public PlayerControllerStats PlayerControllerStats { get; }
    public PhysicsHandler2D PhysicsHandler2D { get; }
    public TurnChecker TurnChecker { get; }
    public AnimationController AnimationController { get; }
    
    public PlayerStateContext(
        InputReader inputReader,
        PlayerControllerStats playerControllerStats,
        PhysicsHandler2D physicsHandler2D,
        TurnChecker turnChecker,
        AnimationController animationController)
    {
        InputReader = inputReader;
        PlayerControllerStats = playerControllerStats;
        PhysicsHandler2D = physicsHandler2D;
        TurnChecker = turnChecker;
        AnimationController = animationController;
    }
}