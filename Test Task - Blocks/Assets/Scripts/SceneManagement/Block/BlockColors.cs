using UnityEngine;

namespace RFTestTaskBlocks
{
    public enum BlockColor
    {
        Blue,
        Red,
        Green,
        Magenta,
        Orange
    }

    public static class BlockColorExtensions
    {
        public static Color BlockColorToColor(this BlockColor value)
        {
            switch (value)
            {
                case BlockColor.Blue: return new Color(0.29f, 0.7f, 1f);
                case BlockColor.Red:  return Color.red;
                case BlockColor.Green:  return new Color(0.02f, 0.76f, 0.07f);
                case BlockColor.Magenta:  return new Color(0.76f, 0f, 0.76f);
                case BlockColor.Orange:  return new Color(1f, 0.5f, 0f);
            }
            return Color.white;
        }
    }
}