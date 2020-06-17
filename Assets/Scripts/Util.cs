using UnityEngine;

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
        public static readonly Color Red = new Color(0.67f, 0.24f, 0.19f);
        public static readonly Color Green = new Color(0.5f, 0.71f, 0.29f);
        public static readonly Color Blue = new Color(0.23f, 0.38f, 0.85f);
        public static readonly Color Orange = new Color(0.87f, 0.51f, 0.16f);
        public static readonly Color Purple = new Color(0.65f, 0.34f, 0.62f);
        public static readonly Color GreenLight = new Color(0.16f, 0.62f, 0.41f);
        public static readonly Color BlueLight = new Color(0.17f, 0.62f, 0.71f);
        public static readonly Color Yellow = new Color(0.86f, 0.86f, 0.15f);

        public static Color GetColorById(int id)
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

    public static Vector3 CwiseMul(this Vector3 lhs, Vector3 rhs)
    {
        Vector3 res;
        res.x = lhs.x * rhs.x;
        res.y = lhs.y * rhs.y;
        res.z = lhs.z * rhs.z;
        return res;
    }
}