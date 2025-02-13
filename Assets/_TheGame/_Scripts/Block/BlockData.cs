using UnityEngine;

namespace _TheGame._Scripts.Block
{
    public static class BlockData
    {
        public static Vector2 TopLeftPos = new Vector2(-0.5f, 0.5f);
        public static Vector2 TopRightPos = new Vector2(0.5f, 0.5f);
        public static Vector2 BottomLeftPos = new Vector2(-0.5f, -0.5f);
        public static Vector2 BottomRightPos = new Vector2(0.5f, -0.5f);

        public static Vector2 TopDoublePos = new Vector2(0, 0.5f);
        public static Vector2 BottomDoublePos = new Vector2(0, -0.5f);
        public static Vector2 LeftDoublePos = new Vector2(-0.5f, 0);
        public static Vector2 RightDoublePos = new Vector2(0.5f, 0);
    }
}
