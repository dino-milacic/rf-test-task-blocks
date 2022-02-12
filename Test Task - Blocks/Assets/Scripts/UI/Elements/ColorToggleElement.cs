using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RFTestTaskBlocks.UI
{
    public class ColorToggleElement : MonoBehaviour
    {
        [SerializeField] private Image _background;
        [SerializeField] private Image _toggleImage;
        [SerializeField] private Toggle _toggle;
        public Toggle Toggle => _toggle;

        private BlockColor _color;
        public BlockColor Color => _color;
        public void Initialize(BlockColor color)
        {
            _color = color;
            _background.color = color.BlockColorToColor();
            _toggleImage.color = color.BlockColorToColor();
        }
    }
}