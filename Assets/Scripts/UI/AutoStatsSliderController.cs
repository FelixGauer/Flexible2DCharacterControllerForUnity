using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Автоматический контроллер для генерации UI слайдеров для всех полей PlayerControllerStats
/// Создает слайдеры автоматически и размещает их в ScrollView
/// </summary>
public class AutoStatsSliderController : MonoBehaviour
{
    [Header("UI Setup")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private string scrollViewName = "StatsScrollView";
    
    [Header("Stats Reference")]
    [SerializeField] private PlayerControllerStats playerStats;
    
    [Header("Slider Configuration")]
    [SerializeField] private bool includeReadOnlyProperties = false;
    [SerializeField] private bool groupByHeaders = true;
    [SerializeField] private float defaultMinValue = 0f;
    [SerializeField] private float defaultMaxValue = 100f;
    [SerializeField] private List<string> excludedFields = new List<string> { "DashDirections", "name", "hideFlags" };
    
    [Header("Visual Settings")]
    [SerializeField] private Color headerColor = Color.white;
    [SerializeField] private Color sliderLabelColor = Color.gray;
    [SerializeField] private int sliderHeight = 30;
    [SerializeField] private int headerHeight = 40;
    [SerializeField] private int spacing = 5;

    private VisualElement _root;
    private ScrollView _scrollView;
    private readonly Dictionary<Slider, FieldInfo> _sliderFieldMap = new Dictionary<Slider, FieldInfo>();
    private readonly Dictionary<string, List<FieldInfo>> _fieldsByCategory = new Dictionary<string, List<FieldInfo>>();
    private bool _isInitializing = false;

    #region Unity Lifecycle
    
    private void Awake()
    {
        ValidateReferences();
        InitializeUI();
    }

    private void OnEnable()
    {
        GenerateSliders();
    }

    private void OnDisable()
    {
        ClearSliders();
    }

    #endregion

    #region Initialization

    private void ValidateReferences()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.LogError($"[{gameObject.name}] UIDocument не найден!");
            enabled = false;
            return;
        }

        if (playerStats == null)
        {
            Debug.LogError($"[{gameObject.name}] PlayerControllerStats не назначен!");
            enabled = false;
            return;
        }
    }

    private void InitializeUI()
    {
        _root = uiDocument.rootVisualElement;
        
        if (_root == null)
        {
            Debug.LogError($"[{gameObject.name}] Не удалось получить корневой элемент UI.");
            enabled = false;
            return;
        }

        // Ищем существующий ScrollView или создаем новый
        _scrollView = _root.Q<ScrollView>(scrollViewName);
        if (_scrollView == null)
        {
            Debug.LogWarning($"ScrollView '{scrollViewName}' не найден. Создаю новый.");
            CreateScrollView();
        }
    }

    private void CreateScrollView()
    {
        _scrollView = new ScrollView(ScrollViewMode.Vertical);
        _scrollView.name = scrollViewName;
        _scrollView.style.flexGrow = 1;
        _scrollView.style.height = Length.Percent(100);
        _root.Add(_scrollView);
    }

    #endregion

    #region Slider Generation

    private void GenerateSliders()
    {
        ClearSliders();
        AnalyzeFields();
        
        if (groupByHeaders)
            GenerateGroupedSliders();
        else
            GenerateSimpleSliders();
            
        SynchronizeSlidersFromStats();
        
        Debug.Log($"[AutoStatsSliderController] Сгенерировано {_sliderFieldMap.Count} слайдеров.");
    }

    private void AnalyzeFields()
    {
        _fieldsByCategory.Clear();
        
        var fields = typeof(PlayerControllerStats).GetFields(BindingFlags.Public | BindingFlags.Instance);
        string currentCategory = "General";
        
        foreach (var field in fields)
        {
            // Пропускаем исключенные поля
            if (excludedFields.Contains(field.Name))
                continue;
                
            // Пропускаем поля неподдерживаемых типов
            if (!IsSupportedFieldType(field.FieldType))
                continue;
                
            // Пропускаем readonly поля, если не включены
            if (field.IsInitOnly && !includeReadOnlyProperties)
                continue;

            // Проверяем атрибуты Header для группировки
            if (groupByHeaders)
            {
                var headerAttr = field.GetCustomAttribute<HeaderAttribute>();
                if (headerAttr != null)
                {
                    currentCategory = headerAttr.header;
                }
            }

            if (!_fieldsByCategory.ContainsKey(currentCategory))
                _fieldsByCategory[currentCategory] = new List<FieldInfo>();
                
            _fieldsByCategory[currentCategory].Add(field);
        }
    }

    private bool IsSupportedFieldType(Type fieldType)
    {
        return fieldType == typeof(float) || 
               fieldType == typeof(int) || 
               fieldType == typeof(bool);
    }

    private void GenerateGroupedSliders()
    {
        foreach (var category in _fieldsByCategory)
        {
            // Создаем заголовок категории
            if (category.Value.Count > 0)
            {
                CreateCategoryHeader(category.Key);
                
                // Создаем слайдеры для полей в категории
                foreach (var field in category.Value)
                {
                    CreateSliderForField(field);
                }
                
                // Добавляем разделитель
                CreateSpacer();
            }
        }
    }

    private void GenerateSimpleSliders()
    {
        var allFields = _fieldsByCategory.SelectMany(kvp => kvp.Value).ToList();
        
        foreach (var field in allFields)
        {
            CreateSliderForField(field);
        }
    }

    private void CreateCategoryHeader(string categoryName)
    {
        var headerContainer = new VisualElement();
        headerContainer.style.height = headerHeight;
        headerContainer.style.justifyContent = Justify.Center;
        headerContainer.style.alignItems = Align.Center;
        headerContainer.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        headerContainer.style.marginBottom = spacing;
        headerContainer.style.marginTop = spacing;
        headerContainer.style.borderBottomWidth = 2;
        headerContainer.style.borderBottomColor = headerColor;

        var headerLabel = new Label(categoryName);
        headerLabel.style.color = headerColor;
        headerLabel.style.fontSize = 16;
        headerLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        
        headerContainer.Add(headerLabel);
        _scrollView.Add(headerContainer);
    }

    private void CreateSliderForField(FieldInfo field)
    {
        var container = new VisualElement();
        container.style.height = sliderHeight;
        container.style.flexDirection = FlexDirection.Row;
        container.style.alignItems = Align.Center;
        container.style.marginBottom = spacing;
        container.style.paddingLeft = 10;
        container.style.paddingRight = 10;


        // Создаем лейбл
        var label = new Label(GetFieldDisplayName(field));
        label.style.color = sliderLabelColor;
        label.style.width = Length.Percent(50);
        label.style.minWidth = 120;
        label.style.unityTextAlign = TextAnchor.MiddleLeft;
        
        container.Add(label);

        // Создаем слайдер в зависимости от типа поля
        VisualElement controlElement = null;
        
        if (field.FieldType == typeof(float))
        {
            controlElement = CreateFloatSlider(field);
        }
        else if (field.FieldType == typeof(int))
        {
            controlElement = CreateIntSlider(field);
        }
        else if (field.FieldType == typeof(bool))
        {
            controlElement = CreateBoolToggle(field);
        }

        if (controlElement != null)
        {
            controlElement.style.width = Length.Percent(50);
            container.Add(controlElement);

            // Создаем поле для отображения значения
            // var valueLabel = new Label();
            // valueLabel.style.width = Length.Percent(20);
            // valueLabel.style.minWidth = 60;
            // valueLabel.style.unityTextAlign = TextAnchor.MiddleRight;
            // valueLabel.style.color = Color.white;
            // valueLabel.name = $"value_{field.Name}";
            //
            // container.Add(valueLabel);
        }

        _scrollView.Add(container);
    }

    private Slider CreateFloatSlider(FieldInfo field)
    {
        var rangeAttr = field.GetCustomAttribute<RangeAttribute>();
        float minValue = rangeAttr?.min ?? defaultMinValue;
        float maxValue = rangeAttr?.max ?? defaultMaxValue;

        var slider = new Slider(minValue, maxValue);
        slider.name = $"slider_{field.Name}";
        slider.showInputField = true;
        
        _sliderFieldMap[slider] = field;
        slider.RegisterValueChangedCallback(evt => OnSliderValueChanged(evt, field));
        
        return slider;
    }

    private SliderInt CreateIntSlider(FieldInfo field)
    {
        var rangeAttr = field.GetCustomAttribute<RangeAttribute>();
        int minValue = (int)(rangeAttr?.min ?? defaultMinValue);
        int maxValue = (int)(rangeAttr?.max ?? defaultMaxValue);

        var slider = new SliderInt(minValue, maxValue);
        slider.name = $"slider_{field.Name}";
        
        slider.RegisterValueChangedCallback(evt => OnIntSliderValueChanged(evt, field));
        
        return slider;
    }

    private Toggle CreateBoolToggle(FieldInfo field)
    {
        var toggle = new Toggle();
        toggle.name = $"toggle_{field.Name}";
        
        toggle.RegisterValueChangedCallback(evt => OnToggleValueChanged(evt, field));
        
        return toggle;
    }

    private void CreateSpacer()
    {
        var spacer = new VisualElement();
        spacer.style.height = spacing * 2;
        _scrollView.Add(spacer);
    }

    private string GetFieldDisplayName(FieldInfo field)
    {
        // Преобразуем camelCase в читаемый вид
        string displayName = field.Name;
        
        // Добавляем пробелы перед заглавными буквами
        for (int i = displayName.Length - 1; i > 0; i--)
        {
            if (char.IsUpper(displayName[i]) && !char.IsUpper(displayName[i - 1]))
            {
                displayName = displayName.Insert(i, " ");
            }
        }
        
        return displayName;
    }

    #endregion

    #region Event Handlers

    private void OnSliderValueChanged(ChangeEvent<float> evt, FieldInfo field)
    {
        if (_isInitializing) return;

        field.SetValue(playerStats, evt.newValue);
        UpdateValueLabel(field.Name, evt.newValue.ToString("F2"));
        
        Debug.Log($"📊 {field.Name}: {evt.previousValue:F2} → {evt.newValue:F2}");
    }

    private void OnIntSliderValueChanged(ChangeEvent<int> evt, FieldInfo field)
    {
        if (_isInitializing) return;

        field.SetValue(playerStats, evt.newValue);
        UpdateValueLabel(field.Name, evt.newValue.ToString());
        
        Debug.Log($"📊 {field.Name}: {evt.previousValue} → {evt.newValue}");
    }

    private void OnToggleValueChanged(ChangeEvent<bool> evt, FieldInfo field)
    {
        if (_isInitializing) return;

        field.SetValue(playerStats, evt.newValue);
        UpdateValueLabel(field.Name, evt.newValue.ToString());
        
        Debug.Log($"📊 {field.Name}: {evt.previousValue} → {evt.newValue}");
    }

    private void UpdateValueLabel(string fieldName, string value)
    {
        var valueLabel = _scrollView.Q<Label>($"value_{fieldName}");
        if (valueLabel != null)
        {
            valueLabel.text = value;
        }
    }

    #endregion

    #region Synchronization

    private void SynchronizeSlidersFromStats()
    {
        _isInitializing = true;

        // Синхронизируем float слайдеры
        foreach (var kvp in _sliderFieldMap)
        {
            var slider = kvp.Key;
            var field = kvp.Value;
            
            var value = field.GetValue(playerStats);
            slider.value = Convert.ToSingle(value);
            UpdateValueLabel(field.Name, value.ToString());
        }

        // Синхронизируем int слайдеры
        var intSliders = _scrollView.Query<SliderInt>().ToList();
        foreach (var slider in intSliders)
        {
            var fieldName = slider.name.Replace("slider_", "");
            var field = typeof(PlayerControllerStats).GetField(fieldName);
            if (field != null)
            {
                var value = (int)field.GetValue(playerStats);
                slider.value = value;
                UpdateValueLabel(fieldName, value.ToString());
            }
        }

        // Синхронизируем toggles
        var toggles = _scrollView.Query<Toggle>().ToList();
        foreach (var toggle in toggles)
        {
            var fieldName = toggle.name.Replace("toggle_", "");
            var field = typeof(PlayerControllerStats).GetField(fieldName);
            if (field != null)
            {
                var value = (bool)field.GetValue(playerStats);
                toggle.value = value;
                UpdateValueLabel(fieldName, value.ToString());
            }
        }

        _isInitializing = false;
        Debug.Log("[AutoStatsSliderController] Синхронизация завершена.");
    }

    private void ClearSliders()
    {
        if (_scrollView != null)
        {
            _scrollView.Clear();
        }
        _sliderFieldMap.Clear();
    }

    #endregion

    #region Public API

    /// <summary>
    /// Принудительно пересоздает все слайдеры
    /// </summary>
    [ContextMenu("Пересоздать слайдеры")]
    public void RegenerateSliders()
    {
        if (!enabled) return;
        GenerateSliders();
    }

    /// <summary>
    /// Принудительно синхронизирует все слайдеры со статами
    /// </summary>
    [ContextMenu("Синхронизировать слайдеры")]
    public void ForceSynchronizeSliders()
    {
        if (!enabled) return;
        SynchronizeSlidersFromStats();
    }

    /// <summary>
    /// Переключает группировку по заголовкам
    /// </summary>
    public void ToggleGrouping()
    {
        groupByHeaders = !groupByHeaders;
        RegenerateSliders();
    }

    #endregion

    #region Editor Helpers

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Обновляем слайдеры при изменении настроек в инспекторе
        if (Application.isPlaying && enabled)
        {
            UnityEditor.EditorApplication.delayCall += () => 
            {
                if (this != null) RegenerateSliders();
            };
        }
    }
#endif

    #endregion
}