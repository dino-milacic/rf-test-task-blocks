using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RFTestTaskBlocks
{
    public class SceneConfiguration
    {
        public Bounds SceneBounds { get; private set; } = new Bounds(new Vector3(0, 0), new Vector3(18, 10));
        public int NumberOfRobots = 1;
        public int NumberOfBlocks = 25;
        public List<BlockColor> AllowedColors= new List<BlockColor>();
        public Dictionary<BlockColor, ContainerConfiguration> ContainerData = new Dictionary<BlockColor, ContainerConfiguration>();

        public void CalculateSize(float horizontalSize)
        {
            float verticalSize = horizontalSize * Screen.height / Screen.width;
            SceneBounds = new Bounds(Vector3.zero, new Vector3(horizontalSize, verticalSize));
        }

        public bool IsValid => NumberOfBlocks >= 1 && NumberOfRobots >= 1 && SceneBounds.size.x > 10 && AllowedColors.Count >= 1 
                               && AllowedColors.All(c => ContainerData.ContainsKey(c));
    }
}