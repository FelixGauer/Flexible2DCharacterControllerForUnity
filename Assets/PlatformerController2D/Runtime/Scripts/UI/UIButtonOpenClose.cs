using UnityEngine;
using UnityEngine.UIElements;

namespace PlatformerController2D.Runtime.Scripts.UI
{
    public class UIButtonOpenClose : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;

        private VisualElement _container;
        private ScrollView _scrollView;

        private Button _openButton;
    
        private bool _opened = true;
    
        private void Awake()
        {
            var root = uiDocument.rootVisualElement;
        
            _container = root.Q<VisualElement>("StatsScrollView");
            _scrollView = root.Q<ScrollView>("StatsScrollView");

            _openButton = root.Q<Button>("ButtonPanel");
        
            _openButton.RegisterCallback<ClickEvent>(OnClick);
        }

        private void OnClick(ClickEvent evt)
        {
            if (_opened)
            {
                _opened = false;
                _openButton.text = "Open";
                _scrollView.style.display = DisplayStyle.None;
            
                // _scrollView.RemoveFromClassList("bottomsheet--up");

            }
            else
            {
                _openButton.text = "Close";
                _opened = true;
                _scrollView.style.display = DisplayStyle.Flex;
            
                // _scrollView.AddToClassList("bottomsheet--up");

            }
        }
    }
}