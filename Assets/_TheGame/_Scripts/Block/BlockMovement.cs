using System;
using _TheGame._Scripts.Data;
using _TheGame._Scripts.References;
using DG.Tweening;
using UnityEngine;

namespace _TheGame._Scripts.Block
{
    public class BlockMovement : MonoBehaviour
    {
        public event Action OnBlockPlaced;
    
        public AnimationCurve animationCurve;
        public bool IsPlaced { get; private set; }
        public bool IsMoving { get; private set; }  
    
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float xDuration = 0.2f;
    
        private Vector3 _initialPosition;
        private bool _isDragging;
        private float _fixedYPosition;
        private float _fixedZPosition;

        public BlockSystem blockSystem;
        
        private void Awake()
        {
            _initialPosition = transform.position;
            _fixedYPosition = _initialPosition.y;
            _fixedZPosition = _initialPosition.z;
            IsPlaced = false;
            IsMoving = false;
        }

        public void StartDragging()
        {
            if (IsPlaced || IsMoving) return;
            _isDragging = true;
        }

        public void UpdateDragPosition(float xPosition)
        {
            if (!_isDragging || IsPlaced || IsMoving) return;
        
            transform.position = new Vector3(
                xPosition,
                _fixedYPosition,
                _fixedZPosition
            );
        }

        public void EndDragging()
        {
            if (IsPlaced || IsMoving || !_isDragging) return;
            _isDragging = false;
            IsMoving = true;  
            MoveToNearestColumn();
        }

        private void MoveToNearestColumn()
        {
            var currentX = transform.position.x;
            var nearestColumn = FindNearestColumn(currentX);

            var (found, targetPosition, row) = ComponentReferences.Instance.boardGrid.GetFirstEmptyPositionInColumn(nearestColumn);

            if (!found)
            {
                transform.DOMove(_initialPosition, moveSpeed)
                    .SetSpeedBased()
                    .OnComplete(() => IsMoving = false);
                return;
            }

            blockSystem.positionData = new Vector2Int(nearestColumn, row);
            
            transform.DOMoveX(targetPosition.x, xDuration).OnComplete(() =>
            {
                var distance = Mathf.Abs(transform.position.y - targetPosition.y);
                var duration = distance / moveSpeed;

                transform.DOMoveY(targetPosition.y, duration)
                    .SetEase(animationCurve)
                    .OnComplete(() =>
                    {
                        IsPlaced = true;
                        IsMoving = false;
                        ComponentReferences.Instance.boardGrid.SetPositionOccupied(row, nearestColumn);
                        ComponentReferences.Instance.boardGrid.RegisterBlockSystem(
                            nearestColumn, 
                            row, 
                            blockSystem
                        );
                        OnBlockPlaced?.Invoke();
                    });
            });
        }

        private int FindNearestColumn(float xPosition)
        {
            var firstColumnPos = ComponentReferences.Instance.boardGrid.GetPosition(0, 0);
            var relativeX = xPosition - firstColumnPos.x;
            var column = Mathf.RoundToInt(relativeX / GameData.ColumnWidth);
            return Mathf.Clamp(column, 0, 5);
        }
    }
}
