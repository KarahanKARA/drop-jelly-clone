using _TheGame._Scripts.Data;
using _TheGame._Scripts.Helpers;
using _TheGame._Scripts.References;
using _TheGame._Scripts.Managers; 
using UnityEngine;

namespace _TheGame._Scripts.Block
{
    public class BlockCreatorSystem : MonoBehaviour
    {
        private BlockMovement _currentActiveBlock;
        public BlockMovement CurrentActiveBlock => _currentActiveBlock;
        
        private BlockDataModel.GameMoveData _moveData;
        private int _currentBlockIndex = 0;

        private void Start()
        {
            _moveData = JsonHelper.LoadMoveData();
            CreateNewBlock();
        }

        private void CreateNewBlock()
        {
            if (_moveData == null || _currentBlockIndex >= _moveData.blocks.Length)
            {
                Debug.LogWarning("No more blocks in move data!");
                return;
            }

            var newBlock = Instantiate(PrefabReferences.Instance.blockPrefab, 
                GameData.BlockSpawnPos, 
                Quaternion.identity);
                
            newBlock.transform.parent = ComponentReferences.Instance.createdBlockParent;
            
            var blockSystem = newBlock.GetComponent<BlockSystem>();
            CreateChildBlocksFromData(blockSystem, _moveData.blocks[_currentBlockIndex]);
            
            _currentActiveBlock = newBlock.GetComponent<BlockMovement>();
            _currentActiveBlock.OnBlockPlaced += HandleBlockPlaced;
            
            _currentBlockIndex++;
        }

        private void CreateChildBlocksFromData(BlockSystem blockSystem, BlockDataModel.BlockData blockData)
        {
            foreach (var childData in blockData.children)
            {
                var positionType = GetConnectionTypeFromString(childData.position);
                var colorType = GetColorTypeFromString(childData.color);
                var connectedWith = childData.connectedWith != null ? 
                    GetConnectionTypeFromString(childData.connectedWith) : 
                    Enums.ConnectionType.None;

                blockSystem.CreateChildBlock(positionType, colorType, connectedWith);
            }
        }

        private Enums.ConnectionType GetConnectionTypeFromString(string position)
        {
            return position switch
            {
                "TopLeft" => Enums.ConnectionType.TopLeft,
                "TopRight" => Enums.ConnectionType.TopRight,
                "BottomLeft" => Enums.ConnectionType.BottomLeft,
                "BottomRight" => Enums.ConnectionType.BottomRight,
                _ => Enums.ConnectionType.None
            };
        }

        private Enums.BlockColorType GetColorTypeFromString(string color)
        {
            return color switch
            {
                "Red" => Enums.BlockColorType.Red,
                "Blue" => Enums.BlockColorType.Blue,
                "Green" => Enums.BlockColorType.Green,
                "Yellow" => Enums.BlockColorType.Yellow,
                "Purple" => Enums.BlockColorType.Purple,
                "Pink" => Enums.BlockColorType.Pink,
                _ => Enums.BlockColorType.None
            };
        }

        private void HandleBlockPlaced()
        {
            if (_currentActiveBlock != null)
            {
                _currentActiveBlock.OnBlockPlaced -= HandleBlockPlaced;
            }
            BoardFlowManager.Instance.StartFullFlowWithCallback(CreateNewBlock);
        }
    }
}
