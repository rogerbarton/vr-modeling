using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Assertions;

namespace libigl
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
    }
}