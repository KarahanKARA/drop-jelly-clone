using _TheGame._Scripts.Helpers;
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
            _meshRenderer.materials[0] = DataManager.Instance.GetMaterialByColor(blockColorType);
        }

        public void SetBlockShape(Enums.BlockShapeType blockShapeType)
        {
            var (scale, position) =DataManager.Instance.GetShapeData(blockShapeType);
            transform.DOScale(scale, animationDuration);
            transform.DOLocalMove(position, animationDuration);
        }
    }
}
