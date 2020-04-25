using System.Runtime.CompilerServices;
using libigl;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Assertions;

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

    /// <summary>
    /// Converts a matrix between column and row major by transposing in place
    /// <br/>Note: one element of T may not correspond to one element in the matrix
    /// </summary>
    /// <param name="rows">Set to 0 (default) to use <paramref name="nativeArray"/>.Length</param>
    /// <typeparam name="T">Type of an element in the array, not necessarily the matrix.</typeparam>
    /// <returns></returns>
    public static unsafe NativeArray<T> TransposeInPlace<T>(this NativeArray<T> nativeArray, int rows = 0, int cols = 3)
        where T : struct
    {
        if (rows == 0)
            rows = nativeArray.Length;
        Native.TransposeInPlace(nativeArray.GetUnsafePtr(), rows, cols);
        return nativeArray;
    }

    /// <summary>
    /// Converts a matrix <paramref name="inNativeArray"/> between column and row major by transposing and
    /// stores the result in <paramref name="outNativeArray"/>.
    /// Lengths of arrays must match.
    /// <br/>Note: one element of T may not correspond to one element in the matrix
    /// </summary>
    /// <param name="rows">Set to 0 (default) to use <paramref name="inNativeArray"/>.Length, in case of T=Vector3</param>
    /// <typeparam name="T">Type of an element in the array, not necessarily the matrix.</typeparam>
    /// <returns>Transposed array <paramref name="outNativeArray"/></returns>
    public static unsafe NativeArray<T> TransposeTo<T>(this NativeArray<T> inNativeArray, NativeArray<T> outNativeArray,
        int rows = 0, int cols = 3) where T : struct
    {
        Assert.IsTrue(inNativeArray.Length == outNativeArray.Length);
        if (rows == 0)
            rows = inNativeArray.Length;
        Native.TransposeTo(inNativeArray.GetUnsafePtr(), outNativeArray.GetUnsafePtr(), rows, cols);
        return outNativeArray;
    }
}
