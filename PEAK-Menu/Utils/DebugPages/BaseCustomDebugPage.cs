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
        protected VisualElement _mainContent;
        
        // Reactive update system
        protected System.Collections.Generic.List<System.Action> _liveUpdateCallbacks;
        private bool _isVisible = false;

        protected BaseCustomDebugPage()
        {
            _liveUpdateCallbacks = new System.Collections.Generic.List<System.Action>();
            InitializeLayout();
        }

        private void InitializeLayout()
        {
            // Create main container with centered content
            _contentContainer = new VisualElement();
            _contentContainer.style.flexGrow = 1;
            _contentContainer.style.paddingTop = 15;
            _contentContainer.style.paddingBottom = 15;
            _contentContainer.style.paddingLeft = 20;
            _contentContainer.style.paddingRight = 20;
            _contentContainer.style.alignItems = Align.Center;

            // Create centered main content container with max width
            _mainContent = new VisualElement();
            _mainContent.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
            _mainContent.style.maxWidth = 800;
            _mainContent.style.minWidth = 400;

            // Create scroll view
            _scrollView = new ScrollView(ScrollViewMode.Vertical);
            _scrollView.style.flexGrow = 1;
            
            _mainContent.Add(_scrollView);
            _contentContainer.Add(_mainContent);
            Add(_contentContainer);
            
            // Set up reactive updates when this page becomes visible
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
            
            BuildContent();
        }

        private void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            _isVisible = true;
            StartLiveUpdates();
        }

        private void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            _isVisible = false;
            StopLiveUpdates();
        }

        protected abstract void BuildContent();

        // Live reactive label that updates based on a function
        protected Label CreateLiveLabel(string prefix, System.Func<string> valueGetter, string className = null)
        {
            var label = CreateLabel($"{prefix}{valueGetter()}", className);
            
            // Register for live updates
            _liveUpdateCallbacks.Add(() => {
                if (_isVisible && label.parent != null) // Only update if visible and still in DOM
                {
                    try
                    {
                        label.text = $"{prefix}{valueGetter()}";
                    }
                    catch (System.Exception ex)
                    {
                        // Handle cases where the data source becomes invalid
                        label.text = $"{prefix}[Error: {ex.Message}]";
                    }
                }
            });

            return label;
        }

        // Live reactive toggle that syncs with actual game state
        protected Toggle CreateLiveToggle(string label, System.Func<bool> stateGetter, System.Action<bool> stateSetter)
        {
            var toggle = new Toggle(label) { value = stateGetter() };
            
            // Set up value change handler
            toggle.RegisterValueChangedCallback(evt => {
                try
                {
                    stateSetter(evt.newValue);
                }
                catch (System.Exception ex)
                {
                    AddToConsole($"[ERROR] Toggle action failed: {ex.Message}");
                    // Revert toggle state on error
                    toggle.SetValueWithoutNotify(stateGetter());
                }
            });

            // Register for live state sync
            _liveUpdateCallbacks.Add(() => {
                if (_isVisible && toggle.parent != null)
                {
                    try
                    {
                        var currentState = stateGetter();
                        if (toggle.value != currentState)
                        {
                            toggle.SetValueWithoutNotify(currentState);
                        }
                    }
                    catch (System.Exception)
                    {
                        // Silently handle errors in state reading
                    }
                }
            });

            toggle.style.marginBottom = 5;
            toggle.style.color = Color.white;
            toggle.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.3f);
            toggle.style.paddingLeft = 5;
            toggle.style.paddingRight = 5;
            toggle.style.paddingTop = 2;
            toggle.style.paddingBottom = 2;
            toggle.style.borderTopLeftRadius = 3;
            toggle.style.borderTopRightRadius = 3;
            toggle.style.borderBottomLeftRadius = 3;
            toggle.style.borderBottomRightRadius = 3;

            return toggle;
        }

        // Live reactive slider that syncs with config values
        protected Slider CreateLiveSlider(string label, System.Func<float> valueGetter, System.Action<float> valueSetter, 
            float min, float max, VisualElement targetSection)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.marginBottom = 8;
            container.style.alignItems = Align.Center;

            var labelElement = new Label(label);
            labelElement.style.width = 120;
            labelElement.style.color = Color.white;
            
            var slider = new Slider(min, max) { value = valueGetter() };
            slider.style.flexGrow = 1;
            slider.style.marginLeft = 10;
            slider.style.marginRight = 10;

            var valueLabel = new Label($"{valueGetter():F2}");
            valueLabel.style.width = 50;
            valueLabel.style.color = Color.white;

            // Set up value change handler
            slider.RegisterValueChangedCallback(evt => {
                try
                {
                    valueSetter(evt.newValue);
                    valueLabel.text = $"{evt.newValue:F2}";
                }
                catch (System.Exception ex)
                {
                    AddToConsole($"[ERROR] Slider action failed: {ex.Message}");
                    // Revert slider on error
                    slider.SetValueWithoutNotify(valueGetter());
                    valueLabel.text = $"{valueGetter():F2}";
                }
            });

            // Register for live state sync
            _liveUpdateCallbacks.Add(() => {
                if (_isVisible && slider.parent != null)
                {
                    try
                    {
                        var currentValue = valueGetter();
                        if (Mathf.Abs(slider.value - currentValue) > 0.01f) // Avoid micro-updates
                        {
                            slider.SetValueWithoutNotify(currentValue);
                            valueLabel.text = $"{currentValue:F2}";
                        }
                    }
                    catch (System.Exception)
                    {
                        // Silently handle errors in value reading
                    }
                }
            });

            container.Add(labelElement);
            container.Add(slider);
            container.Add(valueLabel);
            
            targetSection.Add(container);
            return slider;
        }

        // Live reactive dropdown that syncs with data sources
        protected DropdownField CreateLiveDropdown(string label, System.Func<System.Collections.Generic.List<string>> choicesGetter, 
            int defaultIndex, System.Action<string> onValueChanged)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.marginBottom = 5;
            container.style.alignItems = Align.Center;

            var labelElement = new Label(label);
            labelElement.style.width = 100;
            labelElement.style.color = Color.white;
            
            var dropdown = new DropdownField(choicesGetter(), defaultIndex);
            dropdown.style.flexGrow = 1;
            dropdown.style.maxWidth = 200;
            dropdown.style.color = Color.white;
            dropdown.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            dropdown.style.borderTopWidth = 1;
            dropdown.style.borderBottomWidth = 1;
            dropdown.style.borderLeftWidth = 1;
            dropdown.style.borderRightWidth = 1;
            dropdown.style.borderTopColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            dropdown.style.borderBottomColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            dropdown.style.borderLeftColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            dropdown.style.borderRightColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            dropdown.style.borderTopLeftRadius = 3;
            dropdown.style.borderTopRightRadius = 3;
            dropdown.style.borderBottomLeftRadius = 3;
            dropdown.style.borderBottomRightRadius = 3;

            dropdown.RegisterValueChangedCallback(evt => {
                try
                {
                    onValueChanged?.Invoke(evt.newValue);
                }
                catch (System.Exception ex)
                {
                    AddToConsole($"[ERROR] Dropdown action failed: {ex.Message}");
                }
            });

            // Register for live choices update
            _liveUpdateCallbacks.Add(() => {
                if (_isVisible && dropdown.parent != null)
                {
                    try
                    {
                        var newChoices = choicesGetter();
                        if (newChoices.Count != dropdown.choices.Count || 
                            !System.Linq.Enumerable.SequenceEqual(newChoices, dropdown.choices))
                        {
                            var currentIndex = dropdown.index;
                            dropdown.choices = newChoices;
                            
                            // Try to maintain selection if possible
                            if (currentIndex >= 0 && currentIndex < newChoices.Count)
                            {
                                dropdown.SetValueWithoutNotify(newChoices[currentIndex]);
                            }
                        }
                    }
                    catch (System.Exception)
                    {
                        // Silently handle errors in choices reading
                    }
                }
            });

            container.Add(labelElement);
            container.Add(dropdown);
            
            _scrollView.Add(container);
            return dropdown;
        }

        private void StartLiveUpdates()
        {
            // Use Unity's scheduler for smooth updates without blocking
            schedule.Execute(UpdateLiveElements).Every(16); // ~60 FPS updates
        }

        private void StopLiveUpdates()
        {
            // Stop all scheduled updates
            schedule.Execute(UpdateLiveElements).Pause();
        }

        private void UpdateLiveElements()
        {
            if (!_isVisible) return;

            try
            {
                foreach (var callback in _liveUpdateCallbacks)
                {
                    callback?.Invoke();
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Log?.LogWarning($"Error in live element update: {ex.Message}");
            }
        }

        // Keep existing utility methods
        protected Button CreateButton(string text, Action onClicked, string className = null)
        {
            var button = new Button(onClicked) { text = text };
            
            if (!string.IsNullOrEmpty(className))
            {
                button.AddToClassList(className);
            }

            // Enhanced button styling
            button.style.marginBottom = 5;
            button.style.height = 28;
            button.style.color = Color.white;
            button.style.backgroundColor = new Color(0.388f, 0.388f, 0.388f, 1f);
            button.style.borderTopWidth = 1;
            button.style.borderBottomWidth = 1;
            button.style.borderLeftWidth = 1;
            button.style.borderRightWidth = 1;
            button.style.borderTopColor = new Color(0.553f, 0.553f, 0.553f, 1f);
            button.style.borderBottomColor = new Color(0.180f, 0.180f, 0.180f, 1f);
            button.style.borderLeftColor = new Color(0.553f, 0.553f, 0.553f, 1f);
            button.style.borderRightColor = new Color(0.180f, 0.180f, 0.180f, 1f);
            button.style.borderTopLeftRadius = 3;
            button.style.borderTopRightRadius = 3;
            button.style.borderBottomLeftRadius = 3;
            button.style.borderBottomRightRadius = 3;
            
            // Enhanced button feedback
            button.RegisterCallback<MouseEnterEvent>((evt) =>
            {
                button.style.backgroundColor = new Color(0.463f, 0.463f, 0.463f, 1f);
                button.style.borderTopColor = new Color(0.6f, 0.6f, 0.6f, 1f);
                button.style.borderLeftColor = new Color(0.6f, 0.6f, 0.6f, 1f);
            });
            
            button.RegisterCallback<MouseLeaveEvent>((evt) =>
            {
                button.style.backgroundColor = new Color(0.388f, 0.388f, 0.388f, 1f);
                button.style.borderTopColor = new Color(0.553f, 0.553f, 0.553f, 1f);
                button.style.borderBottomColor = new Color(0.180f, 0.180f, 0.180f, 1f);
                button.style.borderLeftColor = new Color(0.553f, 0.553f, 0.553f, 1f);
                button.style.borderRightColor = new Color(0.180f, 0.180f, 0.180f, 1f);
            });
            
            button.RegisterCallback<MouseDownEvent>((evt) =>
            {
                button.style.backgroundColor = new Color(0.25f, 0.4f, 0.6f, 1f);
                button.style.borderTopColor = new Color(0.1f, 0.1f, 0.1f, 1f);
                button.style.borderBottomColor = new Color(0.7f, 0.7f, 0.7f, 1f);
                button.style.borderLeftColor = new Color(0.1f, 0.1f, 0.1f, 1f);
                button.style.borderRightColor = new Color(0.7f, 0.7f, 0.7f, 1f);
                button.style.scale = new StyleScale(new Scale(new Vector3(0.95f, 0.95f, 1f)));
            });
            
            button.RegisterCallback<MouseUpEvent>((evt) =>
            {
                button.style.backgroundColor = new Color(0.463f, 0.463f, 0.463f, 1f);
                button.style.borderTopColor = new Color(0.6f, 0.6f, 0.6f, 1f);
                button.style.borderBottomColor = new Color(0.180f, 0.180f, 0.180f, 1f);
                button.style.borderLeftColor = new Color(0.6f, 0.6f, 0.6f, 1f);
                button.style.borderRightColor = new Color(0.180f, 0.180f, 0.180f, 1f);
                button.style.scale = new StyleScale(new Scale(Vector3.one));
            });
            
            return button;
        }

        protected Label CreateLabel(string text, string className = null)
        {
            var label = new Label(text);
            if (!string.IsNullOrEmpty(className))
            {
                label.AddToClassList(className);
            }
            label.style.marginBottom = 5;
            label.style.color = Color.white;
            return label;
        }

        protected VisualElement CreateSection(string title)
        {
            var section = new VisualElement();
            section.style.marginBottom = 15;
            section.style.paddingBottom = 10;
            section.style.borderBottomWidth = 1;
            section.style.borderBottomColor = new Color(0.4f, 0.4f, 0.4f, 1f);

            var titleLabel = CreateLabel($"=== {title} ===");
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.fontSize = 14;
            titleLabel.style.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            section.Add(titleLabel);

            return section;
        }

        protected VisualElement CreateRowContainer()
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.marginBottom = 5;
            container.style.flexWrap = Wrap.Wrap;
            return container;
        }

        protected void AddToConsole(string message)
        {
            var debugHandler = DebugUIHandler.Instance;
            debugHandler?.AddLog($"[PEAK] {message}", "", LogType.Log, true);
        }

        // Remove the old UpdateContent method - we're now fully reactive
        public virtual void UpdateContent()
        {
            // Legacy method kept for compatibility
            // All updates are now handled reactively
        }
    }
}