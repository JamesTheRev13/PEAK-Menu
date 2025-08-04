using UnityEngine;
using UnityEngine.UIElements;
using Zorro.Core.CLI;
using System;

namespace PEAK_Menu.Utils.DebugPages
{
    public abstract class BaseCustomDebugPage : DebugPage
    {
        protected VisualElement _contentContainer;
        protected ScrollView _scrollView;

        protected BaseCustomDebugPage()
        {
            InitializeLayout();
        }

        private void InitializeLayout()
        {
            // Create main container
            _contentContainer = new VisualElement();
            _contentContainer.style.flexGrow = 1;
            // TODO: ensure padding is consistent with PEAK Menu design
            _contentContainer.style.paddingTop = new StyleLength(10);
            _contentContainer.style.paddingBottom = new StyleLength(10);
            _contentContainer.style.paddingLeft = new StyleLength(10);
            _contentContainer.style.paddingRight = new StyleLength(10);

            // Create scroll view
            _scrollView = new ScrollView(ScrollViewMode.Vertical);
            _scrollView.style.flexGrow = 1;
            
            _contentContainer.Add(_scrollView);
            Add(_contentContainer);
            
            BuildContent();
        }

        protected abstract void BuildContent();

        protected Button CreateButton(string text, Action onClicked, string className = null)
        {
            var button = new Button(onClicked) { text = text };
            if (!string.IsNullOrEmpty(className))
            {
                button.AddToClassList(className);
            }
            button.style.marginBottom = 5;
            button.style.height = 25;
            return button;
        }

        protected Toggle CreateToggle(string label, bool value, Action<bool> onValueChanged)
        {
            var toggle = new Toggle(label) { value = value };
            toggle.RegisterValueChangedCallback(evt => onValueChanged?.Invoke(evt.newValue));
            toggle.style.marginBottom = 5;
            return toggle;
        }

        protected Label CreateLabel(string text, string className = null)
        {
            var label = new Label(text);
            if (!string.IsNullOrEmpty(className))
            {
                label.AddToClassList(className);
            }
            label.style.marginBottom = 5;
            return label;
        }

        protected Slider CreateSlider(string label, float value, float min, float max, Action<float> onValueChanged)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.marginBottom = 5;

            var labelElement = new Label(label);
            labelElement.style.width = 100;
            
            var slider = new Slider(min, max) { value = value };
            slider.style.flexGrow = 1;
            slider.RegisterValueChangedCallback(evt => onValueChanged?.Invoke(evt.newValue));
            
            var valueLabel = new Label($"{value:F2}");
            valueLabel.style.width = 50;
            slider.RegisterValueChangedCallback(evt => valueLabel.text = $"{evt.newValue:F2}");

            container.Add(labelElement);
            container.Add(slider);
            container.Add(valueLabel);
            
            _scrollView.Add(container);
            return slider;
        }

        protected VisualElement CreateSection(string title)
        {
            var section = new VisualElement();
            section.style.marginBottom = 10;
            section.style.paddingBottom = 10;
            section.style.borderBottomWidth = 1;
            section.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f, 1f);

            var titleLabel = CreateLabel($"=== {title} ===");
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            section.Add(titleLabel);

            return section;
        }

        protected void AddToConsole(string message)
        {
            var debugHandler = DebugUIHandler.Instance;
            debugHandler?.AddLog($"[PEAK] {message}", "", LogType.Log, true);
        }
    }
}