using System;

namespace _TheGame._Scripts.Helpers
{
    public static class Enums
    {
        [Serializable]
        public enum BlockColorType
        {
            None,
            Blue,
            DarkBlue,
            Green,
            Orange,
            Pink,
            Purple,
            Red,
            Yellow,
        }

        [Serializable]
        public enum BlockShapeType
        {
            None,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
            TopDouble,
            BottomDouble,
            LeftDouble,
            RightDouble,
            FullSquare
        }
    }
}
