// using UnityEngine;
// using UnityEngine.UIElements;
//
// [RequireComponent(typeof(UIDocument))]
// public class PlayerStatsUIBinder : MonoBehaviour
// {
//     [Header("References")]
//     [SerializeField] private PlayerControllerStats playerStats;      // Ваш ScriptableObject
//     [SerializeField] private UIDocument uiDocument;                  // Ссылка на UIDocument
//
//     // Имена слайдеров в UXML должны совпадать с этими:
//     private const string MoveSpeedSliderName       = "MySlider";
//     private const string WalkAccelSliderName       = "MySlider2";
//     private const string WalkDecelSliderName       = "MySlider3";
//
//     private Slider _moveSpeedSlider;
//     private Slider _walkAccelSlider;
//     private Slider _walkDecelSlider;
//
//     private void OnEnable()
//     {
//         var root = uiDocument.rootVisualElement;
//
//         // Находим слайдеры
//         _moveSpeedSlider  = root.Q<Slider>(MoveSpeedSliderName);
//         _walkAccelSlider  = root.Q<Slider>(WalkAccelSliderName);
//         _walkDecelSlider  = root.Q<Slider>(WalkDecelSliderName);
//
//         // Инициализируем их текущими значениями из ScriptableObject
//         if (_moveSpeedSlider != null) _moveSpeedSlider.value  = playerStats.MoveSpeed;
//         if (_walkAccelSlider != null) _walkAccelSlider.value  = playerStats.WalkAcceleration;
//         if (_walkDecelSlider != null) _walkDecelSlider.value  = playerStats.WalkDeceleration;
//
//         // Регистрируем колбэки
//         if (_moveSpeedSlider != null) _moveSpeedSlider.RegisterValueChangedCallback(evt =>
//         {
//             playerStats.MoveSpeed = evt.newValue;
//             Debug.Log($"MoveSpeed updated to {evt.newValue:F1}");
//         });
//
//         if (_walkAccelSlider != null) _walkAccelSlider.RegisterValueChangedCallback(evt =>
//         {
//             playerStats.WalkAcceleration = evt.newValue;
//             Debug.Log($"WalkAcceleration updated to {evt.newValue:F1}");
//         });
//
//         if (_walkDecelSlider != null) _walkDecelSlider.RegisterValueChangedCallback(evt =>
//         {
//             playerStats.WalkDeceleration = evt.newValue;
//             Debug.Log($"WalkDeceleration updated to {evt.newValue:F1}");
//         });
//     }
//
//     private void OnDisable()
//     {
//         // Отписываемся, чтобы не было утечек
//         if (_moveSpeedSlider  != null) _moveSpeedSlider.UnregisterValueChangedCallback(null);
//         if (_walkAccelSlider  != null) _walkAccelSlider.UnregisterValueChangedCallback(null);
//         if (_walkDecelSlider  != null) _walkDecelSlider.UnregisterValueChangedCallback(null);
//     }
// }