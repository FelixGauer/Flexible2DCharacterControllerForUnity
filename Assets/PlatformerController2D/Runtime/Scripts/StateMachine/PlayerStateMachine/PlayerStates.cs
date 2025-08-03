namespace PlatformerController2D.Runtime.Scripts.StateMachine.PlayerStateMachine
{
    public class PlayerStates
    {
        public IState IdleState { get; set; }
        public IState LocomotionState { get; set; }
        public IState RunState { get; set; }
        public IState IdleCrouchState { get; set; }
        public IState CrouchState { get; set; }
        public IState JumpState { get; set; }
        public IState FallState { get; set; }
        public IState DashState { get; set; }
        public IState CrouchRollState { get; set; }
        public IState WallSlideState { get; set; }
        public IState WallJumpState { get; set; }
        public IState RunJumpState { get; set; }
        public IState RunFallState { get; set; }
        public IState JumpWallFallState { get; set; }
        public IState DashFallState { get; set; }


    }
}