using UnityEngine;

namespace Util
{
    /// <summary>
    /// Contains custom color constants.
    /// </summary>
    public static class Colors
    {
        public static readonly Color White = Color.white;
        public static readonly Color Black = Color.black;
        public static readonly Color Red = new Color(0.67f, 0.24f, 0.19f);
        public static readonly Color Green = new Color(0.5f, 0.71f, 0.29f);
        public static readonly Color Blue = new Color(0.23f, 0.38f, 0.85f);
        public static readonly Color Orange = new Color(0.87f, 0.51f, 0.16f);
        public static readonly Color Purple = new Color(0.65f, 0.34f, 0.62f);
        public static readonly Color GreenLight = new Color(0.16f, 0.62f, 0.41f);
        public static readonly Color BlueLight = new Color(0.17f, 0.62f, 0.71f);
        public static readonly Color Yellow = new Color(0.86f, 0.86f, 0.15f);

        /// <summary>
        /// Get the color by its id.
        /// </summary>
        public static Color Get(int id)
        {
            switch (id % 8)
            {
                case 0:
                    return Red;
                case 1:
                    return Green;
                case 2:
                    return Blue;
                case 3:
                    return Orange;
                case 4:
                    return Purple;
                case 5:
                    return GreenLight;
                case 6:
                    return BlueLight;
                case 7:
                    return Yellow;
                default:
                    return Color.magenta;
            }
        }
    }
}