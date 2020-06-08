using UnityEngine;

namespace libigl.Behaviour
{
    public unsafe partial class LibiglBehaviour
    {
        private void ActionTransformSelection()
        {
            if (!ExecuteInput.DoTransform || ExecuteInput.ActiveTool != ToolType.Select) return;

            if (!ExecuteInput.SecondaryTransformHandActive)
            {
                // Only translate selection
                var translate = GetTranslateVector(ref ExecuteInput);

                Native.TranslateSelection(State, translate, ExecuteInput.ActiveSelectionId);
            }
            else
            {
                // Do full transformation
                GetTransformData(ref ExecuteInput, out var translate, out var scale, out var angle, out var axis);
                angle *= Mathf.Deg2Rad; // Eigen uses rad, Unity uses deg

                Native.TransformSelection(State, ExecuteInput.ActiveSelectionId, translate, scale, angle, axis.normalized);
            }
            State->DirtyState |= DirtyFlag.VDirty;
        }

        /// <summary>
        /// Determines the softenes translation vector
        /// </summary>
        /// <param name="i">The input state to use</param>
        /// <returns></returns>
        private static Vector3 GetTranslateVector(ref InputState i)
        {
            Vector3 t;
            if(i.PrimaryTransformHand)
                t = i.HandPosR - i.PrevTrafoHandPosR;
            else
                t = i.HandPosL - i.PrevTrafoHandPosL;
            
            var softFactor = i.PrimaryTransformHand ? i.GripR : i.GripL;
            return t * softFactor;
        }

        /// <summary>
        /// Determines the softened transformation from the input state
        /// </summary>
        /// <param name="i">InputState to use</param>
        /// <param name="angle">In degrees</param>
        private static void GetTransformData(ref InputState i, out Vector3 translate, out float scale, out float angle, out Vector3 axis)
        {
            Vector3 v0, v1;
            if(i.PrimaryTransformHand)
            {
                translate = i.HandPosR - i.PrevTrafoHandPosR;
                v0 = i.PrevTrafoHandPosR - i.PrevTrafoHandPosL;
                v1 = i.HandPosR - i.HandPosL;
            }
            else
            {
                translate = i.HandPosL - i.PrevTrafoHandPosL;
                v0 = i.PrevTrafoHandPosL - i.PrevTrafoHandPosR;
                v1 = i.HandPosL - i.HandPosR;
            }
                
            axis = Vector3.Cross(v0, v1);
            angle = Vector3.Angle(v0, v1);
            
            // TODO: scale should be done from positions at start of both grips pressed 
            scale = (i.HandPosL - i.HandPosR).magnitude / (i.PrevTrafoHandPosL - i.PrevTrafoHandPosR).magnitude;
            if (float.IsNaN(scale))
                scale = 1f;
            // Apply soft editing
            var softFactor = i.PrimaryTransformHand ? i.GripR : i.GripL;
            var softFactorSecondary = !i.PrimaryTransformHand ? i.GripR : i.GripL;

            translate *= softFactor;
            scale = (scale -1) * softFactorSecondary + 1;
            angle *= softFactorSecondary;
        }

        private void ActionSelect()
        {
            if (!ExecuteInput.DoSelect) return;
            
            Native.SphereSelect(State, ExecuteInput.SelectPos, ExecuteInput.SelectRadiusSqr, 
                ExecuteInput.ActiveSelectionId, (uint) ExecuteInput.ActiveSelectionMode);
        }

        private void ActionHarmonic()
        {
            if (!ExecuteInput.DoHarmonicOnce) return;
            
            Native.Harmonic(State, -1);
            State->DirtyState |= DirtyFlag.VDirty;
        }

        private void ActionUi()
        {
            if (ExecuteInput.DoClearSelection > 0)
                Native.ClearSelectionMask(State, ExecuteInput.DoClearSelection);
            
            if(ExecuteInput.VisibleSelectionMaskChanged)
                Native.SetColorByMask(State, ExecuteInput.VisibleSelectionMask);
        }

        private void UpdateMeshTransform()
        {
            if (Input.ActiveTool == ToolType.Select || !Input.DoTransform) return;
            
            // Transform the whole mesh
            if (Input.SecondaryTransformHandActive)
            {
                GetTransformData(ref Input, out var translate, out var scale, out var angle, out var axis);
                var uTransform = LibiglMesh.transform;
                uTransform.Translate(translate);
                uTransform.Rotate(axis, angle);
                uTransform.localScale *= scale;
            }
            else
                LibiglMesh.transform.Translate(GetTranslateVector(ref Input));
                
            // Consume the input and update the previous position directly
            Input.PrevTrafoHandPosL = Input.HandPosL;
            Input.PrevTrafoHandPosR = Input.HandPosR;
        }
    }
}