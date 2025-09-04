using PlatformerController2D.Runtime.Scripts.PhysicalModules.Modules;
using PlatformerController2D.Runtime.Scripts.Player;
using PlatformerController2D.Runtime.Scripts.Systems;

namespace PlatformerController2D.Runtime.Scripts.PhysicalModules
{
    public class PlayerModulesFactory
    {
        public PlayerModules CreateModules(
            PlayerControllerStats playerControllerStats,
            CollisionsChecker collisionsChecker,
            PlayerTimerRegistry playerTimerRegistry,
            TurnChecker turnChecker,
            ColliderSpriteResizer colliderSpriteResizer)
        {
            return new PlayerModules(
                new MovementModule(),
                new JumpModule(collisionsChecker, playerControllerStats, playerTimerRegistry.jumpBufferTimer, playerTimerRegistry.jumpCoyoteTimer),
                new FallModule(playerControllerStats, playerTimerRegistry.jumpBufferTimer),
                new DashModule(playerControllerStats, turnChecker, playerTimerRegistry.dashTimer, collisionsChecker),
                new WallSlideModule(playerControllerStats, turnChecker, playerTimerRegistry.wallFallTimer),
                new WallJumpModule(playerControllerStats, playerTimerRegistry.wallJumpTimer),
                new CrouchModule(playerControllerStats, collisionsChecker, colliderSpriteResizer),
                new CrouchRollModule(playerControllerStats, turnChecker, playerTimerRegistry.crouchRollTimer)
            );
        }
    }
}