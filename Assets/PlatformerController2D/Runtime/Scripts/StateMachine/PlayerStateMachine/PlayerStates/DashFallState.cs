using PlatformerController2D.Runtime.Scripts.PhysicalModules.Modules;
using UnityEngine;

namespace PlatformerController2D.Runtime.Scripts.StateMachine.PlayerStateMachine.PlayerState
{
    public class DashFallState : BaseState
    {
        private readonly FallModule _fallModule;
        private readonly MovementModule _movementModule;
    
        private Vector2 _moveVelocity;

        public DashFallState(PlayerStateContext context, FallModule fallModule, MovementModule movementModule) :
            base(context)
        {
            _fallModule = fallModule;
            _movementModule = movementModule;
        }

        public override void OnEnter()
        {
            Debug.Log("DashFallState");

            animationController.PlayAnimation("Fall");
        }

        public override void Update()
        {
            _fallModule.BufferJump(inputReader.GetJumpState());
            _fallModule.SetHoldState(inputReader.GetJumpState().IsHeld);
		
            turnChecker.TurnCheck(inputReader.GetMoveDirection());
        }

        public override void FixedUpdate()
        {
            _moveVelocity.y = _fallModule.HandleFalling(physicsHandler2D.GetVelocity()).y;
            _moveVelocity.x = _movementModule.HandleMovement(physicsHandler2D.GetVelocity(), inputReader.GetNormalizedHorizontalDirection(), playerControllerStats.DashFallSpeed, playerControllerStats.DashFallAirAcceleration, playerControllerStats.DashFallAirDeceleration).x; // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
            physicsHandler2D.AddVelocity(_moveVelocity);
        }
	
        public override void OnExit()
        {

        }
    }
}