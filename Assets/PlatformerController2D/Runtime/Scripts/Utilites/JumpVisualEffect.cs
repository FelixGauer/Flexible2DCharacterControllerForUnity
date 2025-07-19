using System.Collections;
using UnityEngine;

public class JumpVisualEffect : MonoBehaviour
{
    [Header("Visual Settings")]
    public float lifetime = 10f; // Время жизни эффекта
    public float fadeStartTime = 1.5f; // Когда начинать исчезновение
    public Color jumpColor = Color.cyan; // Цвет метки прыжка
    public float scale = 1f; // Размер эффекта
    
    private SpriteRenderer spriteRenderer;
    private static GameObject jumpMarkPrefab;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Создаем простую текстуру-круг если её нет
        if (spriteRenderer.sprite == null)
        {
            spriteRenderer.sprite = CreateCircleSprite();
        }
        
        spriteRenderer.color = jumpColor;
        transform.localScale = Vector3.one * scale;
        
        StartCoroutine(FadeAndDestroy());
    }
    
    // Статический метод для создания метки прыжка
    public static void CreateJumpMark(Vector3 position)
    {
        // Создаем GameObject для визуального эффекта
        GameObject jumpMark = new GameObject("JumpMark");
        jumpMark.transform.position = position;
        
        // Добавляем SpriteRenderer
        SpriteRenderer sr = jumpMark.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 10; // Показываем поверх других объектов
        
        // Добавляем наш компонент
        JumpVisualEffect effect = jumpMark.AddComponent<JumpVisualEffect>();
    }
    
    // Создаем простой круглый спрайт программно
    private Sprite CreateCircleSprite()
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size);
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 2f;
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);
                
                if (distance <= radius)
                {
                    // Создаем градиент от центра к краям
                    float alpha = 1f - (distance / radius);
                    alpha = Mathf.Pow(alpha, 2f); // Делаем более мягкий переход
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
    
    // Корутина для исчезновения и уничтожения
    private IEnumerator FadeAndDestroy()
    {
        Color originalColor = spriteRenderer.color;
        
        // Ждем до начала исчезновения
        yield return new WaitForSeconds(fadeStartTime);
        
        // Время исчезновения
        float fadeTime = lifetime - fadeStartTime;
        float elapsedTime = 0f;
        
        // Плавное исчезновение
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(originalColor.a, 0f, elapsedTime / fadeTime);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
        
        // Уничтожаем объект
        Destroy(gameObject);
    }
}