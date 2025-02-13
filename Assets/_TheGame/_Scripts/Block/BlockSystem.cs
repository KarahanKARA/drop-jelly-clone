using _TheGame._Scripts.Helpers;
using _TheGame._Scripts.References;
using UnityEngine;

namespace _TheGame._Scripts.Block
{
    public class BlockSystem : MonoBehaviour
    {
        public void CreateChildBlocks()
        {
            for (int i = 0; i < 3; i++)
            {
                var childObj = Instantiate(PrefabReferences.Instance.block1X1Prefab,transform);
                var childBlockSystem = childObj.GetComponent<ChildBlockSystem>();
                childBlockSystem.SetBlockColor((Enums.BlockColorType)(i + 1));
                childBlockSystem.SetBlockShape((Enums.BlockShapeType)(i + 1)); 
            }
        }
    }
}
