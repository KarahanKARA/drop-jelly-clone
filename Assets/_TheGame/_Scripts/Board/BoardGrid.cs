using System;
using _TheGame._Scripts.Data;
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
        

        private void Awake()
        {
            InitializeBoardPositions();
        }

        private void InitializeBoardPositions()
        {
            _boardPositions = new GridPosition[GameData.BoardSize, GameData.BoardSize];

            for (var row = 0; row < GameData.BoardSize; row++)
            {
                for (var col = 0; col < GameData.BoardSize; col++)
                {
                    var xPos = GameData.StartX + (col * GameData.Offset);
                    var yPos = GameData.StartY - (row * GameData.Offset);
                    _boardPositions[row, col] = new GridPosition(xPos, yPos);
                }
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

        public bool IsPositionOccupied(int row, int col)
        {
            return IsValidPosition(row, col) && _boardPositions[row, col].IsOccupied;
        }

        public Vector2[] GetColumnPositions(int col)
        {
            if (col is < 0 or >= GameData.BoardSize)
            {
                Debug.LogError($"Invalid column: {col}");
                return Array.Empty<Vector2>();
            }

            var positions = new Vector2[GameData.BoardSize];
            for (var row = 0; row < GameData.BoardSize; row++)
            {
                positions[row] = new Vector2(_boardPositions[row, col].X, _boardPositions[row, col].Y);
            }
            return positions;
        }

        private bool IsValidPosition(int row, int col)
        {
            return row >= 0 && row < GameData.BoardSize && col >= 0 && col < GameData.BoardSize;
        }

        public Vector2 GetPosition(int row, int col)
        {
            if (!IsValidPosition(row, col))
            {
                Debug.LogError($"Invalid position request: row={row}, col={col}");
                return Vector2.zero;
            }

            var pos = _boardPositions[row, col];
            return new Vector2(pos.X, pos.Y);
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