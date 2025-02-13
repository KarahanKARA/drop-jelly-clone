using UnityEngine;

namespace _TheGame._Scripts.Data
{
    public static class GameData
    {
        public const float ColumnWidth = 2.1f;
        public const int BoardSize = 6;
        public const float StartX = -5.25f;
        public const float StartY = 5.25f;
        public const float Offset = 2.1f;
        public static readonly  Vector3 BlockSpawnPos = new Vector3(0, 9, 0.2f);
    }
}
