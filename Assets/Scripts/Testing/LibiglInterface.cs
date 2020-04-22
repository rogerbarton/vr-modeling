using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
// using UnityNativeTool;

namespace libigl
{
    public class LibiglInterface : MonoBehaviour
    {
        public static LibiglInterface get;

        void Awake()
        {
            if (!get) get = this;
            else if (get != this)
            {
                Debug.LogError("A LibiglInterface instance already exists. \n" +
                               "Existing instance: " + get.gameObject.name + ", Second instance: " + gameObject.name);
                enabled = false;
                return;
            }
        }

        public static void CheckExistence()
        {
            if (!get)            //load static scene with the instance scene, blocking load
                SceneManager.LoadScene("StaticScene", LoadSceneMode.Additive);
        }
    }
}