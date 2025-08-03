using PlatformerController2D.Runtime.Scripts.Player;
using UnityEngine;

namespace PlatformerController2D.Runtime.Scripts.Systems
{
    public class CollisionsChecker : MonoBehaviour
    {
        [SerializeField] PlayerControllerStats stats;

        [SerializeField] private Collider2D FeetCollider;
        [SerializeField] private Collider2D BodyCollider;

        public bool IsGrounded { get; private set; }
        public bool BumpedHead { get; private set; }
        public bool IsTouchingWall { get; private set; }
        public bool IsInWallZone { get; private set; } 

        private RaycastHit2D _headHit;
        private RaycastHit2D _wallHit;
        private RaycastHit2D _lastWallHit;
        private bool _lastWallFacingRight; 
    
        private bool _wasGrounded;
        private bool _wasBumpingHead;
        private bool _wasTouchingWall;
    
        public System.Func<bool> IsSitting;
        public System.Func<bool> IsFacingRight;    

        public event System.Action OnGroundTouched;    // Коснулся земли (не был на земле -> стал на земле)
        public event System.Action OnGroundLeft;       // Покинул землю (был на земле -> не на земле)
        public event System.Action OnHeadBumped;       // Ударился головой (не бился -> ударился)
        public event System.Action OnHeadFreed;        // Освободил голову (бился -> не бился)
        public event System.Action OnWallTouched;      // Коснулся стены (не касался -> коснулся)
        public event System.Action OnWallLeft;         // Покинул стену (касался -> не касается)

        void Start()
        {
            _wasGrounded = IsGrounded;
            _wasBumpingHead = BumpedHead;
            _wasTouchingWall = IsTouchingWall;
        }

        void Update()
        {     
            CheckIsGrounded();
            CheckBumpedHead();
            CheckTouchWall();

            if (stats.CanJumpInTheAirTowardsTheWall)
                CheckWallZone();
            else
                IsInWallZone = false;
       
            CheckStateTransitions();
        }

        private void CheckStateTransitions()
        {
            if (!_wasGrounded && IsGrounded)
            {
                OnGroundTouched?.Invoke();
            }
            else if (_wasGrounded && !IsGrounded)
            {
                OnGroundLeft?.Invoke();
            }
            if (!_wasBumpingHead && BumpedHead)
            {
                OnHeadBumped?.Invoke();
            }
            else if (_wasBumpingHead && !BumpedHead)
            {
                OnHeadFreed?.Invoke();
            }

            if (!_wasTouchingWall && IsTouchingWall)
            {
                OnWallTouched?.Invoke();
            }
            else if (_wasTouchingWall && !IsTouchingWall)
            {
                OnWallLeft?.Invoke();
            }

            _wasGrounded = IsGrounded;
            _wasBumpingHead = BumpedHead;
            _wasTouchingWall = IsTouchingWall;
        }

        private void CheckIsGrounded()
        {
            var bounds = FeetCollider.bounds;
            Vector2 boxCastOrigin = new Vector2(bounds.center.x, bounds.min.y);
            Vector2 boxCastSize = new Vector2(bounds.size.x, stats.GroundDetectionRayLenght);

            IsGrounded = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, stats.GroundDetectionRayLenght, stats.GroundLayer).collider != null;
        }

        private void CheckBumpedHead()
        {
            var bounds = FeetCollider.bounds;
            float currentHeadDetectionRayLength = (IsSitting?.Invoke() ?? false) ? 0.4f : stats.HeadDetectionRayLength; // FIXME Заменить 0.4

            Vector2 boxCastOrigin = new Vector2(bounds.center.x, BodyCollider.bounds.max.y);
            Vector2 boxCastSize = new Vector2(bounds.size.x * stats.HeadWidth, stats.HeadDetectionRayLength);

            _headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, currentHeadDetectionRayLength, stats.GroundLayer);
            BumpedHead = _headHit.collider != null;

            Color rayColor = BumpedHead ? Color.green : Color.red;

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * stats.HeadWidth, boxCastOrigin.y), Vector2.up * currentHeadDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2 * stats.HeadWidth, boxCastOrigin.y), Vector2.up * currentHeadDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * stats.HeadWidth, currentHeadDetectionRayLength + boxCastOrigin.y), Vector2.right * boxCastSize.x * stats.HeadWidth, rayColor);
        }

        private void CheckTouchWall()
        {
            float originEndPoint = IsFacingRight() ? BodyCollider.bounds.max.x : BodyCollider.bounds.min.x;
            float adjustedHeight = BodyCollider.bounds.size.y * stats.WallDetectionRayHeightMultiplayer;

            Vector2 boxCastOrigin = new Vector2(originEndPoint, BodyCollider.bounds.center.y);
            Vector2 boxCastSize = new Vector2(stats.WallDetectionRayLength, adjustedHeight);

            _wallHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, transform.right, stats.WallDetectionRayLength, stats.GroundLayer);
       
            if (_wallHit.collider != null)
            {
                _lastWallHit = _wallHit;
                _lastWallFacingRight = IsFacingRight(); 
                IsTouchingWall = true;
            }
            else
            {
                IsTouchingWall = false;
            }

            #region Debug

            Color rayColor = IsTouchingWall ? Color.green : Color.red;

            // Отрисовка отладочного бокса
            Vector2 boxBottomLeft = new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - boxCastSize.y / 2);
            Vector2 boxBottomRight = new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y - boxCastSize.y / 2);
            Vector2 boxTopLeft = new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y + boxCastSize.y / 2);
            Vector2 boxTopRight = new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y + boxCastSize.y / 2);

            Debug.DrawLine(boxBottomLeft, boxBottomRight, rayColor);
            Debug.DrawLine(boxBottomRight, boxTopRight, rayColor);
            Debug.DrawLine(boxTopRight, boxTopLeft, rayColor);
            Debug.DrawLine(boxTopLeft, boxBottomLeft, rayColor);

            #endregion
        }

        private void CheckWallZone()
        {
            if (_lastWallHit.collider == null || IsFacingRight() != _lastWallFacingRight)
            {
                IsInWallZone = false;
                _lastWallHit = new RaycastHit2D();
                return;
            }

            Vector2 playerCenter = BodyCollider.bounds.center;
        
            Vector2 wallPosition = _lastWallHit.point;
        
            Vector2 directionToWall = (wallPosition - playerCenter).normalized;
            Vector2 rayDirection = IsFacingRight() ? Vector2.right : Vector2.left;

            float distanceToWall = Vector2.Distance(playerCenter, wallPosition);
        
            RaycastHit2D wallZoneHit = Physics2D.Raycast(playerCenter, rayDirection, distanceToWall * 10f, stats.GroundLayer);
        
            IsInWallZone = wallZoneHit.collider != null && wallZoneHit.collider == _lastWallHit.collider;

            if (_lastWallHit && !wallZoneHit)
            {
                _lastWallHit = new RaycastHit2D();
            }


            #region Debug Wall Zone
            if (_lastWallHit.collider != null && IsFacingRight() == _lastWallFacingRight)
            {
                // Цвет луча: зеленый если в зоне стены, красный если нет
                Color rayColor = IsInWallZone ? Color.green : Color.red;
            
                // Отрисовка луча от центра игрока к стене
                Debug.DrawRay(playerCenter, rayDirection * distanceToWall, rayColor);
            
                // Отрисовка точки стены
                Debug.DrawRay(wallPosition, Vector2.up * 0.2f, Color.magenta);
                Debug.DrawRay(wallPosition, Vector2.down * 0.2f, Color.magenta);
                Debug.DrawRay(wallPosition, Vector2.left * 0.2f, Color.magenta);
                Debug.DrawRay(wallPosition, Vector2.right * 0.2f, Color.magenta);
            }
            #endregion
        }
    
    
        private void CheckWallZone2()
        {
            if (_lastWallHit.collider == null || IsFacingRight() != _lastWallFacingRight)
            {
                IsInWallZone = false;
                _lastWallHit = new RaycastHit2D();
                return;
            }

            Vector2 playerCenter = BodyCollider.bounds.center;
    
            Vector2 rayDirection = IsFacingRight() ? Vector2.right : Vector2.left;
    
            float checkDistance =  100f; // или любое другое значение
    
            RaycastHit2D wallZoneHit = Physics2D.Raycast(playerCenter, rayDirection, checkDistance, stats.GroundLayer);
    
            IsInWallZone = wallZoneHit.collider != null && wallZoneHit.collider == _lastWallHit.collider;

            #region Debug Wall Zone
            if (_lastWallHit.collider != null && IsFacingRight() == _lastWallFacingRight)
            {
                // Цвет луча: зеленый если в зоне стены, красный если нет
                Color rayColor = IsInWallZone ? Color.green : Color.red;
        
                // Отрисовка луча от центра игрока в сторону взгляда
                Debug.DrawRay(playerCenter, rayDirection * checkDistance, rayColor);
        
                // Отрисовка точки стены для справки
                Vector2 wallPosition = _lastWallHit.point;
                Debug.DrawRay(wallPosition, Vector2.up * 0.2f, Color.magenta);
                Debug.DrawRay(wallPosition, Vector2.down * 0.2f, Color.magenta);
                Debug.DrawRay(wallPosition, Vector2.left * 0.2f, Color.magenta);
                Debug.DrawRay(wallPosition, Vector2.right * 0.2f, Color.magenta);
            }
            #endregion
        }

        public RaycastHit2D GetLastWallHit() => _lastWallHit;
        public RaycastHit2D GetCurrentHeadHit() => _headHit;
        public RaycastHit2D GetCurrentWallHit() => _wallHit;
    
        public bool CanWallJump()
        {
            return IsTouchingWall && !IsInWallZone;
        }
    }
}