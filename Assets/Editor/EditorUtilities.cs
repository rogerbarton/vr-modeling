#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
#if UNITY_2019_1_OR_NEWER
using UnityEditor.ShortcutManagement;
#endif
namespace EditorExtras
{
    /// <summary>
    /// A independent collection of functions for easier editor usage with shortcuts
    /// May be unreliable as the functions often rely on reflection
    /// Shortcuts can be set in Edit > Shortcuts > Tools
    /// </summary>
    public static class EditorUtilities
    {
        /// <summary>
        /// Clears the developer console
        /// </summary>
#if UNITY_2019_1_OR_NEWER
        [Shortcut("Tools/Clear Console")]
#else
        [MenuItem("Tools/Clear Console &c")]
#endif
        public static void ClearConsole()
        {
            // This simply does "LogEntries.Clear()" the long way:
            var logEntries = Type.GetType("UnityEditor.LogEntries,UnityEditor.dll");
            var clearMethod = logEntries.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
            clearMethod.Invoke(null, null);
        }

        /// <summary>
        /// Toggle inspector lock, hover over inspector to use
        /// </summary>
#if UNITY_2019_1_OR_NEWER
        [Shortcut("Tools/Toggle Inspector Lock")]
#else
        [MenuItem("Tools/Toggle Inspector Lock &e")]
#endif
        public static void ToggleInspectorLock()
        {
            // "EditorWindow.focusedWindow" can be used instead
            var inspectorToBeLocked = EditorWindow.mouseOverWindow;
            if (inspectorToBeLocked == null)
                return;

            Type projectBrowserType =
                Assembly.GetAssembly(typeof(UnityEditor.Editor)).GetType("UnityEditor.ProjectBrowser");
            Type inspectorWindowType =
                Assembly.GetAssembly(typeof(UnityEditor.Editor)).GetType("UnityEditor.InspectorWindow");

            PropertyInfo propertyInfo;
            if (inspectorToBeLocked.GetType() == projectBrowserType)
                propertyInfo = projectBrowserType.GetProperty("isLocked",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            else if (inspectorToBeLocked.GetType() == inspectorWindowType)
                propertyInfo = inspectorWindowType.GetProperty("isLocked");
            else
                return;

            bool value = (bool) propertyInfo.GetValue(inspectorToBeLocked, null);
            propertyInfo.SetValue(inspectorToBeLocked, !value, null);
            inspectorToBeLocked.Repaint();
        }

        /// <summary>
        /// Toggle the inspector debug/normal mode, hover over inspector to use 
        /// </summary>
#if UNITY_2019_1_OR_NEWER
        [Shortcut("Tools/Toggle Inspector Debug Mode")]
#else
        [MenuItem("Tools/Toggle Inspector Debug Mode &d")]
#endif
        public static void ToggleInspectorDebug()
        {
            var targetInspector = EditorWindow.mouseOverWindow;
            if (targetInspector == null || targetInspector.GetType().Name != "InspectorWindow")
                return;

            //Get the type of the inspector window to find out the variable/method from
            Type type = Assembly.GetAssembly(typeof(UnityEditor.Editor)).GetType("UnityEditor.InspectorWindow");
            //get the field we want to read, for the type (not our instance)
            FieldInfo field = type.GetField("m_InspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);

            //read the value for our target inspector
            InspectorMode mode = (InspectorMode) field.GetValue(targetInspector);
            mode = (mode == InspectorMode.Normal ? InspectorMode.Debug : InspectorMode.Normal); //toggle the value
            //Debug.Log("New Inspector Mode: " + mode.ToString());

            //Find the method to change the mode for the type
            MethodInfo method = type.GetMethod("SetMode", BindingFlags.NonPublic | BindingFlags.Instance);

            //Call the function on our targetInspector, with the new mode as an object[]
            method.Invoke(targetInspector, new object[] {mode});

            targetInspector.Repaint(); //refresh inspector
        }
    }
}
#endif