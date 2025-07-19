public class PlayerModules
{
    public readonly MovementModule MovementModule;
    public readonly JumpModule JumpModule;
    public readonly FallModule FallModule;
    public readonly DashModule DashModule;
    public readonly WallSlideModule WallSlideModule;
    public readonly WallJumpModule WallJumpModule;
    public readonly CrouchModule CrouchModule;
    public readonly CrouchRollModule CrouchRollModule;

    public PlayerModules(
        MovementModule movementModule,
        JumpModule jumpModule,
        FallModule fallModule,
        DashModule dashModule,
        WallSlideModule wallSlideModule,
        WallJumpModule wallJumpModule,
        CrouchModule crouchModule,
        CrouchRollModule crouchRollModule)
    {
        MovementModule = movementModule;
        JumpModule = jumpModule;
        FallModule = fallModule;
        DashModule = dashModule;
        WallSlideModule = wallSlideModule;
        WallJumpModule = wallJumpModule;
        CrouchModule = crouchModule;
        CrouchRollModule = crouchRollModule;
    }
}