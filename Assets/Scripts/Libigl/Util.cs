using UnityEngine;

namespace Libigl
{
    public static class Util
    {
        public static Vector3[] ToVector3(this float[,] arr)
        {
            Vector3[] result = new Vector3[arr.GetLength(0)];
            for (int i = 0; i < arr.GetLength(0); i++)
            {
                for (int j = 0; j < arr.GetLength(1); j++)
                {
                    result[i][j] = arr[i, j];
                }
            }

            return result;
        }

        public static Color[] ToColor(this float[,] arr)
        {
            Color[] result = new Color[arr.GetLength(0)];
            for (int i = 0; i < arr.GetLength(0); i++)
            {
                result[i].r = arr[i, 0];
                result[i].g = arr[i, 1];
                result[i].b = arr[i, 2];
            }

            return result;
        }
        
        public static class Colors
        {
            public static readonly Color White = Color.white;
            public static readonly Color Black = Color.black;
            public static readonly Color Red = new Color(0.7735849f, 0.3280911f, 0.280972f, 1f);
            public static readonly Color Green = new Color(0.4173074f, 0.7264151f, 0.366634f, 1f);
            public static readonly Color Blue = new Color(0.3019607f, 0.4429668f, 0.858823f, 1f);
            public static readonly Color Orange = new Color(0.8784314f, 0.5314119f, 0.145098f, 1f);

            public static Color GetColorById(int id)
            {
                switch (id % 4)
                {
                    case 0:
                        return Red;
                    case 1:
                        return Green;
                    case 2:
                        return Blue;
                    case 3:
                        return Orange;
                    default:
                        return Color.magenta;
                }
            }
        };
    }
}