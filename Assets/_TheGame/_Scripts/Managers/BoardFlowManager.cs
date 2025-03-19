using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _TheGame._Scripts.Block;
using _TheGame._Scripts.Data;
using _TheGame._Scripts.References;
using DG.Tweening;
using UnityEngine;

namespace _TheGame._Scripts.Managers
{
    /// <summary>
    /// Manages the flow of gameplay on the board, handling block destruction, gravity, and game state checks.
    /// </summary>
    public class BoardFlowManager : Singleton<BoardFlowManager>
    {
        [SerializeField] private float destroyAnimationDuration = 0.3f;
        [SerializeField] private float gravityDelay = 0.8f;
        
        private List<BlockSystem> _registeredBlockSystems = new List<BlockSystem>();

        public event Action OnFlowCompleted;
        public event Action OnBoardCleared;

        private void Awake()
        {
            // Initialize empty list
            _registeredBlockSystems = new List<BlockSystem>();
        }

        /// <summary>
        /// Registers a BlockSystem with the manager to avoid using FindObjectsOfType
        /// </summary>
        public void RegisterBlockSystem(BlockSystem blockSystem)
        {
            if (!_registeredBlockSystems.Contains(blockSystem))
            {
                _registeredBlockSystems.Add(blockSystem);
            }
        }

        /// <summary>
        /// Unregisters a BlockSystem when it's destroyed or removed
        /// </summary>
        public void UnregisterBlockSystem(BlockSystem blockSystem)
        {
            _registeredBlockSystems.Remove(blockSystem);
        }

        /// <summary>
        /// Starts the full flow of gameplay with a callback when completed
        /// </summary>
        public void StartFullFlowWithCallback(Action onComplete)
        {
            StartCoroutine(FullFlowRoutine(onComplete));
        }

        private IEnumerator FullFlowRoutine(Action onComplete)
        {
            yield return new WaitForSeconds(0.2f);
            
            bool hasDestroyedBlocks;
            
            do
            {
                // Create a list for blocks to destroy (more efficient than HashSet for this use case)
                var blocksToDestroy = new List<ChildBlockSystem>();
                
                // Collect all blocks to destroy from currently registered block systems
                foreach (var blockSystem in _registeredBlockSystems)
                {
                    if (blockSystem == null) continue;
                    
                    var blocks = blockSystem.FindBlocksToDestroy();
                    if (blocks != null && blocks.Count > 0)
                    {
                        blocksToDestroy.AddRange(blocks);
                    }
                }
                
                // Remove duplicates (if any)
                blocksToDestroy = blocksToDestroy.Distinct().ToList();
                
                hasDestroyedBlocks = blocksToDestroy.Count > 0;
                
                if (hasDestroyedBlocks)
                {
                    yield return StartCoroutine(DestroyBlocksRoutine(blocksToDestroy));
                    
                    // Refresh board grid positions
                    RefreshBoardGrid();
                    
                    // Process block expansions and transformations
                    ProcessBlockTransformations();
                    
                    // Apply gravity to all columns
                    ApplyGravityToAllColumns();
                    
                    yield return new WaitForSeconds(gravityDelay);
                }
                
            } while (hasDestroyedBlocks);

            // Check for win condition
            CheckForWinCondition(onComplete);
        }
        
        private void RefreshBoardGrid()
        {
            if (ComponentReferences.Instance.boardGrid == null) return;
            
            ComponentReferences.Instance.boardGrid.ClearAllPositions();
            
            // Use only registered block systems
            foreach (var blockSystem in _registeredBlockSystems)
            {
                if (blockSystem == null) continue;
                
                var pos = blockSystem.positionData;
                ComponentReferences.Instance.boardGrid.RegisterBlockSystem(pos.x, pos.y, blockSystem);
                ComponentReferences.Instance.boardGrid.SetPositionOccupied(pos.y, pos.x);
            }
        }
        
        private void ProcessBlockTransformations()
        {
            // Process expansion and square formation
            foreach (var blockSystem in _registeredBlockSystems.ToList())
            {
                if (blockSystem == null) continue;
                
                blockSystem.CheckExpand();
                blockSystem.TryMakeBigSquareIfSingleColor();
                blockSystem.CheckIfEmpty(); // This might destroy empty blocks
            }
        }
        
        private void ApplyGravityToAllColumns()
        {
            if (ComponentReferences.Instance.boardGrid == null) return;
            
            for (var c = 0; c < GameData.BoardSize; c++)
            {
                ComponentReferences.Instance.boardGrid.ApplyGravity(c);
            }
        }
        
        private void CheckForWinCondition(Action onComplete)
        {
            if (ComponentReferences.Instance.boardGrid != null &&
                !ComponentReferences.Instance.boardGrid.HasAnyOccupiedPosition())
            {
                OnBoardCleared?.Invoke();
            }
            else
            {
                OnFlowCompleted?.Invoke();
                onComplete?.Invoke();
            }
        }

        private IEnumerator DestroyBlocksRoutine(List<ChildBlockSystem> blocksToDestroy)
        {
            if (blocksToDestroy == null || blocksToDestroy.Count == 0) yield break;

            var center = CalculateCenterPoint(blocksToDestroy);
            center.z = -5f;
            
            // Create animation sequence
            var seq = DOTween.Sequence();
            
            // Add grow and move animations
            foreach (var block in blocksToDestroy)
            {
                if (block == null) continue;
                seq.Join(block.transform.DOScale(block.transform.localScale * 1.1f, destroyAnimationDuration * 0.5f));
                seq.Join(block.transform.DOMove(center, destroyAnimationDuration * 0.5f).SetEase(Ease.OutQuad));
            }
            
            // Add shrink animations
            foreach (var block in blocksToDestroy)
            {
                if (block == null) continue;
                seq.Join(block.transform.DOScale(0f, destroyAnimationDuration * 0.5f).SetEase(Ease.InBack));
            }
            
            // Wait for animation to complete
            var completed = false;
            seq.OnComplete(() => completed = true);
            while (!completed) yield return null;
            
            // Create particles and destroy blocks
            CreateDestroyParticles(blocksToDestroy);
            
            // Clean up and remove blocks from their parents
            RemoveDestroyedBlocks(blocksToDestroy);
        }
        
        private void CreateDestroyParticles(List<ChildBlockSystem> blocks)
        {
            foreach (var block in blocks)
            {
                if (block == null) continue;
                
                var color = DataManager.Instance.GetColorFromBlock(block.blockColor);
                var particleObject = Instantiate(PrefabReferences.Instance.blockDestroyParticle, block.transform.position, Quaternion.identity);
                
                var particleSystems = particleObject.GetComponentsInChildren<ParticleSystem>();
                foreach (var particleSystem in particleSystems)
                {
                    var main = particleSystem.main;
                    main.startColor = color;
                }
                
                Destroy(particleObject, 2f);
            }
        }
        
        private void RemoveDestroyedBlocks(List<ChildBlockSystem> blocks)
        {
            foreach (var block in blocks)
            {
                if (block == null) continue;
                
                var parent = block.transform.parent.GetComponent<BlockSystem>();
                if (parent != null) 
                {
                    parent._childBlockMap.Remove(block.position);
                }
                
                Destroy(block.gameObject);
            }
        }

        private Vector3 CalculateCenterPoint(List<ChildBlockSystem> blocks)
        {
            if (blocks == null || blocks.Count == 0) return Vector3.zero;
            
            var sumPosition = Vector3.zero;
            var validBlockCount = 0;
            
            foreach (var block in blocks)
            {
                if (block == null) continue;
                
                sumPosition += block.transform.position;
                validBlockCount++;
            }
            
            return validBlockCount > 0 ? sumPosition / validBlockCount : Vector3.zero;
        }
    }
}