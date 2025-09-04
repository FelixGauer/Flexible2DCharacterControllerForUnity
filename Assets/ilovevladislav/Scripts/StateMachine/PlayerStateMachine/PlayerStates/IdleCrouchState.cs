using PlatformerController2D.Runtime.Scripts.PhysicalModules.Modules;
using UnityEngine;

namespace PlatformerController2D.Runtime.Scripts.StateMachine.PlayerStateMachine.PlayerState
{
    public class IdleCrouchState : BaseState
    {
        private readonly CrouchModule _crouchModule;

        public IdleCrouchState(PlayerStateContext context, CrouchModule crouchModule) :
            base(context)
        {
            _crouchModule = crouchModule;
        }

        public override void OnEnter()
        {		
            Debug.Log("IdleCrouchState");
        
            animationController.PlayAnimation("CrouchIdle");
        
            _crouchModule.SetCrouchState(true);

        }

        public override void Update()
        {
            turnChecker.TurnCheck(inputReader.GetMoveDirection());
        }

        public override void OnExit()
        {
            if (inputReader.GetDashState().WasPressedThisFrame) return;

            _crouchModule.SetCrouchState(false);
        }
    }
}