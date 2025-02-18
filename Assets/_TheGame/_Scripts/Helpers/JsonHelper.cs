using UnityEngine;
using _TheGame._Scripts.Data;
using System;

namespace _TheGame._Scripts.Helpers
{
    [Serializable]
    public class MovesContainer
    {
        public LevelData[] levels;
    }

    [Serializable]
    public class LevelData
    {
        public int levelId;
        public BlockDataModel.BlockData[] blocks;
    }

    public static class JsonHelper
    {
        public static BlockDataModel.GameMoveData LoadMoveData(int levelId)
        {
            var jsonText = Resources.Load<TextAsset>("moves").text;
            var container = JsonUtility.FromJson<MovesContainer>(jsonText);
            
            var currentLevel = System.Array.Find(container.levels, x => x.levelId == levelId);
            if (currentLevel == null) return null;

            return new BlockDataModel.GameMoveData
            {
                blocks = currentLevel.blocks
            };
        }
    }
}