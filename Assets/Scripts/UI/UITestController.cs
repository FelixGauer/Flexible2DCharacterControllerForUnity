using UnityEngine;
using UnityEngine.UIElements;

public class UITestController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private PlayerControllerStats playerControllerStats;
    
    private VisualElement _root;

    // Корневые контейнеры для разных групп слайдеров
    private VisualElement[] _sliderContainers;
    private readonly string[] _sliderNames = { "MySlider", "MySlider2", "MySlider3" };
    

    private void Awake()
    {
        // Получаем корневой элемент из UIDocument
        _root = uiDocument.rootVisualElement;

        // Ищем контейнеры по их именам в UXML
        _sliderContainers = new[]
        {
            _root.Q<VisualElement>("MovementRoot"),
            _root.Q<VisualElement>("JumpRoot")
        };
    }

    private void OnEnable()
    {
        // // Регистрируем обработчики изменений для всех слайдеров в указанных контейнерах
        // foreach (var container in _sliderContainers)
        // {
        //     if (container == null)
        //     {
        //         Debug.LogWarning("Один из контейнеров не найден в дереве UI.");
        //         continue;
        //     }
        //
        //     foreach (var name in _sliderNames)
        //     {
        //         var slider = container.Q<Slider>(name);
        //         if (slider != null)
        //         {
        //             slider.RegisterValueChangedCallback(OnSliderChanged);
        //         }
        //         else
        //         {
        //             Debug.LogWarning($"Slider с name='{name}' не найден в контейнере '{container.name}'.");
        //         }
        //     }
        // }
        


        var slider = _sliderContainers[0].Q<Slider>("MySlider");
        slider.RegisterValueChangedCallback(OnSliderChanged);
        

    }
    
    private void OnDisable()
    {
        // Отписываемся при отключении компонента
        // foreach (var container in _sliderContainers)
        // {
        //     if (container == null)
        //         continue;
        //
        //     foreach (var name in _sliderNames)
        //     {
        //         var slider = container.Q<Slider>(name);
        //         if (slider != null)
        //             slider.UnregisterValueChangedCallback(OnSliderChanged);
        //     }
        // }
        
        var slider = _sliderContainers[0].Q<Slider>("MySlider");
        slider.UnregisterValueChangedCallback(OnSliderChanged);
    }

    private void OnSliderChanged(ChangeEvent<float> evt)
    {
        var slider = (Slider)evt.currentTarget;
    
        playerControllerStats.MoveSpeed = evt.newValue;
        Debug.Log($"[{slider.parent.name}] Slider '{slider.name}' changed to {evt.newValue:F2}");
    }
}


