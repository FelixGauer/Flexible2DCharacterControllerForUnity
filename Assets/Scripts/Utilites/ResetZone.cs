using UnityEngine;

public class ResetZone : MonoBehaviour
{
    [Header("Настройки")]
    public Transform playerStartPosition; // Стартовая позиция игрока
    public string playerTag = "Player"; // Тег игрока
    public Transform playerTarget;
    
    [Header("Опциональные эффекты")]
    public AudioSource resetSound; // Звук при возврате
    public ParticleSystem resetEffect; // Эффект частиц при возврате
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем, что это игрок
        if (other.CompareTag(playerTag))
        {
            ResetPlayer(other.gameObject);
        }
    }
    
    private void ResetPlayer(GameObject player)
    {
        // Воспроизводим звук, если он есть
        if (resetSound != null)
        {
            resetSound.Play();
        }
        
        // Воспроизводим эффект частиц, если он есть
        if (resetEffect != null)
        {
            resetEffect.Play();
        }
        
        // Возвращаем игрока в стартовую позицию
        if (playerStartPosition != null)
        {
            playerTarget.position = playerStartPosition.position;
        }
        else
        {
            // Если стартовая позиция не задана, используем Vector3.zero
            player.transform.position = Vector3.zero;
            Debug.LogWarning("Стартовая позиция не задана! Игрок возвращен в (0,0,0)");
        }
        
        // Останавливаем движение игрока (если у него есть Rigidbody2D)
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }
        
        Debug.Log("Игрок возвращен в стартовую позицию");
    }
}