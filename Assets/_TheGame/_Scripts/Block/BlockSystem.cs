using System;
using System.Collections.Generic;
using System.Linq;
using _TheGame._Scripts.Board;
using _TheGame._Scripts.Data;
using _TheGame._Scripts.Helpers;
using _TheGame._Scripts.Managers;
using _TheGame._Scripts.References;
using DG.Tweening;
using UnityEngine;

namespace _TheGame._Scripts.Block
{
    public class BlockSystem : MonoBehaviour
    {
        private const int BoardSize = GameData.BoardSize;

        public List<BlockDataModel.ChildBlockData> childBlockDataList = new List<BlockDataModel.ChildBlockData>();

        private readonly Dictionary<Enums.ConnectionType, ChildBlockSystem> _childBlockMap
            = new Dictionary<Enums.ConnectionType, ChildBlockSystem>();

        private static HashSet<BlockSystem> blockSystemsToExpand = new HashSet<BlockSystem>();

        [Header("FORMAT : COLUMN, ROW")] public Vector2Int positionData = new Vector2Int();

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
                var parentSystem = block.transform.parent.GetComponent<BlockSystem>();
                if (parentSystem != null)
                {
                    blockSystemsToExpand.Add(parentSystem);
                }
            }

            foreach (var block in blocksToDestroy)
            {
                var parentSystem = block.transform.parent.GetComponent<BlockSystem>();
                if (parentSystem != null)
                {
                    parentSystem._childBlockMap.Remove(block.position);
                }

                Destroy(block.gameObject);
            }

            foreach (var blockSystem in blockSystemsToExpand)
            {
                blockSystem.CheckExpand();
            }

            blockSystemsToExpand.Clear();
        }


        public void CheckExpand()
        {
            var emptyPositions = new List<Enums.ConnectionType>();
            foreach (Enums.ConnectionType pos in Enum.GetValues(typeof(Enums.ConnectionType)))
            {
                if (pos != Enums.ConnectionType.None && !_childBlockMap.ContainsKey(pos))
                {
                    emptyPositions.Add(pos);
                }
            }

            if (emptyPositions.Count == 0)
            {
                return;
            }

            foreach (var emptyPos in emptyPositions)
            {
                if (TryExpandVertically(emptyPos))
                {
                    continue;
                }

                if (TryExpandHorizontally(emptyPos))
                {
                    Debug.Log($"Horizontal expand successful for {emptyPos}");
                }
                else
                {
                    Debug.Log($"No expansion possible for {emptyPos}");
                }
            }
        }


        private bool TryExpandHorizontally(Enums.ConnectionType emptyPos)
        {
            var remainingBlocks = _childBlockMap.Values.ToList();

            var sourceBlock = remainingBlocks.FirstOrDefault(block => 
            {
                if (block.IsConnected) return false;

                return (block.position == Enums.ConnectionType.TopLeft && emptyPos == Enums.ConnectionType.TopRight) ||
                       (block.position == Enums.ConnectionType.TopRight && emptyPos == Enums.ConnectionType.TopLeft) ||
                       (block.position == Enums.ConnectionType.BottomLeft && emptyPos == Enums.ConnectionType.BottomRight) ||
                       (block.position == Enums.ConnectionType.BottomRight && emptyPos == Enums.ConnectionType.BottomLeft);
            });

            if (sourceBlock != null)
            {
                CloneAndMove(sourceBlock, emptyPos);
                return true;
            }

            return false;
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

        private bool TryExpandVertically(Enums.ConnectionType emptyPos)
        {
            var remainingBlocks = _childBlockMap.Values.ToList();

            var sourceBlock = remainingBlocks.FirstOrDefault(block => 
            {
                if (block.IsConnected) return false;

                return (block.position == Enums.ConnectionType.TopLeft && emptyPos == Enums.ConnectionType.BottomLeft) ||
                       (block.position == Enums.ConnectionType.TopRight && emptyPos == Enums.ConnectionType.BottomRight) ||
                       (block.position == Enums.ConnectionType.BottomLeft && emptyPos == Enums.ConnectionType.TopLeft) ||
                       (block.position == Enums.ConnectionType.BottomRight && emptyPos == Enums.ConnectionType.TopRight);
            });

            if (sourceBlock != null)
            {
                CloneAndMove(sourceBlock, emptyPos);
                return true;
            }

            return false;
        }

        private void CloneAndMove(ChildBlockSystem sourceBlock, Enums.ConnectionType targetPos)
        {
            var newBlock = Instantiate(PrefabReferences.Instance.block1X1Prefab, transform);
            var childSystem = newBlock.GetComponent<ChildBlockSystem>();

            childSystem.Initialize(targetPos, sourceBlock.transform.localPosition, sourceBlock.transform.localScale);
            childSystem.SetBlockColor(sourceBlock.blockColor);

            var targetPosData = DataManager.Instance.blockShapeScaleAndLocalPosList.Find(
                x => x.blockPositionType == GetBlockPositionType(targetPos));

            if (targetPosData != null)
            {
                var sourceConnectionData = DataManager.Instance.GetConnectionData(sourceBlock.position);
                var targetConnectionData = DataManager.Instance.GetConnectionData(targetPos);

                if (sourceConnectionData == null || targetConnectionData == null)
                {
                    Debug.LogError("Connection data is null!");
                    return;
                }

                var targetSourcePos = sourceBlock.transform.localPosition +
                                      new Vector3(sourceConnectionData.positionOffset.x,
                                          sourceConnectionData.positionOffset.y, 0);
                var targetClonePos = new Vector3(targetPosData.localPos.x, targetPosData.localPos.y, 0) +
                                     new Vector3(targetConnectionData.positionOffset.x,
                                         targetConnectionData.positionOffset.y, 0);

                sourceBlock.transform.DOLocalMove(targetSourcePos, 0.3f).SetEase(Ease.OutBack);
                newBlock.transform.DOLocalMove(targetClonePos, 0.3f).SetEase(Ease.OutBack);

                var targetSourceScale = new Vector3(
                    sourceConnectionData.scaleAdjustment.x,
                    sourceConnectionData.scaleAdjustment.y,
                    0.975f);

                var targetCloneScale = new Vector3(
                    targetConnectionData.scaleAdjustment.x,
                    targetConnectionData.scaleAdjustment.y,
                    0.975f);

                sourceBlock.transform.DOScale(targetSourceScale, 0.3f).SetEase(Ease.OutBack);
                newBlock.transform.DOScale(targetCloneScale, 0.3f).SetEase(Ease.OutBack);

            }
            else
            {
                Debug.LogError($"Could not find target position data for {targetPos}");
                return;
            }

            sourceBlock.SetConnection(targetPos);
            childSystem.SetConnection(sourceBlock.position);

            _childBlockMap[targetPos] = childSystem;
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