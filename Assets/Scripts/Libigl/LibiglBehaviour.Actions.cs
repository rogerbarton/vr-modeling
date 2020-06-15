using UnityEngine;
using XrInput;

namespace Libigl
{
    public unsafe partial class LibiglBehaviour
    {
        private void ActionTransformSelection()
        {
            if (!ExecuteInput.DoTransform || !ExecuteInput.DoTransformPrev || ExecuteInput.Shared.ActiveTool != ToolType.Select) return;

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
                t = i.Shared.HandPosR - i.SharedPrev.HandPosR;
            else
                t = i.Shared.HandPosL - i.SharedPrev.HandPosL;
            
            var softFactor = i.PrimaryTransformHand ? i.Shared.GripR : i.Shared.GripL;
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
                translate = i.Shared.HandPosR - i.SharedPrev.HandPosR;
                v0 = i.SharedPrev.HandPosR - i.SharedPrev.HandPosL;
                v1 = i.Shared.HandPosR - i.Shared.HandPosL;
            }
            else
            {
                translate = i.Shared.HandPosL - i.SharedPrev.HandPosL;
                v0 = i.SharedPrev.HandPosL - i.SharedPrev.HandPosR;
                v1 = i.Shared.HandPosL - i.Shared.HandPosR;
            }
                
            axis = Vector3.Cross(v0, v1);
            angle = Vector3.Angle(v0, v1);
            
            // TODO: scale should be done from positions at start of both grips pressed 
            scale = (i.Shared.HandPosL - i.Shared.HandPosR).magnitude / (i.SharedPrev.HandPosL - i.SharedPrev.HandPosR).magnitude;
            if (float.IsNaN(scale))
                scale = 1f;
            // Apply soft editing
            var softFactor = i.PrimaryTransformHand ? i.Shared.GripR : i.Shared.GripL;
            var softFactorSecondary = !i.PrimaryTransformHand ? i.Shared.GripR : i.Shared.GripL;

            translate *= softFactor;
            scale = (scale -1) * softFactorSecondary + 1;
            angle *= softFactorSecondary;
        }

        private void ActionSelect()
        {
            if (!ExecuteInput.DoSelect) return;
            
            Native.SelectSphere(State, ExecuteInput.SelectPos, ExecuteInput.SelectRadius, 
                ExecuteInput.ActiveSelectionId, (uint) ExecuteInput.Shared.ActiveSelectionMode);
        }

        private void ActionHarmonic()
        {
            if (!ExecuteInput.DoHarmonic) return;
            
            Native.Harmonic(State, ExecuteInput.VisibleSelectionMask, ExecuteInput.HarmonicShowDisplacement);
        }
        
        private void ActionArap()
        {
            if (!ExecuteInput.DoArap) return;
            
            Native.Arap(State, ExecuteInput.VisibleSelectionMask);
        }

        private void ActionUi()
        {
            if (ExecuteInput.DoClearSelection > 0)
                Native.ClearSelectionMask(State, ExecuteInput.DoClearSelection);
            
            if(ExecuteInput.VisibleSelectionMaskChanged)
                Native.SetColorByMask(State, ExecuteInput.VisibleSelectionMask);

            if (ExecuteInput.ResetV)
                Native.ResetV(State);
        }

        private void UpdateMeshTransform()
        {
            if (InputManager.Input.ActiveTool == ToolType.Select || !Input.DoTransform || !Input.DoTransformPrev) return;
            
            // Transform the whole mesh
            if (Input.SecondaryTransformHandActive)
            {
                //TODO: Use InputManager.InputPrev
                GetTransformData(ref Input, out var translate, out var scale, out var angle, out var axis);
                var uTransform = LibiglMesh.transform;
                uTransform.Translate(translate);
                uTransform.Rotate(axis, angle);
                uTransform.localScale *= scale;
            }
            else
                LibiglMesh.transform.Translate(GetTranslateVector(ref Input));
        }
    }
}