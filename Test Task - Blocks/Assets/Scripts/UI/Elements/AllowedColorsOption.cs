using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RFTestTaskBlocks.UI
{
    public class AllowedColorsOption : MonoBehaviour
    {
        [SerializeField] private ToggleGroup _group;
        [SerializeField] private List<ColorToggleElement> _colors;
        private Transform _groupTransform;


        [Serializable] public class ContainerOptionEvent : UnityEvent<List<BlockColor>>{}

        [SerializeField] private ContainerOptionEvent _onValueChanged = new ContainerOptionEvent();
        public ContainerOptionEvent OnValueChanged 
        { 
            get => _onValueChanged;
            set => _onValueChanged = value;
        }


        private void Awake()
        {
            if (_group != null) _groupTransform = _group.transform;
            
            BlockColor[] colors = (BlockColor[]) Enum.GetValues(typeof(BlockColor));
            for (var i = 0; i < colors.Length; i++)
            {
                BlockColor color = colors[i];
                CreateColorToggles(color);
            }
        }

        private void CreateColorToggles(BlockColor color)
        {
            Services.Get<IAssetManager>().InstantiatePrefab<ColorToggleElement>("UI.Element.ColorToggle", x => OnColorToggleLoaded(x, color), _groupTransform);
        }

        private void OnColorToggleLoaded(ColorToggleElement element, BlockColor color)
        {
            _colors.Add(element);
            element.Initialize(color);
            element.Toggle.onValueChanged.RemoveListener(HandleToggleChanged);
            element.Toggle.onValueChanged.AddListener(HandleToggleChanged);
        }

        private void HandleToggleChanged(bool isOn)
        {
            OnValueChanged.Invoke(_colors.Where(x => x.Toggle.isOn).Select(c => c.Color).ToList());
        }
    }
}