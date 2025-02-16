using System;
using System.Collections.Generic;
using _TheGame._Scripts.Helpers;
using UnityEngine;

namespace _TheGame._Scripts.Managers
{
    [Serializable]
    public class BlockColor
    {
        public Enums.BlockColorType blockColorType;
        public Material blockColorMaterial;
    }

    [Serializable]
    public class BlockShapeData
    {
        public Enums.BlockShapeType blockShapeType;
        public Vector2 localPos;
        public Vector2 localScale;
    }
    
    public class DataManager : Singleton<DataManager>
    {
        public List<BlockColor> blockColorList = new List<BlockColor>();
        public List<BlockShapeData> blockShapeScaleAndLocalPosList = new List<BlockShapeData>();

        public Material GetMaterialByColor(Enums.BlockColorType colorType)
        {
            return blockColorList.Find(x => x.blockColorType == colorType).blockColorMaterial;
        }

        public (Vector2 scale, Vector2 position) GetShapeData(Enums.BlockShapeType shapeType)
        {
            var shapeData = blockShapeScaleAndLocalPosList.Find(x => x.blockShapeType == shapeType);
            return (shapeData.localScale, shapeData.localPos);
        }
    }
}
