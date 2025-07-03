using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

/// <summary>
/// Контроллер для связи UI слайдеров со статами игрока
/// Обеспечивает двустороннюю синхронизацию значений
/// </summary>
public class StatsSliderController : MonoBehaviour
{
    [Header("UI Setup")]
    [SerializeField] private UIDocument uiDocument;
    
    [Header("Stats Reference")]
    [SerializeField] private PlayerControllerStats playerStats;
    
    [Header("Slider Configuration")]
    [SerializeField] private List<SliderStatBinding> sliderBindings = new List<SliderStatBinding>
    {
        new SliderStatBinding("MovementRoot", "MySlider", "MoveSpeed"),
        new SliderStatBinding("MovementRoot", "MySlider2", "WalkAcceleration"),
        new SliderStatBinding("MovementRoot", "MySlider3", "WalkDeceleration")
    };

    private VisualElement _root;
    private readonly Dictionary<Slider, SliderStatBinding> _sliderBindingMap = new Dictionary<Slider, SliderStatBinding>();
    private bool _isInitializing = false;

    #region Unity Lifecycle
    
    private void Awake()
    {
        ValidateReferences();
        InitializeUI();
    }

    private void OnEnable()
    {
        RegisterSliderCallbacks();
        SynchronizeSlidersFromStats();
    }

    private void OnDisable()
    {
        UnregisterSliderCallbacks();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Проверяет наличие всех необходимых ссылок
    /// </summary>
    private void ValidateReferences()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.LogError($"[{gameObject.name}] UIDocument не найден! Прикрепите UIDocument к GameObject.", this);
            enabled = false;
            return;
        }

        if (playerStats == null)
        {
            Debug.LogError($"[{gameObject.name}] PlayerControllerStats не назначен! Назначьте ScriptableObject со статами.", this);
            enabled = false;
            return;
        }
    }

    /// <summary>
    /// Инициализирует UI элементы
    /// </summary>
    private void InitializeUI()
    {
        _root = uiDocument.rootVisualElement;
        
        if (_root == null)
        {
            Debug.LogError($"[{gameObject.name}] Не удалось получить корневой элемент UI.", this);
            enabled = false;
        }
    }

    #endregion

    #region Slider Management

    /// <summary>
    /// Регистрирует обработчики событий для всех слайдеров
    /// </summary>
    private void RegisterSliderCallbacks()
    {
        _sliderBindingMap.Clear();

        foreach (var binding in sliderBindings)
        {
            var slider = FindSlider(binding);
            if (slider != null)
            {
                _sliderBindingMap[slider] = binding;
                slider.RegisterValueChangedCallback(OnSliderValueChanged);
                
                Debug.Log($"✓ Слайдер '{binding.SliderName}' в контейнере '{binding.ContainerName}' успешно привязан к стату '{binding.StatFieldName}'");
            }
        }

        Debug.Log($"[StatsSliderController] Зарегистрировано {_sliderBindingMap.Count} слайдеров из {sliderBindings.Count} заданных.");
    }

    /// <summary>
    /// Отменяет регистрацию обработчиков событий
    /// </summary>
    private void UnregisterSliderCallbacks()
    {
        foreach (var kvp in _sliderBindingMap)
        {
            if (kvp.Key != null)
                kvp.Key.UnregisterValueChangedCallback(OnSliderValueChanged);
        }
        
        _sliderBindingMap.Clear();
    }

    /// <summary>
    /// Находит слайдер по заданной привязке
    /// </summary>
    private Slider FindSlider(SliderStatBinding binding)
    {
        if (string.IsNullOrEmpty(binding.ContainerName))
        {
            // Ищем слайдер напрямую в корне
            var slider = _root.Q<Slider>(binding.SliderName);
            if (slider == null)
                Debug.LogWarning($"⚠ Слайдер '{binding.SliderName}' не найден в корневом элементе.");
            return slider;
        }
        else
        {
            // Ищем слайдер в указанном контейнере
            var container = _root.Q<VisualElement>(binding.ContainerName);
            if (container == null)
            {
                Debug.LogWarning($"⚠ Контейнер '{binding.ContainerName}' не найден.");
                return null;
            }

            var slider = container.Q<Slider>(binding.SliderName);
            if (slider == null)
                Debug.LogWarning($"⚠ Слайдер '{binding.SliderName}' не найден в контейнере '{binding.ContainerName}'.");
            
            return slider;
        }
    }

    #endregion

    #region Value Synchronization

    /// <summary>
    /// Синхронизирует значения слайдеров со значениями статов при запуске
    /// </summary>
    private void SynchronizeSlidersFromStats()
    {
        _isInitializing = true;
        
        foreach (var kvp in _sliderBindingMap)
        {
            var slider = kvp.Key;
            var binding = kvp.Value;
            
            var statValue = GetStatValue(binding.StatFieldName);
            if (statValue.HasValue)
            {
                slider.value = statValue.Value;
                Debug.Log($"🔄 Слайдер '{binding.SliderName}' синхронизирован со значением стата '{binding.StatFieldName}': {statValue.Value:F2}");
            }
        }
        
        _isInitializing = false;
        Debug.Log("[StatsSliderController] Начальная синхронизация завершена.");
    }

    /// <summary>
    /// Обработчик изменения значения слайдера
    /// </summary>
    private void OnSliderValueChanged(ChangeEvent<float> evt)
    {
        // Игнорируем события во время инициализации
        if (_isInitializing)
            return;

        var slider = (Slider)evt.currentTarget;
        
        if (_sliderBindingMap.TryGetValue(slider, out var binding))
        {
            SetStatValue(binding.StatFieldName, evt.newValue);
            
            Debug.Log($"📊 Стат '{binding.StatFieldName}' изменен через слайдер '{binding.SliderName}': {evt.previousValue:F2} → {evt.newValue:F2}");
        }
    }

    #endregion

    #region Stats Access

    /// <summary>
    /// Получает значение стата по имени поля
    /// </summary>
    private float? GetStatValue(string fieldName)
    {
        var field = typeof(PlayerControllerStats).GetField(fieldName);
        if (field != null && field.FieldType == typeof(float))
        {
            return (float)field.GetValue(playerStats);
        }
        
        Debug.LogError($"❌ Поле '{fieldName}' не найдено или имеет неверный тип в PlayerControllerStats.");
        return null;
    }

    /// <summary>
    /// Устанавливает значение стата по имени поля
    /// </summary>
    private void SetStatValue(string fieldName, float value)
    {
        var field = typeof(PlayerControllerStats).GetField(fieldName);
        if (field != null && field.FieldType == typeof(float))
        {
            field.SetValue(playerStats, value);
        }
        else
        {
            Debug.LogError($"❌ Не удалось установить значение для поля '{fieldName}' в PlayerControllerStats.");
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// Принудительно синхронизирует все слайдеры со статами
    /// </summary>
    [ContextMenu("Синхронизировать слайдеры со статами")]
    public void ForceSynchronizeSliders()
    {
        if (!enabled || _sliderBindingMap.Count == 0)
        {
            Debug.LogWarning("[StatsSliderController] Невозможно выполнить синхронизацию - контроллер не готов.");
            return;
        }

        SynchronizeSlidersFromStats();
    }

    /// <summary>
    /// Добавляет новую привязку слайдера к стату во время выполнения
    /// </summary>
    public void AddSliderBinding(string containerName, string sliderName, string statFieldName)
    {
        var newBinding = new SliderStatBinding(containerName, sliderName, statFieldName);
        sliderBindings.Add(newBinding);
        
        if (enabled && _root != null)
        {
            var slider = FindSlider(newBinding);
            if (slider != null)
            {
                _sliderBindingMap[slider] = newBinding;
                slider.RegisterValueChangedCallback(OnSliderValueChanged);
                
                // Синхронизируем новый слайдер
                var statValue = GetStatValue(statFieldName);
                if (statValue.HasValue)
                {
                    _isInitializing = true;
                    slider.value = statValue.Value;
                    _isInitializing = false;
                }
                
                Debug.Log($"✅ Динамически добавлена привязка: {sliderName} → {statFieldName}");
            }
        }
    }

    #endregion
}

/// <summary>
/// Класс для описания привязки слайдера к стату
/// </summary>
[System.Serializable]
public class SliderStatBinding
{
    [Tooltip("Имя контейнера, содержащего слайдер (оставьте пустым для поиска в корне)")]
    public string ContainerName;
    
    [Tooltip("Имя слайдера в UI")]
    public string SliderName;
    
    [Tooltip("Имя поля в PlayerControllerStats")]
    public string StatFieldName;

    public SliderStatBinding() { }

    public SliderStatBinding(string containerName, string sliderName, string statFieldName)
    {
        ContainerName = containerName;
        SliderName = sliderName;
        StatFieldName = statFieldName;
    }

    public override string ToString()
    {
        return $"{ContainerName}/{SliderName} → {StatFieldName}";
    }
}