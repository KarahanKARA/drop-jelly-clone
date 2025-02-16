using _TheGame._Scripts.Helpers;
using _TheGame._Scripts.Managers;
using DG.Tweening;
using UnityEngine;

namespace _TheGame._Scripts.Block
{
    public class ChildBlockSystem : MonoBehaviour
    {
        public float animationDuration;
        
        [Space(20)]
        
        public Enums.BlockColorType currentBlockColorType;
        public Enums.BlockShapeType currentBlockShapeType;
        private MeshRenderer _meshRenderer;
        
        private void OnEnable()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        public void SetBlockColor(Enums.BlockColorType blockColorType)
        {
            currentBlockColorType = blockColorType;
            var baseMaterial = DataManager.Instance.GetMaterialByColor(blockColorType);
            var materialInstance = new Material(baseMaterial);
            currentBlockColorType = blockColorType;
            _meshRenderer.materials = new[] { materialInstance };
        }

        public void SetBlockShape(Enums.BlockShapeType blockShapeType)
        {
            currentBlockShapeType = blockShapeType;
            var (scale, position) =DataManager.Instance.GetShapeData(blockShapeType);
            Vector3 scaleVector = scale;
            Vector3 posVector = position;
            scaleVector.z = 1;
            posVector.z = -1;
            
            transform.DOScale(scaleVector, animationDuration);
            transform.DOLocalMove(posVector, animationDuration);
        }
    }
}
