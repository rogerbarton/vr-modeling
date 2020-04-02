using System.Runtime.CompilerServices;
using libigl;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public static class Util
{
    public static Vector3[] ToVector3(this float[,] arr){
        Vector3[] result = new Vector3[arr.GetLength(0)];
        for(int i=0;i<arr.GetLength(0);i++){
            for(int j=0;j<arr.GetLength(1);j++){
                result[i][j] = arr[i,j];    
            }
        }
        return result;
    }

    public static Color[] ToColor(this float[,] arr){
        Color[] result = new Color[arr.GetLength(0)];
        for(int i=0;i<arr.GetLength(0);i++){
            result[i].r = arr[i,0];
            result[i].g = arr[i,1];
            result[i].b = arr[i,2];
        }
        return result;
    }

    /// <summary>
    /// Converts a matrix between column and row major by transposing in place
    /// </summary>
    /// <param name="nativeArray">The native array to modify</param>
    /// <param name="cols">The number of columns of the matrix</param>
    /// <typeparam name="T">Type of an element</typeparam>
    /// <returns></returns>
    public static unsafe NativeArray<T> TransposeInPlace3<T>(this NativeArray<T> nativeArray, int cols = 3) where T : struct
    {
        Native.TransposeInPlace(nativeArray.GetUnsafePtr(), nativeArray.Length / cols);
        return nativeArray;
    }
}
