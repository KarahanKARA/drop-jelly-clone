using System;
using System.Collections.Generic;
using _TheGame._Scripts.Board;
using _TheGame._Scripts.Data;
using _TheGame._Scripts.Helpers;
using _TheGame._Scripts.Managers;
using _TheGame._Scripts.References;
using UnityEngine;

namespace _TheGame._Scripts.Block
{
    
    
    public class BlockSystem : MonoBehaviour
    {
        public List<BlockDataModel.ChildBlockData> childBlockDataList = new List<BlockDataModel.ChildBlockData>();
        private Dictionary<Enums.ConnectionType, ChildBlockSystem> childBlockMap = new Dictionary<Enums.ConnectionType, ChildBlockSystem>();
        
        [Header("FORMAT : COLUMN, ROW")]
        public Vector2Int positionData = new Vector2Int();

        private BoardGrid _boardGrid;
        
        private void OnEnable()
        {
            _boardGrid = ComponentReferences.Instance.boardGrid;
            positionData = new Vector2Int(-10, -10);
        }
        
        public void CreateChildBlock(Enums.ConnectionType positionType, 
            Enums.BlockColorType colorType, 
            Enums.ConnectionType connectedWith)
        {
            var childObj = Instantiate(PrefabReferences.Instance.block1X1Prefab, transform);
            var childBlockSystem = childObj.GetComponent<ChildBlockSystem>();

            var blockPosType = GetBlockPositionType(positionType);
            var shapeData = DataManager.Instance.blockShapeScaleAndLocalPosList.Find(
                x => x.blockPositionType == blockPosType);

            if (shapeData != null)
            {
                var initialPos = new Vector3(shapeData.localPos.x, shapeData.localPos.y, 0f);
                var initialScale = new Vector3(shapeData.localScale.x, shapeData.localScale.y, 1f);
        
                // Sırasıyla:
                // 1. Initialize et (pozisyon ve transform)
                childBlockSystem.Initialize(positionType, initialPos, initialScale);
        
                // 2. Rengi ayarla
                childBlockSystem.SetBlockColor(colorType);

                // 3. Eğer bağlantı varsa, bağlantıyı kur
                if (connectedWith != Enums.ConnectionType.None)
                {
                    childBlockSystem.SetConnection(connectedWith);
                }

                childBlockMap[positionType] = childBlockSystem;
            }
        }

        private Enums.BlockPositionType GetBlockPositionType(Enums.ConnectionType connectionType)
        {
            return connectionType switch
            {
                Enums.ConnectionType.TopLeft => Enums.BlockPositionType.TopLeft,
                Enums.ConnectionType.TopRight => Enums.BlockPositionType.TopRight,
                Enums.ConnectionType.BottomLeft => Enums.BlockPositionType.BottomLeft,
                Enums.ConnectionType.BottomRight => Enums.BlockPositionType.BottomRight,
                _ => Enums.BlockPositionType.TopLeft // default değer
            };
        }

        public void CheckSameColorAsNeighbors()
        {
            var boardGrid = ComponentReferences.Instance.boardGrid;
        
            if (positionData.x != 0) // SOL BLOK KONTROLÜ
            {
                var leftBlock = boardGrid.GetBlockSystemAt(positionData.x - 1, positionData.y);
                if (leftBlock != null)
                {
                    CompareAndConnectBlocks(leftBlock, Enums.ConnectionType.TopLeft);
                }
            }
        }
        
        public void UpdateChildBlockPosition(Enums.ConnectionType positionType, bool isConnected)
        {
            if (childBlockMap.TryGetValue(positionType, out var childBlock))
            {
                var shapeData = DataManager.Instance.blockShapeScaleAndLocalPosList.Find(
                    x => x.blockPositionType == (Enums.BlockPositionType)positionType);

                if (shapeData != null)
                {
                    Vector3 targetPos = new Vector3(shapeData.localPos.x, shapeData.localPos.y, 0f);
                    Vector3 targetScale = new Vector3(shapeData.localScale.x, shapeData.localScale.y, 1f);

                    if (isConnected)
                    {
                        if (targetPos.x < 0) targetPos.x += 0.0125f;
                        else if (targetPos.x > 0) targetPos.x -= 0.0125f;

                        if (targetPos.y > 0) targetPos.y -= 0.0125f;
                        else if (targetPos.y < 0) targetPos.y += 0.0125f;

                        targetScale *= 0.975f;
                    }

                    childBlock.transform.localPosition = targetPos;
                    childBlock.transform.localScale = targetScale;
                }
            }
        }
        
        private void CompareAndConnectBlocks(BlockSystem otherBlock, Enums.ConnectionType connectionType)
        {
            foreach (var myChild in childBlockDataList)
            {
                foreach (var otherChild in otherBlock.childBlockDataList)
                {
                    if (myChild.blockColorType == otherChild.blockColorType)
                    {
                        myChild.isConnected = true;
                        myChild.connectionType = connectionType;
                        otherChild.isConnected = true;
                        otherChild.connectionType = GetOppositeConnectionType(connectionType);
                    
                        // Visual update
                        // TODO: Child block referanslarını düzgün şekilde tutup güncellemek gerekiyor
                    }
                }
            }
        }
        
        private Enums.ConnectionType GetOppositeConnectionType(Enums.ConnectionType type)
        {
            return type switch
            {
                Enums.ConnectionType.TopLeft => Enums.ConnectionType.TopRight,
                Enums.ConnectionType.TopRight => Enums.ConnectionType.TopLeft,
                Enums.ConnectionType.BottomLeft => Enums.ConnectionType.BottomRight,
                Enums.ConnectionType.BottomRight => Enums.ConnectionType.BottomLeft,
                _ => Enums.ConnectionType.None
            };
        }
        
        
        private void OnDestroy()
        {
            if (_boardGrid != null)
            {
                _boardGrid.UnregisterBlockSystem(
                    positionData.x, 
                    positionData.y
                );
            }
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position,Vector3.one * 2);
        }
    }
}
