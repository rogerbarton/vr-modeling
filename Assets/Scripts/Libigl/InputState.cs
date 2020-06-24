using UnityEngine;
using XrInput;

namespace Libigl
{
    
    /// <summary>
    /// Struct for storing the current input *for a mesh*. (This is a value type so assigning will copy).
    /// Anything that may change as we are executing should be in the InputState, as it is copied in PreExecute.
    /// Anything that is the same between all meshes may be put into the <see cref="SharedInputState"/>.
    /// Anything related to what should be executed on the worker thread should be put here (e.g. DoSelect).
    /// </summary>
    public struct InputState
    {
        // Copies for threading
        public SharedInputState Shared;
        public SharedInputState SharedPrev;

        // Transform
        public bool DoTransform;
        public bool DoTransformPrev;
        public TransformDelta TransformDelta;

        // Select
        public int ActiveSelectionId;
        public uint VisibleSelectionMask;
        public bool VisibleSelectionMaskChanged;
        public uint SCountUi; // For UI, will be copied to the state in PreExecute

        public bool DoSelect;
        public bool DoSelectPrev;
        // A Mask of the selections that should be cleared
        public uint DoClearSelection; 

        public Vector3 BrushPosL;
        public Vector3 BrushPosR;

        // Deformations
        public bool DoHarmonic; // Trigger execution once
        public bool DoHarmonicRepeat; // Trigger execution every frame
        public bool HarmonicShowDisplacement;
        public bool DoArap;
        public bool DoArapRepeat;

        public bool ResetV;

        /// <returns>An instance with the default values</returns>
        public static InputState GetInstance()
        {
            return new InputState
            {
                VisibleSelectionMask = uint.MaxValue, 
                HarmonicShowDisplacement = true,
                Shared = InputManager.State,
                SharedPrev = InputManager.StatePrev,
                TransformDelta = TransformDelta.Identity()
            };
        }
    }
}