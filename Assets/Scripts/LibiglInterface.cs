using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace libigl.Interface
{
    public static class LibiglInterface
    {
        [ShowInInspector] public static string modelRoot;

        public static void Initialize(string modelPath, Mesh mesh)
        {
            if (modelRoot == null)
                modelRoot = Application.dataPath + "/Models/";
            //Call cpp init with model root
        }

    }
}