using PlatformerController2D.Runtime.Scripts.Input;
using PlatformerController2D.Runtime.Scripts.Player;
using PlatformerController2D.Runtime.Scripts.Systems;

namespace PlatformerController2D.Runtime.Scripts.StateMachine.PlayerStateMachine
{
    public class PlayerStateContext
    {
        public InputReader InputReader { get; }
        public PlayerControllerStats PlayerControllerStats { get; }
        public PhysicsHandler2D PhysicsHandler2D { get; }
        public TurnChecker TurnChecker { get; }
        public AnimationController AnimationController { get; }
        public CollisionsChecker CollisionsChecker { get; }

    
        public PlayerStateContext(
            InputReader inputReader,
            PlayerControllerStats playerControllerStats,
            PhysicsHandler2D physicsHandler2D,
            TurnChecker turnChecker,
            AnimationController animationController,
            CollisionsChecker collisionsChecker)
        {
            InputReader = inputReader;
            PlayerControllerStats = playerControllerStats;
            PhysicsHandler2D = physicsHandler2D;
            TurnChecker = turnChecker;
            AnimationController = animationController;
            CollisionsChecker = collisionsChecker;
        }
    }
}