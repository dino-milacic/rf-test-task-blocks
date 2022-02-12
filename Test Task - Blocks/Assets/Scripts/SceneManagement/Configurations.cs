using System.Collections.Generic;
using UnityEngine;

namespace RFTestTaskBlocks
{
    public class RobotConfiguration
    {
        public float SpawnPosition;
        public RobotController.RobotDirection StartingDirection = RobotController.RobotDirection.Left;
        public float SpeedMultiplier;
        public float VisionRange;
    }
    
    public class ContainerConfiguration
    {
        public float SpawnPosition;
        public BlockColor Color;
        public ContainerController.ContainerOrientation Orientation = ContainerController.ContainerOrientation.Left;
    }
    
    public class BlockConfiguration
    {
        public List<BlockColor> AllowedColors;
        public Bounds AllowedBounds;
    }
}