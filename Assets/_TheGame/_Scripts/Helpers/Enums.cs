using System;

namespace _TheGame._Scripts.Helpers
{
    public static class Enums
    {
        [Serializable]
        public enum BlockColorType
        {
            None = 0,
            Blue = 1,
            DarkBlue = 2,
            Green = 3,
            Orange = 4,
            Pink = 5,
            Purple = 6,
            Red = 7,
            Yellow = 8,
        }

        [Serializable]
        public enum BlockShapeType
        {
            None = 0,
            TopDouble = 1,
            BottomLeft = 2,
            BottomRight = 3,
            TopRight = 4,
            TopLeft = 5,
            BottomDouble = 6,
            LeftDouble = 7,
            RightDouble = 8,
            FullSquare = 9
        }
    }
}