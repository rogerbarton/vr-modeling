using UI;

namespace libigl.Behaviour
{
    public unsafe partial class LibiglBehaviour
    {
        /// <summary>
        /// Static method that generates the UI to manipulate the <i>active mesh</i> <see cref="MeshManager.ActiveMesh"/>
        /// </summary>
        public static void InitializeActionUi()
        {
            UiManager.get.CreateActionUi("Translate", () => { MeshManager.ActiveMesh.Behaviour.Input.DoTransform = true; }, new []{"translate", "move"}, 1);
            UiManager.get.CreateActionUi("Harmonic", () => { MeshManager.ActiveMesh.Behaviour.Input.DoHarmonic = true; }, new [] {"smooth", "harmonic", "laplacian"}, 2);
            
            UiManager.get.CreateActionUi("Default Tool", () => { MeshManager.ActiveMesh.Behaviour.Input.ActiveTool = ToolType.Default; });
            UiManager.get.CreateActionUi("Select Tool", () => { MeshManager.ActiveMesh.Behaviour.Input.ActiveTool = ToolType.Select; }, new [] {"select"});
            UiManager.get.CreateActionUi("Do Select", () => { MeshManager.ActiveMesh.Behaviour.Input.DoSelect = true; });
        }
    }
}