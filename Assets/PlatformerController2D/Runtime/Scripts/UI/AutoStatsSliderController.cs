using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PlatformerController2D.Runtime.Scripts.Player;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlatformerController2D.Runtime.Scripts.UI
{
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
        [SerializeField] private int vector2Height = 60; // Высота для Vector2 контейнера
        [SerializeField] private int headerHeight = 40;
        [SerializeField] private int spacing = 5;

        private VisualElement _root;
        private ScrollView _scrollView;
        private readonly Dictionary<Slider, FieldInfo> _sliderFieldMap = new Dictionary<Slider, FieldInfo>();
        private readonly Dictionary<string, Vector2SliderGroup> _vector2SliderGroups = new Dictionary<string, Vector2SliderGroup>();
        private readonly Dictionary<string, List<FieldInfo>> _fieldsByCategory = new Dictionary<string, List<FieldInfo>>();
        private bool _isInitializing = false;

        #region Helper Classes

        /// <summary>
        /// Класс для управления парой слайдеров Vector2
        /// </summary>
        private class Vector2SliderGroup
        {
            public FieldInfo Field { get; set; }
            public Slider XSlider { get; set; }
            public Slider YSlider { get; set; }
            public Label ValueLabel { get; set; }
        
            public Vector2 GetValue()
            {
                return new Vector2(XSlider.value, YSlider.value);
            }
        
            public void SetValue(Vector2 value)
            {
                XSlider.value = value.x;
                YSlider.value = value.y;
                UpdateValueLabel();
            }
        
            public void UpdateValueLabel()
            {
                if (ValueLabel != null)
                {
                    var value = GetValue();
                    ValueLabel.text = $"({value.x:F2}, {value.y:F2})";
                }
            }
        }

        #endregion

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
                // Debug.LogError($"[{gameObject.name}] UIDocument не найден!");
                enabled = false;
                return;
            }

            if (playerStats == null)
            {
                // Debug.LogError($"[{gameObject.name}] PlayerControllerStats не назначен!");
                enabled = false;
                return;
            }
        }

        private void InitializeUI()
        {
            _root = uiDocument.rootVisualElement;
        
            if (_root == null)
            {
                // Debug.LogError($"[{gameObject.name}] Не удалось получить корневой элемент UI.");
                enabled = false;
                return;
            }

            // Ищем существующий ScrollView или создаем новый
            _scrollView = _root.Q<ScrollView>(scrollViewName);
            if (_scrollView == null)
            {
                // Debug.LogWarning($"ScrollView '{scrollViewName}' не найден. Создаю новый.");
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
        
            // Debug.Log($"[AutoStatsSliderController] Сгенерировано {_sliderFieldMap.Count} слайдеров и {_vector2SliderGroups.Count} Vector2 групп.");
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
                   fieldType == typeof(bool) ||
                   fieldType == typeof(Vector2);
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
            if (field.FieldType == typeof(Vector2))
            {
                CreateVector2Slider(field);
            }
            else
            {
                CreateSingleValueSlider(field);
            }
        }

        private void CreateSingleValueSlider(FieldInfo field)
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
            }

            _scrollView.Add(container);
        }

        private void CreateVector2Slider(FieldInfo field)
        {
            var mainContainer = new VisualElement();
            mainContainer.style.height = vector2Height;
            mainContainer.style.flexDirection = FlexDirection.Column;
            mainContainer.style.marginBottom = spacing;
            mainContainer.style.paddingLeft = 10;
            mainContainer.style.paddingRight = 10;
            mainContainer.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.3f);
            // mainContainer.style.borderRadius = 5;

            // Заголовок Vector2
            var headerContainer = new VisualElement();
            headerContainer.style.height = 25;
            headerContainer.style.flexDirection = FlexDirection.Row;
            headerContainer.style.alignItems = Align.Center;
            headerContainer.style.justifyContent = Justify.SpaceBetween;
        
            var fieldLabel = new Label(GetFieldDisplayName(field));
            fieldLabel.style.color = sliderLabelColor;
            fieldLabel.style.fontSize = 12;
            fieldLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        
            var valueLabel = new Label();
            valueLabel.style.color = Color.white;
            valueLabel.style.fontSize = 10;
            valueLabel.name = $"vector2_value_{field.Name}";
        
            headerContainer.Add(fieldLabel);
            headerContainer.Add(valueLabel);
            mainContainer.Add(headerContainer);

            // Контейнер для слайдеров
            var slidersContainer = new VisualElement();
            slidersContainer.style.flexGrow = 1;
            slidersContainer.style.flexDirection = FlexDirection.Column;
            slidersContainer.style.justifyContent = Justify.SpaceAround;

            // Получаем диапазон значений
            var rangeAttr = field.GetCustomAttribute<RangeAttribute>();
            float minValue = rangeAttr?.min ?? defaultMinValue;
            float maxValue = rangeAttr?.max ?? defaultMaxValue;

            // X слайдер
            var xContainer = new VisualElement();
            xContainer.style.flexDirection = FlexDirection.Row;
            xContainer.style.alignItems = Align.Center;
            xContainer.style.height = 15;
        
            var xLabel = new Label("X:");
            xLabel.style.width = 20;
            xLabel.style.color = Color.white;
            xLabel.style.fontSize = 10;
        
            var xSlider = new Slider(minValue, maxValue);
            xSlider.name = $"vector2_x_{field.Name}";
            xSlider.style.flexGrow = 1;
            xSlider.showInputField = false;
        
            xContainer.Add(xLabel);
            xContainer.Add(xSlider);
            slidersContainer.Add(xContainer);

            // Y слайдер
            var yContainer = new VisualElement();
            yContainer.style.flexDirection = FlexDirection.Row;
            yContainer.style.alignItems = Align.Center;
            yContainer.style.height = 15;
        
            var yLabel = new Label("Y:");
            yLabel.style.width = 20;
            yLabel.style.color = Color.white;
            yLabel.style.fontSize = 10;
        
            var ySlider = new Slider(minValue, maxValue);
            ySlider.name = $"vector2_y_{field.Name}";
            ySlider.style.flexGrow = 1;
            ySlider.showInputField = false;
        
            yContainer.Add(yLabel);
            yContainer.Add(ySlider);
            slidersContainer.Add(yContainer);

            mainContainer.Add(slidersContainer);

            // Создаем группу для управления Vector2
            var sliderGroup = new Vector2SliderGroup
            {
                Field = field,
                XSlider = xSlider,
                YSlider = ySlider,
                ValueLabel = valueLabel
            };

            _vector2SliderGroups[field.Name] = sliderGroup;

            // Подписываемся на изменения
            xSlider.RegisterValueChangedCallback(evt => OnVector2ValueChanged(sliderGroup));
            ySlider.RegisterValueChangedCallback(evt => OnVector2ValueChanged(sliderGroup));

            _scrollView.Add(mainContainer);
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
        
            Debug.Log($"📊 {field.Name}: {evt.previousValue:F2} → {evt.newValue:F2}");
        }

        private void OnIntSliderValueChanged(ChangeEvent<int> evt, FieldInfo field)
        {
            if (_isInitializing) return;

            field.SetValue(playerStats, evt.newValue);
        
            Debug.Log($"📊 {field.Name}: {evt.previousValue} → {evt.newValue}");
        }

        private void OnToggleValueChanged(ChangeEvent<bool> evt, FieldInfo field)
        {
            if (_isInitializing) return;

            field.SetValue(playerStats, evt.newValue);
        
            Debug.Log($"📊 {field.Name}: {evt.previousValue} → {evt.newValue}");
        }

        private void OnVector2ValueChanged(Vector2SliderGroup sliderGroup)
        {
            if (_isInitializing) return;

            var newValue = sliderGroup.GetValue();
            sliderGroup.Field.SetValue(playerStats, newValue);
            sliderGroup.UpdateValueLabel();
        
            Debug.Log($"📊 {sliderGroup.Field.Name}: → ({newValue.x:F2}, {newValue.y:F2})");
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
                }
            }

            // Синхронизируем Vector2 слайдеры
            foreach (var kvp in _vector2SliderGroups)
            {
                var sliderGroup = kvp.Value;
                var value = (Vector2)sliderGroup.Field.GetValue(playerStats);
                sliderGroup.SetValue(value);
            }

            _isInitializing = false;
            // Debug.Log("[AutoStatsSliderController] Синхронизация завершена.");
        }

        private void ClearSliders()
        {
            if (_scrollView != null)
            {
                _scrollView.Clear();
            }
            _sliderFieldMap.Clear();
            _vector2SliderGroups.Clear();
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
}