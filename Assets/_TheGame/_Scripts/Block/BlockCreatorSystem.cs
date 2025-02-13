using _TheGame._Scripts.Data;
using _TheGame._Scripts.References;
using UnityEngine;

namespace _TheGame._Scripts.Block
{
    public class BlockCreatorSystem : MonoBehaviour
    {
        private BlockMovement _currentActiveBlock;
        public BlockMovement CurrentActiveBlock => _currentActiveBlock;

        private void Start()
        {
            CreateNewBlock();
        }

        public void CreateNewBlock()
        {
            var newBlock = Instantiate(PrefabReferences.Instance.blockPrefab, GameData.BlockSpawnPos, Quaternion.identity);
            _currentActiveBlock = newBlock.GetComponent<BlockMovement>();
            _currentActiveBlock.OnBlockPlaced += HandleBlockPlaced;
        }

        private void HandleBlockPlaced()
        {
            if (_currentActiveBlock != null)
            {
                _currentActiveBlock.OnBlockPlaced -= HandleBlockPlaced;
            }
            CreateNewBlock();
        }
    }
}
