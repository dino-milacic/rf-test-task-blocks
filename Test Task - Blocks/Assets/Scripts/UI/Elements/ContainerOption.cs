using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RFTestTaskBlocks.UI
{
    public class ContainerOption : MonoBehaviour
    {
        [SerializeField] private TMP_Text _positionLabel;
        [SerializeField] private Slider _positionSlider;
        [SerializeField] private TMP_Dropdown _orientationDropdown;
        [SerializeField] private Image _color;

        // public float Position => _positionSlider.value;
        //
        // public ContainerController.ContainerOrientation Orientation
        // {
        //     get
        //     {
        //         ContainerController.ContainerOrientation[] directions =
        //             (ContainerController.ContainerOrientation[]) Enum.GetValues(
        //                 typeof(ContainerController.ContainerOrientation));
        //     
        //         return directions[_orientationDropdown.value];
        //     }
        // }

        private ContainerConfiguration _value = new ContainerConfiguration();
        public ContainerConfiguration Value => _value;

        [Serializable] public class ContainerOptionEvent : UnityEvent<ContainerOption>{}

        [SerializeField] private ContainerOptionEvent _onValueChanged = new ContainerOptionEvent();
        public ContainerOptionEvent OnValueChanged 
        { 
            get => _onValueChanged;
            set => _onValueChanged = value;
        }

        private void Awake()
        {
            _positionSlider.onValueChanged.RemoveListener(HandlePositionChanged);
            _positionSlider.onValueChanged.AddListener(HandlePositionChanged);
            SetPositionLabel(0f);
            
            _orientationDropdown.onValueChanged.RemoveListener(HandleOrientationChanged);
            _orientationDropdown.onValueChanged.AddListener(HandleOrientationChanged);
        }

        public void Initialize(BlockColor color, float position)
        {
            _value.Color = color;
            _color.color = color.BlockColorToColor();

            _value.SpawnPosition = position;
            SetPositionLabel(position);
            _positionSlider.value = position;
            
            gameObject.SetActive(false);
        }

        private void HandleOrientationChanged(int newOrientation)
        {
            Services.Get<ISoundManager>().PlaySFX(SoundAddress.TinyButton);

            _value.Orientation = newOrientation == 0
                ? ContainerController.ContainerOrientation.Left
                : ContainerController.ContainerOrientation.Right;
            
            OnValueChanged.Invoke(this);
        }

        private void HandlePositionChanged(float newValue)
        {
            SetPositionLabel(newValue);
            _value.SpawnPosition = newValue;
            OnValueChanged.Invoke(this);
        }

        private void SetPositionLabel(float newValue)
        {
            _positionLabel.text = $"{Mathf.Round(newValue * 100):F0}%";
        }

        public void SetDefaultPosition(float position)
        {
            _value.SpawnPosition = position;
            SetPositionLabel(position);
            _positionSlider.value = position;
        }
    }
}