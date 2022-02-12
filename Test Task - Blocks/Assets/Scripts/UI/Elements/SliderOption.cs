using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RFTestTaskBlocks.UI
{
    public class SliderOption : MonoBehaviour
    {
        [SerializeField] private string _baseLabel;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Slider _slider;
        [SerializeField] private float _maxValue;
        [SerializeField] private float _minValue;
        [SerializeField] private int _defaultValue;

        public int Value => (int) _slider.value;
        
        [Serializable] public class SliderEvent : UnityEvent<int> {}

        [SerializeField] private SliderEvent _onValueChanged = new SliderEvent();
        public SliderEvent OnValueChanged 
        { 
            get => _onValueChanged;
            set => _onValueChanged = value;
        }

        private void Awake()
        {
            _slider.maxValue = _maxValue;
            _slider.minValue = _minValue;
            _slider.value = _defaultValue;
            _slider.onValueChanged.RemoveListener(HandleValueChanged);
            _slider.onValueChanged.AddListener(HandleValueChanged);
            SetLabel(_defaultValue);
        }

        private void HandleValueChanged(float newValue)
        {
            SetLabel(newValue);
            OnValueChanged.Invoke((int) newValue);
        }

        private void SetLabel(float newValue)
        {
            _label.text = $"{_baseLabel}: {newValue}";
        }
    }
}