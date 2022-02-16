using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RFTestTaskBlocks
{
    public class SceneController : MonoBehaviour, ISceneManager
    {
        [SerializeField] private Transform _staticDivider;
        [SerializeField] private Transform _robotsDivider;
        [SerializeField] private Transform _blocksDivider;
        
        [SerializeField] private Transform _floor;
        [SerializeField] private Transform _wallLeft;
        [SerializeField] private Transform _wallRight;
        [SerializeField] private Camera _camera;

        private SceneConfiguration _config;

        private List<RobotController> _spawnedRobots = new List<RobotController>();
        private List<ContainerController> _spawnedContainers = new List<ContainerController>();
        private List<BlockController> _spawnedBlocks = new List<BlockController>();

        private Vector2 Offset => new Vector2((_config.SceneBounds.size.x - 1f) / 2f - 0.5f, _config.SceneBounds.size.y / 2f - 1f);

        public Bounds SceneSize => new Bounds(_config.SceneBounds.center + new Vector3(0f, 1f), _config.SceneBounds.size - new Vector3(2f, 1f));

        public int NumberOfRobots => _config.NumberOfRobots;
        public int NumberOfBlocks => _config.NumberOfBlocks;

        private void Awake()
        {
            _config = new SceneConfiguration();
            Services.Register(this);
        }

        private void OnDestroy()
        {
            Services.Unregister(this);
        }

        public void Configure(SceneConfiguration config)
        {
            _config = config;
            SetupSize();
        }

        public void Configure(float sceneSize, int robots, int blocks, List<BlockColor> allowedColors)
        {
            _config.NumberOfRobots = robots;
            _config.NumberOfBlocks = blocks;
            _config.AllowedColors = allowedColors;
            _config.ContainerData = new Dictionary<BlockColor, ContainerConfiguration>();
            _config.CalculateSize(sceneSize);

            SetupSize();
        }

        private void SetupSize()
        {
            _floor.localScale = new Vector3(_config.SceneBounds.size.x, 1f, 1f);
            _floor.position = new Vector3(0f, _config.SceneBounds.min.y + 0.5f, 0f);

            _wallLeft.localScale = new Vector3(1f, _config.SceneBounds.size.y / 2f, 1f);
            _wallLeft.position = new Vector3(_config.SceneBounds.min.x, _config.SceneBounds.min.y + _config.SceneBounds.size.y / 4f, 0f);
            
            _wallRight.localScale = new Vector3(1f, _config.SceneBounds.size.y / 2f, 1f);
            _wallRight.position = new Vector3(_config.SceneBounds.max.x, _config.SceneBounds.min.y + _config.SceneBounds.size.y / 4f, 0f);

            _camera.orthographicSize = _config.SceneBounds.size.y / 2f;
        }

        public void Reset()
        {
            foreach (RobotController robot in _spawnedRobots)
            {
                Destroy(robot.gameObject);
            }
            _spawnedRobots.Clear();
            
            foreach (ContainerController container in _spawnedContainers)
            {
                Destroy(container.gameObject);
            }
            _spawnedContainers.Clear();
            
            foreach (BlockController block in _spawnedBlocks)
            {
                Destroy(block.gameObject);
            }
            _spawnedBlocks.Clear();

            Configure(new SceneConfiguration());
        }

        public ContainerController GetContainerForColor(BlockColor color)
        {
            return _spawnedContainers.FirstOrDefault(x => x.Color == color);
        }

        public Vector3 GetPositionInScene(Vector2 gridPosition)
        {
            Vector2 posInScene = gridPosition - Offset;
            return posInScene;
        }

        public Vector3 GetGroundedPositionInScene(float percentage)
        {
            Vector2 groundPos = new Vector2(SceneSize.size.x * percentage, 0f);
            return GetPositionInScene(groundPos);
        }

        public Vector2 GetSceneGridPosition(float percentage)
        {
            return new Vector2(SceneSize.size.x * percentage, 0f);
        }

        public void SpawnRobot(int index)
        {
            SpawnRobot((index+1)/(float)(NumberOfRobots+1), RobotController.RandomDirection);
        }

        public void SpawnRobot(float position, RobotController.RobotDirection direction, float speedMultiplier = 1)
        {
            Debug.LogFormat("Spawn robot @ {0} facing {1}", position, direction);
            RobotConfiguration config = new RobotConfiguration
            {
                SpawnPosition = position,
                SpeedMultiplier = speedMultiplier,
                StartingDirection = direction,
                VisionRange = SceneSize.size.x
            };
            SpawnRobot(config, x => Debug.LogFormat("Robot was spawned!"));
        }

        private void SpawnRobot(RobotConfiguration config, Action<RobotController> onLoaded)
        {
            Services.Get<IAssetManager>().InstantiatePrefab<RobotController>("Robot", x => OnRobotLoaded(x, config, onLoaded));
        }

        public void SpawnContainer(float position, BlockColor color, ContainerController.ContainerOrientation orientation)
        {
            ContainerConfiguration config = new ContainerConfiguration
            {
                SpawnPosition = position,
                Color = color,
                Orientation = orientation
            };
            SpawnContainer(config, x => Debug.LogFormat("{0} container spawned @ {1} facing {2}!", x.Color, position, orientation));
        }

        private void SpawnContainer(ContainerConfiguration config, Action<ContainerController> onLoaded)
        {
            Services.Get<IAssetManager>().InstantiatePrefab<ContainerController>("Container", x => OnContainerLoaded(x, config, onLoaded));
        }

        public void SpawnBlock()
        {
            BlockConfiguration config = new BlockConfiguration
            {
                AllowedColors = _config.AllowedColors,
                AllowedBounds = SceneSize
            };
            SpawnBlock(config);
        }

        public void SpawnBlock(BlockConfiguration config, Action<BlockController> onLoaded = null)
        {
            Services.Get<IAssetManager>().InstantiatePrefab<BlockController>("Block", x => OnBlockLoaded(x, config, onLoaded));
        }

        private void OnRobotLoaded(RobotController robot, RobotConfiguration config, Action<RobotController> onLoaded)
        {
            _spawnedRobots.Add(robot);
            PositionAfterDivider(robot.Transform, SceneHierarchyDivider.Robots);
            robot.Initialize(config);
            onLoaded?.Invoke(robot);
        }

        private void OnContainerLoaded(ContainerController container, ContainerConfiguration config, Action<ContainerController> onLoaded)
        {
            _spawnedContainers.Add(container);
            PositionAfterDivider(container.Transform, SceneHierarchyDivider.Static);
            container.Initialize(config);
            onLoaded?.Invoke(container);
        }

        private void OnBlockLoaded(BlockController block, BlockConfiguration config, Action<BlockController> onLoaded)
        {
            _spawnedBlocks.Add(block);
            PositionAfterDivider(block.Transform, SceneHierarchyDivider.Blocks);
            block.Initialize(config);
            onLoaded?.Invoke(block);
        }

        private void PositionAfterDivider(Transform targetObject, SceneHierarchyDivider divider)
        {
            if (targetObject == null) return;
            
            Transform chosenDivider = null;

            switch (divider)
            {
                case SceneHierarchyDivider.Static: chosenDivider = _staticDivider; break;
                case SceneHierarchyDivider.Robots: chosenDivider = _robotsDivider; break;
                case SceneHierarchyDivider.Blocks: chosenDivider = _blocksDivider; break;
            }

            if (chosenDivider != null)
            {
                targetObject.SetSiblingIndex(chosenDivider.GetSiblingIndex() + 1);    
            }
        }
    }
}