using UnityEngine;

namespace _TheGame._Scripts.Systems
{
    public class InputSystem : MonoBehaviour
    {
        [SerializeField] private BlockSystem activeBlock;
        
        private Camera _mainCamera;
        
        private bool _isDragging;
        private float _lastXPosition;
        private Plane _dragPlane;

        private void Start()
        {
            if (_mainCamera == null)
                _mainCamera = Camera.main;
                
            _dragPlane = new Plane(Vector3.forward, new Vector3(0, 0, activeBlock.transform.position.z));
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
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
            var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

            if (_dragPlane.Raycast(ray, out var entry))
            {
                _isDragging = true;
                var hitPoint = ray.GetPoint(entry);
                _lastXPosition = hitPoint.x;
                activeBlock.StartDragging();
            }
        }

        private void HandleDragging()
        {
            var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

            if (_dragPlane.Raycast(ray, out var entry))
            {
                var hitPoint = ray.GetPoint(entry);
                _lastXPosition = hitPoint.x;
                activeBlock.UpdateDragPosition(_lastXPosition);
            }
        }

        private void HandleDragEnd()
        {
            _isDragging = false;
            activeBlock.EndDragging();
        }
    }
}