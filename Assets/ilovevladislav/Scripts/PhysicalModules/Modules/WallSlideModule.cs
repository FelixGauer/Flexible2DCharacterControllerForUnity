using PlatformerController2D.Runtime.Scripts.Player;
using PlatformerController2D.Runtime.Scripts.Systems;
using PlatformerController2D.Runtime.Scripts.Utilites;
using UnityEngine;

namespace PlatformerController2D.Runtime.Scripts.PhysicalModules.Modules
{
    public class WallSlideModule
    {
        private readonly PlayerControllerStats _playerControllerStats;
        private readonly TurnChecker _turnChecker;
        private readonly CountdownTimer _wallJumpTimer;
    

	
        private Vector2 _moveVelocity;
        private bool _wasWallSliding;

        private bool IsFacingRight => _turnChecker.IsFacingRight;
    
        private float _wallDirection;

        public WallSlideModule(PlayerControllerStats playerControllerStats, TurnChecker turnChecker, CountdownTimer wallJumpTimer) 
        {
            _playerControllerStats = playerControllerStats;
            _turnChecker = turnChecker;
            _wallJumpTimer = wallJumpTimer;
        }
    
        public Vector2 ProcessWallSlide(Vector2 inputDirection)
        {
            // Применяем скольжение по Y оси
            ApplyWallSlidePhysics();
        
            // Обрабатываем таймер отрыва от стены
            // HandleWallDetachmentTimer(inputDirection);
        
            return _moveVelocity;
        }
    
        // public event System.Action OnSlide;    // Коснулся земли (не был на земле -> стал на земле)

        public void OnEnterWallSlide()
        {
            // Устанавливаем начальную скорость скольжения
            _moveVelocity.x = 0f; // Убираем горизонтальную скорость
            _moveVelocity.y = -_playerControllerStats.StartVelocityWallSlide;
        
            // Определяем направление стены
            _wallDirection = CalculateWallDirection();
        
            // OnSlide.Invoke();
        
            // Сбрасываем таймер
            // _wallJumpTimer.Reset();

            StopWallJumpTimer();
        }
    
        public void OnExitWallSlide()
        {
            StopWallJumpTimer();
        }
    
        private void ApplyWallSlidePhysics()
        {
            // Плавное изменение скорости падения до максимальной скорости скольжения
            _moveVelocity.y = Mathf.Lerp(
                _moveVelocity.y, 
                -_playerControllerStats.WallSlideSpeedMax, 
                _playerControllerStats.WallSlideDeceleration * Time.fixedDeltaTime
            );
        }
    
        public void HandleWallDetachmentTimer(Vector2 inputDirection)
        {
        
            // bool isMovingTowardsWall = Mathf.Approximately(inputDirection.x, _wallDirection) || 
            //                            Mathf.Approximately(inputDirection.x, 0f);
            // bool isMovingAwayFromWall = !Mathf.Approximately(inputDirection.x, _wallDirection) && 
            //                             !Mathf.Approximately(inputDirection.x, 0f);
        
            bool isMovingTowardsWall = Mathf.Sign(inputDirection.x) == Mathf.Sign(_wallDirection) || 
                                       Mathf.Approximately(inputDirection.x, 0f);
            bool isMovingAwayFromWall = Mathf.Sign(inputDirection.x) != Mathf.Sign(_wallDirection) && 
                                        !Mathf.Approximately(inputDirection.x, 0f);

            if (isMovingTowardsWall)
            {
                // Игрок прижимается к стене или не двигается - останавливаем таймер
                StopWallJumpTimer();
            }
            else if (isMovingAwayFromWall && !_wallJumpTimer.IsRunning)
            {
                // Игрок пытается отойти от стены - запускаем таймер для возможности wall jump
                _wallJumpTimer.Start();
            }
        }
    
        // Возвращает текущее направление стены (для внешнего использования)
        public float CurrentWallDirection => _wallDirection;

        // Проверяет, активен ли таймер прыжка от стены
        public bool IsWallJumpTimerActive => _wallJumpTimer.IsRunning;
    
        // Проверяет, закончился ли таймер прыжка от стены
        public bool IsWallJumpTimerFinished => _wallJumpTimer.IsFinished;
    
        private float CalculateWallDirection()
        {
            return IsFacingRight ? 1f : -1f;
        }
    
        private void StopWallJumpTimer()
        {
            _wallJumpTimer.Start();
            _wallJumpTimer.Stop();
            _wallJumpTimer.Reset();
        }

	
        // // Обработка скольжения по стене WallSlide
        // public Vector2 HandleWallSlide(Vector2 moveDirection)
        // {
        //     // Плавное Изменение Y на стене,для скольжения 
        //     // Lerp зависит - 1. Начальная скорость скольжения (задается в Enter), 2 - макс. скорость скольжения, 3 - Deceleration скольжения
        //     _moveVelocity.y = Mathf.Lerp(_moveVelocity.y, -_playerControllerStats.WallSlideSpeedMax, _playerControllerStats.WallSlideDeceleration * Time.fixedDeltaTime); //FIXME
        //
        //     HandleWallJumpTimer(moveDirection);
        //
        //     // Если ввод обратный вводу стены, ввод не равен 0, таймер не идет и на стене, запускаем таймер, отвечающий остаток времени на стене
        //     // Запуск таймера при попытке слезть со стены, помогает выполнить прыжок от стены
        //     // if (_moveDirection.x != CalculateWallDirectionX() && _moveDirection.x != 0f && !_wallJumpTimer.IsRunning && _physicsContext.WasWallSliding)
        //     if (moveDirection.x != wallDirectionX && moveDirection.x != 0f && !_wallJumpTimer.IsRunning)
        //     {
        //         _wallJumpTimer.Start();
        //     }
        //     
        //     return _moveVelocity;
        // }
        //
        // // Метод управления таймером
        // private void HandleWallJumpTimer(Vector2 _moveDirection)
        // {
        //     if (_moveDirection.x == CalculateWallDirectionX() || _moveDirection.x == 0f)
        //     {
        //         _wallJumpTimer.Stop();
        //         _wallJumpTimer.Reset();
        //     }
        // }
        //
        // // Метод вычисляет направление стены по X
        // public float CalculateWallDirectionX()
        // {
        //     return IsFacingRight ? 1f : -1f;
        // }
        //
        // public void OnEnterWallSliding()
        // {
        //     // Начальная скорость скольжения, x = 0 - персонаж падает вниз после окончания скольжения, то есть не сохраняет скорость
        //     _moveVelocity = new Vector2(0f, -_playerControllerStats.StartVelocityWallSlide);
        //
        //     CalculateWallDirectionXTEST();
        //
        //     // Сброс таймера
        //     _wallJumpTimer.Reset();
        // }
        //
        // // Метод вызываемый при выходе в состояние wallJump/Slide
        // public void OnExitWallSliding()
        // {
        //     // Остановка таймера
        //     _wallJumpTimer.Stop();
        // }
        //
        // public float wallDirectionX { get; private set;  }
        //
        // // Метод вычисляет направление стены по X
        // private void CalculateWallDirectionXTEST()
        // {
        //     wallDirectionX = IsFacingRight ? 1f : -1f;
        //     
        //     // return IsFacingRight ? 1f : -1f;
        // }
    }
}