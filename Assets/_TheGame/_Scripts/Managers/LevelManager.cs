using System;
using System.Collections;
using _TheGame._Scripts.Block;
using _TheGame._Scripts.Data;
using _TheGame._Scripts.Helpers;
using _TheGame._Scripts.References;
using UnityEngine;

namespace _TheGame._Scripts.Managers
{
    #region Level Data Models
    [Serializable]
    public class BlockGridData
    {
        public int id;   
        public int col;  
        public int row; 
    }

    [Serializable]
    public class Level
    {
        public int id;
        public BlockDataModel.GameMoveData moveData;
        public BlockGridData[] blocksGridPositions; 
    }

    [Serializable]
    public class LevelContainer
    {
        public Level[] levels;
        public Level GetLevelById(int id)
        {
            foreach (var lvl in levels)
            {
                if (lvl.id == id)
                    return lvl;
            }
            return null;
        }
    }
    #endregion

    public class LevelManager : Singleton<LevelManager>
    {
        private IEnumerator Start()
        {
            yield return null;

            var grid = ComponentReferences.Instance.boardGrid;
            if (grid == null)
            {
                Debug.LogError("BoardGrid not found in ComponentReferences!");
                yield break;
            }

            var levelIndex = SaveManager.Instance.GetLastLevelIndex();
            if (levelIndex == -1)
            {
                Debug.Log("Endless mode not implemented yet.");
                yield break;
            }

            var levelsTextAsset = Resources.Load<TextAsset>("levels");
            if (levelsTextAsset == null)
            {
                Debug.LogError("levels.json not found in Resources folder!");
                yield break;
            }

            var container = JsonUtility.FromJson<LevelContainer>(levelsTextAsset.text);
            var currentLevel = container.GetLevelById(levelIndex);
            if (currentLevel == null)
            {
                Debug.LogError("Level with id " + levelIndex + " not found in levels.json!");
                yield break;
            }

            foreach (var blockData in currentLevel.moveData.blocks)
            {
                var gridData = Array.Find(currentLevel.blocksGridPositions, gd => gd.id == blockData.id);
                if (gridData == null)
                {
                    continue;
                }

                var spawnPos = grid.GetPosition(gridData.row, gridData.col);
                var newBlock = Instantiate(PrefabReferences.Instance.blockPrefab, spawnPos, Quaternion.identity);
                newBlock.transform.SetParent(ComponentReferences.Instance.createdBlockParent, false);

                var blockSystem = newBlock.GetComponent<BlockSystem>();
                blockSystem.positionData = new Vector2Int(gridData.col, gridData.row);
                grid.RegisterBlockSystem(gridData.col, gridData.row, blockSystem);
                grid.SetPositionOccupied(gridData.row, gridData.col);

                foreach (var childData in blockData.children)
                {
                    var posType = GetConnectionType(childData.position);
                    var colorType = GetColorType(childData.color);
                    var connectedWith = string.IsNullOrEmpty(childData.connectedWith)
                        ? Enums.ConnectionType.None
                        : GetConnectionType(childData.connectedWith);

                    blockSystem.CreateChildBlock(posType, colorType, connectedWith);
                }
            }
        }

        private Enums.ConnectionType GetConnectionType(string pos)
        {
            switch (pos)
            {
                case "TopLeft": return Enums.ConnectionType.TopLeft;
                case "TopRight": return Enums.ConnectionType.TopRight;
                case "BottomLeft": return Enums.ConnectionType.BottomLeft;
                case "BottomRight": return Enums.ConnectionType.BottomRight;
                default: return Enums.ConnectionType.None;
            }
        }

        private Enums.BlockColorType GetColorType(string color)
        {
            switch (color)
            {
                case "Red": return Enums.BlockColorType.Red;
                case "Blue": return Enums.BlockColorType.Blue;
                case "Green": return Enums.BlockColorType.Green;
                case "Yellow": return Enums.BlockColorType.Yellow;
                case "Purple": return Enums.BlockColorType.Purple;
                case "Pink": return Enums.BlockColorType.Pink;
                default: return Enums.BlockColorType.None;
            }
        }
    }
}