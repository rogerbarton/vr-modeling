using UnityEngine;

namespace Util
{
    /// <summary>
    /// Contains various convenience functions and extension methods.
    /// </summary>
    public static class Misc
    {
        public static Vector3 CwiseMul(this Vector3 lhs, Vector3 rhs)
        {
            Vector3 res;
            res.x = lhs.x * rhs.x;
            res.y = lhs.y * rhs.y;
            res.z = lhs.z * rhs.z;
            return res;
        }
    }
}