using Libigl;

namespace XRInteraction
{
    public struct SharedInputState
    {
        // Tools & Input State
        public int ActiveTool;

        public static SharedInputState GetInstance()
        {
            return new SharedInputState{ActiveTool = ToolType.Select};
        }
    }
}