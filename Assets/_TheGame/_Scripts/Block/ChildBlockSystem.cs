using _TheGame._Scripts.Data;
using _TheGame._Scripts.Helpers;
using _TheGame._Scripts.Managers;
using UnityEngine;

namespace _TheGame._Scripts.Block
{
    public class ChildBlockSystem : MonoBehaviour
    {
        [Header("Block Properties")]
        public Enums.ConnectionType position;
        public Enums.BlockColorType blockColor;
        
        [Header("Connection Info")]
        [SerializeField] private bool isConnected;
        [SerializeField] private Enums.ConnectionType connectedWith;

        private MeshRenderer _meshRenderer;
        private Vector3 _originalPosition;
        private Vector3 _originalScale;
        
        private void Awake()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        // İlk üretildiğinde çağrılacak
        public void Initialize(Enums.ConnectionType positionType, Vector3 initialPos, Vector3 initialScale)
        {
            SetPosition(positionType);
            SetInitialTransform(initialPos, initialScale);
        }

        // Pozisyon tipini ayarla
        private void SetPosition(Enums.ConnectionType positionType)
        {
            position = positionType;
        }

        // İlk transform değerlerini ayarla
        private void SetInitialTransform(Vector3 initialPos, Vector3 initialScale)
        {
            _originalPosition = initialPos;
            _originalScale = initialScale;
            transform.localPosition = initialPos;
            transform.localScale = initialScale;
        }

        // Renk ayarla
        public void SetBlockColor(Enums.BlockColorType blockColorType)
        {
            blockColor = blockColorType;
            var baseMaterial = DataManager.Instance.GetMaterialByColor(blockColorType);
            var materialInstance = new Material(baseMaterial);
            _meshRenderer.materials = new[] { materialInstance };
        }

        // Connection işlemleri
        public void SetConnection(Enums.ConnectionType targetPosition)
        {
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

        // Connection transform değişikliklerini uygula
        private void ApplyConnectionTransform()
        {
            var connectionData = DataManager.Instance.GetConnectionData(position); // Kendi pozisyonuna göre offset al
            if (connectionData != null)
            {
                // Scale'i direkt set et
                transform.localScale = new Vector3(
                    connectionData.scaleAdjustment.x,
                    connectionData.scaleAdjustment.y,
                    0.975f
                );

                // Position için offset ekle
                Vector3 newPosition = _originalPosition;
                newPosition.x += connectionData.positionOffset.x;
                newPosition.y += connectionData.positionOffset.y;
                transform.localPosition = newPosition;

                Debug.Log($"Connection applied for {position} with {connectedWith}:");
                Debug.Log($"Original Position: {_originalPosition}");
                Debug.Log($"Applied Offset: {connectionData.positionOffset}");
                Debug.Log($"New Position: {newPosition}");
            }
        }

        // Transform'u orijinal haline döndür
        private void ResetTransform()
        {
            transform.localPosition = _originalPosition;
            transform.localScale = _originalScale;
        }
    }
}