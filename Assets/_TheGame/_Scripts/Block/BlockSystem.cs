using System;
using System.Collections.Generic;
using _TheGame._Scripts.Helpers;
using _TheGame._Scripts.References;
using UnityEngine;

namespace _TheGame._Scripts.Block
{
    [Serializable]
    public class ChildBlockData
    {
        public Enums.BlockColorType blockColorType;
        public Enums.BlockShapeType blockShapeType;
    }
    public class BlockSystem : MonoBehaviour
    {
        public List<ChildBlockData> childBlockDataList = new List<ChildBlockData>();
        
        public void CreateChildBlocks()
        {
            for (var i = 0; i < 3; i++)
            {
                var childObj = Instantiate(PrefabReferences.Instance.block1X1Prefab,transform);
                var childBlockSystem = childObj.GetComponent<ChildBlockSystem>();
                childBlockSystem.SetBlockColor((Enums.BlockColorType)(i + 1));
                childBlockSystem.SetBlockShape((Enums.BlockShapeType)(i + 1));
                var data = new ChildBlockData
                {
                    blockShapeType = (Enums.BlockShapeType)(i + 1),
                    blockColorType = (Enums.BlockColorType)(i + 1)
                };
                childBlockDataList.Add(data);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position,Vector3.one * 2);
        }
    }
}
