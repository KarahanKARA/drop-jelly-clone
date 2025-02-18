using System;

namespace _TheGame._Scripts.Data
{
    public abstract class BlockDataModel
    {
        [Serializable]
        public class GameMoveData
        {
            public BlockData[] blocks;
        }

        [Serializable]
        public class BlockData
        {
            public int id;
            public bool isSquare;
            public ChildBlockData[] children;
        }

        [Serializable]
        public class ChildBlockData
        {
            public string position;
            public string color;
            public string connectedWith;
        }
    }
}