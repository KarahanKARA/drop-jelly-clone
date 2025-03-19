using System;
using _TheGame._Scripts.Data;
using _TheGame._Scripts.Helpers;
using _TheGame._Scripts.References;
using _TheGame._Scripts.Managers; 
using UnityEngine;

namespace _TheGame._Scripts.Block
{
    /// <summary>
    /// Manages block creation, movement, and placement during gameplay.
    /// Handles loading move data from level files and creating the appropriate blocks.
    /// </summary>
    public class BlockCreatorSystem : MonoBehaviour
    {
        public static event Action<int> OnRemainingMovesChanged;
        public static event Action<BlockMovement> OnBlockCreated;
        public static event Action<BlockSystem> OnBlockPlaced;
        
        private BlockMovement _currentActiveBlock;
        public BlockMovement CurrentActiveBlock => _currentActiveBlock;
        
        private BlockDataModel.GameMoveData _moveData;
        private int _currentBlockIndex = 0;
        private int _totalMoves = 0;

        #region Unity Lifecycle Methods
        
        private void OnEnable()
        {
            if (BoardFlowManager.Instance != null)
            {
                BoardFlowManager.Instance.OnFlowCompleted += OnBoardFlowCompleted;
            }
        }
        
        private void OnDisable()
        {
            if (BoardFlowManager.Instance != null)
            {
                BoardFlowManager.Instance.OnFlowCompleted -= OnBoardFlowCompleted;
            }
            
            if (_currentActiveBlock != null)
            {
                _currentActiveBlock.OnBlockPlaced -= HandleBlockPlaced;
            }
        }

        private void Start()
        {
            LoadLevelData();
            if (_moveData != null && _moveData.blocks != null)
            {
                _totalMoves = _moveData.blocks.Length;
                UpdateMovesUI();
            }

            CreateNewBlock();
        }
        
        #endregion

        #region Level Data Loading
        
        /// <summary>
        /// Loads the move data for the current level
        /// </summary>
        private void LoadLevelData()
        {
            var levelId = SaveManager.Instance.GetLastLevelIndex();
            _moveData = JsonHelper.LoadMoveData(levelId);
            
            if (_moveData == null || _moveData.blocks == null || _moveData.blocks.Length == 0)
            {
                Debug.LogError($"Failed to load move data for level {levelId}!");
            }
        }
        
        #endregion

        #region Block Creation and Management
        
        /// <summary>
        /// Creates a new block based on the current move data
        /// </summary>
        private void CreateNewBlock()
        {
            if (_moveData == null || _currentBlockIndex >= _moveData.blocks.Length)
            {
                CheckGameOver();
                return;
            }

            var newBlock = Instantiate(
                PrefabReferences.Instance.blockPrefab, 
                GameData.BlockSpawnPos, 
                Quaternion.identity
            );
                
            newBlock.transform.SetParent(ComponentReferences.Instance.createdBlockParent, true);
            
            var blockSystem = newBlock.GetComponent<BlockSystem>();
            CreateChildBlocksFromData(blockSystem, _moveData.blocks[_currentBlockIndex]);
            
            _currentActiveBlock = newBlock.GetComponent<BlockMovement>();
            _currentActiveBlock.OnBlockPlaced += HandleBlockPlaced;
            
            _currentBlockIndex++;
            
            OnBlockCreated?.Invoke(_currentActiveBlock);
        }

        /// <summary>
        /// Creates child blocks according to the block data
        /// </summary>
        private void CreateChildBlocksFromData(BlockSystem blockSystem, BlockDataModel.BlockData blockData)
        {
            foreach (var childData in blockData.children)
            {
                var positionType = GetConnectionTypeFromString(childData.position);
                var colorType = GetColorTypeFromString(childData.color);
                var connectedWith = !string.IsNullOrEmpty(childData.connectedWith)
                    ? GetConnectionTypeFromString(childData.connectedWith)
                    : Enums.ConnectionType.None;

                blockSystem.CreateChildBlock(positionType, colorType, connectedWith);
            }
            
            if (blockData.isSquare)
            {
                foreach (var childBlock in blockSystem._childBlockMap.Values)
                {
                    childBlock.isBigSquare = true;
                }
                blockSystem.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
            }
        }

        /// <summary>
        /// Checks if the game is over (no more moves available)
        /// </summary>
        private void CheckGameOver()
        {
            var grid = ComponentReferences.Instance.boardGrid;
            if (grid != null && grid.HasAnyOccupiedPosition())
            {
                UiManager.Instance?.GameFail();
            }
        }
        
        /// <summary>
        /// Updates the UI to show remaining moves
        /// </summary>
        private void UpdateMovesUI()
        {
            var remainingMoves = _totalMoves - _currentBlockIndex;
            UiManager.Instance?.SetMovesText(remainingMoves);
            OnRemainingMovesChanged?.Invoke(remainingMoves);
        }
        
        #endregion

        #region Event Handlers
        
        /// <summary>
        /// Handles the event when a block is placed on the grid
        /// </summary>
        private void HandleBlockPlaced()
        {
            if (_currentActiveBlock != null)
            {
                _currentActiveBlock.OnBlockPlaced -= HandleBlockPlaced;
                
                var blockSystem = _currentActiveBlock.GetComponent<BlockSystem>();
                if (blockSystem != null)
                {
                    OnBlockPlaced?.Invoke(blockSystem);
                }
            }

            UpdateMovesUI();

            BoardFlowManager.Instance?.StartFullFlowWithCallback(null);
        }
        
        /// <summary>
        /// Called when the board flow is completed (after matches and gravity)
        /// </summary>
        private void OnBoardFlowCompleted()
        {
            CreateNewBlock();
        }
        
        #endregion

        #region Helper Methods

        /// <summary>
        /// Converts a position string to a ConnectionType enum
        /// </summary>
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

        /// <summary>
        /// Converts a color string to a BlockColorType enum
        /// </summary>
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
                "Orange" => Enums.BlockColorType.Orange,
                "DarkBlue" => Enums.BlockColorType.DarkBlue,
                _ => Enums.BlockColorType.None
            };
        }
        
        #endregion
    }
}