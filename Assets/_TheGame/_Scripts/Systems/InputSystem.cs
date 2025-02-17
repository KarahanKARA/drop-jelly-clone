using _TheGame._Scripts.Block;
using _TheGame._Scripts.Data;
using _TheGame._Scripts.References;
using UnityEngine;

namespace _TheGame._Scripts.Systems
{
    public class InputSystem : MonoBehaviour
    {
        private Camera _mainCamera;
        private bool _isDragging;
        private float _lastXPosition;
        private Plane _dragPlane;
        private BlockCreatorSystem _blockCreatorSystem;
        private BlockHorizontalAnimation _currentBlockAnimation;
        
        private void Start()
        {
            if (_mainCamera == null)
                _mainCamera = Camera.main;

            _blockCreatorSystem = SystemReferences.Instance.blockCreatorSystem;
        }

        private void Update()
        {
            if (_blockCreatorSystem.CurrentActiveBlock == null) return;
            if (_blockCreatorSystem.CurrentActiveBlock.IsMoving) return; 
        
            _dragPlane = new Plane(Vector3.forward, 
                new Vector3(0, 0, _blockCreatorSystem.CurrentActiveBlock.transform.position.z));

            if (Input.GetMouseButtonDown(0) && !_isDragging) 
            {
                HandleDragStart();
            }
            else if (_isDragging && Input.GetMouseButton(0))
            {
                HandleDragging();
            }
            else if (_isDragging && Input.GetMouseButtonUp(0))
            {
                HandleDragEnd();
            }
        }

        private void HandleDragStart()
        {
            if (_blockCreatorSystem.CurrentActiveBlock.IsPlaced || 
                _blockCreatorSystem.CurrentActiveBlock.IsMoving) return;

            ObjectReferences.Instance.blurObject.SetActive(true);
            var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

            if (_dragPlane.Raycast(ray, out var entry))
            {
                _isDragging = true;
                var hitPoint = ray.GetPoint(entry);
                _lastXPosition = hitPoint.x;
        
                _currentBlockAnimation = _blockCreatorSystem.CurrentActiveBlock
                    .GetComponent<BlockHorizontalAnimation>();
            
                _blockCreatorSystem.CurrentActiveBlock.StartDragging();
            }
        }

        private void HandleDragging()
        {
            var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

            if (_dragPlane.Raycast(ray, out var entry))
            {
                var hitPoint = ray.GetPoint(entry);
                _lastXPosition = hitPoint.x;
                _blockCreatorSystem.CurrentActiveBlock.UpdateDragPosition(_lastXPosition);

                if (_currentBlockAnimation != null)
                    _currentBlockAnimation.UpdateHorizontalAnimation(hitPoint.x);

                var currentX = hitPoint.x;
                var firstColumnPos = ComponentReferences.Instance.boardGrid.GetPosition(0, 0);
                var column = Mathf.RoundToInt((currentX - firstColumnPos.x) / GameData.ColumnWidth);
                column = Mathf.Clamp(column, 0, 5);

                var targetColumnPos = ComponentReferences.Instance.boardGrid.GetPosition(0, column);

                var blurTransform = ObjectReferences.Instance.blurObject.transform;
                blurTransform.position = new Vector3(
                    targetColumnPos.x,
                    blurTransform.position.y,
                    blurTransform.position.z
                );
            }
        }

        private void HandleDragEnd()
        {
            ObjectReferences.Instance.blurObject.SetActive(false);
            _isDragging = false;
        
            _currentBlockAnimation.ResetRotation();
            _blockCreatorSystem.CurrentActiveBlock.EndDragging();
            _currentBlockAnimation = null;
        }
    }
}