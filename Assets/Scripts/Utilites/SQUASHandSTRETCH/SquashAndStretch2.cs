using UnityEngine;

public class SquashAndStretch2 : MonoBehaviour
{
    [Header("Основные настройки")]
    [SerializeField] private float stretch = 0.1f; // Коэффициент растяжения (0.1 = слабо, 0.3 = сильно)
    [SerializeField] private float speedThreshold = 0.1f; // Минимальная скорость для эффекта
    [SerializeField] private float smoothSpeed = 5f; // Скорость сглаживания
    
    [Header("Настройки поворота")]
    [SerializeField] private bool enableRotation = true; // Включить поворот
    [SerializeField] private float maxTiltAngle = 15f; // Максимальный угол наклона (градусы)
    [SerializeField] private bool onlyHorizontalTilt = true; // Наклон только от горизонтального движения
    
    [Header("Компоненты")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform spriteTransform; // Оставить пустым если спрайт на том же объекте
    
    // Приватные переменные
    private Vector3 originalScale;
    private Vector3 targetScale;
    private float targetRotationAngle;
    
    void Start()
    {
        // Автоматически находим компоненты
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
            
        if (spriteTransform == null)
            spriteTransform = transform;
            
        // Запоминаем оригинальный размер
        originalScale = spriteTransform.localScale;
        targetRotationAngle = 0f;
    }
    
    void Update()
    {
        // Получаем скорость персонажа (используем linearVelocity для новых версий Unity)
        Vector3 velocity = rb.linearVelocity;
        
        // Проверяем, движется ли персонаж
        if (velocity.sqrMagnitude > speedThreshold * speedThreshold)
        {
            // Растяжение на основе скорости
            float scaleX = 1.0f + (velocity.magnitude * stretch);
            float scaleY = 1.0f / scaleX;
            targetScale = new Vector3(scaleX, scaleY, originalScale.z);
            
            // Наклон персонажа
            if (enableRotation)
            {
                if (onlyHorizontalTilt)
                {
                    // Наклон только от горизонтального движения (как при беге против ветра)
                    float horizontalSpeed = velocity.x;
                    float tiltIntensity = Mathf.Clamp01(Mathf.Abs(horizontalSpeed) / 8f);
                    targetRotationAngle = -Mathf.Sign(horizontalSpeed) * maxTiltAngle * tiltIntensity;
                }
                else
                {
                    // Легкий наклон в направлении движения (ограниченный)
                    Vector2 direction = velocity.normalized;
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    
                    // Ограничиваем угол наклона
                    angle = Mathf.Clamp(angle, -maxTiltAngle, maxTiltAngle);
                    targetRotationAngle = angle;
                }
            }
        }
        else
        {
            // Возвращаемся к исходному состоянию при низкой скорости
            targetScale = originalScale;
            targetRotationAngle = 0f;
        }
        
        // Плавно применяем изменения
        ApplyEffects();
    }
    
    void ApplyEffects()
    {
        // Плавно меняем размер
        spriteTransform.localScale = Vector3.Lerp(
            spriteTransform.localScale, 
            targetScale, 
            smoothSpeed * Time.deltaTime
        );
        
        // Плавно меняем поворот (только если включен)
        if (enableRotation)
        {
            float currentRotation = spriteTransform.eulerAngles.z;
            if (currentRotation > 180) currentRotation -= 360; // Нормализуем угол
            
            float newRotation = Mathf.LerpAngle(currentRotation, targetRotationAngle, smoothSpeed * Time.deltaTime);
            spriteTransform.rotation = Quaternion.Euler(0, 0, newRotation);
        }
    }
}