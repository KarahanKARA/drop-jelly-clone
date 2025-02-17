using System;
using System.Collections.Generic;
using _TheGame._Scripts.Helpers;
using UnityEngine;

namespace _TheGame._Scripts.Managers
{
    [Serializable]
    public class BlockConnectionData
    {
        public Enums.ConnectionType connectionType;
        public Vector2 positionOffset;   
        public Vector2 scaleAdjustment; 
    }
    
    [Serializable]
    public class BlockColor
    {
        public Enums.BlockColorType blockColorType;
        public Material blockColorMaterial;
    }

    [Serializable]
    public class BlockShapeData
    {
        public Enums.BlockPositionType blockPositionType;
        public Vector2 localPos;
        public Vector2 localScale;
    }
    
    public class DataManager : Singleton<DataManager>
    {
        public List<BlockColor> blockColorList = new List<BlockColor>();
        public List<BlockShapeData> blockShapeScaleAndLocalPosList = new List<BlockShapeData>();

        
        public List<BlockConnectionData> connectionDataList = new List<BlockConnectionData>();
        

        public BlockConnectionData GetConnectionData(Enums.ConnectionType type)
        {
            return connectionDataList.Find(x => x.connectionType == type);
        }
        
        public Material GetMaterialByColor(Enums.BlockColorType colorType)
        {
            return blockColorList.Find(x => x.blockColorType == colorType).blockColorMaterial;
        }
    }
}
