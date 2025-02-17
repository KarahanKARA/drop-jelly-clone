using System;
using System.Collections.Generic;
using _TheGame._Scripts.Board;
using _TheGame._Scripts.Data;
using _TheGame._Scripts.Helpers;
using _TheGame._Scripts.Managers;
using _TheGame._Scripts.References;
using UnityEngine;

namespace _TheGame._Scripts.Block
{
    public class BlockSystem : MonoBehaviour
    {
        private const int BoardSize = GameData.BoardSize; 

        public List<BlockDataModel.ChildBlockData> childBlockDataList = new List<BlockDataModel.ChildBlockData>();

        private readonly Dictionary<Enums.ConnectionType, ChildBlockSystem> _childBlockMap 
            = new Dictionary<Enums.ConnectionType, ChildBlockSystem>();

        [Header("FORMAT : COLUMN, ROW")] 
        public Vector2Int positionData = new Vector2Int();

        private BoardGrid _boardGrid;

        private void OnEnable()
        {
            _boardGrid = ComponentReferences.Instance.boardGrid;
            positionData = new Vector2Int(-10, -10);
        }

        public void CheckSameColorAsNeighbors()
        {
            if (_boardGrid == null) return;

            var blocksToDestroy = new HashSet<ChildBlockSystem>();

            foreach (var kvp in _childBlockMap)
            {
                var childBlock = kvp.Value;
                var neighbors = GetExactNeighborBlocks(childBlock);

                foreach (var neighbor in neighbors)
                {
                    if (neighbor != null && neighbor.blockColor == childBlock.blockColor)
                    {
                        blocksToDestroy.Add(childBlock);
                        blocksToDestroy.Add(neighbor);
                        FindConnectedBlocksToDestroy(childBlock, blocksToDestroy); 
                        FindConnectedBlocksToDestroy(neighbor, blocksToDestroy);   
                    }
                }
            }

            if (blocksToDestroy.Count > 0)
            {
                DestroyMatchingBlocks(blocksToDestroy);
            }
        }

        
        private List<ChildBlockSystem> GetExactNeighborBlocks(ChildBlockSystem childBlock)
        {
            var neighbors = new List<ChildBlockSystem>();

            var x = positionData.x;
            var y = positionData.y;


            switch (childBlock.position)
            {
                case Enums.ConnectionType.TopLeft:
                    if (x - 1 >= 0)
                        neighbors.Add(GetChildBlockAt(x - 1, y, Enums.ConnectionType.TopRight));

                    if (y - 1 >= 0)
                        neighbors.Add(GetChildBlockAt(x, y - 1, Enums.ConnectionType.BottomLeft));
                    break;

                case Enums.ConnectionType.TopRight:
                    if (x + 1 < BoardSize)
                        neighbors.Add(GetChildBlockAt(x + 1, y, Enums.ConnectionType.TopLeft));

                    if (y - 1 >= 0)
                        neighbors.Add(GetChildBlockAt(x, y - 1, Enums.ConnectionType.BottomRight));
                    break;

                case Enums.ConnectionType.BottomLeft:
                    if (x - 1 >= 0)
                        neighbors.Add(GetChildBlockAt(x - 1, y, Enums.ConnectionType.BottomRight));

                    if (y + 1 < BoardSize)
                        neighbors.Add(GetChildBlockAt(x, y + 1, Enums.ConnectionType.TopLeft));
                    break;

                case Enums.ConnectionType.BottomRight:
                    if (x + 1 < BoardSize)
                        neighbors.Add(GetChildBlockAt(x + 1, y, Enums.ConnectionType.BottomLeft));

                    if (y + 1 < BoardSize)
                        neighbors.Add(GetChildBlockAt(x, y + 1, Enums.ConnectionType.TopRight));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return neighbors;
        }


        private ChildBlockSystem GetChildBlockAt(int x, int y, Enums.ConnectionType childPosition)
        {
            var neighborBlockSystem = _boardGrid.GetBlockSystemAt(x, y);
            if (neighborBlockSystem == null) return null;

            return neighborBlockSystem._childBlockMap.GetValueOrDefault(childPosition);
        }

        private void FindConnectedBlocksToDestroy(ChildBlockSystem block, HashSet<ChildBlockSystem> blocksToDestroy)
        {
            if (block.IsConnected)
            {
                var connectedBlock = GetConnectedBlock(block);
                if (connectedBlock != null)
                {
                    blocksToDestroy.Add(connectedBlock);
                }
            }
        }

        private void DestroyMatchingBlocks(HashSet<ChildBlockSystem> blocksToDestroy)
        {
            foreach (var block in blocksToDestroy)
            {
                Destroy(block.gameObject);
            }
        }

        private ChildBlockSystem GetConnectedBlock(ChildBlockSystem block)
        {
            if (block == null || !block.IsConnected) return null;

            var parentBlockSystem = block.transform.parent.GetComponent<BlockSystem>();
            if (parentBlockSystem == null) return null;

            return parentBlockSystem._childBlockMap.GetValueOrDefault(block.ConnectedWith);
        }

        public void CreateChildBlock(Enums.ConnectionType positionType, Enums.BlockColorType colorType,
            Enums.ConnectionType connectedWith)
        {
            var childObj = Instantiate(PrefabReferences.Instance.block1X1Prefab, transform);
            var childBlockSystem = childObj.GetComponent<ChildBlockSystem>();

            var blockPosType = GetBlockPositionType(positionType);
            var shapeData = DataManager.Instance.blockShapeScaleAndLocalPosList.Find(
                x => x.blockPositionType == blockPosType);

            if (shapeData != null)
            {
                var initialPos = new Vector3(shapeData.localPos.x, shapeData.localPos.y, 0f);
                var initialScale = new Vector3(shapeData.localScale.x, shapeData.localScale.y, 1f);

                childBlockSystem.Initialize(positionType, initialPos, initialScale);
                childBlockSystem.SetBlockColor(colorType);

                if (connectedWith != Enums.ConnectionType.None)
                {
                    childBlockSystem.SetConnection(connectedWith);
                }

                _childBlockMap[positionType] = childBlockSystem;
            }
        }

        private Enums.BlockPositionType GetBlockPositionType(Enums.ConnectionType connectionType)
        {
            return connectionType switch
            {
                Enums.ConnectionType.TopLeft => Enums.BlockPositionType.TopLeft,
                Enums.ConnectionType.TopRight => Enums.BlockPositionType.TopRight,
                Enums.ConnectionType.BottomLeft => Enums.BlockPositionType.BottomLeft,
                Enums.ConnectionType.BottomRight => Enums.BlockPositionType.BottomRight,
                _ => Enums.BlockPositionType.TopLeft
            };
        }

        private void OnDestroy()
        {
            if (_boardGrid != null)
            {
                _boardGrid.UnregisterBlockSystem(positionData.x, positionData.y);
            }
        }
    }
}
