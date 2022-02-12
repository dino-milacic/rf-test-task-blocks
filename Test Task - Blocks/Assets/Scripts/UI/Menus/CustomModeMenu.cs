using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RFTestTaskBlocks.UI
{
    public class CustomModeMenu : BaseUIPanel
    {
        [SerializeField] private Button _startButton;
        [SerializeField] private SliderOption _sceneSize;
        [SerializeField] private SliderOption _numberOfRobots;
        [SerializeField] private SliderOption _numberOfBlocks;
        [SerializeField] private AllowedColorsOption _allowedColors;
        [SerializeField] private Transform _containerOptionBox;
        private Dictionary<BlockColor, ContainerOption> _containerOptions;

        private SceneConfiguration _sceneConfig;
        
        private void Awake()
        {
            _sceneConfig = new SceneConfiguration();
            _containerOptions = new Dictionary<BlockColor, ContainerOption>();
            
            _sceneSize.OnValueChanged.RemoveListener(SceneSizeChanged);
            _sceneSize.OnValueChanged.AddListener(SceneSizeChanged);
            
            _numberOfRobots.OnValueChanged.RemoveListener(NumberOfRobotsChanged);
            _numberOfRobots.OnValueChanged.AddListener(NumberOfRobotsChanged);
            
            _numberOfBlocks.OnValueChanged.RemoveListener(NumberOfBlocksChanged);
            _numberOfBlocks.OnValueChanged.AddListener(NumberOfBlocksChanged);
            
            _allowedColors.OnValueChanged.RemoveListener(AllowedColorsChanged);
            _allowedColors.OnValueChanged.AddListener(AllowedColorsChanged);
            
            BlockColor[] colors = (BlockColor[]) Enum.GetValues(typeof(BlockColor));
            for (var i = 0; i < colors.Length; i++)
            {
                BlockColor color = colors[i];
                float position = colors.Length <= 1? 0f : i / (float) (colors.Length - 1);
                CreateContainerOption(color, position);
            }

            ValidateScene();
        }

        private void AllowedColorsChanged(List<BlockColor> allowedColors)
        {
            _sceneConfig.AllowedColors = allowedColors;
            _sceneConfig.ContainerData.Clear();
            
            foreach (var containerOption in _containerOptions)
            {
                BlockColor color = containerOption.Key;
                ContainerOption option = containerOption.Value;
                bool isActive = allowedColors.Contains(color);
                option.gameObject.SetActive(isActive);
                if (isActive)
                {
                    int index = allowedColors.IndexOf(color);
                    float position = allowedColors.Count <= 1 ? 0f : index / (float) (allowedColors.Count - 1);
                    option.SetDefaultPosition(position);
                    _sceneConfig.ContainerData[color] = option.Value;
                }
            }

            ValidateScene();
        }

        private void CreateContainerOption(BlockColor color, float position)
        {
            Services.Get<IAssetManager>().InstantiatePrefab<ContainerOption>("UI.Element.ContainerOption", x => OnContainerOptionLoaded(x, color, position), _containerOptionBox);
        }

        private void OnContainerOptionLoaded(ContainerOption option, BlockColor color, float position)
        {
            _containerOptions[color] = option;
            option.Initialize(color, position);
            option.OnValueChanged.RemoveListener(ContainerOptionChanged);
            option.OnValueChanged.AddListener(ContainerOptionChanged);
        }

        private void ContainerOptionChanged(ContainerOption option)
        {
            _sceneConfig.ContainerData[option.Value.Color] = option.Value;
        }

        private void SceneSizeChanged(int newSize)
        {
            _sceneConfig.CalculateSize(newSize);
            ValidateScene();
        }

        private void NumberOfBlocksChanged(int amount)
        {
            _sceneConfig.NumberOfBlocks = amount;
            ValidateScene();
        }

        private void NumberOfRobotsChanged(int amount)
        {
            _sceneConfig.NumberOfRobots = amount;
            ValidateScene();
        }

        private void ValidateScene()
        {
            _startButton.interactable = _sceneConfig.IsValid;
        }

        public void Back()
        {
            Services.Get<IUIManager>().ShowMenu<MainMenu>();
            Close();
        }

        public void StartGame()
        {
            ISceneManager sceneManager = Services.Get<ISceneManager>();
            
            sceneManager.Configure(_sceneConfig);

            foreach (var data in _sceneConfig.ContainerData)
            {
                sceneManager.SpawnContainer(data.Value.SpawnPosition, data.Value.Color, data.Value.Orientation);
            }
            
            for (int i = 0; i < _sceneConfig.NumberOfRobots; i++)
            {
                sceneManager.SpawnRobot(i);
            }

            for (int i = 0; i < _sceneConfig.NumberOfBlocks; i++)
            {
                sceneManager.SpawnBlock();
            }
            
            Services.Get<IUIManager>().ShowMenu<HUD>();
            Close();
        }
    }
}