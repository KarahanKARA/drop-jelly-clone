using System.Collections.Generic;
using System.Linq;
using _TheGame._Scripts.Board;
using _TheGame._Scripts.Constants;
using _TheGame._Scripts.Data;
using _TheGame._Scripts.Helpers;
using _TheGame._Scripts.Managers;
using _TheGame._Scripts.References;
using DG.Tweening;
using UnityEngine;

namespace _TheGame._Scripts.Block
{
    /// <summary>
    /// Manages a block system consisting of multiple child blocks.
    /// Handles expansion, connection, destruction and transformation of blocks.
    /// </summary>
    public class BlockSystem : MonoBehaviour
    {
        private const int BoardSize = GameData.BoardSize;

        public Dictionary<Enums.ConnectionType, ChildBlockSystem> _childBlockMap
            = new Dictionary<Enums.ConnectionType, ChildBlockSystem>();

        public Vector2Int positionData = new Vector2Int();
        private BoardGrid _boardGrid;

        [Header("Animation Settings")]
        [SerializeField] private float expandAnimationDuration = 0.4f;
        [SerializeField] private float scaleAnimationDuration = 0.1f;

        private BoardFlowManager _boardFlowManager;
        #region Unity Lifecycle Methods

        private void OnEnable()
        {
            _boardGrid = ComponentReferences.Instance.boardGrid;
            positionData = new Vector2Int(-10, -10);
            
            if (BoardFlowManager.Instance != null)
            {
                _boardFlowManager = BoardFlowManager.Instance;
                _boardFlowManager.RegisterBlockSystem(this);
            }
        }

        private void OnDestroy()
        {
            if (_boardGrid != null) 
            {
                _boardGrid.UnregisterBlockSystem(positionData.x, positionData.y);
            }
            
            if (_boardFlowManager != null)
            {
                _boardFlowManager.UnregisterBlockSystem(this);
            }
            
            DOTween.Kill(transform);
        }

        #endregion

        #region Block Creation and Management

        /// <summary>
        /// Creates a child block with specified position, color and connection
        /// </summary>
        public void CreateChildBlock(Enums.ConnectionType positionType, Enums.BlockColorType colorType,
            Enums.ConnectionType connectedWith)
        {
            var blockPositionType = GetBlockPositionType(positionType);
            var shapeData = DataManager.Instance.blockShapeScaleAndLocalPosList
                .Find(x => x.blockPositionType == blockPositionType);
                
            if (shapeData == null) return;

            var obj = Instantiate(PrefabReferences.Instance.block1X1Prefab, transform);
            var childBlock = obj.GetComponent<ChildBlockSystem>();
            
            var initPos = new Vector3(shapeData.localPos.x, shapeData.localPos.y, 0f);
            var initScale = new Vector3(shapeData.localScale.x, shapeData.localScale.y, 1f);
            
            childBlock.Initialize(positionType, initPos, initScale);
            childBlock.SetBlockColor(colorType);

            if (connectedWith != Enums.ConnectionType.None)
            {
                childBlock.SetConnection(connectedWith);
            }

            _childBlockMap[positionType] = childBlock;
        }

        /// <summary>
        /// Finds all blocks that should be destroyed based on matching colors
        /// </summary>
        public List<ChildBlockSystem> FindBlocksToDestroy()
        {
            var blocksToDestroy = new List<ChildBlockSystem>();
            
            foreach (var block in _childBlockMap.Values.Where(b => b != null))
            {
                if (block.isBigSquare)
                {
                    ProcessBigSquareBlock(block, blocksToDestroy);
                    continue;
                }
                
                ProcessRegularBlock(block, blocksToDestroy);
            }
            
            return blocksToDestroy;
        }
        
        private void ProcessBigSquareBlock(ChildBlockSystem block, List<ChildBlockSystem> blocksToDestroy)
        {
            var neighbors = GetExactNeighborBlocks(block);
            var matchingNeighbors = neighbors.Where(n => n != null && n.blockColor == block.blockColor).ToList();
            
            if (matchingNeighbors.Count > 0)
            {
                foreach (var childBlock in _childBlockMap.Values.Where(child => child.blockColor == block.blockColor))
                {
                    blocksToDestroy.Add(childBlock);
                }
                
                foreach (var neighborBlock in matchingNeighbors)
                {
                    var neighborSystem = neighborBlock.transform.parent.GetComponent<BlockSystem>();
                    if (neighborSystem != null && neighborSystem != this)
                    {
                        foreach (var childBlock in neighborSystem._childBlockMap.Values)
                        {
                            if (childBlock.blockColor == block.blockColor)
                            {
                                blocksToDestroy.Add(childBlock);
                            }
                        }
                    }
                }
            }
        }
        
        private void ProcessRegularBlock(ChildBlockSystem block, List<ChildBlockSystem> blocksToDestroy)
        {
            var neighbors = GetExactNeighborBlocks(block);
            
            foreach (var neighbor in neighbors)
            {
                if (neighbor == null || neighbor.blockColor != block.blockColor) continue;
                
                blocksToDestroy.Add(block);
                blocksToDestroy.Add(neighbor);
                
                var neighborSystem = neighbor.transform.parent.GetComponent<BlockSystem>();
                if (neighborSystem != null && neighborSystem != this)
                {
                    foreach (var childBlock in neighborSystem._childBlockMap.Values)
                    {
                        if (childBlock.blockColor == block.blockColor)
                        {
                            blocksToDestroy.Add(childBlock);
                        }
                    }
                }
                
                FindConnectedBlocksToDestroy(block, blocksToDestroy);
                FindConnectedBlocksToDestroy(neighbor, blocksToDestroy);
            }
        }
        
        private void FindConnectedBlocksToDestroy(ChildBlockSystem block, List<ChildBlockSystem> blocksToDestroy)
        {
            if (block == null || !block.IsConnected) return;
            
            var connectedBlock = GetConnectedBlock(block);
            if (connectedBlock != null && connectedBlock.blockColor == block.blockColor)
            {
                blocksToDestroy.Add(connectedBlock);
            }
        }
        
        private ChildBlockSystem GetConnectedBlock(ChildBlockSystem block)
        {
            if (block == null || !block.IsConnected) return null;
            
            return _childBlockMap.TryGetValue(block.ConnectedWith, out var connectedBlock) ? connectedBlock : null;
        }

        #endregion

        #region Block Expansion and Transformation

        /// <summary>
        /// Checks if the block system should expand to fill empty positions
        /// </summary>
        public void CheckExpand()
        {
            var allPositions = new[]
            {
                Enums.ConnectionType.TopLeft,
                Enums.ConnectionType.TopRight,
                Enums.ConnectionType.BottomLeft,
                Enums.ConnectionType.BottomRight
            };
            
            var emptyPositions = allPositions.Where(p => !_childBlockMap.ContainsKey(p)).ToList();
            
            if (emptyPositions.Count == 0) return;
            
            foreach (var emptyPosition in emptyPositions)
            {
                if (TryExpandVertically(emptyPosition)) continue;
                TryExpandHorizontally(emptyPosition);
            }
            
            UpdateBoardGridRegistration();
        }
        
        private void UpdateBoardGridRegistration()
        {
            if (_boardGrid == null || _childBlockMap.Count == 0) return;
            
            var col = positionData.x;
            var row = positionData.y;
            
            if (col >= 0 && row >= 0 && col < BoardSize && row < BoardSize)
            {
                _boardGrid.RegisterBlockSystem(col, row, this);
                _boardGrid.SetPositionOccupied(row, col);
            }
        }
        
        private bool TryExpandVertically(Enums.ConnectionType emptyPos)
        {
            var blocks = _childBlockMap.Values.ToList();
            
            var sourceBlock = blocks.FirstOrDefault(b =>
            {
                if (b.IsConnected) return false;
                
                return (b.position == Enums.ConnectionType.TopLeft && emptyPos == Enums.ConnectionType.BottomLeft) ||
                       (b.position == Enums.ConnectionType.TopRight && emptyPos == Enums.ConnectionType.BottomRight) ||
                       (b.position == Enums.ConnectionType.BottomLeft && emptyPos == Enums.ConnectionType.TopLeft) ||
                       (b.position == Enums.ConnectionType.BottomRight && emptyPos == Enums.ConnectionType.TopRight);
            });
            
            if (sourceBlock != null)
            {
                CloneAndMove(sourceBlock, emptyPos);
                return true;
            }
            
            return false;
        }
        
        private void TryExpandHorizontally(Enums.ConnectionType emptyPos)
        {
            var blocks = _childBlockMap.Values.ToList();
            
            var sourceBlock = blocks.FirstOrDefault(b =>
            {
                if (b.IsConnected) return false;
                
                return (b.position == Enums.ConnectionType.TopLeft && emptyPos == Enums.ConnectionType.TopRight) ||
                       (b.position == Enums.ConnectionType.TopRight && emptyPos == Enums.ConnectionType.TopLeft) ||
                       (b.position == Enums.ConnectionType.BottomLeft && emptyPos == Enums.ConnectionType.BottomRight) ||
                       (b.position == Enums.ConnectionType.BottomRight && emptyPos == Enums.ConnectionType.BottomLeft);
            });
            
            if (sourceBlock != null)
            {
                CloneAndMove(sourceBlock, emptyPos);
            }
        }
        
        private void CloneAndMove(ChildBlockSystem source, Enums.ConnectionType targetPos)
        {
            var newBlock = Instantiate(PrefabReferences.Instance.block1X1Prefab, transform);
            var clonedBlock = newBlock.GetComponent<ChildBlockSystem>();
            
            clonedBlock.Initialize(targetPos, source.transform.localPosition, source.transform.localScale);
            clonedBlock.SetBlockColor(source.blockColor);
            
            var targetShape = DataManager.Instance.blockShapeScaleAndLocalPosList
                .Find(x => x.blockPositionType == GetBlockPositionType(targetPos));
                
            if (targetShape == null)
            {
                Debug.LogError($"Could not find target position data for {targetPos}");
                Destroy(newBlock);
                return;
            }
            
            var sourceConnectionData = DataManager.Instance.GetConnectionData(source.position);
            var targetConnectionData = DataManager.Instance.GetConnectionData(targetPos);
            
            if (sourceConnectionData == null || targetConnectionData == null)
            {
                Debug.LogError("Connection data is null!");
                Destroy(newBlock);
                return;
            }
            
            var finalSourcePosition = source.transform.localPosition +
                                 new Vector3(sourceConnectionData.positionOffset.x, sourceConnectionData.positionOffset.y, 0);
                                 
            var finalClonedPosition = new Vector3(targetShape.localPos.x, targetShape.localPos.y, 0) +
                                new Vector3(targetConnectionData.positionOffset.x, targetConnectionData.positionOffset.y, 0);
            
            var finalSourceScale = new Vector3(sourceConnectionData.scaleAdjustment.x, sourceConnectionData.scaleAdjustment.y, 0.975f);
            var finalClonedScale = new Vector3(targetConnectionData.scaleAdjustment.x, targetConnectionData.scaleAdjustment.y, 0.975f);
            
            source.transform.DOLocalMove(finalSourcePosition, NumericConstants.ExpandAnimDuration);
            source.transform.DOScale(finalSourceScale, NumericConstants.ExpandAnimDuration);
            
            newBlock.transform.DOLocalMove(finalClonedPosition, NumericConstants.ExpandAnimDuration);
            newBlock.transform.DOScale(finalClonedScale, NumericConstants.ExpandAnimDuration);
            
            source.SetConnection(targetPos);
            clonedBlock.SetConnection(source.position);
            
            _childBlockMap[targetPos] = clonedBlock;
        }

        /// <summary>
        /// Checks if the block system is now empty and should be destroyed
        /// </summary>
        public void CheckIfEmpty()
        {
            if (_childBlockMap.Count == 0)
            {
                var col = positionData.x;
                var row = positionData.y;
                
                if (_boardGrid != null)
                {
                    _boardGrid.UnregisterBlockSystem(col, row);
                    _boardGrid.SetPositionEmpty(row, col);
                }
                
                Destroy(gameObject);
                
                if (_boardGrid != null) 
                {
                    _boardGrid.ApplyGravity(col);
                }
            }
        }

        /// <summary>
        /// Try to convert the block system into a big square if all blocks are the same color
        /// </summary>
        public void TryMakeBigSquareIfSingleColor()
        {
            if (_childBlockMap.Count < 2) return;
            
            var blocks = _childBlockMap.Values.ToList();
            var firstBlockColor = blocks[0].blockColor;
            
            if (blocks.Any(block => block.blockColor != firstBlockColor))
            {
                return;
            }
            
            var allPositions = new[]
            {
                Enums.ConnectionType.TopLeft,
                Enums.ConnectionType.TopRight,
                Enums.ConnectionType.BottomLeft,
                Enums.ConnectionType.BottomRight
            };
            
            var missingPositions = allPositions
                .Where(position => !_childBlockMap.ContainsKey(position))
                .ToList();
                
            if (missingPositions.Count == 0) return;
            
            foreach (var missingPosition in missingPositions)
            {
                CloneForBigSquare(missingPosition, firstBlockColor);
            }
            
            foreach (var block in _childBlockMap.Values)
            {
                block.isBigSquare = true;
            }
        }
        
        private void CloneForBigSquare(Enums.ConnectionType targetPos, Enums.BlockColorType color)
        {
            var obj = Instantiate(PrefabReferences.Instance.block1X1Prefab, transform);
            var newBlock = obj.GetComponent<ChildBlockSystem>();
            
            var shape = DataManager.Instance.blockShapeScaleAndLocalPosList
                .Find(x => x.blockPositionType == GetBlockPositionType(targetPos));
                
            if (shape == null)
            {
                Debug.LogError($"Could not find target position data for {targetPos}");
                Destroy(obj);
                return;
            }
            
            var connectionData = DataManager.Instance.GetConnectionData(targetPos);
            if (connectionData == null)
            {
                Debug.LogError($"Connection data is null for big square pos: {targetPos}");
                Destroy(obj);
                return;
            }
            
            var initialPosition = new Vector3(shape.localPos.x, shape.localPos.y, 0f);
            var initialScale = new Vector3(shape.localScale.x, shape.localScale.y, 1f);
            
            newBlock.Initialize(targetPos, initialPosition, initialScale);
            newBlock.SetBlockColor(color);
            
            var finalPosition = initialPosition + new Vector3(connectionData.positionOffset.x, connectionData.positionOffset.y, 0f);
            var finalScale = new Vector3(connectionData.scaleAdjustment.x, connectionData.scaleAdjustment.y, 0.975f);
            
            obj.transform.DOLocalMove(finalPosition, expandAnimationDuration);
            obj.transform.DOScale(finalScale, scaleAnimationDuration);
            
            newBlock.isBigSquare = true;
            _childBlockMap[targetPos] = newBlock;
        }

        #endregion

        #region Neighbor and Block Position Utilities

        /// <summary>
        /// Gets the neighboring blocks that match the exact position of the provided block
        /// </summary>
        private List<ChildBlockSystem> GetExactNeighborBlocks(ChildBlockSystem block)
        {
            var neighbors = new List<ChildBlockSystem>();
            
            var x = positionData.x;
            var y = positionData.y;
            
            switch (block.position)
            {
                case Enums.ConnectionType.TopLeft:
                    if (x - 1 >= 0) neighbors.Add(GetChildBlockAt(x - 1, y, Enums.ConnectionType.TopRight));
                    if (y - 1 >= 0) neighbors.Add(GetChildBlockAt(x, y - 1, Enums.ConnectionType.BottomLeft));
                    break;
                    
                case Enums.ConnectionType.TopRight:
                    if (x + 1 < BoardSize) neighbors.Add(GetChildBlockAt(x + 1, y, Enums.ConnectionType.TopLeft));
                    if (y - 1 >= 0) neighbors.Add(GetChildBlockAt(x, y - 1, Enums.ConnectionType.BottomRight));
                    break;
                    
                case Enums.ConnectionType.BottomLeft:
                    if (x - 1 >= 0) neighbors.Add(GetChildBlockAt(x - 1, y, Enums.ConnectionType.BottomRight));
                    if (y + 1 < BoardSize) neighbors.Add(GetChildBlockAt(x, y + 1, Enums.ConnectionType.TopLeft));
                    break;
                    
                case Enums.ConnectionType.BottomRight:
                    if (x + 1 < BoardSize) neighbors.Add(GetChildBlockAt(x + 1, y, Enums.ConnectionType.BottomLeft));
                    if (y + 1 < BoardSize) neighbors.Add(GetChildBlockAt(x, y + 1, Enums.ConnectionType.TopRight));
                    break;
            }
            
            return neighbors.Where(n => n != null).ToList();
        }
        
        /// <summary>
        /// Gets a child block at the specified position from the board grid
        /// </summary>
        private ChildBlockSystem GetChildBlockAt(int col, int row, Enums.ConnectionType position)
        {
            if (_boardGrid == null) return null;
            
            var blockSystem = _boardGrid.GetBlockSystemAt(col, row);
            if (blockSystem == null) return null;
            
            return blockSystem._childBlockMap.GetValueOrDefault(position);
        }
        
        /// <summary>
        /// Converts a connection type to a block position type
        /// </summary>
        private Enums.BlockPositionType GetBlockPositionType(Enums.ConnectionType connectionType)
        {
            switch (connectionType)
            {
                case Enums.ConnectionType.TopLeft:
                    return Enums.BlockPositionType.TopLeft;
                case Enums.ConnectionType.TopRight:
                    return Enums.BlockPositionType.TopRight;
                case Enums.ConnectionType.BottomLeft:
                    return Enums.BlockPositionType.BottomLeft;
                case Enums.ConnectionType.BottomRight:
                    return Enums.BlockPositionType.BottomRight;
                default:
                    return Enums.BlockPositionType.None;
            }
        }

        #endregion
    }
}