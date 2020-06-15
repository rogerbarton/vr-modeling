
namespace XrInput
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

    public static class ToolType
    {
        public const int Invalid = -1;
        public const int Default = 0;
        public const int Select = 1;
        // public const int Laser = 2;
        // public const int ViewOnly = 3;

        public const int Size = 2;
    }
}