using UnityEngine;

public class CollisionsChecker : MonoBehaviour
{
    [SerializeField] PlayerControllerStats stats;

    [SerializeField] private Collider2D FeetCollider;
    [SerializeField] private Collider2D BodyCollider;

    public bool IsGrounded { get; private set; }
    public bool BumpedHead { get; private set; }
    public bool IsTouchingWall { get; private set; }
    public bool IsInWallZone { get; private set; } // Новое свойство для проверки зоны стены

    private RaycastHit2D _headHit;
    private RaycastHit2D _wallHit;
    private RaycastHit2D _lastWallHit;
    private bool _lastWallFacingRight; // Направление, когда касались стены
    
    // Предыдущие состояния для отслеживания изменений
    private bool _wasGrounded;
    private bool _wasBumpingHead;
    private bool _wasTouchingWall;
    
    // Делегаты для получения состояния персонажа
    public System.Func<bool> IsSitting;
    public System.Func<bool> IsFacingRight;    

    // События для изменений состояния коллизий
    public event System.Action OnGroundTouched;    // Коснулся земли (не был на земле -> стал на земле)
    public event System.Action OnGroundLeft;       // Покинул землю (был на земле -> не на земле)
    public event System.Action OnHeadBumped;       // Ударился головой (не бился -> ударился)
    public event System.Action OnHeadFreed;        // Освободил голову (бился -> не бился)
    public event System.Action OnWallTouched;      // Коснулся стены (не касался -> коснулся)
    public event System.Action OnWallLeft;         // Покинул стену (касался -> не касается)

    void Start()
    {
        // Инициализируем предыдущие состояния
        _wasGrounded = IsGrounded;
        _wasBumpingHead = BumpedHead;
        _wasTouchingWall = IsTouchingWall;
    }

    void Update()
    {     
       CheckIsGrounded();
       CheckBumpedHead();
       CheckTouchWall();

       if (stats.CanWallJumpTowardsTheWall)
           CheckWallZone();
       else
           IsInWallZone = false;
       
       // Проверяем изменения и вызываем соответствующие события
       CheckStateTransitions();
    }

    private void CheckStateTransitions()
    {
        // Проверка изменений касания земли
        if (!_wasGrounded && IsGrounded)
        {
            OnGroundTouched?.Invoke();
        }
        else if (_wasGrounded && !IsGrounded)
        {
            OnGroundLeft?.Invoke();
            // Debug.Log("YES");
        }
        // Проверка изменений удара головой
        if (!_wasBumpingHead && BumpedHead)
        {
            OnHeadBumped?.Invoke();
        }
        else if (_wasBumpingHead && !BumpedHead)
        {
            OnHeadFreed?.Invoke();
        }

        // Проверка изменений касания стены
        if (!_wasTouchingWall && IsTouchingWall)
        {
            OnWallTouched?.Invoke();
        }
        else if (_wasTouchingWall && !IsTouchingWall)
        {
            OnWallLeft?.Invoke();
        }

        // Обновляем предыдущие состояния
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
          _lastWallFacingRight = IsFacingRight(); // Запоминаем направление при касании стены
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

    // Новый метод для проверки зоны стены
    private void CheckWallZone()
    {
        // Проверяем только если у нас есть информация о последней стене и игрок смотрит в ту же сторону
        if (_lastWallHit.collider == null || IsFacingRight() != _lastWallFacingRight)
        {
            IsInWallZone = false;
            _lastWallHit = new RaycastHit2D();
            return;
        }

        // Получаем позицию игрока (центр)
        Vector2 playerCenter = BodyCollider.bounds.center;
        
        // Получаем позицию последней стены
        Vector2 wallPosition = _lastWallHit.point;
        
        // Направление от игрока к стене
        Vector2 directionToWall = (wallPosition - playerCenter).normalized;
        Vector2 rayDirection = IsFacingRight() ? Vector2.right : Vector2.left;

        
        // Расстояние до стены
        float distanceToWall = Vector2.Distance(playerCenter, wallPosition);
        
        // Кастим луч от центра игрока к стене
        RaycastHit2D wallZoneHit = Physics2D.Raycast(playerCenter, rayDirection, distanceToWall, stats.GroundLayer);
        
        // Игрок в зоне стены, если луч попадает в ту же стену
        IsInWallZone = wallZoneHit.collider != null && wallZoneHit.collider == _lastWallHit.collider;


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
    
    
    // Новый метод для проверки зоны стены
    private void CheckWallZone2()
    {
        // Проверяем только если у нас есть информация о последней стене и игрок смотрит в ту же сторону
        if (_lastWallHit.collider == null || IsFacingRight() != _lastWallFacingRight)
        {
            IsInWallZone = false;
            _lastWallHit = new RaycastHit2D();
            return;
        }

        // Получаем позицию игрока (центр)
        Vector2 playerCenter = BodyCollider.bounds.center;
    
        // Направление луча (вправо или влево в зависимости от того, куда смотрит игрок)
        Vector2 rayDirection = IsFacingRight() ? Vector2.right : Vector2.left;
    
        // Расстояние для проверки (можете настроить это значение)
        float checkDistance =  100f; // или любое другое значение
    
        // Кастим луч от центра игрока в сторону, куда он смотрит
        RaycastHit2D wallZoneHit = Physics2D.Raycast(playerCenter, rayDirection, checkDistance, stats.GroundLayer);
    
        // Игрок в зоне стены, если луч попадает в ту же стену, что и раньше
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

    // Дополнительные методы для получения информации о последних коллизиях
    public RaycastHit2D GetLastWallHit() => _lastWallHit;
    public RaycastHit2D GetCurrentHeadHit() => _headHit;
    public RaycastHit2D GetCurrentWallHit() => _wallHit;
    
    // Метод для проверки, можно ли прыгать по стене
    public bool CanWallJump()
    {
        // Игрок может прыгать по стене только если он касается её, но не находится в её зоне
        // Или добавьте свою логику здесь
        return IsTouchingWall && !IsInWallZone;
    }
}