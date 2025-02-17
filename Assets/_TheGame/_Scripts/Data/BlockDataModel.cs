using System;
using _TheGame._Scripts.Helpers;
using UnityEngine;

namespace _TheGame._Scripts.Data
{
    public class BlockDataModel
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
            public ChildBlockData[] children;
        }

        [Serializable]
        public class ChildBlockData
        {
            public string position;
            public string color;
            public string connectedWith;
            public Enums.BlockColorType blockColorType;
            public bool isConnected;
            public Enums.ConnectionType connectionType;
            public Vector2 originalPosition;
            public Vector2 originalScale;
        }
    }
}