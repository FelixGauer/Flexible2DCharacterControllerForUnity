using UnityEngine;
using UnityEngine.Experimental.Playables;

public class PlayerStateMachineFactory : StateMachineFactory<PlayerStates>
{
    private readonly InputReader _inputReader;
    private readonly PlayerControllerStats _playerControllerStats;
    private readonly PhysicsHandler2D _physicsHandler2D;
    private readonly TurnChecker _turnChecker;
    private readonly CollisionsChecker _collisionsChecker;
    private readonly PlayerTimerRegistry _playerTimerRegistry;
    private readonly AnimationController _animationController;
    private readonly MovementLogic _movementLogic;
    private readonly PlayerModules _playerModules;
    
    public PlayerStateMachineFactory(
        InputReader inputReader,
        PlayerControllerStats playerControllerStats,
        PhysicsHandler2D physicsHandler2D,
        TurnChecker turnChecker,
        CollisionsChecker collisionsChecker,
        PlayerTimerRegistry playerTimerRegistry,
        AnimationController animationController,
        MovementLogic movementLogic,
        PlayerModules playerModules)
    {
        _inputReader = inputReader;
        _playerControllerStats = playerControllerStats;
        _physicsHandler2D = physicsHandler2D;
        _turnChecker = turnChecker;
        _collisionsChecker = collisionsChecker;
        _playerTimerRegistry = playerTimerRegistry;
        _animationController = animationController;
        _movementLogic = movementLogic;
        _playerModules = playerModules;
    }

    protected override PlayerStates CreateStates()
    {
        var context = new PlayerStateContext(
            _inputReader, 
            _playerControllerStats,
            _physicsHandler2D, 
            _turnChecker, 
            _animationController);
        
        return new PlayerStates
        {
            IdleState = new IdleState(context),
            LocomotionState = new LocomotionState(context, _playerModules.MovementModule),
            RunState = new RunState(context, _playerModules.MovementModule),
            IdleCrouchState = new IdleCrouchState(context, _playerModules.CrouchModule),
            CrouchState = new CrouchState(context, _playerModules.CrouchModule, _playerModules.MovementModule),
            JumpState = new JumpState(context, _playerModules.JumpModule, _playerModules.FallModule, _playerModules.MovementModule),
            FallState = new FallState(context, _playerModules.FallModule, _playerModules.MovementModule),
            DashState = new DashState(context, _playerModules.DashModule, _playerModules.FallModule),
            CrouchRollState = new CrouchRollState(context, _playerModules.CrouchRollModule, _playerModules.CrouchModule),
            WallSlideState = new WallSlideState(context, _playerModules.WallSlideModule),
            WallJumpState = new WallJumpState(context, _playerModules.WallJumpModule, _playerModules.WallSlideModule),
            RunJumpState = new RunJumpState(context, _playerModules.JumpModule, _playerModules.FallModule, _playerModules.MovementModule),
            RunFallState = new RunFallState(context, _playerModules.FallModule, _playerModules.MovementModule),
            JumpWallFallState = new JumpWallFallState(context, _playerModules.FallModule, _playerModules.MovementModule),
            DashFallState = new DashFallState(context, _playerModules.FallModule, _playerModules.MovementModule)
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
        SetupJumpWallFallTransitions(states);
        SetupDashFallTransitions(states);
    }

    private void SetupIdleTransitions(PlayerStates states)
    {
        At(states.IdleState, states.FallState,
            new FuncPredicate(() => !_collisionsChecker.IsGrounded));
        At(states.IdleState, states.RunJumpState,
            new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame && _movementLogic.ShouldRun(_inputReader.GetRunState())));
        At(states.IdleState, states.JumpState,
            new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame));
        At(states.IdleState, states.DashState,
            new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame));
        At(states.IdleState, states.IdleCrouchState,
            new FuncPredicate(() => _inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection()[0] == 0));
        At(states.IdleState, states.CrouchState,
            new FuncPredicate(() => _inputReader.GetCrouchState().IsHeld));
        At(states.IdleState, states.RunState,
            new FuncPredicate(() => _movementLogic.ShouldRun(_inputReader.GetRunState()) && _inputReader.GetMoveDirection().x != 0f && !_collisionsChecker.IsTouchingWall));
        At(states.IdleState, states.LocomotionState,
            new FuncPredicate(() => _inputReader.GetMoveDirection().x != 0f && !_collisionsChecker.IsTouchingWall && _movementLogic.ShouldWalk(_inputReader.GetRunState())));
    }

    private void SetupLocomotionTransitions(PlayerStates states)
    {
        At(states.LocomotionState, states.FallState,
            new FuncPredicate(() => !_collisionsChecker.IsGrounded));
        At(states.LocomotionState, states.JumpState,
            new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame));
        At(states.LocomotionState, states.RunState,
            new FuncPredicate(() => _movementLogic.ShouldRun(_inputReader.GetRunState())));
        At(states.LocomotionState, states.IdleCrouchState,
            new FuncPredicate(() => _inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection().x == 0 && Mathf.Abs(_physicsHandler2D.GetVelocity().x) == 0f));
        At(states.LocomotionState, states.CrouchState,
            new FuncPredicate(() => _inputReader.GetCrouchState().IsHeld));
        At(states.LocomotionState, states.DashState,
            new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame));
        At(states.LocomotionState, states.IdleState,
            new FuncPredicate(() => (_inputReader.GetMoveDirection().x == 0f && _physicsHandler2D.GetVelocity() == Vector2.zero) || _collisionsChecker.IsTouchingWall));
    }

    private void SetupRunTransitions(PlayerStates states)
    {
        // At(states.RunState, states.FallState,
        // 	new FuncPredicate(() => !_collisionsChecker.IsGrounded));
        At(states.RunState, states.RunJumpState,
            new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame));
        At(states.RunState, states.RunFallState,
            new FuncPredicate(() => !_collisionsChecker.IsGrounded)); // !_inputReader.GetJumpState().WasPressedThisFrame
        At(states.RunState, states.IdleState,
            new FuncPredicate(() => (_collisionsChecker.IsGrounded && _inputReader.GetMoveDirection().x == 0f && _physicsHandler2D.GetVelocity() == Vector2.zero) || _collisionsChecker.IsTouchingWall));
        // At(states.RunState, states.IdleState,
        // 	new FuncPredicate(() => !_inputReader.GetRunState().IsHeld && _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero));
        At(states.RunState, states.LocomotionState,
            new FuncPredicate(() => _movementLogic.ShouldWalk(_inputReader.GetRunState())));
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
            new FuncPredicate(() => (_inputReader.GetCrouchState().IsHeld || _collisionsChecker.BumpedHead) && _inputReader.GetMoveDirection().x != 0 && !_collisionsChecker.IsTouchingWall));
        At(states.IdleCrouchState, states.JumpState,
            new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame && !_collisionsChecker.BumpedHead && _movementLogic.ShouldWalk(_inputReader.GetRunState())));
        At(states.IdleCrouchState, states.RunJumpState,
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
        At(states.CrouchState, states.JumpState,
            new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame && !_collisionsChecker.BumpedHead && _movementLogic.ShouldWalk(_inputReader.GetRunState())));
        At(states.CrouchState, states.RunJumpState,
            new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame && !_collisionsChecker.BumpedHead));
        At(states.CrouchState, states.IdleCrouchState,
            new FuncPredicate(() => ((_inputReader.GetCrouchState().IsHeld || _collisionsChecker.BumpedHead) && _inputReader.GetMoveDirection()[0] == 0 && Mathf.Abs(_physicsHandler2D.GetVelocity().x) == 0f) || _collisionsChecker.IsTouchingWall));
        At(states.CrouchState, states.CrouchRollState,
            new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame));
        At(states.CrouchState, states.IdleState,
            new FuncPredicate(() => _inputReader.GetMoveDirection() == Vector2.zero && !_inputReader.GetCrouchState().IsHeld && !_collisionsChecker.BumpedHead));
        At(states.CrouchState, states.RunState,
            new FuncPredicate(() => _movementLogic.ShouldRun(_inputReader.GetRunState()) && !_inputReader.GetCrouchState().IsHeld && !_collisionsChecker.BumpedHead));
        At(states.CrouchState, states.LocomotionState,
            new FuncPredicate(() => !_inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection() != Vector2.zero && !_collisionsChecker.BumpedHead));
    }

    private void SetupJumpTransitions(PlayerStates states)
    {
        At(states.JumpState, states.FallState,
            new FuncPredicate(() => !_collisionsChecker.IsGrounded && (_playerModules.JumpModule.CanFall(_physicsHandler2D.GetVelocity()) || _collisionsChecker.BumpedHead)));
        At(states.JumpState, states.DashState,
            new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && _playerModules.DashModule.CanDash())); // FIXME
    }

    private void SetupFallTransitions(PlayerStates states)
    {
        At(states.FallState, states.RunJumpState, // БУФФЕР
            new FuncPredicate(() => _collisionsChecker.IsGrounded && _playerTimerRegistry.jumpBufferTimer.IsRunning && _movementLogic.ShouldRun(_inputReader.GetRunState())));
        At(states.FallState, states.RunJumpState, // КОЙОТ + МУЛЬТИ
            new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame && (_playerTimerRegistry.jumpCoyoteTimer.IsRunning || _playerModules.JumpModule.CanMultiJump()) &&_movementLogic.ShouldRun(_inputReader.GetRunState()))); // FIXME

        At(states.FallState, states.JumpState, // БУФФЕР
            new FuncPredicate(() => _collisionsChecker.IsGrounded && _playerTimerRegistry.jumpBufferTimer.IsRunning));
        At(states.FallState, states.JumpState, // КОЙОТ + МУЛЬТИ
            new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame && (_playerTimerRegistry.jumpCoyoteTimer.IsRunning || _playerModules.JumpModule.CanMultiJump()))); // FIXME
        At(states.FallState, states.DashState,
            new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && _playerModules.DashModule.CanDash())); // FIXME
        At(states.FallState, states.IdleState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero && Mathf.Abs(_physicsHandler2D.GetVelocity().x) < 0.1f));
        At(states.FallState, states.IdleCrouchState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection()[0] == 0));
        At(states.FallState, states.CrouchState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetCrouchState().IsHeld));
        At(states.FallState, states.RunState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded && _movementLogic.ShouldRun(_inputReader.GetRunState())));
        At(states.FallState, states.LocomotionState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded));
        At(states.FallState, states.WallSlideState,
            new FuncPredicate(() => _collisionsChecker.IsTouchingWall));
    }

    private void SetupDashTransitions(PlayerStates states)
    {
        At(states.DashState, states.DashFallState,
            new FuncPredicate(() => !_playerTimerRegistry.dashTimer.IsRunning && !_collisionsChecker.IsGrounded));
        // At(states.DashState, states.FallState,
        //     new FuncPredicate(() => !_playerTimerRegistry.dashTimer.IsRunning && !_collisionsChecker.IsGrounded));
        At(states.DashState, states.JumpState,
            new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame && _playerModules.JumpModule.CanMultiJump() && _movementLogic.ShouldWalk(_inputReader.GetRunState())));
        At(states.DashState, states.RunJumpState,
            new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame && _playerModules.JumpModule.CanMultiJump()));
        At(states.DashState, states.IdleState,
            new FuncPredicate(() => !_playerTimerRegistry.dashTimer.IsRunning && _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero));
        At(states.DashState, states.RunState,
            new FuncPredicate(() => !_playerTimerRegistry.dashTimer.IsRunning && _collisionsChecker.IsGrounded && _movementLogic.ShouldRun(_inputReader.GetRunState())));
        At(states.DashState, states.WallSlideState,
            new FuncPredicate(() => _collisionsChecker.IsTouchingWall));
        At(states.DashState, states.IdleCrouchState,
            new FuncPredicate(() => !_playerTimerRegistry.dashTimer.IsRunning && _collisionsChecker.IsGrounded && _inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection()[0] == 0));
        At(states.DashState, states.CrouchState,
            new FuncPredicate(() => !_playerTimerRegistry.dashTimer.IsRunning && _collisionsChecker.IsGrounded && _inputReader.GetCrouchState().IsHeld));
        At(states.DashState, states.LocomotionState,
            new FuncPredicate(() => !_playerTimerRegistry.dashTimer.IsRunning && _collisionsChecker.IsGrounded));
    }

    private void SetupCrouchRollTransitions(PlayerStates states)
    {
        At(states.CrouchRollState, states.FallState,
            new FuncPredicate(() => !_collisionsChecker.IsGrounded));
        At(states.CrouchRollState, states.IdleCrouchState,
            new FuncPredicate(() => !_playerTimerRegistry.crouchRollTimer.IsRunning && (_inputReader.GetCrouchState().IsHeld || _collisionsChecker.BumpedHead) && _inputReader.GetMoveDirection()[0] == 0));
        At(states.CrouchRollState, states.CrouchState,
            new FuncPredicate(() => !_playerTimerRegistry.crouchRollTimer.IsRunning));
    }

    private void SetupWallSlideTransitions(PlayerStates states)
    {
        At(states.WallSlideState, states.RunFallState,
            new FuncPredicate(() => !_collisionsChecker.IsGrounded && (_playerTimerRegistry.wallFallTimer.IsFinished || !_collisionsChecker.IsTouchingWall) && _movementLogic.ShouldRun(_inputReader.GetRunState())));
        // At(states.WallSlideState, states.FallState,
        //     new FuncPredicate(() => !_collisionsChecker.IsGrounded && (_playerTimerRegistry.wallFallTimer.IsFinished || !_collisionsChecker.IsTouchingWall)));
        At(states.WallSlideState, states.WallJumpState,
            new FuncPredicate(() => _inputReader.GetJumpState().WasPressedThisFrame));
        At(states.WallSlideState, states.IdleState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero));
        At(states.WallSlideState, states.LocomotionState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded));
        At(states.WallSlideState, states.DashState,
            new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && _inputReader.GetMoveDirection() != Vector2.zero && _playerModules.WallSlideModule.CurrentWallDirection != _inputReader.GetRawHorizontalDirection() && _inputReader.GetMoveDirection().y != 1f)); //FIXME
        At(states.WallSlideState, states.IdleCrouchState,
            new FuncPredicate(() => _inputReader.GetCrouchState().IsHeld && _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection()[0] == 0));
    }

    private void SetupWallJumpTransitions(PlayerStates states)
    {
        // At(states.WallJumpState, states.RunFallState,
        //     new FuncPredicate(() => !_collisionsChecker.IsGrounded && !_collisionsChecker.IsTouchingWall && _inputReader.GetRunState().IsHeld));
        At(states.WallJumpState, states.JumpWallFallState,
            new FuncPredicate(() => !_collisionsChecker.IsGrounded && !_collisionsChecker.IsTouchingWall));
    }
    
    private void SetupJumpWallFallTransitions(PlayerStates states)
    {
        At(states.JumpWallFallState, states.WallJumpState, // БУФФЕР
            new FuncPredicate(() => _collisionsChecker.IsTouchingWall && _playerTimerRegistry.jumpBufferTimer.IsRunning && _movementLogic.ShouldRun(_inputReader.GetRunState())));
        
        At(states.JumpWallFallState, states.RunJumpState, // БУФФЕР _inputReader.GetMoveDirection().x != _playerModules.WallSlideModule.CurrentWallDirection
            new FuncPredicate(() =>  _collisionsChecker.IsGrounded && _playerTimerRegistry.jumpBufferTimer.IsRunning && _movementLogic.ShouldRun(_inputReader.GetRunState())));
        At(states.JumpWallFallState, states.RunJumpState, // КОЙОТ + МУЛЬТИ
            new FuncPredicate(() => !_collisionsChecker.IsInWallZone && _inputReader.GetJumpState().WasPressedThisFrame && (_playerTimerRegistry.jumpCoyoteTimer.IsRunning || _playerModules.JumpModule.CanMultiJump()) && _movementLogic.ShouldRun(_inputReader.GetRunState()))); // FIXME
        At(states.JumpWallFallState, states.JumpState, // БУФФЕР
            new FuncPredicate(() => _collisionsChecker.IsGrounded && _playerTimerRegistry.jumpBufferTimer.IsRunning));
        At(states.JumpWallFallState, states.JumpState, // КОЙОТ + МУЛЬТИ
            new FuncPredicate(() => !_collisionsChecker.IsInWallZone && _inputReader.GetJumpState().WasPressedThisFrame && (_playerTimerRegistry.jumpCoyoteTimer.IsRunning || _playerModules.JumpModule.CanMultiJump()))); // FIXME
        
        At(states.JumpWallFallState, states.DashState,
            new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && _playerModules.DashModule.CanDash())); // FIXME
        At(states.JumpWallFallState, states.IdleState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero && Mathf.Abs(_physicsHandler2D.GetVelocity().x) < 0.1f));
        At(states.JumpWallFallState, states.IdleCrouchState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection()[0] == 0));
        At(states.JumpWallFallState, states.CrouchState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetCrouchState().IsHeld));
        At(states.JumpWallFallState, states.RunState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded && _movementLogic.ShouldRun(_inputReader.GetRunState())));
        At(states.JumpWallFallState, states.LocomotionState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded));
        At(states.JumpWallFallState, states.WallSlideState,
            new FuncPredicate(() => _collisionsChecker.IsTouchingWall));
    }
    
    private void SetupDashFallTransitions(PlayerStates states) // TODO
    {
        // At(states.DashFallState, states.WallJumpState, // БУФФЕР
        //     new FuncPredicate(() => _collisionsChecker.IsTouchingWall && _playerTimerRegistry.jumpBufferTimer.IsRunning && _movementLogic.ShouldRun(_inputReader.GetRunState())));
        
        At(states.DashFallState, states.RunJumpState, // БУФФЕР _inputReader.GetMoveDirection().x != _playerModules.WallSlideModule.CurrentWallDirection
            new FuncPredicate(() =>  _collisionsChecker.IsGrounded && _playerTimerRegistry.jumpBufferTimer.IsRunning && _movementLogic.ShouldRun(_inputReader.GetRunState())));
        At(states.DashFallState, states.RunJumpState, // КОЙОТ + МУЛЬТИ
            new FuncPredicate(() => !_collisionsChecker.IsInWallZone && _inputReader.GetJumpState().WasPressedThisFrame && (_playerTimerRegistry.jumpCoyoteTimer.IsRunning || _playerModules.JumpModule.CanMultiJump()) && _movementLogic.ShouldRun(_inputReader.GetRunState()))); // FIXME
        At(states.DashFallState, states.JumpState, // БУФФЕР
            new FuncPredicate(() => _collisionsChecker.IsGrounded && _playerTimerRegistry.jumpBufferTimer.IsRunning));
        At(states.DashFallState, states.JumpState, // КОЙОТ + МУЛЬТИ
            new FuncPredicate(() => !_collisionsChecker.IsInWallZone && _inputReader.GetJumpState().WasPressedThisFrame && (_playerTimerRegistry.jumpCoyoteTimer.IsRunning || _playerModules.JumpModule.CanMultiJump()))); // FIXME
        
        At(states.DashFallState, states.DashState,
            new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && _playerModules.DashModule.CanDash())); // FIXME
        At(states.DashFallState, states.IdleState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero && Mathf.Abs(_physicsHandler2D.GetVelocity().x) < 0.1f));
        At(states.DashFallState, states.IdleCrouchState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetCrouchState().IsHeld && _inputReader.GetMoveDirection()[0] == 0));
        At(states.DashFallState, states.CrouchState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetCrouchState().IsHeld));
        At(states.DashFallState, states.RunState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded && _movementLogic.ShouldRun(_inputReader.GetRunState())));
        At(states.DashFallState, states.LocomotionState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded));
        At(states.DashFallState, states.WallSlideState,
            new FuncPredicate(() => _collisionsChecker.IsTouchingWall));
    }
    


    private void SetupRunJumpTransitions(PlayerStates states)
    {
        At(states.RunJumpState, states.RunFallState,
            new FuncPredicate(() => !_collisionsChecker.IsGrounded && (_playerModules.JumpModule.CanFall(_physicsHandler2D.GetVelocity()) || _collisionsChecker.BumpedHead) && !_inputReader.GetJumpState().WasPressedThisFrame));
        // new FuncPredicate(() => !_collisionsChecker.IsGrounded && (_playerModules.JumpModule.CanFall() || _collisionsChecker.BumpedHead) && !_inputReader.GetJumpState().WasPressedThisFrame));
        // new FuncPredicate(() => !_collisionsChecker.IsGrounded && (_physicsHandler2D.GetVelocity().y < 0f || _collisionsChecker.BumpedHead) && !_inputReader.GetJumpState().WasPressedThisFrame));

        At(states.RunJumpState, states.DashState,
            new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && _playerModules.DashModule.CanDash()));
    }

    private void SetupRunFallTransitions(PlayerStates states)
    {
        // At(states.RunFallState, states.WallJumpState,
        //     new FuncPredicate(() => _collisionsChecker.IsTouchingWall && _playerTimerRegistry.jumpBufferTimer.IsRunning));
        
        At(states.RunFallState, states.RunJumpState,
            new FuncPredicate(() => (_inputReader.GetJumpState().WasPressedThisFrame) && (_playerTimerRegistry.jumpCoyoteTimer.IsRunning || _playerModules.JumpModule.CanMultiJump())));
        // new FuncPredicate(() => (_inputReader.GetJumpState().WasPressedThisFrame || _inputReader.GetJumpState().WasReleasedThisFrame) && (_playerTimerRegistry.jumpCoyoteTimer.IsRunning || _playerModules.JumpModule.CanMultiJump())));
        At(states.RunFallState, states.RunJumpState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded && _playerTimerRegistry.jumpBufferTimer.IsRunning));
        At(states.RunFallState, states.JumpState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded && _playerTimerRegistry.jumpBufferTimer.IsRunning && _movementLogic.ShouldWalk(_inputReader.GetRunState())));
        // At(states.RunFallState, states.FallState,
        //     new FuncPredicate(() => !_inputReader.GetRunState().IsHeld && !_collisionsChecker.IsGrounded));
        At(states.RunFallState, states.DashState,
            new FuncPredicate(() => _inputReader.GetDashState().WasPressedThisFrame && _playerModules.DashModule.CanDash())); // FIXME: Возможно, это должно быть dash
        At(states.RunFallState, states.IdleState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded && _inputReader.GetMoveDirection() == Vector2.zero && Mathf.Abs(_physicsHandler2D.GetVelocity().x) < 0.1f));
        At(states.RunFallState, states.WallSlideState,
            new FuncPredicate(() => _collisionsChecker.IsTouchingWall));
        At(states.RunFallState, states.RunState,
            new FuncPredicate(() => _collisionsChecker.IsGrounded && _movementLogic.ShouldRun(_inputReader.GetRunState())));
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