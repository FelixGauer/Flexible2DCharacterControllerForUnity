using UnityEngine;

namespace PlatformerController2D.Runtime.Scripts.Utilites
{
    public class ResetZone : MonoBehaviour
    {
        public Transform playerStartPosition; // Стартовая позиция игрока
        public string playerTag = "Player"; // Тег игрока
        public Transform playerTarget;
    
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
        
            // Возвращаем игрока в стартовую позицию
            if (playerStartPosition != null)
            {
                playerTarget.position = playerStartPosition.position;
            }
            else
            {
                // Если стартовая позиция не задана, используем Vector3.zero
                player.transform.position = Vector3.zero;
            }
        
            // Останавливаем движение игрока (если у него есть Rigidbody2D)
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;
                playerRb.angularVelocity = 0f;
            }
        
        }
    }
}