using UnityEngine;
using UnityEngine.XR;

namespace libigl.Behaviour
{
    public unsafe partial class LibiglBehaviour
    {
        private void UpdateInput()
        {
            if (InputManager.get.RightHand.isValid)
            {
                if (InputManager.get.RightHand.TryGetFeatureValue(CommonUsages.primaryButton,
                    out var primaryBtnValue) && primaryBtnValue)
                {
                    _input.Translate = true;
                }


                if (InputManager.get.RightHand.TryGetFeatureValue(CommonUsages.trigger, out var triggerValue) &&
                    triggerValue > 0.1f)
                {
                    _input.Select = true;
                }
            }
        }

        /// <summary>
        /// Consumes and resets flags raised. Should be called in PreExecute after copying to the State.
        /// </summary>
        private void ConsumeInput()
        {
            // Consume inputs here
            _input.Translate = false;
            _input.Select = false;
            _input.Harmonic = false;
        }

        /// <summary>
        /// Static method that generates the UI to manipulate the <i>active mesh</i> <see cref="MeshManager.ActiveMesh"/>
        /// </summary>
        public static void InitializeActionUi()
        {
            UIActions.get.CreateActionUi("Test", () => Debug.Log("Test"), new []{"test"}, 0);
            UIActions.get.CreateActionUi("Translate", () => { MeshManager.ActiveMesh.Behaviour._input.Translate = true; }, new []{"translate", "move"}, 1);
            UIActions.get.CreateActionUi("Select", () => { MeshManager.ActiveMesh.Behaviour._input.Select = true; }, new [] {"select"});
            UIActions.get.CreateActionUi("Harmonic", () => { MeshManager.ActiveMesh.Behaviour._input.Harmonic = true; }, new [] {"smooth", "harmonic", "laplacian"}, 2);
        }
    }
}