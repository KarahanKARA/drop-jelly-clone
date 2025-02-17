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
        public enum BlockPositionType
        {
            None,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        [Serializable]
        public enum ConnectionType
        {
            None,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }
    }
}