using _TheGame._Scripts.Board;
using _TheGame._Scripts.Data;
using _TheGame._Scripts.References;
using DG.Tweening;
using UnityEngine;

namespace _TheGame._Scripts.Systems
{
    public class BlockSystem : MonoBehaviour
    {
        public AnimationCurve animationCurve;
        
        [SerializeField] private float moveSpeed = 10f;
        
        private Vector3 _initialPosition;
        private bool _isDragging;
        private float _fixedYPosition;
        private float _fixedZPosition;
        private BoardGrid _boardGrid;
        
        private void OnEnable()
        {
            _boardGrid = ComponentReferences.Instance.boardGrid;
        }

        private void Awake()
        {
            _initialPosition = transform.position;
            _fixedYPosition = _initialPosition.y;
            _fixedZPosition = _initialPosition.z;
        }

        public void StartDragging()
        {
            _isDragging = true;
        }

        public void UpdateDragPosition(float xPosition)
        {
            if (!_isDragging) return;
            
            transform.position = new Vector3(
                xPosition,
                _fixedYPosition,
                _fixedZPosition
            );
        }

        public void EndDragging()
        {
            _isDragging = false;
            MoveToNearestColumn();
        }

        private void MoveToNearestColumn()
        {
            var currentX = transform.position.x;
            var nearestColumn = FindNearestColumn(currentX);
        
            var (found, targetPosition, row) = ComponentReferences.Instance.boardGrid.GetFirstEmptyPositionInColumn(nearestColumn);
        
            if (!found)
            {
                transform.DOMove(_initialPosition, moveSpeed).SetSpeedBased();
                return;
            }

            transform.DOMoveX(targetPosition.x, moveSpeed * 4).SetSpeedBased();
        
            var distance = Mathf.Abs(transform.position.y - targetPosition.y);
            transform.DOMoveY(targetPosition.y, distance)
                .SetSpeedBased()
                .SetEase(animationCurve);
        
            _boardGrid.SetPositionOccupied(row, nearestColumn);
        }

        private int FindNearestColumn(float xPosition)
        {
            var firstColumnPos = _boardGrid.GetPosition(0, 0);
            
            var relativeX = xPosition - firstColumnPos.x;
            var column = Mathf.RoundToInt(relativeX / GameData.ColumnWidth);
            
            return Mathf.Clamp(column, 0, 5);
        }
    }
}