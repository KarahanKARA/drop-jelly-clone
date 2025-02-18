using _TheGame._Scripts.Block;
using _TheGame._Scripts.Data;
using DG.Tweening;
using UnityEngine;

namespace _TheGame._Scripts.Board
{
    public readonly struct GridPosition
    {
        public readonly float X;
        public readonly float Y;
        public readonly bool IsOccupied;

        public GridPosition(float x, float y, bool isOccupied = false)
        {
            X = x;
            Y = y;
            IsOccupied = isOccupied;
        }

        public Vector3 ToVector3() => new Vector3(X, Y, 0);
    }

    public class BoardGrid : MonoBehaviour
    {
        public bool showGizmos = true;
        private GridPosition[,] _boardPositions;
        private BlockSystem[,] blockSystemGrid;

        private void Awake()
        {
            InitializeBoardPositions();
            blockSystemGrid = new BlockSystem[GameData.BoardSize, GameData.BoardSize];
        }

        private void InitializeBoardPositions()
        {
            _boardPositions = new GridPosition[GameData.BoardSize, GameData.BoardSize];
            for (int row = 0; row < GameData.BoardSize; row++)
            {
                for (int col = 0; col < GameData.BoardSize; col++)
                {
                    float xPos = GameData.StartX + (col * GameData.Offset);
                    float yPos = GameData.StartY - (row * GameData.Offset);
                    _boardPositions[row, col] = new GridPosition(xPos, yPos);
                }
            }
        }

        public Vector2 GetPosition(int row, int col)
        {
            if (_boardPositions == null)
            {
                Debug.LogError("Board positions not initialized!");
                return Vector2.zero;
            }
            if (!IsValidPosition(row, col))
            {
                Debug.LogError($"Invalid position request: row={row}, col={col}");
                return Vector2.zero;
            }
            GridPosition pos = _boardPositions[row, col];
            return new Vector2(pos.X, pos.Y);
        }

        private bool IsValidPosition(int row, int col)
        {
            return row >= 0 && row < GameData.BoardSize && col >= 0 && col < GameData.BoardSize;
        }
        

        public BlockSystem GetBlockSystemAt(int column, int row)
        {
            if (IsValidPosition(column, row))
            {
                return blockSystemGrid[column, row];
            }
            return null;
        }
        
        
        public void RegisterBlockSystem(int column, int row, BlockSystem blockSystem)
        {
            if (IsValidPosition(column, row))
            {
                blockSystemGrid[column, row] = blockSystem;
            }
        }

        public void UnregisterBlockSystem(int column, int row)
        {
            if (IsValidPosition(column, row))
            {
                blockSystemGrid[column, row] = null;
            }
        }

        public (bool found, Vector2 position, int row) GetFirstEmptyPositionInColumn(int col)
        {
            if (col is < 0 or >= GameData.BoardSize)
            {
                Debug.LogError($"Invalid column: {col}");
                return (false, Vector2.zero, -1);
            }

            for (var row = GameData.BoardSize - 1; row >= 0; row--)
            {
                if (!_boardPositions[row, col].IsOccupied)
                {
                    return (true, new Vector2(_boardPositions[row, col].X, _boardPositions[row, col].Y), row);
                }
            }

            return (false, Vector2.zero, -1); 
        }

        public void SetPositionOccupied(int row, int col)
        {
            if (IsValidPosition(row, col))
            {
                var currentPos = _boardPositions[row, col];
                _boardPositions[row, col] = new GridPosition(currentPos.X, currentPos.Y, true);
            }
        }

        public void SetPositionEmpty(int row, int col)
        {
            if (IsValidPosition(row, col))
            {
                var currentPos = _boardPositions[row, col];
                _boardPositions[row, col] = new GridPosition(currentPos.X, currentPos.Y, false);
            }
        }
        public void ClearAllPositions()
        {
            for (int row = 0; row < GameData.BoardSize; row++)
            {
                for (int col = 0; col < GameData.BoardSize; col++)
                {
                    blockSystemGrid[col, row] = null;
                    SetPositionEmpty(row, col);
                }
            }
        }
        
        public void ApplyGravity(int col)
        {
            for (var row = GameData.BoardSize - 1; row >= 0; row--)
            {
                if (blockSystemGrid[col, row] == null)
                {
                    BlockSystem blockToMove = null;
                    var sourceRow = -1;
            
                    for (var rowAbove = row - 1; rowAbove >= 0; rowAbove--)
                    {
                        if (blockSystemGrid[col, rowAbove] != null)
                        {
                            blockToMove = blockSystemGrid[col, rowAbove];
                            sourceRow = rowAbove;
                            break;
                        }
                    }

                    if (blockToMove != null)
                    {
                        blockSystemGrid[col, sourceRow] = null;
                        SetPositionEmpty(sourceRow, col);

                        blockSystemGrid[col, row] = blockToMove;
                        SetPositionOccupied(row, col);
                
                        blockToMove.positionData = new Vector2Int(col, row);

                        var targetPos = new Vector3(
                            _boardPositions[row, col].X,
                            _boardPositions[row, col].Y,
                            blockToMove.transform.position.z
                        );

                        blockToMove.transform.DOMove(targetPos, 0.3f)
                            .SetEase(Ease.OutBounce);
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (_boardPositions == null || !showGizmos) return;
            
            for (var row = 0; row < GameData.BoardSize; row++)
            {
                for (var col = 0; col < GameData.BoardSize; col++)
                {
                    var pos = _boardPositions[row, col];
                    Gizmos.color = pos.IsOccupied ? Color.red : Color.green;
                    Gizmos.DrawSphere(pos.ToVector3(), 0.2f);
                }
            }
        }
    }
}