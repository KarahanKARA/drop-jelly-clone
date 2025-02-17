using _TheGame._Scripts.Helpers;
using _TheGame._Scripts.Managers;
using UnityEngine;

namespace _TheGame._Scripts.Block
{
    public class ChildBlockSystem : MonoBehaviour
    {
        public bool IsConnected => isConnected;
        public Enums.ConnectionType ConnectedWith => connectedWith;

        [Header("Block Properties")]
        public Enums.ConnectionType position;
        public Enums.BlockColorType blockColor;

        [Header("Connection Info")]
        [SerializeField] private bool isConnected;
        [SerializeField] private Enums.ConnectionType connectedWith;

        private MeshRenderer _meshRenderer;
        private Vector3 _originalPosition;
        private Vector3 _originalScale;

        public bool isBigSquare = false;

        private void Awake()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        public void Initialize(Enums.ConnectionType positionType, Vector3 initialPos, Vector3 initialScale)
        {
            SetPosition(positionType);
            SetInitialTransform(initialPos, initialScale);
        }

        private void SetPosition(Enums.ConnectionType positionType)
        {
            position = positionType;
        }

        private void SetInitialTransform(Vector3 initialPos, Vector3 initialScale)
        {
            _originalPosition = initialPos;
            _originalScale = initialScale;
            transform.localPosition = initialPos;
            transform.localScale = initialScale;
        }

        public void SetBlockColor(Enums.BlockColorType blockColorType)
        {
            blockColor = blockColorType;
            var baseMaterial = DataManager.Instance.GetMaterialByColor(blockColorType);
            var materialInstance = new Material(baseMaterial);
            _meshRenderer.materials = new[] { materialInstance };
        }

        public void SetConnection(Enums.ConnectionType targetPosition)
        {
            if (isBigSquare) return; 
            isConnected = true;
            connectedWith = targetPosition;
            ApplyConnectionTransform();
        }

        public void RemoveConnection()
        {
            isConnected = false;
            connectedWith = Enums.ConnectionType.None;
            ResetTransform();
        }

        private void ApplyConnectionTransform()
        {
            var connectionData = DataManager.Instance.GetConnectionData(position);
            if (connectionData != null)
            {
                transform.localScale = new Vector3(
                    connectionData.scaleAdjustment.x,
                    connectionData.scaleAdjustment.y,
                    0.975f
                );

                var newPosition = _originalPosition;
                newPosition.x += connectionData.positionOffset.x;
                newPosition.y += connectionData.positionOffset.y;
                transform.localPosition = newPosition;
            }
        }

        private void ResetTransform()
        {
            transform.localPosition = _originalPosition;
            transform.localScale = _originalScale;
        }
    }
}
