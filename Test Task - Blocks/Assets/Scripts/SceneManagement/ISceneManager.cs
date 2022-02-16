using System.Collections.Generic;
using UnityEngine;

namespace RFTestTaskBlocks
{
    public interface ISceneManager : IGameService
    {
        void Configure(SceneConfiguration config);
        void Configure(float sceneSize, int robots, int blocks, List<BlockColor> allowedColors);
        int NumberOfRobots { get; }
        int NumberOfBlocks { get; }
        ContainerController GetContainerForColor(BlockColor color);
        Bounds SceneSize { get; }
        Vector3 GetPositionInScene(Vector2 gridPosition);
        Vector3 GetGroundedPositionInScene(float percentage);
        Vector2 GetSceneGridPosition(float percentage);
        void SpawnRobot(int index);
        void SpawnContainer(float position, BlockColor color, ContainerController.ContainerOrientation orientation);
        void SpawnBlock();
        void Reset();
    }
}