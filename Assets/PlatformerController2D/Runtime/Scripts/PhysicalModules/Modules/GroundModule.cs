using System;
using PlatformerController2D.Runtime.Scripts.Player;
using UnityEngine;

namespace PlatformerController2D.Runtime.Scripts.PhysicalModules.Modules
{
    public class GroundModule
    {
        private readonly PlayerControllerStats _playerControllerStats;

        private Vector2 _moveVelocity;

        private readonly JumpModule _jumpModule;
        private readonly DashModule _dashModule;

        public GroundModule(PlayerControllerStats playerControllerStats, JumpModule jumpModule, DashModule dashModule)
        {
            _jumpModule = jumpModule;
            _playerControllerStats = playerControllerStats;
            _dashModule = dashModule;

        }

        public void Test()
        {

        }
    }
}