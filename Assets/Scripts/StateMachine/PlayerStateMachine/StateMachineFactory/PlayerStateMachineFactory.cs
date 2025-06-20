using UnityEngine;

public class PlayerStateMachineFactory : StateMachineFactory<PlayerStates>
{
    private readonly PlayerController _playerController;
    private readonly Animator _animator;
    private readonly InputReader _inputReader;
    private readonly PlayerControllerStats _playerControllerStats;
    private readonly PhysicsHandler2D _physicsHandler2D;
    private readonly TurnChecker _turnChecker;
    private readonly CollisionsChecker _collisionsChecker;
    private readonly PlayerTimerRegistry _playerTimerRegistry;
    private readonly PlayerPhysicsController _playerPhysicsController;

    public PlayerStateMachineFactory(
        PlayerController playerController,
        Animator animator,
        InputReader inputReader,
        PlayerControllerStats playerControllerStats,
        PhysicsHandler2D physicsHandler2D,
        TurnChecker turnChecker,
        CollisionsChecker collisionsChecker,
        PlayerTimerRegistry playerTimerRegistry,
        PlayerPhysicsController playerPhysicsController)
    {
        _playerController = playerController;
        _animator = animator;
        _inputReader = inputReader;
        _playerControllerStats = playerControllerStats;
        _physicsHandler2D = physicsHandler2D;
        _turnChecker = turnChecker;
        _collisionsChecker = collisionsChecker;
        _playerTimerRegistry = playerTimerRegistry;
        _playerPhysicsController = playerPhysicsController;
    }

    protected override PlayerStates CreateStates()
    {
        return new PlayerStates
        {
            IdleState = new IdleState(_playerController, _animator, _inputReader, _playerControllerStats,
                _physicsHandler2D, _turnChecker),
            LocomotionState = new LocomotionState(_playerController, _animator, _inputReader, _playerControllerStats,
                _physicsHandler2D, _turnChecker),
            RunState = new RunState(_playerController, _animator, _inputReader, _playerControllerStats,
                _physicsHandler2D, _turnChecker),
            IdleCrouchState = new IdleCrouchState(_playerController, _animator, _inputReader, _playerControllerStats,
                _physicsHandler2D, _turnChecker),
            CrouchState = new CrouchState(_playerController, _animator, _inputReader, _playerControllerStats,
                _physicsHandler2D, _turnChecker),
            JumpState = new JumpState(_playerController, _animator, _inputReader, _playerControllerStats,
                _physicsHandler2D, _turnChecker),
            FallState = new FallState(_playerController, _animator, _inputReader, _playerControllerStats,
                _physicsHandler2D, _turnChecker),
            DashState = new DashState(_playerController, _animator, _inputReader, _playerControllerStats,
                _physicsHandler2D, _turnChecker),
            CrouchRollState = new CrouchRollState(_playerController, _animator, _inputReader, _playerControllerStats,
                _physicsHandler2D, _turnChecker),
            WallSlideState = new WallSlideState(_playerController, _animator, _inputReader, _playerControllerStats,
                _physicsHandler2D, _turnChecker),
            WallJumpState = new WallJumpState(_playerController, _animator, _inputReader, _playerControllerStats,
                _physicsHandler2D, _turnChecker),
            RunJumpState = new RunJumpState(_playerController, _animator, _inputReader, _playerControllerStats,
                _physicsHandler2D, _turnChecker),
            RunFallState = new RunFallState(_playerController, _animator, _inputReader, _playerControllerStats,
                _physicsHandler2D, _turnChecker)
        };
    }

    protected override IState GetInitialState(PlayerStates states)
    {
        return states.IdleState;
    }

    protected override void SetupTransitions(PlayerStates states)
    {
        SetupIdleTransitions(states);
        SetupLocomotionTransitions(states);
        SetupRunTransitions(states);
        SetupIdleCrouchTransitions(states);
        SetupCrouchTransitions(states);
        SetupJumpTransitions(states);
        SetupFallTransitions(states);
        SetupDashTransitions(states);
        SetupCrouchRollTransitions(states);
        SetupWallSlideTransitions(states);
        SetupWallJumpTransitions(states);
        SetupRunJumpTransitions(states);
        SetupRunFallTransitions(states);
    }

    private void SetupIdleTransitions(PlayerStates states)
    {
        At(states.IdleState, states.DashState,
            new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame));
        At(states.IdleState, states.JumpState,
            new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame));
        At(states.IdleState, states.IdleCrouchState,
            new FuncPredicate(() => _inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection()[0] == 0));
        At(states.IdleState, states.CrouchState,
            new FuncPredicate(() => _inputReader.GetCrouchState().IsHeld));
        At(states.IdleState, states.RunState,
            new FuncPredicate(() => _inputReader.GetRunState().IsHeld && _inputReader.GetMoveDirection() != Vector2.zero));
        At(states.IdleState, states.LocomotionState,
            new FuncPredicate(() => _inputReader.GetMoveDirection() != Vector2.zero));
    }

    private void SetupLocomotionTransitions(PlayerStates states)
    {
        At(states.LocomotionState, states.FallState,
            new FuncPredicate(() => !_collisionsChecker.IsGrounded));
        At(states.LocomotionState, states.JumpState,
            new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame));
        At(states.LocomotionState, states.RunState,
            new FuncPredicate(() => _inputReader.GetRunState().IsHeld));
        At(states.LocomotionState, states.IdleCrouchState,
            new FuncPredicate(() => _inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection()[0] == 0 && Mathf.Abs(_physicsHandler2D.GetVelocity().x) == 0f));
        At(states.LocomotionState, states.CrouchState,
            new FuncPredicate(() => _inputReader.GetCrouchState().IsHeld));
        At(states.LocomotionState, states.DashState,
            new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame));
        At(states.LocomotionState, states.IdleState,
            new FuncPredicate(() => _inputReader.GetMoveDirection() == Vector2.zero && Mathf.Abs(_physicsHandler2D.GetVelocity().x) == 0f));
    }

    private void SetupRunTransitions(PlayerStates states)
    {
        // At(states.RunState, states.FallState,
        // 	new FuncPredicate(() => !_collisionsChecker.IsGrounded));
        At(states.RunState, states.RunFallState,
            new FuncPredicate(() => !_collisionsChecker.IsGrounded));
        At(states.RunState, states.RunJumpState,
            new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame));
        At(states.RunState, states.IdleState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero && Mathf.Abs(_physicsHandler2D.GetVelocity().x) == 0f));
        // At(states.RunState, states.IdleState,
        // 	new FuncPredicate(() => !_inputReader.GetRunState().IsHeld && _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero));
        At(states.RunState, states.LocomotionState,
            new FuncPredicate(() => !_inputReader.GetRunState().IsHeld));
        At(states.RunState, states.DashState,
            new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame));
        At(states.RunState, states.JumpState,
            new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame));
        At(states.RunState, states.CrouchState,
            new FuncPredicate(() => _inputReader.GetCrouchState().IsHeld));
    }

    private void SetupIdleCrouchTransitions(PlayerStates states)
    {
        At(states.IdleCrouchState, states.CrouchState,
            new FuncPredicate(() => (_inputReader.GetCrouchState().IsHeld || _collisionsChecker.BumpedHead) && _inputReader.GetMoveDirection()[0] != 0));
        At(states.IdleCrouchState, states.JumpState,
            new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame && !_collisionsChecker.BumpedHead));
        At(states.IdleCrouchState, states.IdleState,
            new FuncPredicate(() => !_inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection()[0] == 0 && !_collisionsChecker.BumpedHead));
        At(states.IdleCrouchState, states.CrouchRollState,
            new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame));
    }

    private void SetupCrouchTransitions(PlayerStates states)
    {
        At(states.CrouchState, states.FallState,
            new FuncPredicate(() => !_collisionsChecker.IsGrounded));
        At(states.CrouchState, states.IdleCrouchState,
            new FuncPredicate(() => (_inputReader.GetCrouchState().IsHeld || _collisionsChecker.BumpedHead) && _inputReader.GetMoveDirection()[0] == 0 && Mathf.Abs(_physicsHandler2D.GetVelocity().x) == 0f));
        At(states.CrouchState, states.JumpState,
            new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame));
        At(states.CrouchState, states.CrouchRollState,
            new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame));
        At(states.CrouchState, states.RunState,
            new FuncPredicate(() => _inputReader.GetRunState().IsHeld && !_inputReader.GetCrouchState().IsHeld && !_collisionsChecker.BumpedHead));
        At(states.CrouchState, states.IdleState,
            new FuncPredicate(() => _inputReader.GetMoveDirection() == Vector2.zero && !_inputReader.GetCrouchState().IsHeld && !_collisionsChecker.BumpedHead));
        At(states.CrouchState, states.LocomotionState,
            new FuncPredicate(() => !_inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection() != Vector2.zero && !_collisionsChecker.BumpedHead));
    }

    private void SetupJumpTransitions(PlayerStates states)
    {
        At(states.JumpState, states.FallState,
            new FuncPredicate(() => !_collisionsChecker.IsGrounded && (_playerPhysicsController.JumpModule.CanFall() || _collisionsChecker.BumpedHead)));
        At(states.JumpState, states.DashState,
            new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && _playerPhysicsController.DashModule.CanDash())); // FIXME
    }

    private void SetupFallTransitions(PlayerStates states)
    {
        At(states.FallState, states.JumpState, // БУФФЕР
            new FuncPredicate(() => _collisionsChecker.IsGrounded && _playerTimerRegistry.jumpBufferTimer.IsRunning));
        At(states.FallState, states.JumpState, // КОЙОТ + МУЛЬТИ
            new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame && (_playerTimerRegistry.jumpCoyoteTimer.IsRunning || _playerPhysicsController.JumpModule.CanMultiJump()))); // FIXME
        At(states.FallState, states.DashState,
            new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && _playerPhysicsController.DashModule.CanDash())); // FIXME
        At(states.FallState, states.IdleState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero && Mathf.Abs(_physicsHandler2D.GetVelocity().x) < 0.1f));
        At(states.FallState, states.IdleCrouchState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection()[0] == 0));
        At(states.FallState, states.CrouchState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetCrouchState().IsHeld));
        At(states.FallState, states.RunState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetRunState().IsHeld));
        At(states.FallState, states.LocomotionState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded));
        At(states.FallState, states.WallSlideState,
            new FuncPredicate(() => _collisionsChecker.IsTouchingWall));
    }

    private void SetupDashTransitions(PlayerStates states)
    {
        At(states.DashState, states.IdleState,
            new FuncPredicate(() => !_playerTimerRegistry.dashTimer.IsRunning && _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero));
        At(states.DashState, states.RunState,
            new FuncPredicate(() => !_playerTimerRegistry.dashTimer.IsRunning && _collisionsChecker.IsGrounded && _inputReader.GetRunState().IsHeld));
        At(states.DashState, states.FallState,
            new FuncPredicate(() => !_playerTimerRegistry.dashTimer.IsRunning && !_collisionsChecker.IsGrounded));
        At(states.DashState, states.WallSlideState,
            new FuncPredicate(() => _collisionsChecker.IsTouchingWall));
        At(states.DashState, states.JumpState,
            new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame && _playerPhysicsController.JumpModule.CanMultiJump()));
        At(states.DashState, states.IdleCrouchState,
            new FuncPredicate(() => !_playerTimerRegistry.dashTimer.IsRunning && _collisionsChecker.IsGrounded && _inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection()[0] == 0));
        At(states.DashState, states.CrouchState,
            new FuncPredicate(() => !_playerTimerRegistry.dashTimer.IsRunning && _collisionsChecker.IsGrounded && _inputReader.GetCrouchState().IsHeld));
        At(states.DashState, states.LocomotionState,
            new FuncPredicate(() => !_playerTimerRegistry.dashTimer.IsRunning && _collisionsChecker.IsGrounded));
    }

    private void SetupCrouchRollTransitions(PlayerStates states)
    {
        // At(states.CrouchRollState, states.IdleState,
        // 	new FuncPredicate(() => !_playerTimerRegistry.crouchRollTimer.IsRunning && !_inputReader.GetCrouchState().IsHeld && !_collisionsChecker.BumpedHead));
        At(states.CrouchRollState, states.FallState,
            new FuncPredicate(() => !_collisionsChecker.IsGrounded));
        At(states.CrouchRollState, states.IdleCrouchState,
            new FuncPredicate(() => !_playerTimerRegistry.crouchRollTimer.IsRunning && (_inputReader.GetCrouchState().IsHeld || _collisionsChecker.BumpedHead) && _inputReader.GetMoveDirection()[0] == 0));
        At(states.CrouchRollState, states.CrouchState,
            new FuncPredicate(() => !_playerTimerRegistry.crouchRollTimer.IsRunning));
    }

    private void SetupWallSlideTransitions(PlayerStates states)
    {
        At(states.WallSlideState, states.FallState,
            new FuncPredicate(() => !_collisionsChecker.IsGrounded && (_playerTimerRegistry.wallJumpTimer.IsFinished || !_collisionsChecker.IsTouchingWall)));
        At(states.WallSlideState, states.WallJumpState,
            new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame));
        At(states.WallSlideState, states.IdleState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero));
        At(states.WallSlideState, states.LocomotionState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded));
        At(states.WallSlideState, states.DashState,
            new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && _playerPhysicsController.WallSlideModule.CalculateWallDirectionX() != _inputReader.GetMoveDirection().x)); //FIXME
        At(states.WallSlideState, states.IdleCrouchState,
            new FuncPredicate(() => _inputReader.GetCrouchState().IsHeld && _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection()[0] == 0));
    }

    private void SetupWallJumpTransitions(PlayerStates states)
    {
        At(states.WallJumpState, states.FallState,
            new FuncPredicate(() => !_collisionsChecker.IsGrounded && !_collisionsChecker.IsTouchingWall));
    }

    private void SetupRunJumpTransitions(PlayerStates states)
    {
        At(states.RunJumpState, states.RunFallState,
            new FuncPredicate(() => !_collisionsChecker.IsGrounded && _playerPhysicsController.JumpModule.CanFall() || _collisionsChecker.BumpedHead));
        At(states.RunJumpState, states.DashState,
            new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && _playerPhysicsController.DashModule.CanDash()));
    }

    private void SetupRunFallTransitions(PlayerStates states)
    {
        At(states.RunFallState, states.JumpState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded && _playerTimerRegistry.jumpBufferTimer.IsRunning && !_inputReader.GetRunState().IsHeld));
        At(states.RunFallState, states.RunJumpState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded && _playerTimerRegistry.jumpBufferTimer.IsRunning));
        At(states.RunFallState, states.RunJumpState,
            new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame && (_playerTimerRegistry.jumpCoyoteTimer.IsRunning || _playerPhysicsController.JumpModule.CanMultiJump())));
        At(states.RunFallState, states.FallState,
            new FuncPredicate(() => !_inputReader.GetRunState().IsHeld && !_collisionsChecker.IsGrounded));
        At(states.RunFallState, states.DashState,
            new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && _playerPhysicsController.DashModule.CanDash())); // FIXME: Возможно, это должно быть dash
        At(states.RunFallState, states.IdleState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero && Mathf.Abs(_physicsHandler2D.GetVelocity().x) < 0.1f));
        At(states.RunFallState, states.WallSlideState,
            new FuncPredicate(() => _collisionsChecker.IsTouchingWall));
        At(states.RunFallState, states.RunState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetRunState().IsHeld));
        At(states.RunFallState, states.LocomotionState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded));
    }
	
    // private void SetupFallTransitions(PlayerStates states)
    // {
    // 	At(states.FallState, states.JumpState, // КОЙОТ + МУЛЬТИ
    // 		new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame && (_playerTimerRegistry.jumpCoyoteTimer.IsRunning || _playerPhysicsController.CanMultiJump()))); // FIXME
    // 	At(states.FallState, states.JumpState, // БУФФЕР
    // 		new FuncPredicate(() => _collisionsChecker.IsGrounded && _playerTimerRegistry.jumpBufferTimer.IsRunning));
    // 	At(states.FallState, states.DashState,
    // 		new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && _playerPhysicsController.DashModule.CanDash())); // FIXME
    // 	At(states.FallState, states.IdleState,
    // 		new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero && Mathf.Abs(_physicsHandler2D.GetVelocity().x) < 0.1f));
    // 	At(states.FallState, states.IdleCrouchState,
    // 		new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection()[0] == 0));
    // 	At(states.FallState, states.CrouchState,
    // 		new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetCrouchState().IsHeld));
    // 	At(states.FallState, states.RunState,
    // 		new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetRunState().IsHeld));
    // 	At(states.FallState, states.LocomotionState,
    // 		new FuncPredicate(() => _collisionsChecker.IsGrounded));
    // 	At(states.FallState, states.WallSlideState,
    // 		new FuncPredicate(() => _collisionsChecker.IsTouchingWall));
    // }
}